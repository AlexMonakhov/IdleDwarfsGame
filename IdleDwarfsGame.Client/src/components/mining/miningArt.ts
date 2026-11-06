import { GEM_COLORS } from '@/components/inventory/lootStyles';
import { MineType } from '@/models/types/Mining';

// Картинки шахт лежат в assets (например, assets/mining/mining-gold.png). Пока их нет — рисуем заглушки.
// Подхватываются по имени файла, без правки кода:
//   mining-<тип>.png      — шахта этого типа (mining-ruby.png); если нет — mining-gold.png для всех шахт
//   mining-plot.png       — свободное место (скважина)
//   mining-ore-<тип>.png  — руда или кристалл в окне шахты (mining-ore-gold.png)
const images = require.context('@/assets', true, /mining-[\w-]+\.png$/);
const byName = new Map(images.keys().map(key => [key.split('/').pop() as string, key]));

const image = (name: string): string | null => {
    const key = byName.get(name);
    return key ? images(key) as string : null;
};

export const mineImage = (type: MineType): string | null =>
    image(`mining-${type.toLowerCase()}.png`) ?? image('mining-gold.png');

export const plotImage = (): string | null => image('mining-plot.png');

export const oreImage = (type: MineType): string | null => image(`mining-ore-${type.toLowerCase()}.png`);

// Цвет ресурса шахты: золото, опыт (как в шапке) или камень стихии
export const mineColor = (type: MineType): string => {
    if (type === 'Gold') return '#ffd166';
    if (type === 'Experience') return '#a5f3fc';
    return GEM_COLORS[type] ?? '#cbd5e1';
};

// Значок ресурса, пока нет картинок руды, — компонент MineResourceIcon
