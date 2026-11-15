// Переключатель Pixi-отрисовки (нагрузочный тест арены сравнивает Pixi и canvas на одном бою).
// Отдельный модуль без Pixi: его импортирует particleService, а сам Pixi грузится лениво
export const pixiFxSettings = {
  enabled: true,
};
