// ComponentRegistry.ts
import type { ComponentInternalInstance } from 'vue';
import type { Entity } from './Storage';

const componentMap = new Map<Entity, ComponentInternalInstance>();

export function registerComponent(entity: Entity, component: ComponentInternalInstance) {
  componentMap.set(entity, component);
}

export function getComponent(entity: Entity): ComponentInternalInstance | undefined {
  return componentMap.get(entity);
}

export function unregisterComponent(entity: Entity) {
  componentMap.delete(entity);
}
