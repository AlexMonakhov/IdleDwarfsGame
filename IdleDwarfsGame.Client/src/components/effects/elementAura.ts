import type { FrameEffect } from '@/models/types/Account';
import type { HeroElement } from '@/battle/types';

// Эффекты CssAuraEffect: рамки аватара (FrameEffect) и стихийные ауры героев и боссов.
// Стихийные есть только в CSS-версии — канвас-версия (CanvasAuraEffect) рисует лишь рамки аватара
export type AuraEffect = FrameEffect | 'sparks' | 'leaves' | 'shadow' | 'prism';

// Аура стихии: сапфир — снег, рубин — угли, топаз — искры, изумруд — листья, аметист — тени, бриллиант — радужный перелив
export const ELEMENT_AURAS: Record<HeroElement, AuraEffect> = {
    Basic: 'none',
    Fire: 'embers',
    Frost: 'snow',
    Lightning: 'sparks',
    Nature: 'leaves',
    Dark: 'shadow',
    Radiance: 'prism',
};

export const elementAura = (element?: string | null): AuraEffect =>
    ELEMENT_AURAS[element as HeroElement] ?? 'none';

// С какого ранга карточка героя получает ауру своей стихии
export const AURA_MIN_RANK = 5;
