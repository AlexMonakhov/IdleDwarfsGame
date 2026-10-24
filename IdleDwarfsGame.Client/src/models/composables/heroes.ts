// src/models/heroes.ts
import { reactive, ref } from 'vue';
import { HeroModel } from '../hero';
import { heroesSetup, setupHeroes } from './heroesSetup';

// Глобальный реактивный список героев
export const heroes = ref<HeroModel[]>([]);

// Глобальная карта быстрого доступа к героям по id
const heroMap = new Map<number, HeroModel>();

// Заполняет список героев (например, из снимка юнитов боя)
export function setHeroes(models: Omit<HeroModel, 'config'>[]) {
  heroes.value.length = 0;
  heroMap.clear();
  setupHeroes();

  models.forEach(model => {
    const hero = reactive<HeroModel>({
      ...model,
      config: heroesSetup.get(model.id),
    });

    heroes.value.push(hero);
    heroMap.set(model.id, hero);
  });
}

// Функция обновления HP конкретного героя
export function updateHeroHp(entity: number, newHp: number) {
  const hero = heroMap.get(entity);
  if (hero) {
    hero.currentHp = newHp;
  } else {
    console.warn(`Не найден герой с Entity ID = ${entity}`);
  }
}
