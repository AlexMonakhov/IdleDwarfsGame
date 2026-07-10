

// export default function generatePalette(baseColor: string) {

//   function hexToHSL(hex: string) {
//     let r = 0, g = 0, b = 0;

//     if (hex.length === 4) {
//       r = parseInt(hex[1] + hex[1], 16);
//       g = parseInt(hex[2] + hex[2], 16);
//       b = parseInt(hex[3] + hex[3], 16);
//     } else if (hex.length === 7) {
//       r = parseInt(hex[1] + hex[2], 16);
//       g = parseInt(hex[3] + hex[4], 16);
//       b = parseInt(hex[5] + hex[6], 16);
//     }

//     r /= 255;
//     g /= 255;
//     b /= 255;

//     const max = Math.max(r, g, b), min = Math.min(r, g, b);
//     let h = 0, s = 0; const l = (max + min) / 2;

//     if (max !== min) {
//       const d = max - min;
//       s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
//       switch (max) {
//         case r: h = ((g - b) / d + (g < b ? 6 : 0)); break;
//         case g: h = ((b - r) / d + 2); break;
//         case b: h = ((r - g) / d + 4); break;
//       }
//       h /= 6;
//     }

//     return { h, s, l };
//   }

//   function hslToHex(h: number, s: number, l: number): string {
//     let r, g, b;

//     const hue2rgb = (p: number, q: number, t: number) => {
//       if (t < 0) t += 1;
//       if (t > 1) t -= 1;
//       if (t < 1 / 6) return p + (q - p) * 6 * t;
//       if (t < 1 / 2) return q;
//       if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
//       return p;
//     };

//     if (s === 0) {
//       r = g = b = l;
//     } else {
//       const q = l < 0.5 ? l * (1 + s) : l + s - l * s;
//       const p = 2 * l - q;
//       r = hue2rgb(p, q, h + 1 / 3);
//       g = hue2rgb(p, q, h);
//       b = hue2rgb(p, q, h - 1 / 3);
//     }

//     const toHex = (x: number) => {
//       const hex = Math.round(x * 255).toString(16);
//       if(hex.length === 1) return '0' + hex;
//       if(Number(hex) < 0) return (Number(hex) * -1).toString();
//       return hex.length === 1 ? '0' + hex : hex;
//     };

//     return `#${toHex(r)}${toHex(g)}${toHex(b)}`;
//   }

//   const base = hexToHSL(baseColor);

//   return {
//     mainColor: hslToHex(base.h, base.s, base.l),
//     secondColor: hslToHex(base.h, base.s * 0.8, base.l * 1.1),
//     thirdColor: '#e28422',
//     accentColor: hslToHex(base.h, base.s * 1.2, base.l * 0.7),
//     faceColor: hslToHex(base.h, base.s * 0.5, base.l * 1.6),
//     skinColor: hslToHex(base.h, base.s * 0.6, base.l * 1.2),

//     crystalDarkColor: hslToHex(base.h, base.s, base.l * 0.4),
//     crystalMediumColor: hslToHex(base.h, base.s, base.l),
//     crystalLightColor: hslToHex(base.h, base.s * 0.8, base.l * 1.2),

//     secondaryCrystalDarkColor: hslToHex((base.h + 0.5) % 1, base.s, base.l * 0.4),
//     secondaryCrystalMediumColor: hslToHex((base.h + 0.5) % 1, base.s, base.l),
//     secondaryCrystalLightColor: hslToHex((base.h + 0.5) % 1, base.s * 0.8, base.l * 1.2),

//     secondaryCrystalStroke: hslToHex((base.h + 0.5) % 1, base.s * 0.4, base.l * 0.8),
//   };
// }




export default function generatePalette(){

    const baseColor = getRandomHexColor();
    const secondaryColor = getRandomHexColor();
    console.log(baseColor);
    console.log(getColorGroupFromHex(baseColor));
    return {
    mainColor: adjustBrightness(baseColor, AdjustTypes.darker),
    secondColor: adjustBrightness(baseColor, AdjustTypes.medium),
    thirdColor: adjustBrightness('#e28422'),
    accentColor: adjustBrightness('#8c3712'),
    faceColor: adjustBrightness('#fd7f56'),
    skinColor: adjustBrightness(baseColor, AdjustTypes.darker),
    stroke: adjustBrightness(baseColor, AdjustTypes.verydarken),

    crystalDarkColor: adjustBrightness(baseColor, AdjustTypes.darker),
    crystalMediumColor: baseColor,
    crystalLightColor: adjustBrightness(baseColor, AdjustTypes.lighter),

    secondaryCrystalDarkColor: adjustBrightness(secondaryColor, AdjustTypes.darker),
    secondaryCrystalMediumColor: adjustBrightness(secondaryColor, AdjustTypes.medium),
    secondaryCrystalLightColor: adjustBrightness(secondaryColor, AdjustTypes.lighter),
    secondaryCrystalStroke: adjustBrightness(secondaryColor, AdjustTypes.lighter),
    
  };
}


function adjustBrightness(baseColor: string, type: AdjustTypes = AdjustTypes.medium) {
  // Удаляем # если есть
  baseColor = baseColor.replace(/^#/, '');
  // Преобразуем HEX в RGB
  const num = parseInt(baseColor, 16);
  let r = (num >> 16) + Math.floor(Math.random() * type + type/5);
  let g = ((num >> 8) & 0x00FF) + Math.floor(Math.random() * type + type/5 );
  let b = (num & 0x0000FF) + Math.floor(Math.random() * type + type/5 );

  // Ограничиваем значения от 0 до 255
  r = Math.min(255, Math.max(0, r));
  g = Math.min(255, Math.max(0, g));
  b = Math.min(255, Math.max(0, b));

  // Возвращаем HEX-строку
  return  '#' + (r << 16 | g << 8 | b).toString(16).padStart(6, '0');

  
}

function getColorGroupFromHex(hex: string) {
  const r = parseInt(hex.slice(1, 3), 16) ;
  const g = parseInt(hex.slice(3, 5), 16) ;
  const b = parseInt(hex.slice(5, 7), 16) ;

  const max = Math.max(r, g, b), min = Math.min(r, g, b);
  let h = 0;
  const d = max - min;

  if (d === 0) {
    h = 0;
  } else {
    switch (max) {
      case r: h = (g - b) / d + (g < b ? 6 : 0); break;
      case g: h = (b - r) / d + 2; break;
      case b: h = (r - g) / d + 4; break;
    }
    h *= 60;
  }

  console.log(`r: ${r},g: ${g},b: ${b}, h: ${h}`)

  // Теперь определим цветовую группу
  if (h < 30 || h >= 330) return 'красный';
  if (h < 50) return 'оранжевый';
  if (h < 65) return 'жёлтый';
  if (h < 155) return 'зелёный';
  if (h < 205) return 'голубой';
  if (h < 250) return 'синий';
  if (h < 330) return 'фиолетовый';
}

function getRandomHexColor(): string {
  const randomColor = Math.floor(Math.random() * 0xffffff);
  return `#${randomColor.toString(16).padStart(6, '0')}`;
}

export enum AdjustTypes{
    lighter = 60,
    darker = -60,
    verydarken = -200,
    medium = 0
}