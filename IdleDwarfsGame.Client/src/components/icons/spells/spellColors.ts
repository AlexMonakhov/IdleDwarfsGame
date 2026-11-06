import type { HeroElement } from '@/battle/types';

// Цвет правой половины иконки заклинания — цвет его стихии. Чуть светлее камней (lootStyles.GEM_COLORS),
// чтобы читаться на тёмном фоне; у сияния белый: светлый камень сливался бы с бежевой левой половиной
export const SPELL_COLORS: Record<Exclude<HeroElement, 'Basic'>, string> = {
  Fire: '#ef4444',
  Frost: '#60a5fa',
  Lightning: '#facc15',
  Nature: '#4ade80',
  Dark: '#c084fc',
  Radiance: '#ffffff',
  Blood: '#f0346e',
};
