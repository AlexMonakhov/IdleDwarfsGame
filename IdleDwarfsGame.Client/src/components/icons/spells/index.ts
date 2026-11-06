import type { Component } from 'vue';
import IconFireball from './IconFireball.vue';
import IconFlameWave from './IconFlameWave.vue';
import IconMeteor from './IconMeteor.vue';
import IconFrostBolt from './IconFrostBolt.vue';
import IconBlizzard from './IconBlizzard.vue';
import IconGlacialPrison from './IconGlacialPrison.vue';
import IconLightning from './IconLightning.vue';
import IconChainLightning from './IconChainLightning.vue';
import IconThunderclap from './IconThunderclap.vue';
import IconHealingRune from './IconHealingRune.vue';
import IconVenomThorns from './IconVenomThorns.vue';
import IconEntanglingRoots from './IconEntanglingRoots.vue';
import IconShadowBolt from './IconShadowBolt.vue';
import IconHex from './IconHex.vue';
import IconSoulHarvest from './IconSoulHarvest.vue';
import IconPrismRay from './IconPrismRay.vue';
import IconRadiantNova from './IconRadiantNova.vue';
import IconDawnlight from './IconDawnlight.vue';
import IconLifeDrain from './IconLifeDrain.vue';
import IconHemorrhage from './IconHemorrhage.vue';
import IconBloodPrice from './IconBloodPrice.vue';

// Иконки заклинаний по имени семейства, как его присылает сервер (ISpell.Name, SkillName в логе боя)
export const SPELL_ICONS: Record<string, Component> = {
  'Fireball': IconFireball,
  'Flame Wave': IconFlameWave,
  'Meteor': IconMeteor,
  'Frost Bolt': IconFrostBolt,
  'Blizzard': IconBlizzard,
  'Glacial Prison': IconGlacialPrison,
  'Lightning': IconLightning,
  'Chain Lightning': IconChainLightning,
  'Thunderclap': IconThunderclap,
  'Healing Rune': IconHealingRune,
  'Venom Thorns': IconVenomThorns,
  'Entangling Roots': IconEntanglingRoots,
  'Shadow Bolt': IconShadowBolt,
  'Hex': IconHex,
  'Soul Harvest': IconSoulHarvest,
  'Prism Ray': IconPrismRay,
  'Radiant Nova': IconRadiantNova,
  'Dawnlight': IconDawnlight,
  'Life Drain': IconLifeDrain,
  'Hemorrhage': IconHemorrhage,
  'Blood Price': IconBloodPrice,
};

// Список заклинаний героя приходит строками «Имя уровень» (SpellRegistry.Describe): «Flame Wave 2»
export const parseSpell = (described: string): { name: string; level: number | null } => {
  const match = described.match(/^(.*?)\s+(\d+)$/);
  return match ? { name: match[1], level: Number(match[2]) } : { name: described, level: null };
};

// Иконка по имени заклинания или по строке «Имя уровень»; undefined — иконки для него нет
export const spellIcon = (nameOrDescribed: string | null | undefined): Component | undefined =>
  nameOrDescribed ? SPELL_ICONS[nameOrDescribed] ?? SPELL_ICONS[parseSpell(nameOrDescribed).name] : undefined;
