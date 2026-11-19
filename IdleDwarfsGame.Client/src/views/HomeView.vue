<script lang="ts" setup>
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import SquadBonusIcon from '@/components/squadBonus/SquadBonusIcon.vue';
import SquadBonusBadge from '@/components/squadBonus/SquadBonusBadge.vue';
import ElementPlayButton from '@/components/landing/ElementPlayButton.vue';
import LandingAtmosphere from '@/components/landing/LandingAtmosphere.vue';
import LandingSquadBuilder from '@/components/landing/LandingSquadBuilder.vue';
import { bonusColor, bonusName } from '@/components/squadBonus/squadBonusText';
import { BONUS_ELEMENTS, PRISM, SquadBonus, monoBonus, pairBonus } from '@/battle/squadBonuses';
import { LANDING_MEDIA, LandingSection, isVideo } from '@/components/landing/landingMedia';
import { useAuth } from '@/composables/UseAuth';
import { AppLocale, LOCALE_NAMES, SUPPORTED_LOCALES, setLocale } from '@/i18n';

// Лендинг: страница, на которую ведут ссылки-приглашения. Два слоя:
//   задний (fixed) — далёкий фон, атмосфера в цвет текущего бонуса стихий (LandingAtmosphere) и клипы игры
//                    со щитами бонусов, которые едут медленнее страницы (параллакс);
//   передний — блоки с описаниями, мини-конструктор отряда и вопросы, прокручиваются как обычно.
// Медиа заднего слоя и их расстановка — в components/landing/landingMedia.ts
const { t, locale } = useI18n();
const router = useRouter();
const { isAuthenticated } = useAuth();

const play = () => router.push(isAuthenticated.value ? { name: 'town' } : { name: 'register' });
const toLogin = () => router.push({ name: 'login' });
const switchLocale = (code: AppLocale) => setLocale(code);

// Блоки описаний: сторона — где стоит панель, медиа в landingMedia.ts расставлены с другой стороны;
// по центру — широкие блоки (конструктор отряда, вопросы)
interface FeatureSection {
    key: Exclude<LandingSection, 'hero' | 'final'>;
    side: 'left' | 'right' | 'center';
}
const FEATURES: FeatureSection[] = [
    { key: 'squad', side: 'left' },
    { key: 'bonus', side: 'right' },
    { key: 'builder', side: 'center' },
    { key: 'battle', side: 'left' },
    { key: 'idle', side: 'right' },
    { key: 'modes', side: 'left' },
    { key: 'loot', side: 'right' },
    { key: 'faq', side: 'center' },
];
const STATS = ['elements', 'bonuses', 'clicks'] as const;
const FAQ = ['free', 'phone', 'progress', 'download'] as const;

const elementShields = BONUS_ELEMENTS.map(element => monoBonus(element, 1));
const showcaseBonuses = [
    monoBonus('Fire', 3),
    pairBonus('Fire', 'Lightning'),
    pairBonus('Fire', 'Nature'),
    pairBonus('Frost', 'Lightning'),
    pairBonus('Lightning', 'Nature'),
    PRISM,
];
const MODES = ['campaign', 'tower', 'delve', 'pvp', 'guild', 'events'] as const;
const playLabel = computed(() => (isAuthenticated.value ? t('landing.playShort') : t('landing.play')));

// ===== Бонус страницы =====
// У каждого блока свой бонус стихий: значок в углу (как на арене) и кнопки «Играть» в его цвете.
// Блок доехал до центра экрана — значок «прокает» и меняется, кнопки перетекают в цвет новой стихии
const SECTION_ORDER: LandingSection[] = ['hero', 'squad', 'bonus', 'builder', 'battle', 'idle', 'modes', 'loot', 'faq', 'final'];
const SECTION_BONUS: Record<LandingSection, SquadBonus> = {
    hero: monoBonus('Fire', 3),
    squad: monoBonus('Frost', 3),
    bonus: pairBonus('Fire', 'Lightning'),
    // В конструкторе — бонус собранного отряда (пока отряд без бонуса — этот)
    builder: monoBonus('Lightning', 1),
    battle: pairBonus('Lightning', 'Dark'),
    idle: monoBonus('Nature', 3),
    modes: pairBonus('Dark', 'Blood'),
    loot: monoBonus('Radiance', 3),
    faq: pairBonus('Frost', 'Radiance'),
    final: PRISM,
};
const activeSection = ref<LandingSection>('hero');
const builderBonus = ref<SquadBonus | null>(null);
const activeBonus = computed(() => (activeSection.value === 'builder' && builderBonus.value
    ? builderBonus.value
    : SECTION_BONUS[activeSection.value]));
// Значок в углу меняется с задержкой: старый сжимается с поворотом, на его месте выпрыгивает новый
// и сразу «прокает» — счётчик срабатываний: новое значение пускает волну и вспышку (SquadBonusFigure)
const SWAP_MS = 220;
const shownBonus = ref<SquadBonus>(activeBonus.value);
const badgeSwapping = ref(false);
const badgePulse = ref(0);
let swapTimer = 0;
watch(activeBonus, next => {
    window.clearTimeout(swapTimer);
    badgeSwapping.value = true;
    swapTimer = window.setTimeout(() => {
        shownBonus.value = next;
        badgeSwapping.value = false;
        badgePulse.value++;
    }, SWAP_MS);
});

// ===== Параллакс =====
const sectionEls = new Map<LandingSection, HTMLElement>();
const setSection = (key: LandingSection) => (el: unknown) => {
    if (el instanceof HTMLElement) sectionEls.set(key, el);
    else sectionEls.delete(key);
};
// Ссылки по индексу: у ref-массива из v-for порядок не гарантирован
const mediaEls = ref<HTMLElement[]>([]);
const setMedia = (i: number) => (el: unknown) => {
    if (el instanceof HTMLElement) mediaEls.value[i] = el;
};
const farEl = ref<HTMLElement | null>(null);
const scrolled = ref(false);
// Узкий экран: кнопка в шапке и значок бонуса поменьше
const compactHeader = ref(window.innerWidth < 760);

// Замеры при изменении размеров: где по прокрутке центр каждого блока и какой высоты элементы медиа
let viewportH = window.innerHeight;
let mobile = false;
const sectionCenter = new Map<LandingSection, number>();
const mediaHeight: number[] = [];
const mediaShown: boolean[] = [];

const measure = () => {
    viewportH = window.innerHeight;
    mobile = window.innerWidth < 760;
    compactHeader.value = mobile;
    sectionEls.forEach((el, key) => {
        const rect = el.getBoundingClientRect();
        sectionCenter.set(key, rect.top + window.scrollY + rect.height / 2 - viewportH / 2);
    });
    mediaEls.value.forEach((el, i) => { mediaHeight[i] = el.offsetHeight; });
};

const FAR_TRAVEL = 400;
const clamp01 = (value: number) => Math.min(1, Math.max(0, value));

const update = () => {
    frame = 0;
    const scroll = window.scrollY;
    scrolled.value = scroll > 40;
    // Далёкий фон за всю страницу сдвигается на FAR_TRAVEL px — он выше окна ровно на столько, низ не открывается
    const maxScroll = Math.max(1, document.documentElement.scrollHeight - viewportH);
    if (farEl.value) farEl.value.style.transform = `translate3d(0, ${-Math.min(1, scroll / maxScroll) * FAR_TRAVEL}px, 0)`;

    // Текущий блок — тот, чей центр ближе всего к центру экрана
    let nearest: LandingSection = activeSection.value;
    let best = Infinity;
    SECTION_ORDER.forEach(key => {
        const center = sectionCenter.get(key);
        if (center === undefined) return;
        const distance = Math.abs(center - scroll);
        if (distance < best) { best = distance; nearest = key; }
    });
    activeSection.value = nearest;

    // На телефоне элементы крупнее (ширины заданы в vw) — иначе скриншоты превращаются в марки
    const scale = mobile ? 1.7 : 1;
    LANDING_MEDIA.forEach((item, i) => {
        const el = mediaEls.value[i];
        if (!el) return;
        const center = sectionCenter.get(item.section) ?? 0;
        const distance = center - scroll;
        const y = viewportH / 2 + item.dy * viewportH + distance * item.speed;
        const half = (mediaHeight[i] ?? 0) * scale / 2;
        // Медленные элементы задерживаются на экране дольше своего блока — гасим их по мере удаления от него
        const fade = clamp01(1.4 - Math.abs(distance) / (viewportH * 0.8));
        const shown = fade > 0 && y + half > -40 && y - half < viewportH + 40;

        if (shown !== mediaShown[i]) {
            mediaShown[i] = shown;
            el.style.visibility = shown ? 'visible' : 'hidden';
            const video = el.querySelector('video');
            if (video) {
                if (shown) video.play().catch(() => { /* автозапуск запрещён — останется первый кадр */ });
                else video.pause();
            }
        }
        if (!shown) return;
        el.style.opacity = String(fade);
        el.style.transform = `translate3d(-50%, ${y}px, 0) translateY(-50%) rotate(${item.rotate ?? 0}deg) scale(${scale})`;
    });
};

let frame = 0;
const requestUpdate = () => {
    if (!frame) frame = requestAnimationFrame(update);
};
const onResize = () => {
    measure();
    requestUpdate();
};

// Блоки описаний проявляются, когда доезжают до экрана
let revealObserver: IntersectionObserver | null = null;
const revealed = ref(new Set<string>());

let resizeObserver: ResizeObserver | null = null;

onMounted(async () => {
    await nextTick();
    measure();
    update();
    window.addEventListener('scroll', requestUpdate, { passive: true });
    window.addEventListener('resize', onResize);
    // Картинки догружаются и меняют высоту элементов и блоков — перемеряем
    resizeObserver = new ResizeObserver(onResize);
    mediaEls.value.forEach(el => resizeObserver?.observe(el));
    sectionEls.forEach(el => resizeObserver?.observe(el));

    revealObserver = new IntersectionObserver(entries => {
        const next = new Set(revealed.value);
        entries.forEach(entry => {
            const key = (entry.target as HTMLElement).dataset.reveal;
            if (entry.isIntersecting && key) next.add(key);
        });
        revealed.value = next;
    }, { threshold: 0.2 });
    document.querySelectorAll<HTMLElement>('[data-reveal]').forEach(el => revealObserver?.observe(el));
});

onBeforeUnmount(() => {
    window.removeEventListener('scroll', requestUpdate);
    window.removeEventListener('resize', onResize);
    if (frame) cancelAnimationFrame(frame);
    window.clearTimeout(swapTimer);
    resizeObserver?.disconnect();
    revealObserver?.disconnect();
});

const scrollToContent = () => {
    sectionEls.get('squad')?.scrollIntoView({ behavior: 'smooth', block: 'center' });
};
</script>

<template>
    <div class="landing">
        <!-- Задний слой: далёкий фон и плывущие скриншоты -->
        <div class="landing-back" aria-hidden="true">
            <div ref="farEl" class="landing-far"></div>
            <LandingAtmosphere :bonus="activeBonus" :compact="compactHeader" />
            <div
                v-for="(item, i) in LANDING_MEDIA"
                :key="i"
                :ref="setMedia(i)"
                class="landing-media"
                :class="{ 'landing-media-shield': item.bonus }"
                :style="{ left: `${item.x}%`, width: item.width, '--depth': Math.min(1, item.speed), '--shield-color': item.bonus ? bonusColor(item.bonus) : undefined }"
            >
                <!-- Щит бонуса: покачивается сам, параллакс двигает обёртку -->
                <span v-if="item.bonus" class="landing-shield" :style="{ animationDelay: `${-i * 0.7}s` }">
                    <SquadBonusIcon :bonus="item.bonus" size="100%" />
                </span>
                <video
                    v-else-if="item.src && isVideo(item.src)"
                    :src="item.src"
                    muted
                    loop
                    playsinline
                    preload="none"
                ></video>
                <img v-else :src="item.src" alt="" loading="lazy" decoding="async" @load="onResize">
            </div>
            <div class="landing-vignette"></div>
        </div>

        <!-- Шапка: появляется подложка, как только страницу начали листать -->
        <header class="landing-header" :class="{ 'landing-header-solid': scrolled }">
            <span class="landing-logo">{{ t('landing.title') }}</span>
            <div class="landing-header-actions">
                <div class="landing-locales" role="group">
                    <button
                        v-for="code in SUPPORTED_LOCALES"
                        :key="code"
                        type="button"
                        class="landing-locale"
                        :class="{ active: locale === code }"
                        :title="LOCALE_NAMES[code]"
                        @click="switchLocale(code)"
                    >{{ code.toUpperCase() }}</button>
                </div>
                <button v-if="!isAuthenticated" type="button" class="landing-link" @click="toLogin">{{ t('nav.login') }}</button>
                <ElementPlayButton :bonus="activeBonus" :size="compactHeader ? 'sm' : 'md'" @click="play">{{ t('landing.playShort') }}</ElementPlayButton>
            </div>
        </header>

        <!-- Значок бонуса стихий, как на арене: меняется с блоком -->
        <!-- На телефоне в конструкторе прячем: он закрыл бы отряд, а бонус и так виден в самом конструкторе -->
        <div class="landing-bonus-corner" :class="{ 'landing-bonus-corner-hidden': compactHeader && activeSection === 'builder' }">
            <div class="landing-badge" :class="{ 'landing-badge-swapping': badgeSwapping }">
                <SquadBonusBadge
                    :bonus="shownBonus"
                    :size="compactHeader ? '40px' : '58px'"
                    :label="!compactHeader"
                    animated
                    :pulse="badgePulse"
                />
            </div>
        </div>

        <!-- Передний слой: описания -->
        <main class="landing-front">
            <section :ref="setSection('hero')" class="landing-section landing-hero">
                <div class="landing-hero-inner">
                    <h1 class="landing-title">{{ t('landing.title') }}</h1>
                    <p class="landing-tagline">{{ t('landing.tagline') }}</p>
                    <div class="landing-cta">
                        <ElementPlayButton :bonus="activeBonus" size="lg" @click="play">{{ playLabel }}</ElementPlayButton>
                        <button v-if="!isAuthenticated" type="button" class="landing-link" @click="toLogin">{{ t('landing.login') }}</button>
                    </div>
                    <ul class="landing-stats">
                        <li v-for="stat in STATS" :key="stat">
                            <span class="landing-stat-value">{{ t(`landing.stats.${stat}.value`) }}</span>
                            <span class="landing-stat-label">{{ t(`landing.stats.${stat}.label`) }}</span>
                        </li>
                    </ul>
                </div>
                <button type="button" class="landing-scroll" :class="{ 'landing-scroll-hidden': scrolled }" @click="scrollToContent">
                    <span>{{ t('landing.scroll') }}</span>
                    <span class="landing-scroll-arrow" aria-hidden="true"></span>
                </button>
            </section>

            <section
                v-for="feature in FEATURES"
                :key="feature.key"
                :ref="setSection(feature.key)"
                class="landing-section"
                :class="`landing-section-${feature.side}`"
            >
                <article
                    class="landing-panel"
                    :class="{ revealed: revealed.has(feature.key), 'landing-panel-wide': feature.side === 'center' }"
                    :data-reveal="feature.key"
                >
                    <h2 class="landing-heading">{{ t(`landing.${feature.key}Title`) }}</h2>
                    <p class="landing-text">{{ t(`landing.${feature.key}Text`) }}</p>

                    <div v-if="feature.key === 'squad'" class="landing-elements">
                        <SquadBonusIcon v-for="bonus in elementShields" :key="bonus.id" :bonus="bonus" :show-tier="false" size="44px" />
                    </div>

                    <ul v-else-if="feature.key === 'bonus'" class="landing-bonuses">
                        <li v-for="bonus in showcaseBonuses" :key="bonus.id">
                            <SquadBonusIcon :bonus="bonus" size="52px" />
                            <span>{{ bonusName(bonus, false) }}</span>
                        </li>
                    </ul>

                    <ul v-else-if="feature.key === 'modes'" class="landing-chips">
                        <li v-for="mode in MODES" :key="mode">{{ t(`landing.modes.${mode}`) }}</li>
                    </ul>

                    <LandingSquadBuilder v-else-if="feature.key === 'builder'" @bonus="builderBonus = $event" />

                    <div v-else-if="feature.key === 'faq'" class="landing-faq">
                        <details v-for="item in FAQ" :key="item" class="landing-faq-item">
                            <summary>{{ t(`landing.faq.${item}.q`) }}</summary>
                            <p>{{ t(`landing.faq.${item}.a`) }}</p>
                        </details>
                    </div>
                </article>
            </section>

            <section :ref="setSection('final')" class="landing-section landing-final">
                <div class="landing-panel landing-panel-final" :class="{ revealed: revealed.has('final') }" data-reveal="final">
                    <h2 class="landing-heading">{{ t('landing.finalTitle') }}</h2>
                    <p class="landing-text">{{ t('landing.finalText') }}</p>
                    <div class="landing-cta">
                        <ElementPlayButton :bonus="activeBonus" size="lg" @click="play">{{ playLabel }}</ElementPlayButton>
                        <button v-if="!isAuthenticated" type="button" class="landing-link" @click="toLogin">{{ t('landing.login') }}</button>
                    </div>
                </div>
            </section>
        </main>
    </div>
</template>

<style scoped>
.landing {
    --landing-bg: #061a1f;
    --landing-panel: rgba(7, 32, 38, 0.82);
    --landing-gold: #ffd77a;
    position: relative;
    min-height: 100vh;
    background: var(--landing-bg);
    color: #eef6f7;
    text-align: left;
    overflow-x: clip;
    /* Обычная страница, а не игра: текст можно выделить и скопировать */
    -webkit-user-select: text;
    user-select: text;
}

/* ===== Задний слой ===== */
.landing-back {
    position: fixed;
    inset: 0;
    overflow: hidden;
    pointer-events: none;
    z-index: 0;
}

.landing-far {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    height: calc(100vh + 400px);
    background: url('@/assets/town/background.webp') center 30% / cover no-repeat;
    opacity: 0.2;
    filter: saturate(0.8);
    will-change: transform;
}

.landing-media {
    position: absolute;
    top: 0;
    visibility: hidden;
    border-radius: 14px;
    padding: 4px;
    background: var(--golden-background);
    box-shadow: 0 18px 40px rgba(0, 0, 0, 0.55);
    /* Дальние (медленные) элементы темнее — так глубина читается без прозрачности, сквозь которую видно соседей */
    filter: brightness(calc(0.55 + 0.45 * var(--depth, 1)));
    will-change: transform, opacity;
}

.landing-media img,
.landing-media video {
    display: block;
    width: 100%;
    height: auto;
    border-radius: 10px;
    background: #0a2a30;
}

.landing-media-shield {
    padding: 0;
    background: none;
    box-shadow: none;
    aspect-ratio: 1;
}

.landing-shield {
    display: block;
    width: 100%;
    height: 100%;
    filter: drop-shadow(0 0 18px color-mix(in srgb, var(--shield-color) 70%, transparent)) drop-shadow(0 12px 18px rgba(0, 0, 0, 0.5));
    animation: landing-shield-float 6s ease-in-out infinite;
}

.landing-shield :deep(svg) {
    display: block;
}

@keyframes landing-shield-float {
    0%, 100% { transform: translateY(0) rotate(0deg); }
    50% { transform: translateY(-12px) rotate(4deg); }
}

.landing-vignette {
    position: absolute;
    inset: 0;
    background: radial-gradient(ellipse 120% 90% at 50% 50%, transparent 55%, rgba(3, 12, 14, 0.75) 100%);
}

/* ===== Шапка ===== */
.landing-header {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    z-index: 10;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    padding: 12px max(16px, 4vw);
    transition: background 0.25s, box-shadow 0.25s;
}

.landing-header-solid {
    background: rgba(5, 22, 26, 0.88);
    box-shadow: 0 6px 20px rgba(0, 0, 0, 0.35);
    backdrop-filter: blur(6px);
}

.landing-logo {
    font-size: 20px;
    font-weight: 800;
    letter-spacing: 1px;
    text-transform: uppercase;
    background: var(--golden-background);
    -webkit-background-clip: text;
    background-clip: text;
    color: transparent;
    white-space: nowrap;
}

.landing-header-actions {
    display: flex;
    align-items: center;
    gap: 14px;
}

.landing-locales {
    display: flex;
    border: 1px solid rgba(255, 255, 255, 0.2);
    border-radius: 8px;
    overflow: hidden;
}

.landing-locale {
    padding: 5px 9px;
    border: 0;
    background: transparent;
    color: rgba(255, 255, 255, 0.65);
    font: inherit;
    font-size: 13px;
    font-weight: 700;
    cursor: pointer;
}

.landing-locale.active {
    background: rgba(24, 192, 192, 0.3);
    color: #fff;
}

.landing-link {
    padding: 0;
    border: 0;
    background: none;
    color: #9fe7ec;
    font: inherit;
    font-size: 15px;
    font-weight: 600;
    text-decoration: underline;
    text-underline-offset: 3px;
    cursor: pointer;
}

.landing-link:hover {
    color: #fff;
}

/* ===== Передний слой ===== */
.landing-front {
    position: relative;
    z-index: 1;
}

.landing-section {
    display: flex;
    align-items: center;
    min-height: 100vh;
    min-height: 100svh;
    padding: 96px max(16px, 6vw);
    box-sizing: border-box;
}

.landing-section-left {
    justify-content: flex-start;
}

.landing-section-right {
    justify-content: flex-end;
}

.landing-section-center {
    justify-content: center;
}

.landing-panel-wide {
    width: min(760px, 100%);
}

.landing-stats {
    display: flex;
    justify-content: center;
    gap: 14px;
    margin: 30px 0 0;
    padding: 0;
    list-style: none;
}

.landing-stats li {
    display: flex;
    flex-direction: column;
    align-items: center;
    min-width: 120px;
    padding: 10px 16px;
    border-radius: 12px;
    background: rgba(4, 20, 24, 0.7);
    border: 1px solid rgba(255, 215, 122, 0.25);
}

.landing-stat-value {
    font-size: 30px;
    font-weight: 900;
    line-height: 1.1;
    background: var(--golden-background);
    -webkit-background-clip: text;
    background-clip: text;
    color: transparent;
}

.landing-stat-label {
    margin-top: 2px;
    font-size: 13px;
    color: #cfe6e9;
}

.landing-faq {
    display: flex;
    flex-direction: column;
    gap: 10px;
    margin-top: 20px;
}

.landing-faq-item {
    border-radius: 12px;
    border: 1px solid rgba(255, 255, 255, 0.1);
    background: rgba(0, 0, 0, 0.25);
}

.landing-faq-item summary {
    padding: 14px 18px;
    font-size: 16px;
    font-weight: 700;
    color: #f1f5f9;
    cursor: pointer;
    list-style: none;
}

.landing-faq-item summary::-webkit-details-marker {
    display: none;
}

.landing-faq-item summary::after {
    content: '+';
    float: right;
    color: var(--landing-gold);
    font-weight: 400;
    transition: transform 0.2s;
}

.landing-faq-item[open] summary::after {
    transform: rotate(45deg);
}

.landing-faq-item p {
    margin: 0;
    padding: 0 18px 16px;
    font-size: 15px;
    line-height: 1.55;
    color: #d6e8ea;
}

.landing-hero {
    position: relative;
    justify-content: center;
    text-align: center;
}

.landing-hero-inner {
    max-width: 760px;
    padding: 36px 28px;
    border-radius: 24px;
    background: radial-gradient(ellipse at center, rgba(4, 20, 24, 0.82) 0%, rgba(4, 20, 24, 0.55) 60%, transparent 100%);
}

.landing-title {
    margin: 0;
    font-size: clamp(48px, 9vw, 112px);
    font-weight: 900;
    line-height: 1;
    letter-spacing: 2px;
    text-transform: uppercase;
    background: var(--golden-background);
    -webkit-background-clip: text;
    background-clip: text;
    color: transparent;
    filter: drop-shadow(0 4px 0 rgba(0, 0, 0, 0.6)) drop-shadow(0 10px 24px rgba(0, 0, 0, 0.6));
}

.landing-tagline {
    margin: 22px auto 0;
    max-width: 600px;
    font-size: clamp(17px, 2.2vw, 22px);
    line-height: 1.5;
    color: #e3f1f2;
    text-shadow: 0 2px 8px rgba(0, 0, 0, 0.8);
}

.landing-cta {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 14px 22px;
    margin-top: 28px;
}

.landing-hero .landing-cta {
    justify-content: center;
}

.landing-bonus-corner {
    position: fixed;
    left: max(16px, 2.5vw);
    bottom: max(16px, 3vh);
    z-index: 9;
    display: flex;
    align-items: center;
    /* Запас под кольцо и искры анимированного значка (выходят за щит на треть размера) */
    padding: 22px 22px 22px 30px;
    border-radius: 16px;
    background: rgba(5, 22, 26, 0.72);
    border: 1px solid rgba(255, 255, 255, 0.1);
    box-shadow: 0 10px 26px rgba(0, 0, 0, 0.4);
    backdrop-filter: blur(6px);
}

.landing-bonus-corner {
    transition: opacity 0.3s, transform 0.3s;
}

.landing-bonus-corner-hidden {
    opacity: 0;
    transform: translateY(20px);
    pointer-events: none;
}

.landing-bonus-corner :deep(.squad-bonus-badge-name) {
    font-size: 15px;
    margin-left: 16px;
}

/* Смена значка: старый сжимается с поворотом, новый выпрыгивает с перелётом — следом бьёт волна импульса */
.landing-badge {
    transition: transform 0.4s cubic-bezier(0.3, 1.6, 0.5, 1), opacity 0.2s ease-out;
}

.landing-badge-swapping {
    transform: scale(0.3) rotate(-120deg);
    opacity: 0;
    transition: transform 0.22s ease-in, opacity 0.22s ease-in;
}

.landing-scroll {
    position: absolute;
    bottom: 26px;
    left: 50%;
    transform: translateX(-50%);
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;
    border: 0;
    background: none;
    color: rgba(255, 255, 255, 0.7);
    font: inherit;
    font-size: 13px;
    letter-spacing: 1px;
    text-transform: uppercase;
    cursor: pointer;
    transition: opacity 0.3s;
}

.landing-scroll-hidden {
    opacity: 0;
    pointer-events: none;
}

.landing-scroll-arrow {
    width: 14px;
    height: 14px;
    border-right: 2px solid currentColor;
    border-bottom: 2px solid currentColor;
    transform: rotate(45deg);
    animation: landing-bounce 1.8s ease-in-out infinite;
}

@keyframes landing-bounce {
    0%, 100% { translate: 0 0; }
    50% { translate: 0 8px; }
}

.landing-panel {
    width: min(540px, 100%);
    padding: 30px 32px;
    box-sizing: border-box;
    border-radius: 18px;
    background: var(--landing-panel);
    border: 1px solid rgba(255, 255, 255, 0.1);
    border-top: 3px solid rgba(255, 215, 122, 0.7);
    box-shadow: 0 24px 50px rgba(0, 0, 0, 0.45);
    backdrop-filter: blur(8px);
    opacity: 0;
    transform: translateY(40px);
    transition: opacity 0.7s ease, transform 0.7s ease;
}

.landing-panel.revealed {
    opacity: 1;
    transform: none;
}

.landing-heading {
    margin: 0 0 14px;
    font-size: clamp(26px, 3.4vw, 38px);
    line-height: 1.15;
    color: var(--landing-gold);
    text-shadow: 0 2px 0 rgba(0, 0, 0, 0.5);
}

.landing-text {
    margin: 0;
    font-size: 17px;
    line-height: 1.6;
    color: #d6e8ea;
}

.landing-elements {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
    margin-top: 22px;
}

.landing-bonuses {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 16px 10px;
    margin: 24px 0 0;
    padding: 0;
    list-style: none;
}

.landing-bonuses li {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 6px;
    font-size: 13px;
    font-weight: 600;
    text-align: center;
    color: #f2f7f7;
}

.landing-chips {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
    margin: 22px 0 0;
    padding: 0;
    list-style: none;
}

.landing-chips li {
    padding: 6px 12px;
    border-radius: 999px;
    border: 1px solid rgba(24, 192, 192, 0.5);
    background: rgba(24, 192, 192, 0.12);
    font-size: 14px;
    font-weight: 600;
}

.landing-final {
    justify-content: center;
    text-align: center;
}

.landing-panel-final {
    width: min(640px, 100%);
}

.landing-panel-final .landing-cta {
    justify-content: center;
}

@media (max-width: 760px) {
    .landing-section-left,
    .landing-section-right {
        justify-content: center;
    }

    .landing-panel {
        padding: 24px 20px;
    }

    .landing-stats {
        gap: 8px;
    }

    .landing-stats li {
        min-width: 0;
        flex: 1;
        padding: 8px 6px;
    }

    .landing-stat-value {
        font-size: 24px;
    }

    .landing-stat-label {
        font-size: 11px;
    }

    .landing-logo {
        font-size: 16px;
    }

    .landing-header-actions {
        gap: 10px;
    }

    .landing-header-actions .landing-link {
        display: none;
    }

    .landing-bonus-corner {
        left: 12px;
        bottom: 12px;
        padding: 16px;
    }
}

@media (prefers-reduced-motion: reduce) {
    .landing-panel {
        transform: none;
        transition: opacity 0.3s;
    }

    .landing-scroll-arrow,
    .landing-shield {
        animation: none;
    }
}
</style>
