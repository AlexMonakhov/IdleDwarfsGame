
import { Entity } from "@/ecs/entities/Storage";

export interface HeroModel {
  id: Entity;
  name: string;
  currentHp: number;
  maxHp: number;
  lvl: number;
  color: 'blue' |  'red' | 'green' | 'yellow';
  config?: ICharacterConfig;
}

type Implements<T, U extends T> = U;

export interface ICharacterConfig {
  mainColor?: string;
  secondColor?: string;
  thirdColor?: string;
  accentColor?: string;
  faceColor?: string;
  skinColor?: string;
  stroke: string;
  crystalLightColor: string;
  crystalDarkColor: string; 
  crystalMediumColor: string;
  crystalStroke?: string;
  secondaryCrystalLightColor?: string;
  secondaryCrystalDarkColor?: string; 
  secondaryCrystalMediumColor?: string;
  secondaryCrystalStroke?: string;
}

export type DwarfCharacterConfig  = Implements<ICharacterConfig,{
  mainColor?: string;
  secondColor?: string;
  thirdColor?: string;
  accentColor: string;
  faceColor: string;
  skinColor: string;
  stroke: string;
  crystalLightColor: string;
  crystalDarkColor: string; 
  crystalMediumColor: string;
  crystalStroke?: string;
  secondaryCrystalLightColor?: string;
  secondaryCrystalDarkColor?: string; 
  secondaryCrystalMediumColor?: string;
  secondaryCrystalStroke?: string;
  
}>
