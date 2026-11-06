import type { Component } from 'vue';
import { IconAxe, IconPickaxe, IconShield, IconSpyglass, IconSword } from './ui';

// Иконки профессий дворфов (HeroProfession на сервере)
export const PROFESSION_ICONS: Record<string, Component> = {
  Defender: IconShield,
  Warrior: IconSword,
  Axeman: IconAxe,
  Miner: IconPickaxe,
  Scout: IconSpyglass,
};
