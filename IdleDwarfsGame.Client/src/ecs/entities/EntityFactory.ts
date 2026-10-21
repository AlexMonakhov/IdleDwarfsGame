import { healths, damages, speeds, names, Entity } from './Storage';

let nextId = 1;

export function createEntity(name: string, hp: number, dmg: number, spd: number): Entity {
  const id = nextId++;
  healths.set(id, { value: hp });
  damages.set(id, { value: dmg });
  speeds.set(id, { value: spd });
  names.set(id, name);
  return id;
}

