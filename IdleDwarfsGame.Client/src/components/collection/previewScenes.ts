import { t, statusLabel } from '@/i18n';
import type { CombatLogEntry, CombatResult, CombatUnitInfo, DamageType, HeroElement, StatusEffectLogData } from '@/battle/types';
import {
    AuraSample,
    HEAL_STATUSES,
    PREVIEW_RANK,
    SKIP_TURN_STATUSES,
    STATUS_DAMAGE,
    SpellSample,
} from './catalog';

// Сцены превью в окне «Коллекция» — это обычные бои из синтетических логов:
// их проигрывает тот же BattlePlayer, что и арену, так что эффекты выглядят ровно как в бою

// Что показывает панель превью (CollectionSplit)
export interface CollectionPreview {
    // Смена ключа перезапускает сцену с нуля
    key: string;
    scene: CombatResult;
    // Стихия задаёт фон сцены
    element: HeroElement;
    auraPulse?: boolean;
    title: string;
    // Цвет заголовка — цвет эффекта
    color?: string;
    description?: string;
}

// Номинальные статы участников: числа на сцене для наглядности
const ATTACK = 80;
const MAX_HP = 1200;

// Сила статуса за ход на сцене: урон или лечение, у мороза — процент замедления энергии
const STATUS_POWER: Record<string, number> = {
    Burn: 24,
    Frost: 25,
    Poison: 24,
    Curse: 20,
    Shock: 16,
    Bleed: 16,
    Regen: 12,
};

// Статусы, где каждое наложение — отдельный слой со своими ходами (LayeredStatusEffect на сервере)
const LAYERED_STATUSES = new Set(['Burn', 'Poison', 'Shock', 'Frost', 'Regen']);

// Вкладка «Эффекты»: статус накладывается несколько раз подряд — по длительностям наложений видно,
// как он складывается (у кровотечения длительности нет, каждое наложение — ход)
const DEMO_APPLICATIONS: Record<string, number[]> = {
    Curse: [3, 1, 1],
    Bleed: [1, 1, 1],
    Stun: [2],
    Freeze: [2],
};
const DEMO_LAYERS = [3, 2, 1];

interface SceneStatus {
    layers: { power: number; turns: number }[];
    turns: number;
    power: number;
    stacks: number;
}

// Слои -> итог: сила суммой, длительность по самому долгому слою
const syncLayers = (status: SceneStatus) => {
    status.power = status.layers.reduce((sum, l) => sum + l.power, 0);
    status.turns = status.layers.reduce((max, l) => Math.max(max, l.turns), 0);
    status.stacks = status.layers.length;
};

const statusData = (name: string, status: SceneStatus): StatusEffectLogData => ({
    effectId: name,
    name,
    duration: status.turns,
    stacks: status.stacks,
    power: status.power,
});

// Сколько юнитов на стороне целей у заклинания по нескольким целям
const CROWD = 3;

// Реестр видов юнитов общий с ареной (UnitRegistry): id не должны пересекаться с её entityId.
// Цели идут подряд: TARGET, TARGET + 1, ...
const CASTER = 9001;
const TARGET = 9002;

interface UnitOptions {
    id: number;
    name: string;
    element: HeroElement;
    level: number;
    team: number;
    position: number;
    // Доля HP на старте: раненому есть что лечить
    hpShare?: number;
    auraEffect?: string;
    auraTarget?: AuraSample['target'];
}

const previewUnit = (options: UnitOptions): CombatUnitInfo => ({
    entityId: options.id,
    name: options.name,
    unitType: 'Dwarf',
    element: options.element,
    level: options.level,
    team: options.team,
    position: options.position,
    maxHp: MAX_HP,
    hp: Math.round(MAX_HP * (options.hpShare ?? 1)),
    attack: ATTACK,
    speed: 10,
    maxEnergy: 100,
    energy: 100,
    spells: [],
    immunities: [],
    auraEffect: options.auraEffect ?? null,
    auraTarget: options.auraTarget ?? null,
    power: { total: 0, stats: 0, level: 0, skills: 0, passives: 0, equipment: 0 },
});

type EntryInit = Partial<CombatLogEntry> & Pick<CombatLogEntry, 'eventType' | 'attackerId' | 'targetId'>;

// Пишет логи боя и следит за HP, чтобы у каждой записи был верный остаток здоровья
class SceneBuilder {
    private readonly logs: CombatLogEntry[] = [];
    private readonly hp = new Map<number, number>();
    private readonly teams = new Map<number, number>();
    // `${цель}:${статус}` -> состояние статуса
    private readonly statuses = new Map<string, SceneStatus>();
    private action = 0;
    private round = 1;

    constructor(private readonly units: CombatUnitInfo[]) {
        units.forEach(u => {
            this.hp.set(u.entityId, u.hp);
            this.teams.set(u.entityId, u.team);
        });
    }

    cast(casterId: number, targetId: number, spell: SpellSample, level: number) {
        this.action++;
        this.push({ eventType: 'SkillCast', attackerId: casterId, targetId, skillName: spell.name, skillLevel: level, damageType: spell.damageType, attackerEnergy: 0 });
    }

    damage(attackerId: number, targetId: number, amount: number, damageType: DamageType, init: Partial<CombatLogEntry> = {}) {
        const damage = Math.round(amount);
        this.hp.set(targetId, Math.max(0, (this.hp.get(targetId) ?? 0) - damage));
        this.push({ eventType: 'Damage', attackerId, targetId, damage, damageType, ...init });
    }

    heal(attackerId: number, targetId: number, amount: number, init: Partial<CombatLogEntry> = {}) {
        const heal = Math.round(amount);
        this.hp.set(targetId, Math.min(MAX_HP, (this.hp.get(targetId) ?? 0) + heal));
        this.push({ eventType: 'Heal', attackerId, targetId, heal, ...init });
    }

    // Наложение складывается с уже висящим статусом по тем же правилам, что на сервере (StatusEffectBase.Absorb)
    applyStatus(attackerId: number, targetId: number, name: string, skillName = '', turns = 1, power = STATUS_POWER[name] ?? 0) {
        const key = `${targetId}:${name}`;
        let status = this.statuses.get(key);
        if (!status) {
            status = { layers: [{ power, turns }], turns: name === 'Bleed' ? 1 : turns, power, stacks: 1 };
            this.statuses.set(key, status);
        } else if (LAYERED_STATUSES.has(name)) {
            status.layers.push({ power, turns });
            syncLayers(status);
        } else if (name === 'Bleed') {
            status.power += Math.ceil(power / 2);
            status.turns++;
            status.stacks = status.turns;
        } else if (name === 'Curse') {
            status.power += power;
            status.stacks++;
        } else {
            status.turns = Math.max(status.turns, turns);
        }
        this.push({ eventType: 'StatusApplied', attackerId, targetId, skillName, appliedEffects: [statusData(name, status)] });
    }

    hasStatus(targetId: number, name: string) {
        return this.statuses.has(`${targetId}:${name}`);
    }

    // Ход носителя: урон, лечение, пропуск хода или замедление (мороз — в конце хода); истёкший статус спадает
    tick(targetId: number, name: string, sourceId = targetId) {
        const key = `${targetId}:${name}`;
        const status = this.statuses.get(key);
        if (!status) return;

        const amount = status.power;
        if (LAYERED_STATUSES.has(name)) {
            status.layers.forEach(l => l.turns--);
            status.layers = status.layers.filter(l => l.turns > 0);
            syncLayers(status);
        } else {
            status.turns--;
            if (name === 'Bleed') status.stacks = status.turns;
        }

        const init = { phase: name === 'Frost' ? 'TurnEnd' : 'TurnStart', actionType: name, activeEffects: [statusData(name, status)] };
        if (SKIP_TURN_STATUSES.has(name)) {
            this.push({ eventType: 'Stunned', attackerId: targetId, targetId, ...init });
        } else if (HEAL_STATUSES.has(name)) {
            this.heal(sourceId, targetId, amount, init);
        } else if (STATUS_DAMAGE[name]) {
            this.damage(sourceId, targetId, amount, STATUS_DAMAGE[name], { eventType: 'StatusTick', ...init });
        } else {
            this.push({ eventType: 'StatusTick', attackerId: sourceId, targetId, ...init });
        }

        if (status.turns <= 0) {
            this.statuses.delete(key);
            this.push({ eventType: 'StatusExpired', attackerId: targetId, targetId, phase: 'TurnEnd', activeEffects: [statusData(name, status)] });
        }
    }

    nextRound() {
        this.round++;
        this.push({ eventType: 'RoundStart', attackerId: 0, targetId: 0 });
    }

    result(): CombatResult {
        return { units: this.units, logs: this.logs, winningTeam: 0, rounds: this.round };
    }

    private push(init: EntryInit) {
        this.logs.push({
            sequence: this.logs.length + 1,
            actionId: this.action,
            attackerTeam: this.teams.get(init.attackerId) ?? 0,
            targetTeam: this.teams.get(init.targetId) ?? 0,
            actionType: '',
            round: this.round,
            phase: 'Action',
            skillName: '',
            damageType: 'Physical',
            damage: 0,
            heal: 0,
            targetRemainingHp: this.hp.get(init.targetId) ?? 0,
            isCritical: false,
            appliedEffects: [],
            activeEffects: [],
            ...init,
        });
    }
}

// Заклинание героя на уровне level: заклинатель и его цели. Лечащее заклинание без урона целится в раненых союзников рядом.
// Если заклинание бьёт по нескольким, на стороне целей CROWD юнитов, поражены первые targets из них
export function spellScene(element: HeroElement, sample: SpellSample, level: number): CombatResult {
    const spell = sample.levels[level - 1];
    const healsAlly = !spell.damage;
    const selfHeal = !healsAlly && !!spell.heal;
    const crowd = (spell.targets ?? 1) > 1 ? CROWD : 1;
    const caster = previewUnit({
        id: CASTER,
        name: t(`collection.heroNames.${element}`),
        element,
        level: PREVIEW_RANK,
        team: 1,
        position: healsAlly ? crowd + 1 : 1,
        hpShare: selfHeal ? 0.55 : 1,
    });
    const targets = Array.from({ length: crowd }, (_, i) => previewUnit({
        id: TARGET + i,
        name: healsAlly ? t('collection.ally') : t('collection.target'),
        element: 'Basic',
        level: 1,
        team: healsAlly ? 1 : 2,
        position: i + 1,
        hpShare: healsAlly ? 0.4 : 1,
    }));

    const scene = new SceneBuilder([caster, ...targets]);
    const statuses = spell.statuses ?? [];
    const hit = targets.slice(0, Math.min(spell.targets ?? 1, crowd)).map(u => u.entityId);
    const afflicted = hit.slice(0, spell.statusTargets ?? hit.length);

    const skill = { skillName: sample.name };
    scene.cast(CASTER, hit[0], sample, level);
    for (const id of hit) {
        if (spell.damage) scene.damage(CASTER, id, ATTACK * spell.damage, sample.damageType, skill);
        if (afflicted.includes(id)) statuses.forEach(name => scene.applyStatus(CASTER, id, name, sample.name));
        if (healsAlly && spell.heal) scene.heal(CASTER, id, ATTACK * spell.heal, skill);
    }
    if (selfHeal && spell.heal) scene.heal(CASTER, CASTER, ATTACK * spell.heal, skill);

    // Ход целей: статусы срабатывают и спадают
    if (statuses.length) {
        scene.nextRound();
        afflicted.forEach(id => statuses.forEach(name => scene.tick(id, name, CASTER)));
    }
    return scene.result();
}

// Аура: один носитель, волны пускает сама сцена (PreviewStage, режим auraPulse)
export function auraScene(aura: AuraSample): CombatResult {
    const bearer = previewUnit({
        id: CASTER,
        name: t(`collection.auras.${aura.key}`),
        element: aura.element,
        level: PREVIEW_RANK,
        team: 1,
        position: 1,
        auraEffect: aura.effect,
        auraTarget: aura.target,
    });
    return new SceneBuilder([bearer]).result();
}

// Статус на одном юните: накладывается несколько раз (DEMO_APPLICATIONS), срабатывает каждый ход, пока не спадёт
export function statusScene(name: string): CombatResult {
    const unit = previewUnit({
        id: TARGET,
        name: statusLabel(name),
        element: 'Basic',
        level: 1,
        team: 1,
        position: 1,
        hpShare: HEAL_STATUSES.has(name) ? 0.4 : 1,
    });

    const scene = new SceneBuilder([unit]);
    (DEMO_APPLICATIONS[name] ?? DEMO_LAYERS).forEach(turns => scene.applyStatus(TARGET, TARGET, name, '', turns));
    while (scene.hasStatus(TARGET, name)) {
        scene.nextRound();
        scene.tick(TARGET, name);
    }
    return scene.result();
}
