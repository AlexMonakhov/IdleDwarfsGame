import { Container, Graphics } from 'pixi.js';
import {
  conceptById, startConcept, stepConcept, stopConcept,
  type ArtifactTrigger, type Concept, type ConceptRun, type ConceptUnit, type SpellLevel, type StageUnits,
} from '@/services/pixiFx/concepts';
import { tintOf } from '@/services/pixiFx/effect';
import type { PixiScene } from '@/services/pixiFx/PixiScene';
import type { BattleFx } from './BattleFx';
import { getUnitView } from './UnitRegistry';

// Заклинания боя на концептах (services/pixiFx/concepts): один концепт на заклинание. Прежние визуалы
// (FxRegistry.SPELL_VISUALS) остаются в проекте и играют, если концепта нет или сцена ещё не готова
export const BATTLE_CONCEPTS: Record<string, string> = {
  Fireball: 'fire-serpent',
  'Flame Wave': 'fire-surf-pillars',
  Meteor: 'meteor-sky',
  'Frost Bolt': 'frost-lance',
  Blizzard: 'blizzard-icicles',
  'Glacial Prison': 'prison-fangs',
  Lightning: 'bolt-sky',
  'Chain Lightning': 'ground-surge',
  Thunderclap: 'thunder-dome',
  'Healing Rune': 'rune-bloom',
  'Venom Thorns': 'venom-spit',
  'Entangling Roots': 'roots-hand',
  'Shadow Bolt': 'void-orb',
  Hex: 'dark-eyes',
  'Soul Harvest': 'soul-harvest',
  'Life Drain': 'blood-soul-drain',
  Hemorrhage: 'blood-slash',
  'Blood Price': 'blood-price',
  'Prism Ray': 'prism-crystal',
  'Radiant Nova': 'supernova',
  Dawnlight: 'dawn',
};

// Срабатывание ауры: после прежних расходящихся кругов (BattleFx.auraBurst) волна наложения статуса — концепт
// из auras.ts по эффекту ауры (выбраны на странице /fx-concepts)
export const BATTLE_AURA_WAVES: Record<string, string> = {
  Burn: 'aura-burn-ring',
  Frost: 'aura-frost-ring',
  Shock: 'aura-shock-field',
  Poison: 'aura-poison-cloud',
  EnergyBurn: 'aura-void-eyes',
  Bleed: 'aura-blood-heart',
  Regen: 'aura-regen-sunbeams',
};

// Тряска в бою слабее, чем на странице концептов: полоски здоровья — в DOM и не трясутся вместе со сценой
const SHAKE_SCALE = 0.6;
// Дольше концепт не ждём, даже если он длиннее (зависшая вкладка, ошибка в концепте), с
const MAX_RUN = 6;

export interface ConceptCast {
  skillName: string;
  // Концепт напрямую (волна ауры); без него — по заклинанию из BATTLE_CONCEPTS
  conceptId?: string;
  level: number;
  damageType: string;
  casterId: number;
  // Враги, в которых бьёт заклинание, и союзники, которых лечит (по записям лога)
  enemyIds: number[];
  healIds: number[];
  // Все цели с записями: попадание или лечение по ним применяет их записи
  entryIds: number[];
  // Цели, на которых сработал артефакт (есть запись с TriggerKind)
  procIds: Set<number>;
  allyIds: number[];
  bystanderIds: number[];
  artifacts: string[];
  trigger: ArtifactTrigger | null;
  originId: number | null;
}

export interface ConceptPlayback {
  // По промису на цель с записями: резолвится, когда концепт по ней попал (hit) или вылечил (heal)
  hits: Map<number, Promise<void>>;
  // Концепту осталось доиграть не больше lead мс (или он уже доиграл)
  settled: Promise<void>;
}

interface Active {
  run: ConceptRun;
  // Время концепта (с), с которого он считается почти доигравшим, и что тогда сделать
  settleAt: number;
  settle: () => void;
  finish: () => void;
}

export class ConceptSpells {
  private back: Container | null = null;
  private front: Container | null = null;
  private flashLayer: Graphics | null = null;
  private scene: PixiScene | null = null;
  private active: Active[] = [];
  private flash: { alpha: number; left: number; total: number } | null = null;
  private shake: { power: number; left: number; total: number } | null = null;

  constructor(private readonly fx: BattleFx, private readonly speed: () => number) {}

  conceptFor(skillName: string, conceptId?: string): Concept | undefined {
    const id = conceptId ?? BATTLE_CONCEPTS[skillName];
    return id ? conceptById(id) : undefined;
  }

  // Сцена есть и заклинанию есть концепт с подходящими целями
  canPlay(cast: Pick<ConceptCast, 'skillName' | 'conceptId' | 'enemyIds' | 'healIds'>) {
    const concept = this.conceptFor(cast.skillName, cast.conceptId);
    if (!concept || !this.attach()) return false;
    // Концепту лечения нужны лечимые, остальным — хотя бы одна цель среди врагов
    return concept.heals && !concept.targets ? cast.healIds.length > 0 : cast.enemyIds.length > 0;
  }

  play(cast: ConceptCast, lead = 0): ConceptPlayback {
    const concept = this.conceptFor(cast.skillName, cast.conceptId);
    const scene = this.attach();
    const pending = new Map<number, () => void>();
    const hits = new Map<number, Promise<void>>();
    for (const id of cast.entryIds) hits.set(id, new Promise<void>(resolve => pending.set(id, resolve)));
    const resolveAll = () => {
      pending.forEach(resolve => resolve());
      pending.clear();
    };
    if (!concept || !scene || !this.back || !this.front) {
      resolveAll();
      return { hits, settled: Promise.resolve() };
    }

    const involved = new Set([cast.casterId, ...cast.enemyIds, ...cast.healIds]);
    const units = new Map<number, ConceptUnit>();
    const unit = (id: number): ConceptUnit => {
      let made = units.get(id);
      if (made) return made;
      const view = getUnitView(id);
      const el = view?.el?.() ?? null;
      const point = (anchor: 'center' | 'feet' | 'head') => this.fx.point(el, anchor) ?? { x: 0, y: 0 };
      const settle = () => {
        const resolve = pending.get(id);
        if (!resolve) return false;
        pending.delete(id);
        resolve();
        return true;
      };
      made = {
        center: point('center'),
        feet: point('feet'),
        head: point('head'),
        proc: cast.procIds.has(id),
        // Первое попадание применяет записи цели (урон, статусы — с их анимацией), следующие — только вспышка силуэта
        hit: (crit, flash) => {
          if (!settle() && involved.has(id)) view?.hit?.(cast.damageType, { crit, flash });
        },
        heal: () => {
          settle();
        },
      };
      units.set(id, made);
      return made;
    };

    const caster = unit(cast.casterId);
    const stage: StageUnits = {
      caster,
      enemies: cast.enemyIds.map(unit),
      allies: [caster, ...cast.allyIds.filter(id => id !== cast.casterId).map(unit)],
      bystanders: cast.bystanderIds.map(unit),
      healed: cast.healIds.map(unit),
      width: scene.size.width,
      height: scene.size.height,
      // Взмах кастера уже проиграл BattlePlayer
      cast: () => undefined,
      shake: (power, seconds = 0.3) => (this.shake = { power: power * SHAKE_SCALE, left: seconds, total: seconds }),
      flash: (css, alpha = 0.5, seconds = 0.3) => {
        if (!this.flashLayer) return;
        this.flashLayer.clear().rect(0, 0, scene.size.width, scene.size.height).fill(tintOf(css));
        this.flash = { alpha: alpha * 0.7, left: seconds, total: seconds };
      },
    };
    // Концепты нарисованы для кастера слева; кастер справа — отражаем
    const targets = [...stage.enemies, ...stage.healed].filter(u => u !== caster);
    const aim = targets.length ? targets.reduce((sum, u) => sum + u.center.x, 0) / targets.length : scene.size.width / 2;
    const mirror = aim < caster.center.x;
    const origin = cast.originId != null ? unit(cast.originId).center : null;

    let run: ConceptRun;
    try {
      run = startConcept(concept, stage, { back: this.back, front: this.front }, {
        level: Math.min(3, Math.max(1, cast.level || 1)) as SpellLevel,
        mirror,
        artifacts: cast.artifacts,
        trigger: cast.trigger,
        origin,
      });
    } catch (error) {
      console.error('concept spell failed', cast.skillName, error);
      resolveAll();
      return { hits, settled: Promise.resolve() };
    }
    run.duration = Math.min(run.duration, MAX_RUN);
    const settled = new Promise<void>(resolve => {
      this.active.push({
        run,
        settleAt: run.duration - lead / 1000,
        settle: resolve,
        finish: () => {
          resolveAll();
          resolve();
        },
      });
    });
    return { hits, settled };
  }

  // Убрать всё (бой закончился, перемотка)
  clear() {
    this.active.forEach(({ run, finish }) => {
      stopConcept(run);
      finish();
    });
    this.active = [];
    this.flash = null;
    this.shake = null;
    if (this.flashLayer) this.flashLayer.visible = false;
    this.scene?.app.stage.position.set(0, 0);
  }

  // Контейнеры и покадровый шаг на сцене арены (она создаётся лениво и может пересоздаться)
  private attach(): PixiScene | null {
    const scene = this.fx.scene();
    if (scene === this.scene && this.back && !this.back.destroyed) return scene;
    this.scene?.removeTicker(this.tick);
    this.active = [];
    this.scene = scene;
    if (!scene) return null;
    this.back = new Container();
    this.front = new Container();
    this.flashLayer = new Graphics();
    this.flashLayer.blendMode = 'add';
    this.flashLayer.visible = false;
    scene.background.addChild(this.back);
    scene.foreground.addChild(this.front, this.flashLayer);
    scene.addTicker(this.tick);
    return scene;
  }

  private readonly tick = (frame: number) => {
    const scene = this.scene;
    if (!scene) return;
    const dt = frame * this.speed();
    this.active = this.active.filter(({ run, settleAt, settle, finish }) => {
      let over: boolean;
      try {
        over = stepConcept(run, dt);
      } catch (error) {
        console.error('concept spell failed', error);
        stopConcept(run);
        over = true;
      }
      if (over) finish();
      else if (run.t >= settleAt) settle();
      return !over;
    });
    if (this.flash && this.flashLayer) {
      this.flash.left -= dt;
      this.flashLayer.visible = this.flash.left > 0;
      this.flashLayer.alpha = this.flash.alpha * Math.max(this.flash.left / this.flash.total, 0);
      if (this.flash.left <= 0) this.flash = null;
    }
    if (this.shake) {
      this.shake.left -= dt;
      const power = this.shake.power * Math.max(this.shake.left / this.shake.total, 0);
      scene.app.stage.position.set((Math.random() - 0.5) * 2 * power, (Math.random() - 0.5) * 2 * power);
      if (this.shake.left <= 0) {
        this.shake = null;
        scene.app.stage.position.set(0, 0);
      }
    }
  };
}
