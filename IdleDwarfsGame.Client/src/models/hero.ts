import { SpriteColor } from '@/battle/sprites';

export interface HeroModel {
  id: number;
  name: string;
  currentHp: number;
  maxHp: number;
  lvl: number;
  color: SpriteColor;
  unitType?: string;
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
