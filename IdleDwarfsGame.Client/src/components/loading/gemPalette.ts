// Цвета камней кнопок из App.vue (--{color}-gem-color) — общие для загрузчиков

export type GemColor = 'pink' | 'green' | 'blue' | 'yellow' | 'red' | 'purple' | 'white';

export const GEM_COLOR_NAMES: GemColor[] = ['pink', 'green', 'blue', 'yellow', 'red', 'purple', 'white'];

const COLOR_FALLBACK: Record<GemColor, string> = {
  pink: '#e91e63', green: '#00c853', blue: '#2196f3', yellow: '#ffb300',
  red: '#d40808', purple: '#9b30ff', white: '#dff3ff',
};

export const gemColorVar = (color: GemColor) => `var(--${color}-gem-color, ${COLOR_FALLBACK[color]})`;

export const pickGemColor = (rand: () => number, except?: GemColor | null): GemColor => {
  const pool = GEM_COLOR_NAMES.filter(c => c !== except);
  return pool[Math.floor(rand() * pool.length)];
};

/** 'random' превращает в конкретный цвет палитры; второй цвет не совпадает с первым */
export function resolveGemColors(
  rand: () => number,
  color: GemColor | 'random',
  colorTo: GemColor | 'random' | null,
): { a: GemColor; b: GemColor | null } {
  const a = color === 'random' ? pickGemColor(rand) : color;
  const b = colorTo === 'random' ? pickGemColor(rand, a) : colorTo;
  return { a, b };
}
