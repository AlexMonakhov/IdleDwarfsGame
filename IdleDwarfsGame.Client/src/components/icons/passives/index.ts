import type { Component } from 'vue';
import IconAvenger from './IconAvenger.vue';
import IconCleave from './IconCleave.vue';
import IconCriticalStrike from './IconCriticalStrike.vue';
import IconDoubleStrike from './IconDoubleStrike.vue';
import IconElementMastery from './IconElementMastery.vue';
import IconExecutioner from './IconExecutioner.vue';
import IconLastStand from './IconLastStand.vue';
import IconRuneResonance from './IconRuneResonance.vue';
import IconSharpTongue from './IconSharpTongue.vue';
import IconSorcerer from './IconSorcerer.vue';
import IconStoneSkin from './IconStoneSkin.vue';
import IconSurvivalOfTheWeakest from './IconSurvivalOfTheWeakest.vue';
import IconThorns from './IconThorns.vue';
import IconVampirism from './IconVampirism.vue';

// Иконки пассивок по названию HeroPassive (так пассивка приходит с сервера: ability.key, hero.passives[].passive)
export const PASSIVE_ICONS: Record<string, Component> = {
  Avenger: IconAvenger,
  Cleave: IconCleave,
  CriticalStrike: IconCriticalStrike,
  DoubleStrike: IconDoubleStrike,
  ElementMastery: IconElementMastery,
  Executioner: IconExecutioner,
  LastStand: IconLastStand,
  RuneResonance: IconRuneResonance,
  SharpTongue: IconSharpTongue,
  Sorcerer: IconSorcerer,
  StoneSkin: IconStoneSkin,
  SurvivalOfTheWeakest: IconSurvivalOfTheWeakest,
  Thorns: IconThorns,
  Vampirism: IconVampirism,
};
