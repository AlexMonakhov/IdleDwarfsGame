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
import IconEnergyBurn from './IconEnergyBurn.vue';
import IconMark from './IconMark.vue';
import IconEnergyGain from './IconEnergyGain.vue';

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
  // Не статус, а мгновенное выжигание энергии аурой тьмы — в логе боя оно тоже StatusApplied
  EnergyBurn: IconEnergyBurn,
  // Метка артефакта «Печать бездны»: заклинания бьют по цели сильнее
  Mark: IconMark,
  // Не статус: артефакт вернул кастеру энергию (StatusApplied «EnergyGain»)
  EnergyGain: IconEnergyGain,
};

export { IconBurn, IconFrost, IconStun, IconBleed, IconHeal, IconPoison, IconCurse, IconShock, IconFreeze, IconEnergyBurn, IconMark, IconEnergyGain };
