import { soundService } from '@/services/soundService/SoundService';
import { dialogueService } from '@/services/dialogueService/DialogueService';
import { DamageSource } from '@/services/dialogueService/types';
import { t } from '@/i18n';
import { BASE_BATTLE_SPEED, BattleFx } from './BattleFx';
import { BattleState, BattleUnit, FloatingText, FloatingTextDetails, TURN_COLOR, UnitFocus, addFloatingText, auraEffectOf, resetBattleState } from './BattleState';
import { DAMAGE_COLORS, STATUS_VISUALS, resolveAuraVisual, resolveSpellVisual } from './FxRegistry';
import { CombatLogEntry, CombatResult, StatusEffectLogData } from './types';
import { getUnitView } from './UnitRegistry';
import { ConceptSpells, type ConceptCast } from './ConceptSpells';

// Аура тьмы (EnergyBurnEffect на сервере)
const ENERGY_BURN = 'EnergyBurn';
// Артефакт вернул энергию кастеру (ItemEffectActions.GainEnergy на сервере)
const ENERGY_GAIN = 'EnergyGain';

interface DamageOptions {
  // Визуал попадания уже проигран (взрыв заклинания)
  silent?: boolean;
  // Урон от тика статуса с таким именем
  status?: string;
}

// Записи, которые входят в группу каста (идут после SkillCast с тем же ActionId)
const CAST_GROUP_EVENTS = new Set(['Damage', 'Heal', 'StatusApplied', 'Immune', 'StatusExpired']);

// Запись относится к касту: статус, снятый самим заклинанием (Жатва душ сжигает проклятие), идёт в фазе Action,
// а истечение статусов в конце хода кастера (TurnEnd) — уже отдельное событие
const inCastGroup = (entry: CombatLogEntry, cast: CombatLogEntry) =>
  entry.actionId === cast.actionId
  && CAST_GROUP_EVENTS.has(entry.eventType)
  && (entry.eventType !== 'StatusExpired' || entry.phase === 'Action');

// События срабатывания статуса: по их ActiveEffects обновляются цифры на иконке
const STATUS_TICK_EVENTS = new Set(['StatusTick', 'Heal', 'Stunned']);

// События-действия, после которых делаем паузу, чтобы анимации не сливались
const ACTION_EVENTS = new Set(['Attack', 'Damage', 'StatusTick', 'Heal', 'Stunned']);

// Базовые длительности (мс) при скорости x1
const TIMING = {
  roundBanner: 500,
  attackWindup: 350,
  attackRecover: 250,
  castWindup: 300,
  // После попаданий каста ждём, пока эффект почти доиграет (см. FX_OVERLAP), но в этих пределах
  castRecoverMin: 120,
  castRecoverMax: 500,
  tick: 380,
  // Шаг между тиками нескольких статусов одного юнита: они идут почти разом, а не по очереди
  tickStagger: 150,
  stunned: 450,
  turnStart: 150,
  death: 500,
  floatingText: 1100,
  actionGap: 120,
  speech: 2400,
  auraWave: 300,
  auraRecover: 450,
};

// Следующий ход начинается за столько мс до конца эффекта каста — хвост гаснет уже под него
const FX_OVERLAP = 300;

// Облачко с репликой не короче этого даже на x4 — иначе не успеть прочитать
const MIN_SPEECH_MS = 1500;

// Дольше этого спрайты перед боем не ждём (медленная сеть): бой начнётся, недогруженные листы появятся по ходу
const SPRITE_WAIT_MS = 15000;

let speechId = 0;

const nextFrame = () => new Promise<void>(resolve => requestAnimationFrame(() => resolve()));

// Проигрывает бой, полностью восстанавливая состояние сцены из серверных логов.
export class BattlePlayer {
  private skipping = false;
  private runId = 0;
  // Юнит, чей сейчас ход: подсвечен с первого события хода до конца его действия
  private currentActor: number | null = null;
  speed = 1;
  // Чего дождаться перед первым ходом, когда сцена уже готова (на арене — пока уйдет экран загрузки)
  beforeStart: (() => Promise<void>) | null = null;
  // Перед каждой записью лога: бой ждёт, пока промис не разрешится (паузы обучающего боя, tutorial/battle.ts).
  // При пропуске и мгновенном показе не вызывается
  beforeEntry: ((log: CombatLogEntry) => Promise<void> | void) | null = null;

  readonly fx: BattleFx;
  // Заклинания на концептах (ConceptSpells.BATTLE_CONCEPTS); без концепта — прежний визуал из FxRegistry
  readonly concepts: ConceptSpells;
  // Откуда идут срабатывания артефактов без кастера: последний умерший (заражение, раскол) и последний вылеченный
  private lastDeathId: number | null = null;
  private lastHealedId: number | null = null;

  constructor(private readonly state: BattleState, fx: BattleFx) {
    this.fx = fx;
    this.concepts = new ConceptSpells(fx, () => this.rate);
  }

  get isSkipping() {
    return this.skipping;
  }

  // Мгновенно доиграть бой без анимаций
  skip() {
    this.skipping = true;
    this.concepts.clear();
  }

  // Прервать проигрывание (например, при уходе со страницы)
  stop() {
    this.runId++;
    this.concepts.clear();
  }

  // instant — сразу показать итог без анимаций (восстановление боя после перезагрузки страницы)
  async play(result: CombatResult, options: { instant?: boolean } = {}) {
    const runId = ++this.runId;
    this.skipping = false;
    this.currentActor = null;
    this.concepts.clear();
    this.lastDeathId = null;
    this.lastHealedId = null;
    resetBattleState(this.state, result);
    this.state.playing = true;
    // Сцена под экраном загрузки, пока у юнитов не готовы спрайт-листы:
    // иначе их загрузка и распаковка приходится на первые удары, а юниты появляются уже на глазах
    this.state.preparing = true;

    // Дать Vue отрисовать юнитов, чтобы у эффектов были координаты
    await nextFrame();

    const units = Object.values(this.state.units);
    let readyCount = 0;
    await Promise.race([
      Promise.all(units.map(async u => {
        await getUnitView(u.id)?.ready?.();
        if (runId === this.runId) this.state.prepareProgress = ++readyCount / units.length;
      })),
      new Promise(resolve => setTimeout(resolve, SPRITE_WAIT_MS)),
    ]);
    // Первые кадры с готовыми листами рисуются ещё под экраном загрузки
    await nextFrame();
    await nextFrame();
    this.state.preparing = false;
    if (runId !== this.runId) return;
    await this.beforeStart?.();
    if (runId !== this.runId) return;
    if (options.instant) this.skipping = true;

    const logs = [...result.logs].sort((a, b) => a.sequence - b.sequence);
    let i = 0;

    while (i < logs.length) {
      if (runId !== this.runId) return;
      const log = logs[i];

      if (this.beforeEntry && !this.skipping) {
        await this.beforeEntry(log);
        if (runId !== this.runId) return;
      }

      await this.beginTurn(log);
      if (runId !== this.runId) return;

      if (log.eventType === 'SkillCast') {
        const group: CombatLogEntry[] = [];
        let j = i + 1;
        while (j < logs.length && inCastGroup(logs[j], log)) {
          group.push(logs[j++]);
        }
        await this.playCast(log, group);
        await this.delay(TIMING.actionGap);
        i = j;
        continue;
      }

      // Аура накладывает статус на всех целей подряд — проигрываем это одним всплеском
      if (this.isAuraTrigger(log)) {
        const group = [log];
        let j = i + 1;
        while (j < logs.length && logs[j].attackerId === log.attackerId && this.isAuraTrigger(logs[j])) {
          group.push(logs[j++]);
        }
        await this.playAura(log.attackerId, group);
        i = j;
        continue;
      }

      // Тики статусов юнита в начале его хода (горение, яд, регенерация) — почти разом, одной пачкой
      if (this.isTurnTick(log)) {
        const group = [log];
        let j = i + 1;
        while (j < logs.length && logs[j].targetId === log.targetId && this.isTurnTick(logs[j])) {
          group.push(logs[j++]);
        }
        if (await this.playTicks(group)) await this.delay(TIMING.actionGap);
        i = j;
        continue;
      }

      await this.playEntry(log);
      // StatusTick без урона ничего не показывает — паузу не делаем
      if (ACTION_EVENTS.has(log.eventType) && !(log.eventType === 'StatusTick' && log.damage <= 0)) {
        await this.delay(TIMING.actionGap);
      }
      i++;
    }

    if (runId !== this.runId) return;
    this.state.playing = false;
    this.state.finished = true;
    if (this.state.winningTeam == null) this.state.winningTeam = result.winningTeam;
  }

  // ===== Обработчики событий =====

  private async playEntry(log: CombatLogEntry) {
    this.show(log);
    if (STATUS_TICK_EVENTS.has(log.eventType)) this.syncStatus(log);

    switch (log.eventType) {
      case 'RoundStart':
        this.endTurn();
        this.state.round = log.round;
        await this.delay(TIMING.roundBanner);
        break;

      case 'ArenaBuff':
        // Арена усилила весь урон: баннер раунда показывает новую прибавку, все бойцы вспыхивают
        this.state.damageBonus = log.damageBonusPercent ?? 0;
        this.sound(s => s.cast());
        Object.values(this.state.units)
          .filter(u => u.alive)
          .forEach(u => this.float(u.id, t('arena.float.fury', { bonus: this.state.damageBonus }), 'status'));
        await this.delay(TIMING.roundBanner);
        break;

      case 'Attack':
        await this.playAttack(log);
        break;

      case 'Damage':
        // Урон вне каста — просто попадание; от пассивки (шипы) подписываем её названием
        if (log.phase === 'Passive' && log.skillName) this.float(log.attackerId, t(`passives.names.${log.skillName}`), 'status');
        this.applyDamage(log);
        await this.delay(TIMING.tick);
        break;

      case 'StatusApplied':
        this.applyStatus(log);
        break;

      case 'StatusExpired':
        this.removeStatus(log);
        break;

      case 'StatusTick':
        if (log.damage > 0) {
          this.applyDamage(log, { status: log.activeEffects[0]?.name ?? log.actionType });
          await this.delay(TIMING.tick);
        }
        break;

      case 'Heal':
        // Лечение от пассивки (вампиризм, последний рубеж) подписываем её названием
        if (log.phase === 'Passive' && log.skillName) this.float(log.targetId, t(`passives.names.${log.skillName}`), 'status');
        this.applyHeal(log);
        await this.delay(TIMING.tick);
        break;

      case 'Stunned':
        this.float(log.attackerId, t('arena.float.stunned'), 'status');
        this.sound(s => s.stun());
        await this.delay(TIMING.stunned);
        this.endTurn();
        break;

      case 'Immune':
        this.applyImmune(log);
        break;

      case 'Death':
        this.applyDeath(log);
        await this.delay(TIMING.death);
        break;

      case 'CombatEnd':
        this.state.winningTeam = log.winningTeam ?? null;
        this.playVictory(log.winningTeam ?? 0);
        break;
    }
  }

  private async playAttack(log: CombatLogEntry) {
    const attacker = this.unit(log.attackerId);
    this.setFocus(log.attackerId, { role: 'attacker', color: TURN_COLOR });
    this.setFocus(log.targetId, { role: 'target' });
    this.animate(log.attackerId, v => v.attack?.());
    this.sound(s => s.swing());
    // Удар от пассивки (двойной удар) подписываем, чтобы было понятно, откуда он
    if (log.skillName) this.float(log.attackerId, t(`passives.names.${log.skillName}`), 'status');
    await this.delay(TIMING.attackWindup);

    this.applyDamage(log);
    if (attacker && log.attackerEnergy != null) attacker.energy = log.attackerEnergy;

    await this.delay(TIMING.attackRecover);
    this.endTurn();
  }

  private async playCast(cast: CombatLogEntry, group: CombatLogEntry[]) {
    this.show(cast);
    const caster = this.unit(cast.attackerId);
    if (caster && cast.attackerEnergy != null) caster.energy = cast.attackerEnergy;

    // Лечащий каст — тот, что никого не ранит: у вампиризма и сияющей вспышки лечение идёт вместе с уроном
    const visual = resolveSpellVisual(cast.skillName, cast.damageType, cast.skillLevel);
    const isHealCast = group.some(e => e.eventType === 'Heal') && !group.some(e => e.eventType === 'Damage');
    const color = isHealCast && !visual.support ? DAMAGE_COLORS.Heal : visual.color;

    this.setFocus(cast.attackerId, { role: 'caster', color });
    if (!isHealCast) {
      group.filter(e => e.targetId !== cast.attackerId).forEach(e => this.setFocus(e.targetId, { role: 'target' }));
    }

    this.animate(cast.attackerId, v => v.cast?.(color));
    this.sound(s => s.cast());
    this.float(cast.attackerId, cast.skillName, 'cast', { color });

    // Плата здоровьем (Кровавая цена) — урон кастера по самому себе
    const isSacrifice = (e: CombatLogEntry) => e.eventType === 'Damage' && e.targetId === cast.attackerId;
    // Записи группируем по цели: заклинание до неё дошло — применяем всё, что к ней относится
    const byTarget = new Map<number, CombatLogEntry[]>();
    for (const entry of group.filter(e => !isSacrifice(e))) {
      const entries = byTarget.get(entry.targetId) ?? [];
      entries.push(entry);
      byTarget.set(entry.targetId, entries);
    }

    const concept = this.skipping ? null : this.conceptCast(cast, byTarget);
    if (concept && this.concepts.canPlay(concept)) {
      await this.playConceptCast(cast, group.filter(isSacrifice), byTarget, concept);
      return;
    }

    if (!this.skipping) {
      this.fx.castCircle(getUnitView(cast.attackerId)?.el?.(), color, TIMING.castWindup + 700);
    }
    await this.delay(TIMING.castWindup);

    // Плату показываем сразу, до доставки
    const sacrifice = group.filter(isSacrifice);
    if (sacrifice.length) {
      sacrifice.forEach(entry => {
        this.show(entry);
        this.applyDamage(entry, { silent: true });
      });
      if (!this.skipping) this.fx.play(visual.sacrifice, getUnitView(cast.attackerId)?.el?.());
      await this.delay(visual.sacrificeDelay ?? 0);
    }

    // Визуал заклинания идёт в тех, кого оно ранит, а у лечащих со своим визуалом (Рассвет) — в лечимых союзников.
    // Остальное (вампиризм кастера, подлечивание союзников вспышкой) срабатывает после попаданий
    const isStruck = (entries: CombatLogEntry[]) => (isHealCast ? !!visual.support : !entries.some(e => e.eventType === 'Heal'));
    const struck = [...byTarget.entries()].filter(([, entries]) => isStruck(entries));
    const rest = [...byTarget.entries()].filter(([, entries]) => !isStruck(entries));

    const hits = this.skipping
      ? []
      : this.fx.spell(getUnitView(cast.attackerId)?.el?.(), struck.map(([id]) => getUnitView(id)?.el?.()), visual);
    await Promise.all(struck.map(async ([, entries], i) => {
      await hits[i];
      this.applyCastEntries(entries);
    }));

    if (struck.length && rest.length) await this.delay(visual.healDelay ?? 0);
    rest.forEach(([, entries]) => this.applyCastEntries(entries));

    // Попадание уже было — ждём, пока взрыв на цели почти догорит
    const impactLeft = Math.max(0, ...visual.impact.map(spec => (spec.delay ?? 0) + spec.duration));
    await this.delay(this.castRecover(impactLeft - FX_OVERLAP));
    this.endTurn();
  }

  // Каст для концепта: враги-цели, лечимые, союзники и остальные враги — по записям лога и расстановке
  private conceptCast(cast: CombatLogEntry, byTarget: Map<number, CombatLogEntry[]>): ConceptCast | null {
    const caster = this.unit(cast.attackerId);
    if (!caster) return null;
    const byPosition = (a: number, b: number) => (this.unit(a)?.position ?? 0) - (this.unit(b)?.position ?? 0);
    const ids = [...byTarget.keys()];
    const enemyIds = ids.filter(id => this.unit(id) && this.unit(id)?.team !== caster.team).sort(byPosition);
    const healIds = ids.filter(id => byTarget.get(id)?.some(e => e.eventType === 'Heal')).sort(byPosition);
    const living = Object.values(this.state.units).filter(u => u.alive || ids.includes(u.id));
    const trigger = cast.triggerKind ? { kind: cast.triggerKind, spell: cast.triggerSpell ?? '' } : null;
    // Раскол и заражение идут от умершего носителя, яд «Семени древа» — от вылеченного союзника
    const originId = trigger?.kind === 'SpreadOnDeath' || trigger?.kind === 'Shatter'
      ? this.lastDeathId
      : trigger?.kind === 'OverhealProc' ? this.lastHealedId : null;
    return {
      skillName: cast.skillName,
      level: cast.skillLevel ?? 1,
      damageType: cast.damageType,
      casterId: caster.id,
      enemyIds,
      healIds,
      entryIds: ids,
      procIds: new Set(ids.filter(id => byTarget.get(id)?.some(e => !!e.triggerKind))),
      allyIds: living.filter(u => u.team === caster.team).map(u => u.id).sort(byPosition),
      bystanderIds: living.filter(u => u.team !== caster.team && !enemyIds.includes(u.id)).map(u => u.id).sort(byPosition),
      artifacts: cast.artifactKinds ?? [],
      trigger,
      originId,
    };
  }

  // Заклинание на концепте: записи цели применяются, когда концепт до неё дошёл (попал или вылечил)
  private async playConceptCast(cast: CombatLogEntry, sacrifice: CombatLogEntry[], byTarget: Map<number, CombatLogEntry[]>, concept: ConceptCast) {
    // Плата кровью — сразу: из ран кастера и начинается анимация
    sacrifice.forEach(entry => {
      this.show(entry);
      this.applyDamage(entry, { silent: true });
    });
    const { hits, settled } = this.concepts.play(concept, FX_OVERLAP);
    await Promise.all([...byTarget.entries()].map(async ([id, entries]) => {
      await hits.get(id);
      this.applyCastEntries(entries);
    }));
    // Следующий ход — когда концепту осталось FX_OVERLAP до конца (хвост догорает уже под него),
    // но не раньше castRecoverMin и не позже castRecoverMax после попаданий
    await Promise.race([
      Promise.all([settled, this.delay(TIMING.castRecoverMin)]),
      this.delay(TIMING.castRecoverMax),
    ]);
    this.endTurn();
  }

  private applyCastEntries(entries: CombatLogEntry[]) {
    for (const entry of entries) {
      this.show(entry);
      if (entry.eventType === 'Damage') this.applyDamage(entry, { silent: true });
      else if (entry.eventType === 'Heal') this.applyHeal(entry);
      else if (entry.eventType === 'StatusApplied') this.applyStatus(entry);
      else if (entry.eventType === 'Immune') this.applyImmune(entry);
      else if (entry.eventType === 'StatusExpired') this.removeStatus(entry);
    }
  }

  // Срабатывание ауры: носитель вспыхивает, от него расходится волна, статусы ложатся на цели вслед за ней
  private async playAura(bearerId: number, group: CombatLogEntry[]) {
    const bearer = this.unit(bearerId);
    const visual = resolveAuraVisual(bearer ? auraEffectOf(bearer) : null, bearer?.auraTarget);

    this.animate(bearerId, v => v.aura?.(visual.color));
    this.sound(s => s.cast());
    if (!this.skipping) this.fx.auraBurst(getUnitView(bearerId)?.el?.(), visual);
    await this.delay(TIMING.auraWave);

    for (const entry of group) {
      this.show(entry);
      if (entry.eventType === 'Immune') {
        this.applyImmune(entry);
        continue;
      }
      this.applyStatus(entry);
      if (!this.skipping) this.fx.play(visual.hit, getUnitView(entry.targetId)?.el?.());
    }
    await this.delay(TIMING.auraRecover);
  }

  // Запись тиков начала хода: урон или лечение от статуса и истечение статуса
  private isTurnTick(log: CombatLogEntry) {
    return log.phase === 'TurnStart' && (log.eventType === 'StatusTick' || log.eventType === 'Heal' || log.eventType === 'StatusExpired');
  }

  // Пачка тиков одного юнита: видимые идут с шагом tickStagger, затем одна пауза tick.
  // true — что-то было видно (тики без урона и истечения ничего не показывают)
  private async playTicks(group: CombatLogEntry[]) {
    let shown = 0;
    for (const entry of group) {
      const visible = entry.eventType === 'Heal' || (entry.eventType === 'StatusTick' && entry.damage > 0);
      if (visible && shown++ > 0) await this.delay(TIMING.tickStagger);
      this.show(entry);
      if (STATUS_TICK_EVENTS.has(entry.eventType)) this.syncStatus(entry);
      if (entry.eventType === 'StatusExpired') this.removeStatus(entry);
      else if (entry.eventType === 'Heal') this.applyHeal(entry);
      else if (visible) this.applyDamage(entry, { status: entry.activeEffects[0]?.name ?? entry.actionType });
    }
    if (shown) await this.delay(TIMING.tick);
    return shown > 0;
  }

  // Статус наложен аурой: источник — её носитель и эффект совпадает с эффектом ауры.
  // Статусы от заклинаний сюда не попадают — они разбираются раньше, в группе каста, а от пассивок отмечены фазой.
  private isAuraTrigger(log: CombatLogEntry) {
    if (log.eventType !== 'StatusApplied' && log.eventType !== 'Immune') return false;
    if (log.phase === 'Passive') return false;
    const bearer = this.unit(log.attackerId);
    const effect = log.appliedEffects[0]?.name;
    return !!bearer && !!effect && auraEffectOf(bearer) === effect;
  }

  private playVictory(team: number) {
    // Джингл играем и при пропуске боя — это единственный звук в конце
    if (team === 1) soundService.victory();
    else soundService.defeat();

    // Не через animate: тот при пропуске боя молчит, а победная поза нужна и после пропуска — юниты в ней и остаются
    Object.values(this.state.units)
      .filter(u => u.team === team && u.alive)
      .forEach(u => getUnitView(u.id)?.victory?.());
  }

  // ===== Изменение состояния =====

  private applyDamage(log: CombatLogEntry, options: DamageOptions = {}) {
    const target = this.unit(log.targetId);
    if (!target) return;

    const crit = log.isCritical;
    target.hp = log.targetRemainingHp;
    this.float(target.id, crit ? `-${log.damage}!` : `-${log.damage}`, 'damage', { damageType: log.damageType, crit, amount: log.damage });

    const flash = options.status ? STATUS_VISUALS[options.status]?.tickFlash : undefined;
    this.animate(target.id, v => v.hit?.(log.damageType, { crit, flash }));
    // Звук попадания нужен и для silent: взрыв заклинания беззвучный
    this.sound(s => s.hit(log.damageType, crit));

    const source: DamageSource = options.status ? 'status' : log.eventType === 'Attack' ? 'attack' : 'spell';
    this.maybeSpeak(log, target, source);

    if (this.skipping || options.silent) return;
    const targetEl = getUnitView(target.id)?.el?.();
    if (options.status) {
      this.fx.statusTick(targetEl, options.status, log.damageType);
    } else {
      this.fx.hit(targetEl, log.damageType, { attacker: getUnitView(log.attackerId)?.el?.(), crit });
    }
  }

  private applyHeal(log: CombatLogEntry) {
    const target = this.unit(log.targetId);
    if (!target) return;

    target.hp = log.targetRemainingHp;
    this.lastHealedId = target.id;
    this.float(target.id, `+${log.heal}`, 'heal', { amount: log.heal });
    this.animate(target.id, v => v.heal?.());
    this.sound(s => s.heal());
    if (!this.skipping) this.fx.heal(getUnitView(target.id)?.el?.());
  }

  private applyImmune(log: CombatLogEntry) {
    this.float(log.targetId, t('arena.float.immune'), 'immune');
    this.sound(s => s.block());
  }

  // Сервер держит на юните один статус каждого вида и пишет в лог его итоговое состояние
  // после наложения: его и показываем, ничего не складывая на клиенте
  private applyStatus(log: CombatLogEntry) {
    const target = this.unit(log.targetId);
    if (!target || !target.alive) return;
    const data = log.appliedEffects[0];
    // Выжигание энергии мгновенное и статусом не висит: только полоска энергии и всплывающая цифра
    if (data?.name === ENERGY_BURN) {
      if (log.targetEnergy != null) target.energy = log.targetEnergy;
      if ((data.power ?? 0) > 0) this.float(target.id, t('arena.float.energyBurn', { value: data.power }), 'status');
      return;
    }
    if (data?.name === ENERGY_GAIN) {
      if (log.targetEnergy != null) target.energy = log.targetEnergy;
      if ((data.power ?? 0) > 0) this.float(target.id, t('arena.float.energyGain', { value: data.power }), 'status');
      return;
    }
    this.setStatus(target, data, log.actionType);
  }

  // Тик статуса (урон, лечение, пропуск хода): в ActiveEffects — сколько осталось после него
  private syncStatus(log: CombatLogEntry) {
    const target = this.unit(log.targetId);
    const data = log.activeEffects[0];
    if (!target || !target.alive || !data || !target.statuses[data.name]) return;
    this.setStatus(target, data, data.name);
  }

  private setStatus(target: BattleUnit, data: StatusEffectLogData | undefined, fallbackName: string) {
    target.statuses[data?.name ?? fallbackName] = {
      turns: data?.duration ?? 1,
      power: data?.power ?? 0,
      stacks: data?.stacks ?? 1,
    };
  }

  private removeStatus(log: CombatLogEntry) {
    const target = this.unit(log.targetId);
    if (target) delete target.statuses[log.activeEffects[0]?.name ?? log.actionType];
  }

  private applyDeath(log: CombatLogEntry) {
    const target = this.unit(log.targetId);
    if (!target) return;
    target.hp = 0;
    target.alive = false;
    this.lastDeathId = target.id;
    target.statuses = {};
    target.focus = null;
    this.sound(s => s.death());
  }

  // ===== Утилиты =====

  private show(log: CombatLogEntry) {
    this.state.shownLogs.push(log);
  }

  private unit(id: number): BattleUnit | undefined {
    return this.state.units[id];
  }

  private animate(id: number, action: (view: NonNullable<ReturnType<typeof getUnitView>>) => void) {
    if (this.skipping) return;
    const view = getUnitView(id);
    if (view) action(view);
  }

  private setFocus(unitId: number, focus: UnitFocus) {
    const unit = this.unit(unitId);
    if (!unit || !unit.alive || this.skipping) return;
    unit.focus = focus;
  }

  private clearFocus() {
    Object.values(this.state.units).forEach(u => (u.focus = null));
  }

  // Чей ход открывает запись: действие — атакующего, события фазы TurnStart (тики, стан) — их носителя
  private actorOf(log: CombatLogEntry): number | null {
    if (log.eventType === 'Attack' || log.eventType === 'SkillCast') return log.attackerId;
    if (log.phase === 'TurnStart') return log.targetId;
    return null;
  }

  // Подсветить юнита в начале его хода и дать игроку заметить, кто сейчас ходит
  private async beginTurn(log: CombatLogEntry) {
    const actor = this.actorOf(log);
    if (actor == null || actor === this.currentActor) return;

    this.clearFocus();
    this.currentActor = actor;
    if (!this.unit(actor)?.alive) return;

    this.setFocus(actor, { role: 'attacker', color: TURN_COLOR });
    await this.delay(TIMING.turnStart);
  }

  private endTurn() {
    this.clearFocus();
    this.currentActor = null;
  }

  private sound(play: (sound: typeof soundService) => void) {
    if (!this.skipping) play(soundService);
  }

  private float(unitId: number, text: string, kind: FloatingText['kind'], details?: FloatingTextDetails) {
    const unit = this.unit(unitId);
    if (!unit || this.skipping) return;

    addFloatingText(unit, TIMING.floatingText, text, kind, details);
  }

  // Иногда (см. DialogueService.chance) атакующий или цель что-то говорит
  private maybeSpeak(log: CombatLogEntry, target: BattleUnit, source: DamageSource) {
    if (this.skipping) return;

    const attacker = log.attackerId !== log.targetId ? this.unit(log.attackerId) : undefined;
    const line = dialogueService.tryPick({
      attacker,
      target,
      source,
      damage: log.damage,
      crit: log.isCritical,
      killed: target.hp <= 0,
      log,
    });
    if (line) this.say(line.speakerId, line.text);
  }

  private say(unitId: number, text: string) {
    const unit = this.unit(unitId);
    if (!unit) return;

    const speech = { id: ++speechId, text };
    unit.speech = speech;
    setTimeout(() => {
      if (unit.speech?.id === speech.id) unit.speech = null;
    }, Math.max(MIN_SPEECH_MS, TIMING.speech / this.rate));
  }

  // Пауза после каста, мс при x1: сколько эффекту осталось, в пределах castRecoverMin..castRecoverMax
  private castRecover(left: number) {
    return Math.min(TIMING.castRecoverMax, Math.max(TIMING.castRecoverMin, left));
  }

  // Фактическая скорость: выбранная игроком (speed) с учётом базовой
  private get rate() {
    return this.speed * BASE_BATTLE_SPEED;
  }

  private delay(ms: number) {
    if (this.skipping) return Promise.resolve();
    return new Promise<void>(resolve => setTimeout(resolve, ms / this.rate));
  }
}
