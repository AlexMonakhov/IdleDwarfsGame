import type { BattleUnit } from '@/battle/BattleState';
import { AppLocale, currentLocale } from '@/i18n';
import { DIALOGUE_RULES, isType } from './dialogueRules';
import type { DialogueContext, DialogueRule } from './types';

export interface DialogueLine {
  speakerId: number;
  text: string;
  ruleId: string;
}

const randomItem = <T>(items: T[]): T => items[Math.floor(Math.random() * items.length)];

// Выбирает, кто и что скажет при попадании. Сами правила и фразы — в dialogueRules.ts.
export class DialogueService {
  // Шанс, что попадание вообще вызовет реплику
  chance = 0.1;

  // Последняя фраза каждого правила — чтобы одна и та же не звучала дважды подряд
  private readonly lastPhrase = new Map<string, string>();

  constructor(private readonly rules: DialogueRule[]) {}

  // С шансом chance подобрать реплику к попаданию
  tryPick(ctx: DialogueContext, locale: AppLocale = currentLocale()): DialogueLine | null {
    if (Math.random() >= this.chance) return null;
    return this.pick(ctx, locale);
  }

  // Подобрать реплику без броска шанса (удобно для отладки правил)
  pick(ctx: DialogueContext, locale: AppLocale = currentLocale()): DialogueLine | null {
    const matching = this.rules.filter(rule => {
      const speaker = this.speakerOf(rule, ctx);
      return this.canSpeak(speaker)
        && (!rule.speakerTypes || isType(speaker, ...rule.speakerTypes))
        && this.matches(rule, ctx);
    });
    if (!matching.length) return null;

    const topPriority = Math.max(...matching.map(rule => rule.priority ?? 0));
    const rule = randomItem(matching.filter(r => (r.priority ?? 0) === topPriority));
    const speaker = this.speakerOf(rule, ctx);
    if (!speaker) return null;

    const phrase = this.pickPhrase(rule, locale);
    if (!phrase) return null;

    return { speakerId: speaker.id, text: this.format(phrase, ctx), ruleId: rule.id };
  }

  private speakerOf(rule: DialogueRule, ctx: DialogueContext): BattleUnit | undefined {
    return rule.speaker === 'attacker' ? ctx.attacker : ctx.target;
  }

  // Юнит, у которого уже висит облачко, не перебиваем
  private canSpeak(unit: BattleUnit | undefined): unit is BattleUnit {
    return !!unit && unit.speech == null;
  }

  // Ошибка в правиле не должна ломать проигрывание боя
  private matches(rule: DialogueRule, ctx: DialogueContext): boolean {
    try {
      return rule.when(ctx);
    } catch (e) {
      console.error(`Ошибка в правиле диалога "${rule.id}":`, e);
      return false;
    }
  }

  private pickPhrase(rule: DialogueRule, locale: AppLocale): string | undefined {
    const localized = rule.phrases[locale];
    const phrases = localized?.length ? localized : rule.phrases.en;
    if (!phrases.length) return undefined;

    const key = `${rule.id}:${locale}`;
    const last = this.lastPhrase.get(key);
    const candidates = phrases.length > 1 ? phrases.filter(p => p !== last) : phrases;
    const phrase = randomItem(candidates);
    this.lastPhrase.set(key, phrase);
    return phrase;
  }

  private format(phrase: string, ctx: DialogueContext): string {
    return phrase
      .replace(/\{attacker\}/g, ctx.attacker?.name ?? '')
      .replace(/\{target\}/g, ctx.target.name);
  }
}

export const dialogueService = new DialogueService(DIALOGUE_RULES);
