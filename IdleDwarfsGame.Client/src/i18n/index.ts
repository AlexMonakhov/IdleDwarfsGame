import { createI18n } from 'vue-i18n';
import en from './locales/en';
import ru from './locales/ru';

// Чтобы добавить язык: создать locales/<код>.ts по образцу ru.ts и дописать код сюда и в messages
export const SUPPORTED_LOCALES = ['en', 'ru'] as const;
export type AppLocale = typeof SUPPORTED_LOCALES[number];

// Название языка на нём самом — в настройках его должен узнать и тот, кто не понимает текущий язык
export const LOCALE_NAMES: Record<AppLocale, string> = {
  en: 'English',
  ru: 'Русский',
};

const STORAGE_KEY = 'locale';
const DEFAULT_LOCALE: AppLocale = 'en';

const isSupported = (value: unknown): value is AppLocale =>
  SUPPORTED_LOCALES.includes(value as AppLocale);

// Сохранённый выбор игрока, иначе язык браузера, иначе английский
const detectLocale = (): AppLocale => {
  try {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (isSupported(saved)) return saved;
  } catch {
    // localStorage может быть недоступен (приватный режим) — не страшно
  }
  const browser = navigator.language?.slice(0, 2).toLowerCase();
  return isSupported(browser) ? browser : DEFAULT_LOCALE;
};

const initialLocale = detectLocale();
document.documentElement.lang = initialLocale;

export const i18n = createI18n({
  legacy: false,
  locale: initialLocale,
  fallbackLocale: DEFAULT_LOCALE,
  messages: { en, ru },
});

// Для кода вне компонентов (сервисы, BattlePlayer). Внутри шаблонов вызовы тоже реактивны.
export const t = i18n.global.t;

export const currentLocale = (): AppLocale => i18n.global.locale.value as AppLocale;

export function setLocale(locale: AppLocale) {
  i18n.global.locale.value = locale;
  document.documentElement.lang = locale;
  try {
    localStorage.setItem(STORAGE_KEY, locale);
  } catch {
    // см. detectLocale
  }
}

// Имена статусов приходят с сервера по-английски; неизвестные показываем как есть
export const statusLabel = (name: string): string =>
  i18n.global.te(`statuses.${name}`) ? t(`statuses.${name}`) : name;

export default i18n;
