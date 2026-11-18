// Каталог звуков игры: ключ → канал, длительность, где играет.
//
// Файл звука называется как ключ и лежит в любой подпапке src/assets/sounds/ (ui/, battle/, music/ — для порядка):
//   src/assets/sounds/ui/ui_click.mp3
// Варианты одного звука (играют случайно, чтобы частые звуки не приедались) — с суффиксом -2, -3…:
//   ui_click.mp3, ui_click-2.mp3, ui_click-3.mp3
// Форматы: .mp3, .ogg, .wav, .webm. Нет файла — звучит запасной (fallback), у боевых — синтез (sounds.ts), иначе тишина.
// Полный список с пояснениями — docs/sounds.md: при добавлении ключа сюда допишите его и туда.

// ui — интерфейс, sfx — бой на арене, music — фоновая музыка (каждый со своей громкостью в настройках)
export type SoundChannel = 'ui' | 'sfx' | 'music';

export interface SoundDef {
  channel: SoundChannel;
  // Примерная длительность — ориентир для подбора звука
  duration: string;
  // Громкость относительно канала, 0..1 (по умолчанию 1)
  volume?: number;
  // Не чаще раза в N мс (по умолчанию 40)
  throttle?: number;
  // Случайный разброс высоты тона, доля (0.05 = ±5%). Для частых звуков, чтобы не звучали одинаково
  pitch?: number;
  // Нет своего файла — играет файл этого ключа (цепочкой)
  fallback?: string;
}

const defineSounds = <T extends Record<string, SoundDef>>(sounds: T) => sounds;

const DAMAGE_TYPES = ['physical', 'magical', 'fire', 'frost', 'lightning', 'poison', 'bleed', 'true', 'dark', 'radiant'] as const;
export type DamageSoundType = typeof DAMAGE_TYPES[number];

export const SOUNDS = defineSounds({
  // ===== Интерфейс: базовые элементы =====
  // Клики и наведение играют сами для всех кнопок (uiSounds.ts); data-sound у кнопки меняет звук
  ui_hover: { channel: 'ui', duration: '30–60 мс', volume: 0.35, throttle: 60, pitch: 0.04 },
  // Наведение на здание города (здание и его плашка — одно целое) и на карточку (герой, предмет, самоцвет, сундук,
  // способность, пассивка, карточки спуска)
  ui_hover_town: { channel: 'ui', duration: '100–300 мс', volume: 0.6, throttle: 60, pitch: 0.03 },
  ui_hover_card: { channel: 'ui', duration: '40–100 мс', volume: 0.5, throttle: 60, pitch: 0.04 },
  // Наведение на гем-кнопку и кнопку меню
  ui_hover_button: { channel: 'ui', duration: '40–100 мс', volume: 0.5, throttle: 60, pitch: 0.04 },
  // Клик по кнопке; ui_click — по остальному кликабельному (карточки, плитки, здания), без своего файла — как кнопка
  ui_click_button: { channel: 'ui', duration: '50–100 мс', pitch: 0.04 },
  ui_click: { channel: 'ui', duration: '50–100 мс', pitch: 0.04, fallback: 'ui_click_button' },
  ui_click_primary: { channel: 'ui', duration: '100–200 мс', fallback: 'ui_click_button' },
  // Отказ: клик по недоступной кнопке, сервер отказал, действие нельзя (отряд полон)
  ui_click_error: { channel: 'ui', duration: '~0.55 с', throttle: 150 },
  ui_click_disabled: { channel: 'ui', duration: '80–150 мс', throttle: 150, fallback: 'ui_click_error' },
  ui_tab: { channel: 'ui', duration: '50–100 мс', fallback: 'ui_click_button' },
  ui_toggle_on: { channel: 'ui', duration: '60–120 мс', fallback: 'ui_click_button' },
  ui_toggle_off: { channel: 'ui', duration: '60–120 мс', fallback: 'ui_toggle_on' },
  ui_modal_open: { channel: 'ui', duration: '200–400 мс', throttle: 150 },
  ui_modal_close: { channel: 'ui', duration: '150–300 мс', throttle: 150 },
  ui_confirm_open: { channel: 'ui', duration: '200–350 мс', fallback: 'ui_modal_open' },
  ui_error: { channel: 'ui', duration: '200–400 мс', throttle: 300, fallback: 'ui_click_error' },
  ui_notify: { channel: 'ui', duration: '200–500 мс', throttle: 1000 },
  ui_slider: { channel: 'ui', duration: '20–40 мс', volume: 0.4, throttle: 70, pitch: 0.03 },

  // ===== Ресурсы и экономика =====
  // Играют сами, когда сервер прислал больше валюты (UseResources)
  coin_gain_small: { channel: 'ui', duration: '150–300 мс', throttle: 120, pitch: 0.05 },
  coin_gain_big: { channel: 'ui', duration: '0.8–1.5 с', throttle: 400, fallback: 'coin_gain_small' },
  gem_gain: { channel: 'ui', duration: '300–600 мс', throttle: 200, fallback: 'coin_gain_small' },
  currency_gain: { channel: 'ui', duration: '200–400 мс', throttle: 200, fallback: 'coin_gain_small' },
  purchase_success: { channel: 'ui', duration: '400–800 мс' },
  reroll: { channel: 'ui', duration: '300–600 мс' },
  collect: { channel: 'ui', duration: '0.5–1 с' },
  reward_claim: { channel: 'ui', duration: '300–600 мс', throttle: 200 },

  // ===== Сундуки =====
  chest_appear: { channel: 'ui', duration: '300–600 мс' },
  chest_open_common: { channel: 'ui', duration: '0.6–1 с' },
  chest_open_uncommon: { channel: 'ui', duration: '0.8–1.2 с', fallback: 'chest_open_common' },
  chest_open_rare: { channel: 'ui', duration: '1–1.5 с', fallback: 'chest_open_uncommon' },
  chest_open_mythic: { channel: 'ui', duration: '1.5–2 с', fallback: 'chest_open_rare' },
  chest_open_legendary: { channel: 'ui', duration: '2–3 с', fallback: 'chest_open_mythic' },
  card_deal: { channel: 'ui', duration: '150–300 мс', throttle: 60, pitch: 0.06 },
  // Удар по руде: карта сундука (жила) и руда в шахте. Свои файлы card_hit / mine_dig — если звуки должны различаться
  ui_click_ore_card: { channel: 'ui', duration: '300–600 мс', pitch: 0.06 },
  card_hit: { channel: 'ui', duration: '100–200 мс', throttle: 30, pitch: 0.06, fallback: 'ui_click_ore_card' },
  // Третий удар: карта раскалывается
  ui_click_ore_card_crack: { channel: 'ui', duration: '~0.65 с', throttle: 30, pitch: 0.05 },
  card_reveal: { channel: 'ui', duration: '150–300 мс', throttle: 60, pitch: 0.04 },
  card_reveal_rare: { channel: 'ui', duration: '0.5–1.2 с', throttle: 150, fallback: 'card_reveal' },
  card_reveal_legendary: { channel: 'ui', duration: '1–2 с', throttle: 300, fallback: 'card_reveal_rare' },
  chest_skip: { channel: 'ui', duration: '200–400 мс' },

  // ===== Герои, кузница, способности =====
  hero_upgrade: { channel: 'ui', duration: '0.6–1.2 с' },
  hero_fusion: { channel: 'ui', duration: '1.5–3 с' },
  ability_combine: { channel: 'ui', duration: '0.8–1.5 с' },
  ability_insert: { channel: 'ui', duration: '150–300 мс' },
  ability_extract: { channel: 'ui', duration: '0.8–1.5 с' },
  forge_fuse_items: { channel: 'ui', duration: '0.8–1.5 с' },
  forge_fuse_gems: { channel: 'ui', duration: '0.6–1.2 с', fallback: 'forge_fuse_items' },
  equip_item: { channel: 'ui', duration: '150–350 мс' },
  unequip_item: { channel: 'ui', duration: '150–300 мс', fallback: 'equip_item' },
  drag_pick: { channel: 'ui', duration: '60–150 мс' },
  drag_drop: { channel: 'ui', duration: '80–150 мс' },
  squad_save: { channel: 'ui', duration: '200–400 мс' },
  hero_rename: { channel: 'ui', duration: '150–300 мс', fallback: 'ui_click_primary' },

  // ===== Шахты, экспедиции, гильдия =====
  mine_dig: { channel: 'ui', duration: '150–350 мс', throttle: 80, pitch: 0.06, fallback: 'ui_click_ore_card' },
  mine_build: { channel: 'ui', duration: '1–2 с' },
  building_upgrade: { channel: 'ui', duration: '0.8–1.5 с' },
  dwarf_assign: { channel: 'ui', duration: '200–400 мс' },
  dwarf_recall: { channel: 'ui', duration: '200–400 мс', fallback: 'dwarf_assign' },
  expedition_start: { channel: 'ui', duration: '0.5–1 с' },

  // ===== Прогресс и награды =====
  achievement_claim: { channel: 'ui', duration: '1.5–2.5 с', fallback: 'reward_claim' },
  calendar_claim: { channel: 'ui', duration: '0.5–1 с', fallback: 'reward_claim' },
  calendar_claim_big: { channel: 'ui', duration: '1.5–2.5 с', fallback: 'calendar_claim' },
  delve_start: { channel: 'ui', duration: '0.8–1.5 с' },
  delve_move: { channel: 'ui', duration: '300–700 мс', pitch: 0.05 },
  delve_relic: { channel: 'ui', duration: '0.8–1.5 с' },
  delve_camp: { channel: 'ui', duration: '0.8–1.5 с' },
  delve_revive: { channel: 'ui', duration: '1–1.5 с' },
  delve_retreat: { channel: 'ui', duration: '0.5–1 с' },
  arena_bet: { channel: 'ui', duration: '300–500 мс' },
  social_action: { channel: 'ui', duration: '300–600 мс' },

  // ===== Системное =====
  tutorial_step: { channel: 'ui', duration: '200–400 мс', throttle: 200 },
  login_success: { channel: 'ui', duration: '1–2 с' },

  // ===== Бой на арене =====
  // Нет файла — синтез из sounds.ts (как раньше), так что арена не замолкает
  battle_start: { channel: 'sfx', duration: '1–2 с' },
  battle_round: { channel: 'sfx', duration: '300–700 мс' },
  battle_arena_buff: { channel: 'sfx', duration: '0.8–1.5 с' },
  battle_swing: { channel: 'sfx', duration: '150–250 мс', pitch: 0.06 },
  battle_hit_physical: { channel: 'sfx', duration: '100–250 мс', pitch: 0.06 },
  battle_hit_magical: { channel: 'sfx', duration: '200–350 мс', pitch: 0.05 },
  battle_hit_fire: { channel: 'sfx', duration: '300–500 мс', pitch: 0.05 },
  battle_hit_frost: { channel: 'sfx', duration: '200–400 мс', pitch: 0.05 },
  battle_hit_lightning: { channel: 'sfx', duration: '150–300 мс', pitch: 0.05 },
  battle_hit_poison: { channel: 'sfx', duration: '150–300 мс', pitch: 0.05 },
  battle_hit_bleed: { channel: 'sfx', duration: '150–250 мс', pitch: 0.05 },
  battle_hit_true: { channel: 'sfx', duration: '100–250 мс', pitch: 0.05 },
  battle_hit_dark: { channel: 'sfx', duration: '250–400 мс', pitch: 0.05 },
  battle_hit_radiant: { channel: 'sfx', duration: '300–500 мс', pitch: 0.05 },
  battle_crit: { channel: 'sfx', duration: '300–600 мс', pitch: 0.04 },
  battle_cast: { channel: 'sfx', duration: '250–500 мс', pitch: 0.04 },
  battle_cast_physical: { channel: 'sfx', duration: '250–500 мс', pitch: 0.04, fallback: 'battle_cast' },
  battle_cast_magical: { channel: 'sfx', duration: '250–500 мс', pitch: 0.04, fallback: 'battle_cast' },
  battle_cast_fire: { channel: 'sfx', duration: '250–500 мс', pitch: 0.04, fallback: 'battle_cast' },
  battle_cast_frost: { channel: 'sfx', duration: '250–500 мс', pitch: 0.04, fallback: 'battle_cast' },
  battle_cast_lightning: { channel: 'sfx', duration: '250–500 мс', pitch: 0.04, fallback: 'battle_cast' },
  battle_cast_poison: { channel: 'sfx', duration: '250–500 мс', pitch: 0.04, fallback: 'battle_cast' },
  battle_cast_bleed: { channel: 'sfx', duration: '250–500 мс', pitch: 0.04, fallback: 'battle_cast' },
  battle_cast_true: { channel: 'sfx', duration: '250–500 мс', pitch: 0.04, fallback: 'battle_cast' },
  battle_cast_dark: { channel: 'sfx', duration: '250–500 мс', pitch: 0.04, fallback: 'battle_cast' },
  battle_cast_radiant: { channel: 'sfx', duration: '250–500 мс', pitch: 0.04, fallback: 'battle_cast' },
  battle_aura: { channel: 'sfx', duration: '0.5–1 с', fallback: 'battle_cast' },
  battle_heal: { channel: 'sfx', duration: '300–500 мс', pitch: 0.04 },
  battle_status: { channel: 'sfx', duration: '150–300 мс', volume: 0.6, throttle: 80, pitch: 0.05 },
  battle_stun: { channel: 'sfx', duration: '250–400 мс' },
  battle_immune: { channel: 'sfx', duration: '150–250 мс' },
  battle_death: { channel: 'sfx', duration: '0.5–0.8 с' },
  battle_dialogue: { channel: 'sfx', duration: '100–250 мс', volume: 0.5, throttle: 300 },
  battle_victory: { channel: 'sfx', duration: '2–4 с' },
  battle_defeat: { channel: 'sfx', duration: '1.5–3 с' },

  // ===== Музыка (циклы) =====
  // Нет файла сцены — играет запасная: бой → город, босс → бой, вход → город
  music_town: { channel: 'music', duration: 'цикл 1.5–3 мин' },
  music_battle: { channel: 'music', duration: 'цикл 1–2 мин', fallback: 'music_town' },
  music_boss: { channel: 'music', duration: 'цикл 1.5–2.5 мин', fallback: 'music_battle' },
  music_login: { channel: 'music', duration: 'цикл 1–2 мин', fallback: 'music_town' },
});

export type SoundKey = keyof typeof SOUNDS;
export type MusicKey = { [K in SoundKey]: typeof SOUNDS[K]['channel'] extends 'music' ? K : never }[SoundKey];

export const soundDef = (key: string): SoundDef | undefined => (SOUNDS as Record<string, SoundDef>)[key];

// Тип урона с сервера («Fire», «Physical») → суффикс ключа; незнакомый — физический
export const damageSound = (damageType: string | null | undefined): DamageSoundType => {
  const type = (damageType ?? '').toLowerCase();
  return (DAMAGE_TYPES as readonly string[]).includes(type) ? (type as DamageSoundType) : 'physical';
};
