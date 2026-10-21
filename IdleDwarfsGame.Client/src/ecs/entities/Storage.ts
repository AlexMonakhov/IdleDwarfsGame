import { Health } from "../components/Health";
import { Damage } from "../components/Damage";
import { Speed } from "../components/Speed";
import { Animation } from "../components/Animation";

export type Entity = number;

//characters
export const healths = new Map<Entity, Health>();
export const damages = new Map<Entity, Damage>();
export const speeds = new Map<Entity, Speed>();
export const names = new Map<Entity, string>();

//animations
export const animations = new Map<Entity, Animation[]>();
