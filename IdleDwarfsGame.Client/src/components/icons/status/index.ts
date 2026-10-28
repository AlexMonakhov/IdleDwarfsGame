import type { Component } from 'vue';
import IconBurn from './IconBurn.vue';
import IconFrost from './IconFrost.vue';
import IconStun from './IconStun.vue';
import IconBleed from './IconBleed.vue';
import IconHeal from './IconHeal.vue';
import IconPoison from './IconPoison.vue';
import IconCurse from './IconCurse.vue';
import IconShock from './IconShock.vue';
import IconFreeze from './IconFreeze.vue';

// Имя статуса с сервера (StatusEffectBase.EffectName) -> иконка
export const STATUS_ICONS: Record<string, Component> = {
  Burn: IconBurn,
  Frost: IconFrost,
  Stun: IconStun,
  Bleed: IconBleed,
  Regen: IconHeal,
  Poison: IconPoison,
  Curse: IconCurse,
  Shock: IconShock,
  Freeze: IconFreeze,
};

export { IconBurn, IconFrost, IconStun, IconBleed, IconHeal, IconPoison, IconCurse, IconShock, IconFreeze };
