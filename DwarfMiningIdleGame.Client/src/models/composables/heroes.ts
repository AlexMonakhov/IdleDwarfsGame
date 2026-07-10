// src/models/heroes.ts
import { reactive, ref } from 'vue';
import { names, healths, Entity } from '@/ecs/entities/Storage'; // Путь подстрой под свой проект
import { HeroModel } from '../hero'; // Подстрой под свой проект
import { heroesSetup, setupHeroes } from './heroesSetup';

// Глобальный реактивный список героев
export const heroes = ref<HeroModel[]>([]);

// Глобальная карта быстрого доступа к героям по id
const heroMap = new Map<Entity, HeroModel>();

// Функция создания героев на основе ECS компонентов
export async function buildHeroes() {
  heroes.value.length = 0;
  heroMap.clear();
  setupHeroes();

  names.forEach((name, id) => {
    const health = healths.get(id);
    if (!health) {
      console.warn(`Сущность ${id} пропущена — нет компонента здоровья.`);
      return;
    }

    const hero = reactive<HeroModel>({
        id,
        name,
        currentHp: health.value,
        maxHp: health.value,
        config: heroesSetup.get(id),
        lvl:4, // Временная заглушка, потом будет динамически
        color: 'red' // Временная заглушка, потом будет динамически
      });

    heroes.value.push(hero);
    heroMap.set(id, hero);
  });
}

// Функция обновления HP конкретного героя
export function updateHeroHp(entity: Entity, newHp: number) {
  const hero = heroMap.get(entity);
  if (hero) {
    hero.currentHp = newHp;
  } else {
    console.warn(`Не найден герой с Entity ID = ${entity}`);
  }
}
