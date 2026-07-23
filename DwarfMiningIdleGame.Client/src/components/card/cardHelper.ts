export interface OverlayBlock {
  id: number;
  left: number;
  top: number;
  width: number;
  height: number;
  opacity?: number;
}

export function generateOverlaySets(baseCount: number): [OverlayBlock[], OverlayBlock[], OverlayBlock[]] {
  
  // Вспомогательная функция для дробления. 
  // Принимает текущий массив блоков и продолжает его резать до нужного количества.
  const splitBlocks = (initialBlocks: OverlayBlock[], targetCount: number, startId: number) => {
    // Копируем массив, чтобы не сломать предыдущий уровень, но сохраняем старые id
    const blocks = initialBlocks.map(b => ({ ...b }));
    let nextId = startId;

    while (blocks.length < targetCount) {
      let largestIndex = 0;
      let maxArea = 0;
      
      blocks.forEach((b, index) => {
        const area = b.width * b.height;
        if (area > maxArea) {
          maxArea = area;
          largestIndex = index;
        }
      });

      const target = blocks[largestIndex];
      blocks.splice(largestIndex, 1); // Удаляем разрезаемый блок

      const splitVertically = target.width > target.height;
      const splitRatio = 0.3 + (Math.random() * 0.4); 

      if (splitVertically) {
        const w1 = target.width * splitRatio;
        const w2 = target.width - w1;
        blocks.push({ id: nextId++, left: target.left, top: target.top, width: w1, height: target.height, });
        blocks.push({ id: nextId++, left: target.left + w1, top: target.top, width: w2, height: target.height, });
      } else {
        const h1 = target.height * splitRatio;
        const h2 = target.height - h1;
        blocks.push({ id: nextId++, left: target.left, top: target.top, width: target.width, height: h1,  });
        blocks.push({ id: nextId++, left: target.left, top: target.top + h1, width: target.width, height: h2,  });
      }
    }
    
    return { blocks, nextId };
  };

  // Стартовая точка: 1 сплошной блок
  const initial: OverlayBlock[] = [{ id: 1, left: 0, top: 0, width: 100, height: 100,}];
  // Генерируем 1-й массив (например, 5 блоков)
  const step1 = splitBlocks(initial, baseCount, 2);

  // Генерируем 2-й массив (например, 10 блоков), продолжая резать 1-й
  const step2 = splitBlocks(step1.blocks, baseCount * 2, step1.nextId);

  // Генерируем 3-й массив (например, 15 блоков), продолжая резать 2-й
  const step3 = splitBlocks(step2.blocks, baseCount * 3, step2.nextId);

  // Возвращаем три массива
  return [step1.blocks, step2.blocks, step3.blocks];
}