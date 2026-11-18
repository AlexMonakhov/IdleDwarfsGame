import { soundService } from './SoundService';
import { soundDef, type SoundKey } from './soundCatalog';

// Звуки кликов и наведения для всего интерфейса — одним слушателем на документ, без правок в каждой кнопке.
// Кликабельное — кнопки, ссылки, вкладки и всё с курсором-рукой (здания города, плитки предметов).
//
// Кнопки звучат ui_click_button, остальное кликабельное (карточки, плитки, здания) — ui_click.
// Разметка меняет звук:
//   data-sound="ui_click_primary"    — свой ключ клика (главные кнопки: «В бой», «Купить», «Открыть»)
//   data-sound="none"                — без звука клика (у действия свой звук: крестик окна, открытие сундука)
//   data-sound-hover="ui_hover_card" — свой звук наведения; работает и на некликабельном (карточки)
//   data-sound-hover="none"          — без звука наведения
//   data-hover-id="mine"             — элементы с одним id звучат как одно целое (здание и его плашка)
// Вкладки (role="tab") — ui_tab, флажки — ui_toggle_on/off, ползунки — ui_slider, недоступные кнопки — ui_click_disabled

const CLICKABLE = 'button, a[href], [role="button"], [role="tab"], [data-sound], input[type="checkbox"], input[type="radio"], select, summary';

// Курсоры кликабельного: рука, «взять» (перетаскиваемые карточки героев), «нельзя» (недоступная карточка)
const CLICK_CURSORS = new Set(['pointer', 'grab', 'not-allowed']);
const cursorOf = (el: Element) => getComputedStyle(el).cursor;
const isClickable = (el: Element) => CLICK_CURSORS.has(cursorOf(el));

// Что считать «кнопкой» под указателем: явный кликабельный элемент, иначе ближайший предок с кликабельным курсором,
// расширенный до самого внешнего такого же. Подъём нужен потому, что внутри карточки бывают элементы со своим
// курсором (иконка в тултипе — «help») и SVG-иконки; а самый внешний — потому что курсор наследуется
// (картинка внутри здания тоже «рука» — звук должен быть одним на всё здание)
function soundTarget(target: EventTarget | null): HTMLElement | null {
  if (!(target instanceof Element)) return null;
  const explicit = target.closest<HTMLElement>(CLICKABLE);
  if (explicit) return explicit;
  let el: Element | null = target;
  while (el && el !== document.body && !isClickable(el)) el = el.parentElement;
  if (!el || el === document.body) return null;
  for (let parent = el.parentElement; parent && parent !== document.body && isClickable(parent); parent = parent.parentElement) {
    el = parent;
  }
  // Самым внешним может оказаться SVG — звук вешаем на ближайший HTML-элемент
  while (el && !(el instanceof HTMLElement)) el = el.parentElement;
  return el instanceof HTMLElement ? el : null;
}

// Что звучит при наведении: ближайший элемент со своим звуком наведения (карточка — целиком, вместе с кнопками
// внутри), иначе кликабельное под указателем
function hoverTarget(target: EventTarget | null): HTMLElement | null {
  const custom = target instanceof Element ? target.closest<HTMLElement>('[data-sound-hover]') : null;
  return custom ?? soundTarget(target);
}

// Ключ из разметки, если он есть в каталоге; опечатку показываем в консоли
function soundKey(value: string, fallback: SoundKey): SoundKey {
  if (soundDef(value)) return value as SoundKey;
  console.warn(`[sounds] Нет ключа «${value}» в soundCatalog.ts`);
  return fallback;
}

const BUTTON = 'button, [role="button"], a[href], summary, select';

const isDisabled = (el: HTMLElement) =>
  el.matches(':disabled') || el.getAttribute('aria-disabled') === 'true' || el.closest('[aria-disabled="true"]') !== null
  || cursorOf(el) === 'not-allowed';

function clickSound(el: HTMLElement): SoundKey | null {
  const custom = el.dataset.sound;
  if (custom === 'none') return null;
  if (custom) return soundKey(custom, 'ui_click');
  if (el instanceof HTMLInputElement && (el.type === 'checkbox' || el.type === 'radio')) {
    return el.checked ? 'ui_toggle_on' : 'ui_toggle_off';
  }
  if (el.getAttribute('role') === 'tab') return 'ui_tab';
  return el.matches(BUTTON) ? 'ui_click_button' : 'ui_click';
}

let installed = false;

export function installUiSounds() {
  if (installed || typeof document === 'undefined') return;
  installed = true;

  // Что сейчас под указателем: элемент или общий data-hover-id
  let hovered: HTMLElement | string | null = null;
  let hoveredEl: HTMLElement | null = null;

  document.addEventListener('pointerover', event => {
    // На сенсорных экранах наведения нет — только клик
    if (event.pointerType !== 'mouse') return;
    const el = hoverTarget(event.target);
    const identity = el?.dataset.hoverId ?? el;
    if (identity === hovered) return;
    // Вернулись с вложенного (кнопка «В бой» внутри карточки боя) на саму карточку — она уже звучала
    const backToParent = !!el && !!hoveredEl && el.contains(hoveredEl);
    hovered = identity;
    hoveredEl = el;
    if (!el || backToParent || isDisabled(el)) return;
    const custom = el.dataset.soundHover;
    if (custom === 'none' || (!custom && el.dataset.sound === 'none')) return;
    soundService.play(custom ? soundKey(custom, 'ui_hover') : 'ui_hover');
  }, { capture: true, passive: true });

  // Недоступная кнопка click не получает, а pointerdown — да
  document.addEventListener('pointerdown', event => {
    const el = soundTarget(event.target);
    if (el && isDisabled(el)) soundService.play('ui_click_disabled');
  }, { capture: true, passive: true });

  // В фазе перехвата: раньше обработчиков, которые останавливают всплытие (@click.stop)
  document.addEventListener('click', event => {
    if (!event.isTrusted) return;
    const el = soundTarget(event.target);
    if (!el || isDisabled(el)) return;
    const key = clickSound(el);
    if (key) soundService.play(key);
  }, { capture: true, passive: true });

  document.addEventListener('input', event => {
    const el = event.target;
    if (el instanceof HTMLInputElement && el.type === 'range' && el.dataset.sound !== 'none') soundService.play('ui_slider');
  }, { capture: true, passive: true });
}
