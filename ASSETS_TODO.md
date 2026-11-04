# Чего не хватает из графики

Список составлен по коду на 27.10.2026. Пока картинки нет, игра подставляет ближайшую похожую:
юнит без спрайта рисуется дворфом, дворф без своего цвета или уровня — ближайшим листом.

## 1. Спрайты юнитов — самое заметное

**Как устроены.** Лист 2048×2048, 5 колонок кадров по 409.6 px. Кладутся в
`IdleDwarfsGame.Client/src/assets/sprite-lists/` (папка внутри любая), имя файла:

```
{юнит}_{анимация}_lvl{уровень}_{цвет}_{вариант}.png      пример: dwarf_attack_lvl4_red_2.png
```

- юнит: `dwarf`, `orc`, `goblin`, `skeleton`, `troll`, `yeti`, `ogre`, `golem` (маленькими буквами, как `UnitType` на сервере);
- анимация: `idle`, `attack`, `victory`, `death`;
- уровень: ранг героя, 1–5;
- цвет: `basic` (только 1 ранг), `red` рубин, `blue` сапфир, `yellow` топаз, `green` изумруд, `purple` тьма, `violet` кровь, `white` бриллиант, `rainbow` и `negative` — дворфы 5-го ранга. На `lvl2` у крови своего листа нет, она рисуется листом `purple` (таблица `SHEET_COLORS`);
- вариант: `1`, `2`… (у атаки их может быть несколько, берутся по очереди).

Код трогать не нужно: после `npm run atlas` лист появится в игре по своему имени ([sprites.ts](IdleDwarfsGame.Client/src/battle/sprites.ts)). В игру и сборку идут только атласы, сами листы можно хранить вне проекта.

**Что есть сейчас:** только `dwarf` 4-го уровня, красный и синий: `idle`, `attack` (2 варианта), `victory`.

### Дворфы 5-го ранга

Только два дворфа: `rainbow` (все стихии, кроме тьмы) и `negative` (тьма). Новые листы кладутся как `lvl5`. Героев этого ранга пока нет ни в базе, ни в сундуках (`HeroLevels.Available`).

- `rainbow`: своих листов нет. Пока рисуются `idle` и `walk` от `lvl4_rainbow`, а `attack` и `victory` — от `white` 4-го ранга.
- `negative`: нет ничего, ни листов, ни портрета: рисуется тёмный дворф 4-го ранга.

### Дворфы — по приоритету

1. **1-й уровень, `basic`** — idle, attack, victory. С этим уровнем начинает *каждый* новый аккаунт,
   а сейчас вместо них показывается красный/синий дворф 4-го уровня.
2. **4-й уровень, недостающие цвета:** `yellow`, `green`, `purple`, `white` — idle, attack, victory.
3. **5-й уровень, все 6 цветов** — высший ранг, им нужен свой, самый эффектный вид.
4. **2-й и 3-й уровни, все 6 цветов.**
5. **`death`** — анимации смерти нет ни у кого (тип в коде заведён, листов нет).

Полная матрица дворфов: 1 × 4 (basic) + 6 цветов × 4 уровня × 4 анимации = **100 листов** (без вторых вариантов атаки).

### Враги — своих спрайтов нет ни у кого, все рисуются дворфами

| Юнит | Где встречается | Стихии (цвет листа) |
|---|---|---|
| `orc` | башня, кампания (Orc Brute, Orc Warlord, Gorrak), босс гильдии Ashmaw | `red`, `blue` |
| `goblin` | башня, кампания (Goblin Scout / Shaman, Frost Goblin) | `blue` |
| `skeleton` | башня, кампания (Skeleton Mage, Rime Skeleton, Hrimgar), босс гильдии Nyxara | `blue`, `purple` |
| `troll` | босс гильдии Mossback Troll | `green` |
| `yeti` | босс гильдии Rimehorn Yeti | `blue` |
| `ogre` | босс гильдии Grukk Stormcaller | `yellow` |
| `golem` | босс гильдии Seraphel the Shattered Light | `white` |

Враги все 4-го уровня — хватит `lvl4`. Минимум на юнита: `idle` + `attack`; `death` — когда будет у дворфов.

## 2. Фоны арен

Картинки лежат в `IdleDwarfsGame.Client/src/assets/arenas/`, на `/arena-stress` новый файл появляется сам. В игру подключаются в [arenaBackgrounds.ts](IdleDwarfsGame.Client/src/battle/arenaBackgrounds.ts): импорт картинки и ключ.

| Ключ | Арена | Стихия | Что сейчас |
|---|---|---|---|
| `lava` | Lava Arena | огонь | есть (`arena_red.png`) |
| `ice` | Ice Arena | мороз | есть (`arena_blue.png`) |
| `storm` | Storm Peak — вершина в грозе, молнии бьют в скалы | молния | есть (`arena_yellow.png`) |
| `marsh` | Venom Marsh — ядовитое болото, зелёные испарения | природа | есть (`arena_green.png`) |
| `crypt` | Shadow Crypt — склеп с аметистовыми тенями | тьма | есть (`arena_dark.png`) |
| `sanctum` | Crystal Sanctum — зал из бриллиантов, радужный свет | сияние | есть (`arena_white.png`) |
| `altar` | Blood Altar — алтарь с гранатовыми рунами | кровь | есть (`arena_violet.png`) |
| `colosseum` | Арена PvP | — | есть (`arena_violet.png`, общая с `altar`) |

Попутно:
- Картинки арен весят **6–7 МБ** каждая (3356×1874) — стоит пережать (webp/jpg), это фон каждого боя.
- В `src/assets/` лежат картинки, на которые нет ссылок в коде: `arena-bg-blue-crystals.png`, `background.png`, `background1.png`,
  `background3.png`, `background4123.png`, `backgroundasd.png`, `background-home-v1-clip.png`, `crystal-button-background.png`,
  `mining.png`, `stone-crack.png`, `Gemini_Generated_Image_*.png`, `R.png`, `ыыыы.jpg` — разобрать или удалить.

## 3. Иконки

- **Статусы.** Нарисованы SVG-иконки: Burn, Frost, Stun, Bleed, Regen (Heal), а также — временные, от меня — Poison, Curse,
  Shock, Freeze ([components/icons/status/](IdleDwarfsGame.Client/src/components/icons/status/)). Новые четыре стоит перерисовать
  в общем стиле.
- **Заклинания.** Иконок нет совсем, в подсказках только текст. 18 семейств:
  Fireball, Flame Wave, Meteor · Frost Bolt, Blizzard, Glacial Prison · Lightning, Chain Lightning, Thunderclap ·
  Healing Rune, Venom Thorns, Entangling Roots · Shadow Bolt, Hex · Life Drain · Prism Ray, Radiant Nova, Dawnlight.
- **Стихии / камни.** Стихия везде подписана текстом («Рубин · Огонь»), гемы в коллекции — цветные CSS-плашки.
  Нужны 7 иконок камней: рубин, сапфир, топаз, изумруд, аметист, бриллиант, гранат.
- **Сундуки** показываются эмодзи (🛡 💎 ♾ ⚒) — нужны 4 иконки.

## 4. Шахты

Картинки кладутся в `IdleDwarfsGame.Client/src/assets/` (папка внутри любая, например `assets/mining/`) и подхватываются
по имени файла, код трогать не нужно ([miningArt.ts](IdleDwarfsGame.Client/src/components/mining/miningArt.ts)). Пока картинки нет — рисуется заглушка.

| Файл | Что это | Что сейчас |
|---|---|---|
| `mining-gold.png` | Шахта — для всех типов, пока у них нет своей | нет, значок ⛏ |
| `mining-<тип>.png` | Своя картинка шахты: `mining-ruby.png`, `mining-experience.png`… | нет, берётся `mining-gold.png` |
| `mining-plot.png` | Свободное место под шахту (скважина) | нет, пунктирная плашка ＋ |
| `mining-ore-<тип>.png` | Руда или кристалл в окне шахты, по нему кликают: `mining-ore-gold.png`, `mining-ore-ruby.png`… | нет, значок 🪙 ✦ ◆ |

Типы: `gold`, `experience`, `ruby`, `sapphire`, `topaz`, `emerald`, `amethyst`, `diamond`, `garnet`.
