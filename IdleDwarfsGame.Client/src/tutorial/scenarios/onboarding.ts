import { findAnchor } from '../anchors'
import type { TutorialScenario, TutorialStep } from '../types'

// Стартовое обучение новичка (docs/tutorial-plan.md, «Стартовое обучение — порядок»; сервер — Onboarding.cs):
// город → кампания → обучающий бой → награда → склад, стартовый сундук → герои → алтарь, улучшение → где взять
// золото: экспедиция → шахта → алтарь, улучшение → слияние → новый дворф в отряд. За ним — обзор остальных зданий (tour.ts).
// Обязательное: шаги блокирующие, Esc не закрывает, пропуск — с подтверждением (сервер выдаёт всё сразу).
// Контрольные точки — только шаги «нажми здание в городе»: после перезагрузки до них всегда можно дойти.
// Что игрок уже сделал до перезагрузки, пропускается (skipIf по прогрессу, skipWhen по появившемуся окну)
export const ONBOARDING_ID = 'onboarding'

const step = (id: string, rest: Omit<TutorialStep, 'id' | 'text'>): TutorialStep =>
  ({ id, text: `onboarding.${id}`, wait: true, placement: 'auto', ...rest })

const click = (id: string, target: string, rest: Partial<TutorialStep> = {}) =>
  step(id, { target, advance: 'click-target', ...rest })

const say = (id: string, target: string | undefined, rest: Partial<TutorialStep> = {}) =>
  step(id, { target, advance: 'next', ...rest })

const cleared = (context: { campaignCleared: number }) => context.campaignCleared >= 1
const did = (activity: string) => (context: { done: string[] }) => context.done.includes(activity)

export const onboarding: TutorialScenario = {
  id: ONBOARDING_ID,
  trigger: 'manual',
  mandatory: true,
  steps: [
    // ===== Кампания и обучающий бой =====
    click('toTown', 'nav.town', { skipIf: () => !!findAnchor('town.campaign') }),
    click('campaign', 'town.campaign', { checkpoint: true }),
    click('fight', 'campaign.fight', { skipIf: cleared }),
    // Паузы самого боя — tutorial/battle.ts, поверх этого сценария
    click('result', 'arena.result', { skipIf: cleared }),
    click('toCampaign', 'arena.toCampaign', { skipIf: cleared }),
    // Уже забрана (продолжение после перезагрузки) — кнопки нет, а строка помечена rewardDone
    click('claim', 'campaign.reward.1', { skipWhen: 'campaign.rewardDone.1' }),
    click('closeCampaign', 'campaign.close'),

    // ===== Склад: стартовый сундук и герои =====
    click('warehouse', 'town.warehouse', { checkpoint: true }),
    click('chest', 'warehouse.chest.open', { before: [{ selectTab: { modal: 'warehouse', tab: 'chests' } }], skipIf: did('warehouse') }),
    step('chestOpening', { hidden: true, advance: { event: 'chest:closed' }, skipIf: did('warehouse') }),
    say('heroes', 'warehouse.heroes', { before: [{ selectTab: { modal: 'warehouse', tab: 'heroes' } }] }),
    click('heroCard', 'warehouse.hero.squad'),

    // ===== Алтарь: первое улучшение =====
    click('upgrade', 'altar.upgrade', { skipIf: did('altar') }),
    say('noGold', 'resources'),
    click('closeAltar', 'altar.close'),

    // ===== Экспедиция =====
    click('expeditions', 'town.expedition', { checkpoint: true }),
    click('expeditionCard', 'expeditions.tutorial'),
    click('expeditionHero', 'expeditions.reserve.first', { skipIf: did('expeditions') }),
    click('expeditionStart', 'expeditions.start', { skipIf: did('expeditions') }),
    // Отряд в пути: окно экспедиции подсвечено, но нажать нельзя. Кнопка «Забрать» появляется, когда вернулись
    step('expeditionWait', { target: 'expeditions.progress', advance: { event: 'expedition:claimed' }, passive: true, skipWhen: 'expeditions.claim' }),
    step('expeditionClaim', { target: 'expeditions.claim', advance: { event: 'expedition:claimed' } }),
    click('closeExpeditions', 'expeditions.close'),

    // ===== Шахта =====
    click('mine', 'town.mine', { checkpoint: true }),
    click('minePlot', 'mine.plot.1'),
    click('mineBuild', 'mine.build.confirm', { skipWhen: 'mine.upgrade.Level' }),
    click('mineUpgrade', 'mine.upgrade.Level', { skipWhen: 'mine.candidate.first' }),
    click('mineDwarf', 'mine.candidate.first', { skipWhen: 'mine.dwarf' }),
    // Дворф копает несколько секунд — облачко появляется, когда есть что собрать
    step('mineCollect', { target: 'mine.collect', advance: { event: 'mine:collected' }, delay: 5000 }),
    click('closeMine', 'mine.detail.close'),
    click('closeMining', 'mine.close'),

    // ===== Алтарь: улучшение и слияние =====
    click('altar', 'town.altar', { checkpoint: true }),
    click('altarHero', 'altar.hero.squad', { before: [{ selectTab: { modal: 'altar', tab: 'heroes' } }] }),
    click('upgradeAgain', 'altar.upgrade'),
    step('fusionPick', { target: 'fusion.grid', advance: { event: 'fusion:ready' }, before: [{ selectTab: { modal: 'altar', tab: 'fusion' } }], placement: 'left' }),
    step('fusionFuse', { target: 'fusion.fuse', advance: { event: 'fusion:done' } }),
    say('fusionResult', 'fusion.result'),
    say('fusionMore', 'fusion.result'),
    // Новый дворф — в отряд: вкладка «Отряд», освободить место (если отряд полон), поставить его, сохранить
    click('squadTab', 'altar.tab.squad'),
    // Освободить место: стрелка над последним в отряде; шаг кончается, когда место освободилось
    click('squadRemove', 'squad.slot.remove', { pointer: true, skipWhen: ['squad.free', 'squad.ready'] }),
    // Подсвечены и стрелками помечены оба: новый дворф в запасе и свободный слот — нажать или перетащить
    step('squadAdd', { target: 'squad.place', pointer: true, advance: { event: 'squad:placed' }, skipWhen: 'squad.ready' }),
    step('squadSave', { target: 'squad.save', advance: { event: 'squad:saved' }, skipWhen: 'squad.clean' }),
    click('closeAltarAgain', 'altar.close'),
    // Дальше — обзор остальных зданий, отдельным сценарием (tour.ts): его можно пропустить и пройти заново
  ],
}
