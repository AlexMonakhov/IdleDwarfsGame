<script lang="ts" setup>
import { computed, type Component } from 'vue';
import { useI18n } from 'vue-i18n';
import {
    IconCoin,
    IconExperience,
    IconRainbowCoin,
    IconBloodCoin,
    IconRune,
} from '@/components/icons/ui';
import UnitAvatar from '@/components/interface/UnitAvatar.vue';
import CssAuraEffect from '@/components/effects/CssAuraEffect.vue';
import { AURA_MIN_RANK, elementAura } from '@/components/effects/elementAura';
import CrystalIcon from '@/components/loading/CrystalIcon.vue';
import type { GemShape } from '@/components/loading/crystalGeometry';
import { ELEMENT_COLORS, GEM_COLORS, RARITY_COLORS, getItemImage } from '@/components/inventory/lootStyles';
import type { ChestRewardCard, ChestRewardKind } from './chestRewards';

// Лицевая сторона карты в окне открытия сундука. Герой — портрет как на карточке героя (с 4-го ранга в ауре стихии),
// гем — кристалл как в инвентаре, снаряжение — картинка предмета, доп. награды — значки
// bowl — иконка лежит в вогнутой чаше; без неё висит прямо над фоном окна карты (CardBackdrop в ChestOpening)
const props = withDefaults(defineProps<{ reward: ChestRewardCard; bowl?: boolean }>(), { bowl: false });

const { t, te } = useI18n();

const BONUS_ICONS: Partial<Record<ChestRewardKind, Component>> = {
    Gold: IconCoin,
    Experience: IconExperience,
    RainbowCoins: IconRainbowCoin,
    EventCurrency: IconBloodCoin,
    ItemShards: IconRune,
    HeroShards: IconRune,
};

// Огранка как у GemTile
const GEM_SHAPES: Record<string, GemShape> = {
    Ruby: 'brilliant', Diamond: 'brilliant', Garnet: 'brilliant',
    Sapphire: 'crystal', Emerald: 'crystal', Topaz: 'crystal', Amethyst: 'crystal',
};

const hero = computed(() => {
    const r = props.reward;
    if (r.kind !== 'Hero' || !r.element) return null;
    return { name: heroName.value, unitType: 'Dwarf', element: r.element, level: r.rank ?? 1, isBoss: false };
});
// У настоящего героя своё имя; без него (мок) — название дворфа его стихии
const heroName = computed(() => {
    if (props.reward.title) return props.reward.title;
    const key = `collection.heroNames.${props.reward.element}`;
    return te(key) ? t(key) : '';
});
const heroAura = computed(() => ((props.reward.rank ?? 0) >= AURA_MIN_RANK ? elementAura(props.reward.element) : 'none'));

const title = computed(() => {
    const r = props.reward;
    if (r.kind === 'Hero') return heroName.value;
    if (r.kind === 'Gem' && r.gem) return te(`gems.${r.gem}`) ? t(`gems.${r.gem}`) : r.gem;
    if (r.kind === 'Equipment' && r.itemKey) return te(`equipment.items.${r.itemKey}`) ? t(`equipment.items.${r.itemKey}`) : r.itemKey;
    return r.title;
});

// Вторая половина значка — светлый оттенок цвета награды (по умолчанию у SplitIcon она бирюзовая)
const iconLight = computed(() => `color-mix(in srgb, ${accent.value} 18%, #fffaf0)`);

const accent = computed(() => {
    const r = props.reward;
    if (r.kind === 'Hero' && r.element) return ELEMENT_COLORS[r.element];
    if (r.kind === 'Gem' && r.gem) return GEM_COLORS[r.gem] ?? RARITY_COLORS[r.rarity];
    return r.accent ?? RARITY_COLORS[r.rarity];
});
</script>

<template>
    <div class="reward-face" :style="{ '--reward-accent': accent, '--reward-rarity': RARITY_COLORS[reward.rarity] }">
        <span v-if="reward.bonus" class="reward-face-bonus">{{ t('chests.reveal.bonus') }}</span>

        <!-- Чаша: вогнутое углубление в карте (край на поверхности), на дне — тень иконки -->
        <span class="reward-face-well" :class="{ 'well-flat': !bowl }">
        <span v-if="bowl" class="reward-face-floor-shadow"></span>
        <span class="reward-face-icon">
            <span v-if="hero" class="reward-face-hero" :class="{ 'reward-face-prestige': heroAura !== 'none' }">
                <CssAuraEffect :effect="heroAura" :size="84">
                    <UnitAvatar :unit="hero" :size="84" :show-level="false" />
                </CssAuraEffect>
            </span>
            <CrystalIcon
                v-else-if="reward.kind === 'Gem' && reward.gem"
                :color="GEM_COLORS[reward.gem] ?? '#cbd5e1'"
                :shape="GEM_SHAPES[reward.gem] ?? 'brilliant'"
                size="72px"
            />
            <img v-else-if="reward.kind === 'Equipment' && reward.itemKey" class="reward-face-item" :src="getItemImage(reward.itemKey)" alt="" />
            <component
                :is="BONUS_ICONS[reward.kind]"
                v-else-if="BONUS_ICONS[reward.kind]"
                size="72px"
                :primary-color="accent"
                :secondary-color="iconLight"
            />
        </span>
        </span>

        <span v-if="reward.kind === 'Hero' && reward.rank" class="reward-face-stars">
            <span v-for="star in reward.rank" :key="star">★</span>
        </span>
        <span v-if="reward.amount" class="reward-face-amount">×{{ reward.amount.toLocaleString() }}</span>
        <span class="reward-face-title">{{ title }}</span>
        <span class="reward-face-rarity">{{ t(`rarity.${reward.rarity}`) }}</span>
    </div>
</template>

<style scoped>
.reward-face {
    width: 130px;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 6px;
    text-transform: none;
    letter-spacing: 0;
}

.reward-face-bonus {
    font-size: 10px;
    padding: 2px 8px;
    border-radius: 999px;
    background: rgba(0, 0, 0, 0.45);
    border: 1px solid rgba(255, 255, 255, 0.35);
    text-transform: uppercase;
    letter-spacing: 1px;
}

/* Вогнутая чаша. Свет сверху: верхняя внутренняя стенка в тени, нижняя подсвечена, у нижнего края — светлая кромка.
   Блик на дне смещается против наклона карты (--tilt-x/y из CardComponent) — дно глубже поверхности */
.reward-face-well {
    position: relative;
    width: 112px;
    height: 112px;
    margin: 0 0 2px;
    display: grid;
    place-items: center;
    border-radius: 50%;
    background:
        radial-gradient(
            circle at calc(50% - var(--tilt-x, 0) * 14%) calc(74% - var(--tilt-y, 0) * 14%),
            color-mix(in srgb, var(--reward-accent) 55%, #3a3446) 0%,
            color-mix(in srgb, var(--reward-accent) 22%, #17131e) 42%,
            #060509 78%
        );
    box-shadow:
        inset 0 14px 16px rgba(0, 0, 0, 0.85),
        inset 0 -4px 7px color-mix(in srgb, var(--reward-accent) 35%, rgba(255, 255, 255, 0.25)),
        0 -2px 2px rgba(0, 0, 0, 0.55),
        0 2px 1px rgba(255, 255, 255, 0.28);
    transition: background-position 0.3s;
}

/* Тень иконки на дне чаши: дно глубже иконки, поэтому сдвигается сильнее неё */
/* Без чаши: иконка висит прямо над фоном окна */
.well-flat {
    background: none;
    box-shadow: none;
}

.reward-face-floor-shadow {
    position: absolute;
    left: 50%;
    bottom: 10px;
    width: 66px;
    height: 16px;
    margin-left: -33px;
    border-radius: 50%;
    background: radial-gradient(ellipse, rgba(0, 0, 0, 0.75) 0%, rgba(0, 0, 0, 0.35) 45%, transparent 72%);
    transform: translate(calc(var(--tilt-x, 0) * -14px), calc(var(--tilt-y, 0) * -10px));
    transition: transform 0.3s cubic-bezier(0.23, 1, 0.32, 1);
}

.reward-face-icon {
    position: relative;
    width: 92px;
    height: 92px;
    /* Утоплена в карту: при наклоне сдвигается против него, фон окна — ещё сильнее */
    transform: translate(calc(var(--tilt-x, 0) * -9px), calc(var(--tilt-y, 0) * -9px));
    transition: transform 0.3s cubic-bezier(0.23, 1, 0.32, 1);
    display: grid;
    place-items: center;
    /* Своя тень вниз (свет сверху) + тёмный контур + слабое свечение цвета награды */
    filter:
        drop-shadow(0 1px 0 rgba(0, 0, 0, 0.9))
        drop-shadow(0 7px 5px rgba(0, 0, 0, 0.7))
        drop-shadow(0 0 7px color-mix(in srgb, var(--reward-accent) 55%, transparent));
}

.reward-face-item {
    max-width: 88px;
    max-height: 88px;
}

.reward-face-hero {
    position: relative;
    display: grid;
    place-items: center;
    border-radius: 50%;
}

/* Дворф 4-го ранга: по кромке портрета бежит отблеск цвета стихии (как кромка карточки героя высшего ранга) */
.reward-face-prestige::before {
    content: '';
    position: absolute;
    inset: -5px;
    border-radius: 50%;
    background: conic-gradient(from 0deg, transparent 0deg 250deg, var(--reward-accent) 300deg, #fff 330deg, transparent 360deg);
    -webkit-mask: radial-gradient(circle, transparent 62%, #000 64%);
    mask: radial-gradient(circle, transparent 62%, #000 64%);
    animation: prestige-spin 2.4s linear infinite;
    z-index: 2;
    pointer-events: none;
}

@keyframes prestige-spin {
    to { rotate: 360deg; }
}

.reward-face-stars {
    font-size: 14px;
    line-height: 1;
    color: #ffd166;
    letter-spacing: 1px;
}

.reward-face-amount {
    font-size: 22px;
    line-height: 1;
}

.reward-face-title {
    font-size: 13px;
    line-height: 1.2;
    min-height: 2.4em;
}

.reward-face-rarity {
    font-size: 11px;
    /* Светлый оттенок редкости: читается на любом цвете карты */
    color: color-mix(in srgb, var(--reward-rarity) 30%, #fff);
    text-transform: uppercase;
    letter-spacing: 1px;
    text-shadow: 0 0 6px rgba(0, 0, 0, 0.9), 1px 1px 0 rgba(0, 0, 0, 0.8);
}
</style>
