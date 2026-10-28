import type { BattleUnit } from '@/battle/BattleState';
import type { DialogueRule } from './types';

// ===== Типы существ =====
// Должны совпадать с UnitType героя на сервере (HeroSeedData). Сравнение без учёта регистра.
// Golem и Draconid на сервере пока нет — когда появятся, проверьте, что названия совпадают.
export const UNIT = {
  Dwarf: 'Dwarf',
  Golem: 'Golem',
  Draconid: 'Draconid',
} as const;

// ===== Помощники для правил =====

// Относится ли юнит к одному из типов: isType(ctx.target, UNIT.Golem)
export const isType = (unit: BattleUnit | undefined, ...types: string[]): boolean =>
  !!unit && types.some(type => type.toLowerCase() === unit.unitType.toLowerCase());

// Висит ли на юните статус (имена с сервера: Stun, Burn, Frost, Regen, Bleed...)
export const hasStatus = (unit: BattleUnit | undefined, name: string): boolean =>
  !!unit && !!unit.statuses[name];

// Есть ли на юните хоть какой-то статус
export const hasAnyStatus = (unit: BattleUnit | undefined): boolean =>
  !!unit && Object.keys(unit.statuses).length > 0;

// Здоровье в долях от максимума (0..1) — уже после удара
export const hpRatio = (unit: BattleUnit): number => (unit.maxHp > 0 ? unit.hp / unit.maxHp : 0);

// ===== Правила =====
// Как это работает: при попадании с шансом DialogueService.chance (по умолчанию 1 из 10)
// берутся все правила, которые подходят: тип говорящего входит в speakerTypes и when() вернул true.
// Из них — правила с наибольшим priority, случайное правило и случайная фраза из него.
// Фраза показывается над speaker.
//
// Чтобы добавить правило — допишите объект в массив. Фразы на английском обязательны.
// Существа без своих правил (сейчас орки, гоблины, скелеты) молчат.
// Учтите: оглушённый юнит пропускает ход, поэтому правило «атакующий под станом» никогда не сработает.
//
// Ориентиры по приоритетам:
//   30 добивание · 20 статусы (стан, горение...) · 18 насмешки между типами и реплики големов
//   15 урон от статусов · 12 криты · 10 мало HP · 0 общие фразы

const DWARF = [UNIT.Dwarf];
const GOLEM = [UNIT.Golem];
const DRACONID = [UNIT.Draconid];

export const DIALOGUE_RULES: DialogueRule[] = [
  // ==================== Дворфы ====================

  // --- Добивание ---
  {
    id: 'dwarf-kill-boast',
    speaker: 'attacker',
    speakerTypes: DWARF,
    priority: 30,
    // Для големов и драконидов есть свои фразы (см. насмешки)
    when: ctx => ctx.killed && !isType(ctx.target, UNIT.Golem, UNIT.Draconid),
    phrases: {
      en: ['Next!', 'Back to the mine with you!', 'Rest in rocks, {target}.', 'That one was easy.'],
      ru: ['Следующий!', 'Отправляйся обратно в шахту!', 'Покойся в камнях, {target}.', 'Даже не вспотел.'],
    },
  },
  {
    id: 'dwarf-last-words',
    speaker: 'target',
    speakerTypes: DWARF,
    priority: 30,
    when: ctx => ctx.killed,
    phrases: {
      en: ['Tell my pickaxe... I loved it...', 'Avenge me, brothers!', 'Not... like... this...'],
      ru: ['Передайте моей кирке... я её любил...', 'Отомстите за меня, братья!', 'Не... так... я... хотел...'],
    },
  },

  // --- Бьём оглушённого ---
  {
    id: 'dwarf-hit-stunned-target',
    speaker: 'attacker',
    speakerTypes: DWARF,
    priority: 20,
    when: ctx => hasStatus(ctx.target, 'Stun'),
    phrases: {
      en: ['Wakey wakey, {target}!', 'Sleeping on the job?', 'Hold still, this won\'t take long.'],
      ru: ['Подъём, {target}!', 'Спим на смене?', 'Не дёргайся, это ненадолго.', 'Лежачего не бьют? Не слышал.'],
    },
  },
  {
    id: 'dwarf-hurt-while-stunned',
    speaker: 'target',
    speakerTypes: DWARF,
    priority: 20,
    when: ctx => hasStatus(ctx.target, 'Stun') && ctx.source !== 'status',
    phrases: {
      en: ['Stars... so many stars...', 'Who turned off the lights?', 'Unfair! I can\'t move!'],
      ru: ['Звёздочки... сколько звёздочек...', 'Кто выключил свет?', 'Нечестно, я не могу пошевелиться!'],
    },
  },

  // --- Атакующий под эффектом ---
  {
    id: 'dwarf-hurt-by-burning',
    speaker: 'target',
    speakerTypes: DWARF,
    priority: 20,
    when: ctx => hasStatus(ctx.attacker, 'Burn'),
    phrases: {
      en: ['You\'re on fire, genius!', 'Go put yourself out first!', 'Hey, you\'re smoking!'],
      ru: ['Ты же горишь, умник!', 'Сначала потуши себя!', 'Эй, ты весь дымишься!'],
    },
  },
  {
    id: 'dwarf-hurt-by-frozen',
    speaker: 'target',
    speakerTypes: DWARF,
    priority: 20,
    when: ctx => hasStatus(ctx.attacker, 'Frost'),
    phrases: {
      en: ['Even frozen you hit hard!', 'Chill out, will you?', 'That was a cold one.'],
      ru: ['Даже замёрзший бьёт больно!', 'Остынь, а?', 'Холодный приём, однако.'],
    },
  },
  {
    id: 'dwarf-hit-while-burning',
    speaker: 'attacker',
    speakerTypes: DWARF,
    priority: 15,
    when: ctx => hasStatus(ctx.attacker, 'Burn'),
    phrases: {
      en: ['If I burn, you burn with me!', 'Hot hot hot!'],
      ru: ['Гореть — так вместе!', 'Горячо, горячо, горячо!'],
    },
  },

  // --- Урон от статусов ---
  {
    id: 'dwarf-burn-tick',
    speaker: 'target',
    speakerTypes: DWARF,
    priority: 15,
    when: ctx => ctx.source === 'status' && ctx.log.skillName === 'Burn',
    phrases: {
      en: ['My beard is on fire!', 'Water! Ale! Anything!', 'Hot! Hot!'],
      ru: ['Моя борода горит!', 'Воды! Эля! Хоть чего-нибудь!', 'Жжётся!'],
    },
  },
  {
    id: 'dwarf-frost-hurt',
    speaker: 'target',
    speakerTypes: DWARF,
    priority: 10,
    when: ctx => hasStatus(ctx.target, 'Frost'),
    phrases: {
      en: ['So... cold...', 'My toes are ice!', 'Should\'ve brought a warmer cloak.'],
      ru: ['Как... холодно...', 'Пальцы на ногах в лёд!', 'Надо было брать плащ потеплее.'],
    },
  },

  // --- Критические удары ---
  {
    id: 'dwarf-crit-boast',
    speaker: 'attacker',
    speakerTypes: DWARF,
    priority: 12,
    when: ctx => ctx.crit,
    phrases: {
      en: ['Right in the helmet!', 'Feel the forge!', 'Now THAT is a swing!'],
      ru: ['Прямо по шлему!', 'Почувствуй мощь кузни!', 'Вот это замах!'],
    },
  },
  {
    id: 'dwarf-crit-hurt',
    speaker: 'target',
    speakerTypes: DWARF,
    priority: 12,
    when: ctx => ctx.crit,
    phrases: {
      en: ['Ouch! That one went deep!', 'My armor! It\'s brand new!'],
      ru: ['Ай! Аж до костей!', 'Моя броня! Она же новая!'],
    },
  },

  // --- Мало здоровья ---
  {
    id: 'dwarf-low-hp',
    speaker: 'target',
    speakerTypes: DWARF,
    priority: 10,
    when: ctx => hpRatio(ctx.target) < 0.25,
    phrases: {
      en: ['I need a healer!', 'Just a scratch... a big one.', 'I\'m not done yet!'],
      ru: ['Лекаря мне!', 'Это просто царапина... большая.', 'Я ещё не закончил!'],
    },
  },

  // --- Общие фразы ---
  {
    id: 'dwarf-hit',
    speaker: 'attacker',
    speakerTypes: DWARF,
    when: () => true,
    phrases: {
      en: ['Take that!', 'For the mountain!', 'Have a taste of steel!', 'Ha!'],
      ru: ['Получай!', 'За гору!', 'Отведай стали!', 'Ха!', 'Борода не помешает!'],
    },
  },
  {
    id: 'dwarf-hurt',
    speaker: 'target',
    speakerTypes: DWARF,
    when: ctx => ctx.source !== 'status',
    phrases: {
      en: ['Ow!', 'Is that all you\'ve got?', 'You\'ll pay for that!', 'Watch the beard!'],
      ru: ['Ай!', 'И это всё?', 'Ты за это заплатишь!', 'Только не по бороде!'],
    },
  },

  // ==================== Насмешки между типами ====================

  // --- Дворф против драконида: эль и пиво ---
  {
    id: 'dwarf-taunt-draconid',
    speaker: 'attacker',
    speakerTypes: DWARF,
    priority: 18,
    when: ctx => isType(ctx.target, UNIT.Draconid),
    phrases: {
      en: [
        'Doesn\'t drink ale? Then what are you even living for?',
        'That\'s for every mug of ale you turned down, lizard!',
        'Here, have a sip of steel, since you don\'t take ale!',
        'Scales and no beer. Pitiful.',
      ],
      ru: [
        'Не пьёшь эль? Тогда зачем вообще живёшь?',
        'Это тебе за каждую кружку эля, от которой ты отказался, ящерица!',
        'Раз эль не пьёшь — отхлебни стали!',
        'Чешуя есть, а пива нет. Жалкое зрелище.',
      ],
    },
  },
  {
    id: 'dwarf-hurt-by-draconid',
    speaker: 'target',
    speakerTypes: DWARF,
    priority: 18,
    when: ctx => isType(ctx.attacker, UNIT.Draconid) && ctx.source !== 'status',
    phrases: {
      en: ['Hits like a teetotaler!', 'A pint would fix that swing of yours, lizard.', 'Sober and still missing the point!'],
      ru: ['Бьёт как трезвенник!', 'Кружка пива исправила бы твой замах, ящерица.', 'Трезвый, а всё равно мажешь мимо сути!'],
    },
  },
  {
    id: 'dwarf-kill-draconid',
    speaker: 'attacker',
    speakerTypes: DWARF,
    priority: 30,
    when: ctx => ctx.killed && isType(ctx.target, UNIT.Draconid),
    phrases: {
      en: ['I\'ll drink to that! You won\'t, of course.', 'One less lizard to spoil the party.'],
      ru: ['За это надо выпить! Тебе, конечно, не предлагаю.', 'Минус одна ящерица — праздник не испорчен.'],
    },
  },

  // --- Дворф против голема: «каменный» ---
  {
    id: 'dwarf-taunt-golem',
    speaker: 'attacker',
    speakerTypes: DWARF,
    priority: 18,
    when: ctx => isType(ctx.target, UNIT.Golem),
    phrases: {
      en: [
        'I\'ve mined tougher rocks than you before breakfast!',
        'You\'d make a fine cobblestone road.',
        'Hold still, rubble, I\'m prospecting!',
        'Any gems in there?',
      ],
      ru: [
        'Я и потвёрже породу до завтрака колол!',
        'Из тебя выйдет отличная мостовая.',
        'Стой смирно, булыжник, я тут жилу ищу!',
        'Самоцветы внутри есть?',
      ],
    },
  },
  {
    id: 'dwarf-hurt-by-golem',
    speaker: 'target',
    speakerTypes: DWARF,
    priority: 18,
    when: ctx => isType(ctx.attacker, UNIT.Golem) && ctx.source !== 'status',
    phrases: {
      en: ['Like a cave-in with legs!', 'Who taught the rocks to punch?!', 'I\'ll turn you into gravel!'],
      ru: ['Будто обвал на ножках!', 'Кто научил камни драться?!', 'Я из тебя щебень сделаю!'],
    },
  },
  {
    id: 'dwarf-kill-golem',
    speaker: 'attacker',
    speakerTypes: DWARF,
    priority: 30,
    when: ctx => ctx.killed && isType(ctx.target, UNIT.Golem),
    phrases: {
      en: ['Gravel. Good gravel.', 'Somebody sweep this up.', 'And not a single gem. Shame.'],
      ru: ['Щебень. Хороший щебень.', 'Кто-нибудь, подметите.', 'И ни одного самоцвета. Обидно.'],
    },
  },

  // --- Драконид против дворфа ---
  {
    id: 'draconid-taunt-dwarf',
    speaker: 'attacker',
    speakerTypes: DRACONID,
    priority: 18,
    when: ctx => isType(ctx.target, UNIT.Dwarf),
    phrases: {
      en: ['You reek of ale, short one!', 'Your beer stinks worse than your beard.', 'Go drown in your barrel!'],
      ru: ['От тебя разит элем, коротышка!', 'Твоё пиво воняет хуже твоей бороды.', 'Иди утопись в своей бочке!'],
    },
  },
  {
    id: 'draconid-hurt-by-dwarf',
    speaker: 'target',
    speakerTypes: DRACONID,
    priority: 18,
    when: ctx => isType(ctx.attacker, UNIT.Dwarf) && ctx.source !== 'status',
    phrases: {
      en: ['At least don\'t breathe on me!', 'Drunkard luck!', 'Ugh, that ale breath!'],
      ru: ['Только не дыши на меня!', 'Пьяным везёт!', 'Фу, этот перегар!'],
    },
  },

  // ==================== Големы: только урчат ====================
  {
    id: 'golem-death',
    speaker: 'target',
    speakerTypes: GOLEM,
    priority: 30,
    when: ctx => ctx.killed,
    phrases: {
      en: ['Gruuurk...', 'Urk... grrk...', '...gurk.'],
      ru: ['Гррррк...', 'Урк... гррк...', '...гурк.'],
    },
  },
  {
    id: 'golem-hit',
    speaker: 'attacker',
    speakerTypes: GOLEM,
    priority: 18,
    when: () => true,
    phrases: {
      en: ['URK!', 'Gurk!', 'GRAAK!', 'Urk-urk.'],
      ru: ['УРК!', 'Гурк!', 'ГРААК!', 'Урк-урк.'],
    },
  },
  {
    id: 'golem-hurt',
    speaker: 'target',
    speakerTypes: GOLEM,
    priority: 18,
    when: ctx => ctx.source !== 'status',
    phrases: {
      en: ['Urk?', 'Gurk...', 'Grrk!', 'Hrrm.'],
      ru: ['Урк?', 'Гурк...', 'Грк!', 'Хррм.'],
    },
  },

  // ==================== Дракониды ====================
  {
    id: 'draconid-last-words',
    speaker: 'target',
    speakerTypes: DRACONID,
    priority: 30,
    when: ctx => ctx.killed,
    phrases: {
      en: ['My flame... fades...', 'The sky... calls me...'],
      ru: ['Моё пламя... гаснет...', 'Небо... зовёт меня...'],
    },
  },
  {
    id: 'draconid-burn-tick',
    speaker: 'target',
    speakerTypes: DRACONID,
    priority: 15,
    when: ctx => ctx.source === 'status' && ctx.log.skillName === 'Burn',
    phrases: {
      en: ['Fire? On me? Cute.', 'It tickles.'],
      ru: ['Огонь? На меня? Мило.', 'Щекотно.'],
    },
  },
  {
    id: 'draconid-hit',
    speaker: 'attacker',
    speakerTypes: DRACONID,
    when: () => true,
    phrases: {
      en: ['Burn!', 'Kneel before the scales!', 'Feel the claw!'],
      ru: ['Гори!', 'Склонись перед чешуёй!', 'Отведай когтей!'],
    },
  },
  {
    id: 'draconid-hurt',
    speaker: 'target',
    speakerTypes: DRACONID,
    when: ctx => ctx.source !== 'status',
    phrases: {
      en: ['Hss!', 'You scratched my scales!', 'You will regret that.'],
      ru: ['Ш-ш-ш!', 'Ты поцарапал мою чешую!', 'Ты пожалеешь об этом.'],
    },
  },
];
