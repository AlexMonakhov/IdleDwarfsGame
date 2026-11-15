import type { Component } from 'vue';
import {
    IconBloodCoin,
    IconCoin,
    IconElderSeal,
    IconExperience,
    IconHonorBadge,
    IconNugget,
    IconRainbowCoin,
} from '@/components/icons/ui';
import type { MailCurrency } from '@/models/types/Mail';

// Значок и цвет стекла карточки валюты (CurrencyTile)
export const CURRENCY_ICONS: Record<MailCurrency, Component> = {
    Gold: IconCoin,
    Experience: IconExperience,
    RainbowCoins: IconRainbowCoin,
    BloodCoins: IconBloodCoin,
    HonorBadges: IconHonorBadge,
    ElderSeals: IconElderSeal,
    EventCurrency: IconNugget,
};

export const CURRENCY_COLORS: Record<MailCurrency, string> = {
    Gold: '#fbbf24',
    Experience: '#4ade80',
    RainbowCoins: '#c084fc',
    BloodCoins: '#ef4444',
    HonorBadges: '#60a5fa',
    ElderSeals: '#f5d0a9',
    EventCurrency: '#fb923c',
};

// Порядок валют в каталоге админки и в письме
export const CURRENCY_ORDER: MailCurrency[] = ['Gold', 'Experience', 'RainbowCoins', 'BloodCoins', 'HonorBadges', 'ElderSeals', 'EventCurrency'];
