import { Sprite, type Texture } from 'pixi.js';

// Пул спрайтов частиц: родилась — берёт готовый, умерла — возвращает. Создавать и удалять сотни спрайтов
// в секунду (снег бурана, искры взрывов) — лишний мусор для сборщика и рывки кадра
const pools = new Map<Texture, Sprite[]>();
// Больше этого на текстуру не храним — лишнее удаляем
const MAX_PER_TEXTURE = 600;

export const takeSprite = (texture: Texture): Sprite => {
  const sprite = pools.get(texture)?.pop();
  if (sprite) {
    sprite.visible = true;
    return sprite;
  }
  const created = new Sprite(texture);
  created.anchor.set(0.5);
  return created;
};

export const giveSprite = (sprite: Sprite) => {
  sprite.removeFromParent();
  const pool = pools.get(sprite.texture) ?? [];
  if (pool.length >= MAX_PER_TEXTURE) {
    sprite.destroy();
    return;
  }
  // Сброс того, что эффекты меняют
  sprite.alpha = 1;
  sprite.tint = 0xffffff;
  sprite.rotation = 0;
  sprite.scale.set(1);
  pools.set(sprite.texture, pool);
  pool.push(sprite);
};
