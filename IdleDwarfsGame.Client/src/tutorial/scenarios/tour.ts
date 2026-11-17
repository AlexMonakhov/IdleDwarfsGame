import type { TutorialScenario, TutorialStep } from '../types'

// Обзор города: что ещё есть, кроме пройденного в стартовом обучении. Запускается сразу после него (слияние — последнее
// действие), а ещё при входе в игру, если обзор не пройден и не пропущен: после «Сбросить обучение» в аккаунте
// («Начать заново» — сразу). Необязательный: пропуск одной кнопкой. Пропуск стартового обучения пропускает и его
export const TOUR_ID = 'tour'

const say = (id: string, target: string | undefined): TutorialStep =>
  ({ id, target, text: `onboarding.${id}`, wait: true, placement: 'auto', advance: 'next' })

export const tour: TutorialScenario = {
  id: TOUR_ID,
  trigger: 'manual',
  steps: [
    say('tourForge', 'town.blacksmith'),
    say('tourMarket', 'town.market'),
    say('tourTower', 'town.tower'),
    say('tourArena', 'town.arena'),
    say('tourGuild', 'town.guild'),
    say('tourQuests', 'nav.quests'),
    say('tourDelve', 'nav.delve'),
    say('done', undefined),
  ],
}
