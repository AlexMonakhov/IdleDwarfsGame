<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useItemDrag } from '@/composables/UseItemDrag';
import { HeroInfo } from '@/models/types/Heroes';
import { EquipmentInfo, canWear } from '@/models/types/Equipment';
import GlassButton from '@/components/common/GlassButton.vue';
import { ELEMENT_ICONS, RARITY_COLORS, getItemImage } from '@/components/inventory/lootStyles';
import ItemTooltip from './ItemTooltip.vue';
import { itemName } from './equipmentText';

const props = defineProps<{
    item: EquipmentInfo;
    // Выбранный герой: не подходящий ему по стихии предмет приглушён
    hero?: HeroInfo | null;
    ownerName?: string | null;
    // Только значок — для слота на герое
    compact?: boolean;
    draggable?: boolean;
}>();

const { t } = useI18n();
const { start, end } = useItemDrag();

const locked = computed(() => !!props.hero && !canWear(props.item, props.hero.element));

// Кто может надеть. Пустой список — любой герой, значки не нужны
const elementIcons = computed(() => props.item.elements.flatMap(element => {
    const icon = ELEMENT_ICONS[element];
    return icon ? [{ element, icon }] : [];
}));

const onDragStart = (event: DragEvent) => {
    start(event, props.item, !!props.compact);
};
</script>

<template>
    <ItemTooltip
        class="item-tile-trigger"
        :class="{ 'item-tile-trigger-compact': compact }"
        :item="item"
        :hero="hero"
        :owner-name="ownerName"
    >
        <div
            class="item-tile"
            :class="[`item-tile-${item.rarity}`, {
                'item-tile-compact': compact,
                'item-tile-locked': locked,
                'item-tile-worn': !compact && item.heroId,
            }]"
            :style="{ '--rarity-color': RARITY_COLORS[item.rarity] }"
            :draggable="draggable"
            @dragstart="onDragStart"
            @dragend="end"
        >
            <!-- Кристалл как у карточки дворфа, только почти прозрачный и окрашен в цвет редкости -->
            <GlassButton tag="div" class="item-tile-glass">
                <!-- draggable=false: иначе браузер потащит картинку, а не плитку -->
                <img class="item-tile-image" :src="getItemImage(item.key)" alt="" draggable="false" />
                <template v-if="!compact">
                    <span class="item-tile-name">{{ itemName(item) }}</span>
                    <span class="item-tile-rarity">{{ t(`rarity.${item.rarity}`) }}</span>
                    <span v-if="ownerName" class="item-tile-owner">{{ ownerName }}</span>
                </template>
            </GlassButton>
            <span v-if="!compact && elementIcons.length" class="item-tile-elements">
                <img
                    v-for="{ element, icon } in elementIcons"
                    :key="element"
                    :src="icon"
                    :alt="t(`elements.${element}`)"
                    draggable="false"
                />
            </span>
        </div>
    </ItemTooltip>
</template>

<style scoped>
/* Обёртка подсказки не должна мешать: плитка тянется на всю высоту ряда, а в слоте — на весь слот */
.dwarf-tooltip-wrapper.item-tile-trigger {
    display: flex;
    cursor: inherit;
}

.item-tile-trigger-compact {
    width: 100%;
    height: 100%;
}

/* Кто может надеть — значки стихий в левом верхнем углу стекла */
.item-tile-elements {
    position: absolute;
    top: 12px;
    left: 12px;
    z-index: 3;
    display: flex;
    flex-direction: column;
    row-gap: 2px;
    pointer-events: none;
}

.item-tile-elements img {
    display: block;
    width: 18px;
    height: 18px;
    filter: drop-shadow(0 1px 1px rgba(0, 0, 0, 0.8));
}

/* Оправа и стекло как у карточки дворфа. Стекло почти прозрачное, сквозь него видно фон окна */
.item-tile {
    /* Насколько плотное стекло */
    --glass-alpha: 18%;
    --glass-top: color-mix(in srgb, var(--rarity-color) var(--glass-alpha), transparent);
    --glass-mid: color-mix(in srgb, var(--rarity-color) calc(var(--glass-alpha) * 0.7), transparent);
    --glass-bottom: color-mix(in srgb, var(--rarity-color) calc(var(--glass-alpha) * 0.4), rgba(10, 14, 24, 0.5));
    position: relative;
    width: 124px;
    box-sizing: border-box;
    display: flex;
    flex-direction: column;
    padding: 8px;
    border-radius: 14px;
    box-shadow:
        0 4px 3px 2px #181717,
        0 3px 12px 2px rgba(0, 0, 0, 0.6);
    font-family: 'Arial', sans-serif;
    text-align: center;
    /* Стекло бывает любого цвета: читаемость держит обводка */
    text-shadow: var(--text-outline);
    user-select: none;
    transition: transform 0.2s cubic-bezier(0.34, 1.4, 0.64, 1), box-shadow 0.3s ease;
}

/* Металл только кольцом по краю: под стеклом его быть не должно */
.item-tile::before {
    content: '';
    position: absolute;
    inset: 0;
    padding: 8px;
    border-radius: inherit;
    background: var(--metalic-background);
    box-shadow: inset 0 1px 2px rgba(255, 255, 255, 0.8);
    -webkit-mask: linear-gradient(#000 0 0) content-box, linear-gradient(#000 0 0);
    -webkit-mask-composite: xor;
    mask: linear-gradient(#000 0 0) content-box, linear-gradient(#000 0 0);
    mask-composite: exclude;
    pointer-events: none;
}

.item-tile:hover {
    transform: translateY(-3px);
    box-shadow:
        0 4px 3px 2px #181717,
        0 3px 18px 4px color-mix(in srgb, var(--rarity-color) 55%, transparent);
}

/* Два класса — чтобы перебить размеры, скругление и оправу самого GlassButton */
.item-tile .item-tile-glass {
    --glass-glow: color-mix(in srgb, var(--rarity-color) 35%, transparent);
    flex: 1;
    width: 100%;
    min-width: 0;
    min-height: 0;
    justify-content: flex-start;
    row-gap: 2px;
    padding: 8px 4px;
    border-radius: 8px;
    box-shadow:
        0 0 0 2px var(--rarity-color),
        0 0 0 3px #333,
        inset 0 2px 1px rgba(255, 255, 255, 0.35),
        inset 0 -4px 8px rgba(0, 0, 0, 0.25),
        inset 0 0 14px var(--glass-glow);
}

.item-tile:hover .item-tile-glass {
    --glass-glow: color-mix(in srgb, var(--rarity-color) 70%, transparent);
}

.item-tile:hover .item-tile-glass::after {
    transform: translateX(120%);
}

/* Блик по форме плитки и слабее: прозрачное стекло он бы засветил */
.item-tile .item-tile-glass::before {
    border-radius: 6px 6px 50% 50% / 6px 6px 30% 30%;
    opacity: 0.35;
}

/* Содержимое над бликом; пробегающий отблеск — поверх всего */
.item-tile-glass > * {
    position: relative;
    z-index: 1;
}

.item-tile .item-tile-glass::after {
    z-index: 2;
}

/* Артефакты — стекло плотнее и краснее остальных */
.item-tile-Artifact {
    --glass-alpha: 55%;
    box-shadow:
        0 4px 3px 2px #181717,
        0 0 12px 2px rgba(239, 68, 68, 0.6);
}

.item-tile-Artifact .item-tile-rarity {
    color: #fecaca;
}

.item-tile-Legendary {
    box-shadow:
        0 4px 3px 2px #181717,
        0 0 10px 2px color-mix(in srgb, var(--rarity-color) 45%, transparent);
}

/* Не подходит выбранному герою по стихии */
.item-tile-locked {
    opacity: 0.45;
    filter: grayscale(0.6);
}

.item-tile-worn::after {
    content: '✔';
    position: absolute;
    top: 12px;
    right: 13px;
    z-index: 3;
    color: #86efac;
    font-size: 12px;
}

.item-tile-image {
    width: 64px;
    height: 64px;
    object-fit: contain;
    filter: drop-shadow(0 0 5px var(--rarity-color));
}

.item-tile-name {
    color: #f8fafc;
    font-size: 12px;
    font-weight: 700;
    line-height: 1.15;
}

.item-tile-rarity {
    color: var(--rarity-color);
    font-size: 11px;
}

.item-tile-owner {
    max-width: 100%;
    overflow: hidden;
    color: #a5f3fc;
    font-size: 11px;
    text-overflow: ellipsis;
    white-space: nowrap;
}

/* В слоте своя рамка: остаётся только стекло с картинкой на весь слот */
.item-tile-compact,
.item-tile-compact:hover {
    width: 100%;
    height: 100%;
    padding: 0;
    box-shadow: none;
}

.item-tile-compact::before {
    display: none;
}

.item-tile-compact .item-tile-glass {
    justify-content: center;
    padding: 0;
    box-shadow:
        inset 0 0 0 2px var(--rarity-color),
        inset 0 0 14px var(--glass-glow);
}

.item-tile-compact .item-tile-image {
    width: 44px;
    height: 44px;
}
</style>
