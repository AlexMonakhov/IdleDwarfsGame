import type { Component } from 'vue';
import { IconFang, IconNugget } from '@/components/icons/ui';
import type { WorldEventKind } from '@/models/types/Quests';

// Валюта у каждого вида ивента своя: самородки общих шахт, клыки мировых боссов
export const EVENT_CURRENCY_ICONS: Record<WorldEventKind, Component> = {
    CommunityMines: IconNugget,
    WorldBosses: IconFang,
};

// Таймер по часам сервера: сколько осталось до момента, в формате «2д 3:04:05» / «3:04:05»
export const formatCountdown = (ms: number, dayLabel: string): string => {
    const total = Math.max(0, Math.ceil(ms / 1000));
    const days = Math.floor(total / 86400);
    const pad = (n: number) => String(n).padStart(2, '0');
    const time = `${Math.floor((total % 86400) / 3600)}:${pad(Math.floor((total % 3600) / 60))}:${pad(total % 60)}`;
    return days > 0 ? `${days}${dayLabel} ${time}` : time;
};
