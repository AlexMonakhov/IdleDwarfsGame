<script lang="ts" setup>
import { onBeforeUnmount, onMounted, ref, watch } from 'vue';
import CharacterSkeleton from '@/components/character-skeleton.vue';
import { HeroModel } from '@/models/hero';
import { HeroInfo } from '@/models/types/Heroes';
import { elementColor, SpriteColor } from '@/battle/sprites';

// Декоративная сцена задания: никакого расчёта боя, просто показываем, что герои заняты делом.
// Цикл: справа выходят враги → обмен ударами → враги исчезают, герои празднуют → снова враги.
const props = defineProps<{
    heroes: HeroInfo[];
    background: string;
    // Задание выполнено: врагов больше нет, герои празднуют
    finished?: boolean;
}>();

type Skeleton = InstanceType<typeof CharacterSkeleton>;

interface SceneEnemy {
    id: number;
    model: HeroModel;
}

const ENEMY_COLORS: SpriteColor[] = ['red', 'blue', 'basic'];
const MAX_ENEMIES = 4;

// Спрайт 409.6px, персонаж занимает его центр
const FRAME = 409.6;
const SCALE = 0.55;

const heroModels = ref<HeroModel[]>([]);
const enemies = ref<SceneEnemy[]>([]);

const heroRefs: (Skeleton | null)[] = [];
const enemyRefs = new Map<number, Skeleton>();

// Короткие CSS-эффекты по юнитам: рывок атакующего и тряска цели
const heroFx = ref<Record<number, string>>({});
const enemyFx = ref<Record<number, string>>({});

watch(() => props.heroes, heroes => {
    heroModels.value = heroes.map((hero, index) => ({
        id: index,
        name: hero.name,
        currentHp: hero.maxHp,
        maxHp: hero.maxHp,
        lvl: hero.level,
        color: elementColor(hero.element),
        unitType: hero.unitType,
    }));
}, { immediate: true });

const random = (min: number, max: number) => min + Math.floor(Math.random() * (max - min + 1));
const pick = <T,>(list: T[]): T => list[Math.floor(Math.random() * list.length)];

/* ===== Таймеры ===== */
let alive = true;
const timers = new Set<number>();

const sleep = (ms: number) => new Promise<void>(resolve => {
    const id = window.setTimeout(() => {
        timers.delete(id);
        resolve();
    }, ms);
    timers.add(id);
});

const flashFx = (fx: typeof heroFx, index: number, cls: string, duration: number) => {
    fx.value = { ...fx.value, [index]: cls };
    sleep(duration).then(() => {
        if (fx.value[index] === cls) {
            const next = { ...fx.value };
            delete next[index];
            fx.value = next;
        }
    });
};

/* ===== Цикл сцены ===== */
let nextEnemyId = 1;

const spawnEnemies = () => {
    enemies.value = Array.from({ length: random(1, MAX_ENEMIES) }, () => {
        const id = nextEnemyId++;
        return {
            id,
            model: { id, name: '', currentHp: 1, maxHp: 1, lvl: random(1, 4), color: pick(ENEMY_COLORS), unitType: 'dwarf' },
        };
    });
};

// Один удар: случайный юнит одной стороны бьёт случайного юнита другой
const exchange = () => {
    if (!heroModels.value.length || !enemies.value.length) return;

    const heroIndex = random(0, heroModels.value.length - 1);
    const enemy = pick(enemies.value);
    const heroAttacks = Math.random() < 0.6;

    if (heroAttacks) {
        heroRefs[heroIndex]?.playAnimation('attack');
        flashFx(heroFx, heroIndex, 'scene-lunge', 350);
        sleep(250).then(() => {
            enemyRefs.get(enemy.id)?.flash('#ff1414', 0.8, 400);
            flashFx(enemyFx, enemy.id, 'scene-hit', 450);
        });
    } else {
        enemyRefs.get(enemy.id)?.playAnimation('attack');
        flashFx(enemyFx, enemy.id, 'scene-lunge', 350);
        sleep(250).then(() => {
            heroRefs[heroIndex]?.flash('#ff1414', 0.8, 400);
            flashFx(heroFx, heroIndex, 'scene-hit', 450);
        });
    }
};

const celebrate = (on: boolean) => {
    heroRefs.forEach(skeleton => skeleton?.playAnimation(on ? 'victory' : 'idle'));
};

const run = async () => {
    // Даём спрайтам героев загрузиться
    await sleep(400);
    while (alive && !props.finished) {
        spawnEnemies();
        await sleep(900);

        const blows = random(4, 7);
        for (let i = 0; i < blows && alive && !props.finished; i++) {
            exchange();
            await sleep(random(550, 850));
        }
        if (!alive) return;

        enemies.value = [];
        celebrate(true);
        await sleep(2600);
        if (!alive || props.finished) break;
        celebrate(false);
        await sleep(700);
    }

    if (alive) {
        enemies.value = [];
        celebrate(true);
    }
};

onMounted(run);

// Задание закончилось, пока сцена открыта — цикл сам выйдет на ближайшей проверке
watch(() => props.finished, finished => {
    if (finished) enemies.value = [];
});

onBeforeUnmount(() => {
    alive = false;
    timers.forEach(id => window.clearTimeout(id));
    timers.clear();
});

const setHeroRef = (index: number) => (el: unknown) => {
    heroRefs[index] = el as Skeleton | null;
};

const setEnemyRef = (id: number) => (el: unknown) => {
    if (el) enemyRefs.set(id, el as Skeleton);
    else enemyRefs.delete(id);
};
</script>

<template>
    <div class="expedition-scene" :style="{ backgroundImage: `url(${background})` }">
        <div class="scene-side scene-heroes">
            <div
                v-for="(model, index) in heroModels"
                :key="index"
                class="scene-unit"
                :class="[heroFx[index], { 'scene-unit-back': index % 2 === 1 }]"
            >
                <div class="scene-unit-name">{{ model.name }}</div>
                <div class="scene-unit-body">
                    <CharacterSkeleton
                        :ref="setHeroRef(index)"
                        :model="model"
                        :frame-width="FRAME"
                        :frame-height="FRAME"
                        :frame-count="24"
                        :columns="5"
                        :fps="16"
                        :scale="SCALE"
                    />
                </div>
            </div>
        </div>

        <TransitionGroup name="scene-enemy" tag="div" class="scene-side scene-enemies">
            <div
                v-for="(enemy, index) in enemies"
                :key="enemy.id"
                class="scene-unit scene-unit-enemy"
                :class="[enemyFx[enemy.id], { 'scene-unit-back': index % 2 === 1 }]"
            >
                <div class="scene-unit-body">
                    <CharacterSkeleton
                        :ref="setEnemyRef(enemy.id)"
                        :model="enemy.model"
                        :frame-width="FRAME"
                        :frame-height="FRAME"
                        :frame-count="24"
                        :columns="5"
                        :fps="16"
                        :scale="SCALE"
                    />
                </div>
            </div>
        </TransitionGroup>

        <slot />
    </div>
</template>

<style scoped>
.expedition-scene {
    position: relative;
    box-sizing: border-box;
    width: 100%;
    max-width: 1200px;
    height: 300px;
    margin: 0 auto;
    overflow: hidden;
    border-radius: 14px;
    border: 2px solid #333;
    background-color: #1b1210;
    background-size: cover;
    background-position: center 65%;
    box-shadow: 0 0 0 3px rgb(157, 173, 175), inset 0 0 40px rgba(0, 0, 0, 0.85);
}

/* Затемнение снизу, чтобы герои «стояли» на полу */
.expedition-scene::after {
    content: '';
    position: absolute;
    inset: 0;
    background: linear-gradient(to bottom, rgba(0, 0, 0, 0.15) 0%, rgba(0, 0, 0, 0) 45%, rgba(0, 0, 0, 0.45) 100%);
    pointer-events: none;
}

.scene-side {
    position: absolute;
    bottom: -18px;
    z-index: 1;
    display: flex;
    align-items: flex-end;
}

.scene-heroes {
    left: 2%;
}

/* Враги стоят зеркально героям */
.scene-enemies {
    right: 2%;
    flex-direction: row-reverse;
}

.scene-unit {
    position: relative;
    transition: transform 0.18s ease-out;
}

.scene-unit + .scene-unit {
    margin-left: -120px;
}

.scene-enemies .scene-unit + .scene-unit {
    margin-left: 0;
    margin-right: -120px;
}

/* Второй ряд: чуть выше, темнее и позади первого, чтобы группа читалась объёмно */
.scene-unit {
    z-index: 2;
}

.scene-unit-back {
    z-index: 1;
    margin-bottom: 26px;
    filter: brightness(0.85);
}

.scene-unit-enemy .scene-unit-body {
    transform: rotateY(180deg);
}

.scene-unit-body canvas {
    display: block;
}

.scene-unit-name {
    position: absolute;
    top: 22%;
    left: 50%;
    transform: translateX(-50%);
    z-index: 2;
    color: #f8fafc;
    font-family: 'Arial', sans-serif;
    font-size: 12px;
    font-weight: 700;
    white-space: nowrap;
    text-shadow: 0 1px 3px #000;
}

.scene-lunge {
    transform: translateX(26px);
}

.scene-unit-enemy.scene-lunge {
    transform: translateX(-26px);
}

.scene-hit {
    animation: scene-shake 0.45s ease;
}

@keyframes scene-shake {
    0%, 100% { translate: 0; }
    20% { translate: -10px; }
    40% { translate: 8px; }
    60% { translate: -6px; }
    80% { translate: 4px; }
}

/* Враги выходят справа и растворяются после поражения */
.scene-enemy-enter-active {
    transition: transform 0.8s ease-out, opacity 0.8s ease-out;
}

.scene-enemy-leave-active {
    transition: opacity 0.6s ease-in, filter 0.6s ease-in, transform 0.6s ease-in;
}

.scene-enemy-enter-from {
    opacity: 0;
    transform: translateX(160px);
}

.scene-enemy-leave-to {
    opacity: 0;
    filter: grayscale(1) brightness(0.4);
    transform: translateY(12px);
}
</style>
