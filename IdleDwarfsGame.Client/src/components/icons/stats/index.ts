import type { Component } from 'vue';
import IconPower from './IconPower.vue';
import IconHp from './IconHp.vue';
import IconAttack from './IconAttack.vue';
import IconSpeed from './IconSpeed.vue';
import IconEnergy from './IconEnergy.vue';
import IconStartEnergy from './IconStartEnergy.vue';
import IconEnergyPerAttack from './IconEnergyPerAttack.vue';

// Иконки характеристик. Ключи — как в окне прокачки (heroUpgrade.statNames) и у бонусов предметов (equipment.stats)
export const STAT_ICONS: Record<string, Component> = {
  power: IconPower,
  hp: IconHp,
  maxHp: IconHp,
  attack: IconAttack,
  speed: IconSpeed,
  energy: IconEnergy,
  maxEnergy: IconEnergy,
  startEnergy: IconStartEnergy,
  energyPerAttack: IconEnergyPerAttack,
};

export { IconPower, IconHp, IconAttack, IconSpeed, IconEnergy, IconStartEnergy, IconEnergyPerAttack };
