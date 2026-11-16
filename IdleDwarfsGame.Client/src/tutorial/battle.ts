import type { CombatLogEntry, CombatResult } from '@/battle/types'
import type { TutorialStep } from './types'

// Паузы обучающего боя: первая попытка 1-го уровня кампании (сервер — TutorialBattle.cs, ответ с tutorial: true).
// Бой всегда одинаковый, поэтому каждая пауза гарантированно наступит; что события есть и идут в этом порядке,
// проверяют TutorialBattleTests. Пауза срабатывает один раз — перед первой подходящей записью лога, бой ждёт «Дальше».
// Наставник из облачка и есть дворф в отряде: на этот бой он встал на последнее место

// Тот же id, что у наставника на сервере (TutorialBattle.MentorId)
export const MENTOR_HERO_ID = '7e7e7e7e-0000-4000-8000-000000000001'

interface BattlePause {
  id: string
  text: string
  match: (log: CombatLogEntry, mentor: number) => boolean
  // Кого подсветить: юнита арены (якорь battle.unit.<id>)
  target?: (log: CombatLogEntry, mentor: number) => number
}

const BATTLE_PAUSES: BattlePause[] = [
  { id: 'start', text: 'battle.start', match: (log, mentor) => log.eventType === 'Attack' && log.attackerId === mentor, target: (_, mentor) => mentor },
  { id: 'enemySpell', text: 'battle.enemySpell', match: log => log.eventType === 'SkillCast' && log.attackerTeam === 2, target: log => log.attackerId },
  { id: 'death', text: 'battle.death', match: log => log.eventType === 'Death' && log.targetTeam === 2 },
  { id: 'mentorSpell', text: 'battle.mentorSpell', match: (log, mentor) => log.eventType === 'SkillCast' && log.attackerId === mentor, target: (_, mentor) => mentor },
  { id: 'victory', text: 'battle.victory', match: log => log.eventType === 'CombatEnd' },
]

export const unitAnchor = (unitId: number) => `battle.unit.${unitId}`

// Хук для BattlePlayer.beforeEntry: перед записью, на которой стоит пауза, показывает шаг и ждёт его конца.
// «Пропустить» в облачке снимает все оставшиеся паузы — бой доигрывается сам
export const battlePauses = (
  result: CombatResult,
  pause: (step: TutorialStep) => Promise<'completed' | 'skipped'>,
): ((log: CombatLogEntry) => Promise<void> | void) | null => {
  const mentor = result.units.find(unit => unit.heroId === MENTOR_HERO_ID)?.entityId
  if (mentor === undefined) return null
  const pending = [...BATTLE_PAUSES]
  return async log => {
    const index = pending.findIndex(p => p.match(log, mentor))
    if (index < 0) return
    const [current] = pending.splice(index, 1)
    const target = current.target?.(log, mentor)
    const status = await pause({
      id: current.id,
      text: current.text,
      target: target === undefined ? undefined : unitAnchor(target),
      placement: 'auto',
      advance: 'next',
      block: true,
    })
    if (status === 'skipped') pending.length = 0
  }
}
