import { computed, type Component } from 'vue';
import { IconChest, IconCrown, IconFlame, IconSkull } from '@/components/icons/ui';
import { IconPower } from '@/components/icons/stats';
import type { DelveNodeInfo, DelveNodeType, DelveRunInfo } from '@/models/types/Social';

// Общая логика карты спуска: иконки и цвета узлов, регион, кто достижим
export interface DelveMapProps {
    run: DelveRunInfo;
    columns: number;
    bossEvery: number;
    canMove: boolean;
    busy: boolean;
}

export const NODE_ICONS: Record<DelveNodeType, Component> = {
    Fight: IconPower,
    Elite: IconSkull,
    Camp: IconFlame,
    Treasure: IconChest,
    Boss: IconCrown,
};

const NODE_COLORS: Record<DelveNodeType, { main: string; light: string; dark: string }> = {
    Fight: { main: '#cbd5e1', light: '#f1f5f9', dark: '#94a3b8' },
    Elite: { main: '#f87171', light: '#fecaca', dark: '#ef4444' },
    Camp: { main: '#fb923c', light: '#fed7aa', dark: '#f97316' },
    Treasure: { main: '#60a5fa', light: '#bfdbfe', dark: '#3b82f6' },
    Boss: { main: '#facc15', light: '#fef08a', dark: '#eab308' },
};

// CSS-переменные узла: цвет оправы и половинки SplitIcon
export const nodeStyle = (type: DelveNodeType) => ({
    '--node-color': NODE_COLORS[type].main,
    '--split-icon-primary': NODE_COLORS[type].light,
    '--split-icon-secondary': NODE_COLORS[type].dark,
});

// Регион меняется каждые bossEvery этажей: арены кампании по кругу
export const REGIONS: { arena: string; color: string }[] = [
    { arena: 'lava', color: 'var(--red-gem-color)' },
    { arena: 'ice', color: 'var(--blue-gem-color)' },
    { arena: 'storm', color: 'var(--yellow-gem-color)' },
    { arena: 'marsh', color: 'var(--green-gem-color)' },
    { arena: 'crypt', color: 'var(--purple-gem-color)' },
    { arena: 'sanctum', color: 'var(--white-gem-color)' },
    { arena: 'altar', color: 'var(--pink-gem-color)' },
];

// Регион этажа: меняется после каждого босса
export const regionOf = (floor: number, bossEvery: number) => REGIONS[Math.floor(floor / bossEvery) % REGIONS.length];

export const useDelveMap = (props: DelveMapProps) => {
    const region = computed(() => regionOf(props.run.floor, props.bossEvery));
    const columnList = computed(() => Array.from({ length: props.columns }, (_, i) => i));
    const nextNode = (column: number) => props.run.next.find(n => n.column === column) ?? null;
    const isBossFloor = (floor: number) => floor > 0 && floor % props.bossEvery === 0;
    const nextBoss = computed(() => (Math.floor(props.run.floor / props.bossEvery) + 1) * props.bossEvery);
    // Пройдено этажей текущего региона: 0..bossEvery-1
    const bossProgress = computed(() => props.run.floor % props.bossEvery);

    // Куда можно шагнуть по геометрии (соседний столбец или вход); с учётом canMove — куда можно прямо сейчас
    const inReach = (node: DelveNodeInfo | null) =>
        !!node && (props.run.floor === 0 || Math.abs(node.column - props.run.column) <= 1);
    const isReachable = (node: DelveNodeInfo | null) => props.canMove && inReach(node);
    return { region, columnList, nextNode, isBossFloor, nextBoss, bossProgress, inReach, isReachable };
};
