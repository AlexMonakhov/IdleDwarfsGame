
import { Entity, names } from "@/ecs/entities/Storage";
import { DwarfCharacterConfig, ICharacterConfig } from "../hero";
import generatePalette from "./colorUtils";

export const heroesSetup = new Map<Entity, ICharacterConfig>();



export async function setupHeroes(){

    const config: DwarfCharacterConfig = {
            mainColor: '#b41012',
            secondColor: '#d7271b',
            thirdColor: '#e28422',
            accentColor: '#8c3712',
            faceColor: '#fd7f56',
            skinColor: '#d5251b',
            crystalDarkColor: '#b60f13',
            crystalLightColor: '#fc4e39',
            crystalMediumColor: '#d9271c',
            secondaryCrystalLightColor: '#27a1c9',
            secondaryCrystalMediumColor: '#054b94',
            secondaryCrystalDarkColor: '#03146f',
            secondaryCrystalStroke: '#7195a9',
            stroke: '#6a1500'
     }

     

     const config2 = generatePalette();
     const config3 = generatePalette();
     const config4 = generatePalette();
     const config5 = generatePalette();

    
    

    heroesSetup.set(1, config);
    heroesSetup.set(2, config2);

    heroesSetup.set(3, config3);

    heroesSetup.set(4, config4);
    heroesSetup.set(5, config5);

    

    
}

function getRandomHexColor(): string {
  const randomColor = Math.floor(Math.random() * 0xffffff);
  return `#${randomColor.toString(16).padStart(6, '0')}`;
}
