import { soundService } from '@/services/soundService/SoundService';
import { dialogueService } from '@/services/dialogueService/DialogueService';
import { DamageSource } from '@/services/dialogueService/types';
import { t } from '@/i18n';
import { BattleFx } from './BattleFx';
import { BattleState, BattleUnit, FloatingText, UnitFocus, auraEffectOf, resetBattleState } from './BattleState';
import { DAMAGE_COLORS, STATUS_VISUALS, resolveAuraVisual, resolveSpellVisual } from './FxRegistry';
import { CombatLogEntry, CombatResult, StatusEffectLogData } from './types';
import { getUnitView } from './UnitRegistry';

interface DamageOptions {
  // Визуал попадания уже проигран (взрыв заклинания)
  silent?: boolean;
  // Урон от тика статуса с таким именем
  status?: string;
}

// Записи, которые входят в группу каста (идут после SkillCast с тем же ActionId)
const CAST_GROUP_EVENTS = new Set(['Damage', 'Heal', 'StatusApplied', 'Immune']);

// События срабатывания статуса: по их ActiveEffects обновляются цифры на иконке
const STATUS_TICK_EVENTS = new Set(['StatusTick', 'Heal', 'Stunned']);

// События-действия, после которых делаем паузу, чтобы анимации не сливались
const ACTION_EVENTS = new Set(['Attack', 'Damage', 'StatusTick', 'Heal', 'Stunned']);

// Базовые длительности (мс) при скорости x1
const TIMING = {
  roundBanner: 500,
  attackWindup: 350,
  attackRecover: 450,
  castWindup: 300,
  castRecover: 400,
  tick: 380,
  stunned: 450,
  turnStart: 300,
  death: 500,
  floatingText: 1100,
  actionGap: 400,
  speech: 2400,
  auraWave: 300,
  auraRecover: 450,
};

// Облачко с репликой не короче этого даже на x4 — иначе не успеть прочитать
const MIN_SPEECH_MS = 1500;

// Подсветка юнита, чей сейчас ход
const TURN_COLOR = '#ffe8a3';

let floatingTextId = 0;
let speechId = 0;

// Проигрывает бой, полностью восстанавливая состояние сцены из серверных логов.
export class BattlePlayer {
  private skipping = false;
  private runId = 0;
  // Юнит, чей сейчас ход: подсвечен с первого события хода до конца его действия
  private currentActor: number | null = null;
  speed = 1;

  readonly fx: BattleFx;

  constructor(private readonly state: BattleState, fx: BattleFx) {
    this.fx = fx;
  }

  get isSkipping() {
    return this.skipping;
  }

  // Мгновенно доиграть бой без анимаций
  skip() {
    this.skipping = true;
  }

  // Прервать проигрывание (например, при уходе со страницы)
  stop() {
    this.runId++;
  }

  async play(result: CombatResult) {
    const runId = ++this.runId;
    this.skipping = false;
    this.currentActor = null;
    resetBattleState(this.state, result);
    this.state.playing = true;

    // Дать Vue отрисовать юнитов, чтобы у эффектов были координаты
    await this.delay(300, true);

    const logs = [...result.logs].sort((a, b) => a.sequence - b.sequence);
    let i = 0;

    while (i < logs.length) {
      if (runId !== this.runId) return;
      const log = logs[i];

      await this.beginTurn(log);
      if (runId !== this.runId) return;

      if (log.eventType === 'SkillCast') {
        const group: CombatLogEntry[] = [];
        let j = i + 1;
        while (j < logs.length && logs[j].actionId === log.actionId && CAST_GROUP_EVENTS.has(logs[j].eventType)) {
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
        // Урон вне каста (на всякий случай) — просто попадание
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
    this.float(cast.attackerId, cast.skillName, 'status');
    if (!this.skipping) {
      this.fx.castCircle(getUnitView(cast.attackerId)?.el?.(), color, TIMING.castWindup + 700);
    }
    await this.delay(TIMING.castWindup);

    // Записи группируем по цели: заклинание до неё дошло — применяем всё, что к ней относится
    const byTarget = new Map<number, CombatLogEntry[]>();
    for (const entry of group) {
      const entries = byTarget.get(entry.targetId) ?? [];
      entries.push(entry);
      byTarget.set(entry.targetId, entries);
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

    await this.delay(TIMING.castRecover);
    this.endTurn();
  }

  private applyCastEntries(entries: CombatLogEntry[]) {
    for (const entry of entries) {
      this.show(entry);
      if (entry.eventType === 'Damage') this.applyDamage(entry, { silent: true });
      else if (entry.eventType === 'Heal') this.applyHeal(entry);
      else if (entry.eventType === 'StatusApplied') this.applyStatus(entry);
      else if (entry.eventType === 'Immune') this.applyImmune(entry);
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

  // Статус наложен аурой: источник — её носитель и эффект совпадает с эффектом ауры.
  // Статусы от заклинаний сюда не попадают — они разбираются раньше, в группе каста.
  private isAuraTrigger(log: CombatLogEntry) {
    if (log.eventType !== 'StatusApplied' && log.eventType !== 'Immune') return false;
    const bearer = this.unit(log.attackerId);
    const effect = log.appliedEffects[0]?.name;
    return !!bearer && !!effect && auraEffectOf(bearer) === effect;
  }

  private playVictory(team: number) {
    // Джингл играем и при пропуске боя — это единственный звук в конце
    if (team === 1) soundService.victory();
    else soundService.defeat();

    Object.values(this.state.units)
      .filter(u => u.team === team && u.alive)
      .forEach(u => this.animate(u.id, v => v.victory?.()));
  }

  // ===== Изменение состояния =====

  private applyDamage(log: CombatLogEntry, options: DamageOptions = {}) {
    const target = this.unit(log.targetId);
    if (!target) return;

    const crit = log.isCritical;
    target.hp = log.targetRemainingHp;
    this.float(target.id, crit ? `-${log.damage}!` : `-${log.damage}`, 'damage', log.damageType, crit);

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
    this.float(target.id, `+${log.heal}`, 'heal');
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
    this.setStatus(target, log.appliedEffects[0], log.actionType);
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

  private float(unitId: number, text: string, kind: FloatingText['kind'], damageType?: string, crit = false) {
    const unit = this.unit(unitId);
    if (!unit || this.skipping) return;

    const entry: FloatingText = { id: ++floatingTextId, text, kind, damageType, crit };
    unit.floatingTexts.push(entry);
    setTimeout(() => {
      const index = unit.floatingTexts.findIndex(t => t.id === entry.id);
      if (index >= 0) unit.floatingTexts.splice(index, 1);
    }, TIMING.floatingText);
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
    }, Math.max(MIN_SPEECH_MS, TIMING.speech / this.speed));
  }

  private delay(ms: number, ignoreSpeed = false) {
    if (this.skipping) return Promise.resolve();
    const duration = ignoreSpeed ? ms : ms / this.speed;
    return new Promise<void>(resolve => setTimeout(resolve, duration));
  }
}
