import type { Component } from 'vue';
import IconBurn from './IconBurn.vue';
import IconFrost from './IconFrost.vue';
import IconStun from './IconStun.vue';
import IconBleed from './IconBleed.vue';
import IconHeal from './IconHeal.vue';

// Имя статуса с сервера (StatusEffectBase.EffectName) -> иконка
export const STATUS_ICONS: Record<string, Component> = {
  Burn: IconBurn,
  Frost: IconFrost,
  Stun: IconStun,
  Bleed: IconBleed,
  Regen: IconHeal,
};

export { IconBurn, IconFrost, IconStun, IconBleed, IconHeal };
