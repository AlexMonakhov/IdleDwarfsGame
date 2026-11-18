import { Container } from 'pixi.js';
import {
  LOOKS, attachLook, disposeLook, removeLook, stepLook, triggerLook,
  type ConceptUnit, type Look, type LookInstance, type StageUnits,
} from '@/services/pixiFx/concepts';
import type { PixiScene } from '@/services/pixiFx/PixiScene';
import type { Point } from '@/services/particleService/types/emitter';

// Статусы в бою на обликах (services/pixiFx/concepts/statusLooks.ts): висят на юните, пока статус на нём,
// срабатывают на тике. Прежние визуалы (FxRegistry.STATUS_VISUALS: эмиттеры aura и tick) остаются в проекте
// и играют для статусов без облика (метка) или пока сцена не готова. Окраска силуэта статусом — по-прежнему оттуда

// Имя статуса с сервера → облик (выбраны на странице /fx-concepts)
export const BATTLE_STATUS_LOOKS: Record<string, string> = {
  Burn: 'sl-burn-tongues',
  Poison: 'sl-poison-bubbles',
  Shock: 'sl-shock-arcs',
  Frost: 'sl-frost-mist',
  Freeze: 'sl-freeze-statue',
  Stun: 'sl-stun-stars',
  Curse: 'sl-curse-shadow',
  Bleed: 'sl-bleed-drips',
  Regen: 'sl-regen-halo',
  // Мгновенный, статусом не висит: облик ставится, срабатывает и сразу снимается
  EnergyBurn: 'sl-eburn-flare',
};

// Эффект ауры героя → облик на носителе (выбраны на странице /fx-concepts; у крови — сердце, у остальных — купола). Прежняя сфера aura-sphere
// остаётся и рисуется для аур без облика
export const BATTLE_AURA_LOOKS: Record<string, string> = {
  Burn: 'al-burn-dome',
  Frost: 'al-frost-dome',
  Shock: 'al-shock-dome',
  Poison: 'al-poison-bubble',
  EnergyBurn: 'al-void-dome',
  Bleed: 'al-blood-heart',
  Regen: 'al-regen-dome',
};
// Ключ ауры среди обликов юнита, чтобы не путать со статусом того же имени (горение-аура и горение-статус)
const auraKey = (effect: string) => `aura:${effect}`;

// Сколько мгновенный облик живёт после срабатывания, с
const INSTANT_LIFE = 1.4;
// Сколько ждать сцену для облика, повешенного раньше неё, мс
const PENDING_WAIT_MS = 20000;
// Префикс id, по которому BattleFx.stopAura отличает облик от эмиттера
export const LOOK_ID_PREFIX = 'look:';

type UnitEl = HTMLElement | null | undefined;

interface Attached {
  id: string;
  el: HTMLElement;
  name: string;
  inst: LookInstance;
  // Ноги юнита в момент наложения: облик следует за юнитом сдвигом своих контейнеров
  base: Point;
  // Мгновенный облик: снять после этого времени
  removeAt: number | null;
}

let sequence = 0;

const lookById = (id: string | undefined) => (id ? LOOKS.find(look => look.id === id) : undefined);

export class StatusLooks {
  private scene: PixiScene | null = null;
  private back: Container | null = null;
  private front: Container | null = null;
  private attached: Attached[] = [];
  // Облики, повешенные до того, как появилась сцена (она создаётся лениво): встают на место, как только она готова
  private pending: { id: string; el: HTMLElement; look: Look; name: string }[] = [];
  private waiter: ReturnType<typeof setInterval> | null = null;

  constructor(
    private readonly sceneOf: () => PixiScene | null,
    private readonly point: (el: UnitEl, anchor?: 'center' | 'feet' | 'head') => Point | null,
    private readonly speed: () => number,
    // Во сколько раз юнит крупнее обычного (драконид, голем, босс): облик рисуется как на обычном и растягивается от ног
    private readonly size: (el: UnitEl) => number = () => 1,
  ) {}

  lookFor(name: string): Look | undefined {
    return lookById(BATTLE_STATUS_LOOKS[name]);
  }

  // Повесить облик статуса на юнита; null — облика нет (тогда играет прежний визуал)
  start(el: UnitEl, name: string): string | null {
    return this.startLook(el, this.lookFor(name), name);
  }

  // Купол ауры на носителе; null — облика нет (тогда прежняя сфера)
  startAura(el: UnitEl, effect: string): string | null {
    return this.startLook(el, lookById(BATTLE_AURA_LOOKS[effect]), auraKey(effect));
  }

  // Аура сработала (начало раунда): купол вздрагивает
  triggerAura(el: UnitEl, effect: string) {
    return this.trigger(el, auraKey(effect));
  }

  // wait — сцены ещё нет: облик ждёт её (для мгновенных не ждём)
  private startLook(el: UnitEl, look: Look | undefined, name: string, wait = true): string | null {
    if (!look || !el) return null;
    const id = `${LOOK_ID_PREFIX}${++sequence}`;
    if (this.place(id, el, look, name)) return id;
    if (!wait) return null;
    this.pending.push({ id, el, look, name });
    this.waitForScene();
    return id;
  }

  private waitForScene() {
    if (this.waiter) return;
    const started = performance.now();
    this.waiter = setInterval(() => {
      this.pending = this.pending.filter(p => p.el.isConnected && !this.place(p.id, p.el, p.look, p.name));
      if (!this.pending.length || performance.now() - started > PENDING_WAIT_MS) {
        if (this.waiter) clearInterval(this.waiter);
        this.waiter = null;
        this.pending = [];
      }
    }, 100);
  }

  // Поставить облик на сцену; false — сцена или раскладка юнита ещё не готовы
  private place(id: string, el: HTMLElement, look: Look, name: string) {
    const scene = this.attach();
    if (!scene || !this.back || !this.front) return false;
    const center = this.point(el, 'center');
    if (!center) return false;
    const feet = this.point(el, 'feet') ?? center;
    const head = this.point(el, 'head') ?? center;
    // Облики нарисованы под обычного юнита (купол — фиксированных размеров в пикселях). Для крупного строим обычного
    // с теми же ногами и растягиваем контейнеры облика от ног: купол, частицы и круг на земле растут вместе с юнитом
    const k = Math.max(0.5, this.size(el) || 1);
    const shrink = (p: Point): Point => ({ x: feet.x + (p.x - feet.x) / k, y: feet.y + (p.y - feet.y) / k });
    const unit: ConceptUnit = {
      center: shrink(center),
      feet,
      head: shrink(head),
      // Вспышку силуэта и цифры урона на тике показывает BattlePlayer
      hit: () => undefined,
      heal: () => undefined,
    };
    const stage: StageUnits = {
      caster: unit, enemies: [unit], allies: [], bystanders: [], healed: [],
      width: scene.size.width, height: scene.size.height,
      cast: () => undefined, shake: () => undefined, flash: () => undefined,
    };
    try {
      const inst = attachLook(look, unit, stage, { back: this.back, front: this.front }, 3);
      inst.layers.forEach(layer => {
        layer.pivot.set(feet.x, feet.y);
        layer.position.set(feet.x, feet.y);
        layer.scale.set(k);
      });
      this.attached.push({ id, el, name, inst, base: feet, removeAt: null });
    } catch (error) {
      console.error('status look failed', name, error);
    }
    // Сбой облика не повод ждать дальше
    return true;
  }

  stop(id: string) {
    this.pending = this.pending.filter(p => p.id !== id);
    const item = this.attached.find(a => a.id === id);
    if (item) removeLook(item.inst);
  }

  // Срабатывание статуса на юните (тик, пропуск хода); true — облик есть и сработал
  trigger(el: UnitEl, name: string) {
    const items = this.attached.filter(a => a.el === el && a.name === name && a.inst.removedAt === null);
    items.forEach(a => triggerLook(a.inst));
    return items.length > 0;
  }

  // Мгновенный эффект (выжигание энергии): облик появляется, срабатывает и снимается сам
  instant(el: UnitEl, name: string) {
    const id = this.startLook(el, this.lookFor(name), name, false);
    const item = id ? this.attached.find(a => a.id === id) : undefined;
    if (!item) return false;
    triggerLook(item.inst);
    item.removeAt = item.inst.t + INSTANT_LIFE;
    return true;
  }

  has(name: string) {
    return !!this.lookFor(name);
  }

  clear() {
    this.attached.forEach(a => disposeLook(a.inst));
    this.attached = [];
    this.pending = [];
  }

  // Контейнеры и покадровый шаг на сцене арены (она создаётся лениво и может пересоздаться)
  private attach(): PixiScene | null {
    const scene = this.sceneOf();
    if (scene === this.scene && this.back && !this.back.destroyed) return scene;
    this.scene?.removeTicker(this.tick);
    this.attached = [];
    this.scene = scene;
    if (!scene) return null;
    this.back = new Container();
    this.front = new Container();
    scene.background.addChild(this.back);
    scene.foreground.addChild(this.front);
    scene.addTicker(this.tick);
    return scene;
  }

  private readonly tick = (frame: number) => {
    const dt = frame * this.speed();
    this.attached = this.attached.filter(a => {
      if (a.removeAt !== null && a.inst.t >= a.removeAt) removeLook(a.inst);
      // Юнит ушёл со сцены (бой закончился) — снимаем
      if (!a.el.isConnected) removeLook(a.inst);
      const now = this.point(a.el, 'feet');
      if (now) a.inst.layers.forEach(layer => layer.position.set(now.x, now.y));
      try {
        return !stepLook(a.inst, dt);
      } catch (error) {
        console.error('status look failed', a.name, error);
        disposeLook(a.inst);
        return false;
      }
    });
  };
}
