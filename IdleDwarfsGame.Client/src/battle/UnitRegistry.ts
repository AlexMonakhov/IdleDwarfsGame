// Связь entityId -> отрисованный компонент юнита, чтобы проигрыватель логов мог дёргать его анимации
export interface UnitHitOptions {
  crit?: boolean;
  // Цвет вспышки силуэта (по умолчанию красный)
  flash?: string;
}

export interface UnitView {
  el?: () => HTMLElement | null;
  attack?: () => void;
  cast?: (color?: string) => void;
  hit?: (damageType?: string, options?: UnitHitOptions) => void;
  heal?: () => void;
  // Сработала аура носителя: вспышка силуэта её цветом
  aura?: (color: string) => void;
  victory?: () => void;
}

const views = new Map<number, UnitView>();

export function registerUnitView(entity: number, view: UnitView) {
  views.set(entity, view);
}

export function getUnitView(entity: number): UnitView | undefined {
  return views.get(entity);
}

export function unregisterUnitView(entity: number, view?: UnitView) {
  if (!view || views.get(entity) === view) views.delete(entity);
}
