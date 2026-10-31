// Спрайт-листы (атласы), заранее распакованные и уменьшенные до размера на экране.
// Рисовать каждый кадр прямо из файла дорого: браузер не держит все распакованные листы в памяти (4K-лист — 64 МБ)
// и распаковывает их заново посреди боя — на смене анимации анимация дёргается, а при сильном сжатии drawImage
// ещё и теряет детали. Готовая копия (ImageBitmap) распаковывается вне основного потока один раз и рисуется мгновенно.
// Копии общие для всех юнитов: ключ — адрес листа и ширина копии.
// Юнит берёт копию через acquireSheet и отдаёт через releaseSheet. Никому не нужные копии ещё какое-то время
// лежат (повторный бой с теми же героями не грузит их заново), но не больше UNUSED_BUDGET — старые закрываются
export type SheetImage = ImageBitmap | HTMLCanvasElement | HTMLImageElement;

// Сколько памяти могут занимать копии, которые сейчас никто не рисует
const UNUSED_BUDGET = 256 * 1024 * 1024;

interface Entry {
  image: Promise<SheetImage>;
  // Готовая копия (null — ещё грузится)
  ready: SheetImage | null;
  refs: number;
  // Когда копию отдал последний юнит: из ненужных первыми закрываются самые давние
  releasedAt: number;
}

const cache = new Map<string, Entry>();

export const sheetWidth = (image: SheetImage) => (image instanceof HTMLImageElement ? image.naturalWidth : image.width);
export const sheetHeight = (image: SheetImage) => (image instanceof HTMLImageElement ? image.naturalHeight : image.height);

const sheetBytes = (image: SheetImage) => sheetWidth(image) * sheetHeight(image) * 4;

const loadImage = (url: string) => new Promise<HTMLImageElement>((resolve, reject) => {
  const image = new Image();
  image.onload = () => resolve(image);
  image.onerror = reject;
  image.src = url;
});

// Запасной путь, если браузер не умеет createImageBitmap с resize: уменьшение не больше чем вдвое за шаг,
// иначе сглаживание пропускает пиксели и детали теряются
function downscale(source: ImageBitmap | HTMLImageElement, width: number, height: number): HTMLCanvasElement {
  let current: CanvasImageSource = source;
  let w = sheetWidth(source);
  let h = sheetHeight(source);
  let canvas: HTMLCanvasElement;
  do {
    w = Math.max(width, Math.round(w / 2));
    h = Math.max(height, Math.round(h / 2));
    canvas = document.createElement('canvas');
    canvas.width = w;
    canvas.height = h;
    const ctx = canvas.getContext('2d');
    if (!ctx) break;
    ctx.imageSmoothingQuality = 'high';
    ctx.drawImage(current, 0, 0, w, h);
    current = canvas;
  } while (w > width || h > height);
  return canvas;
}

async function scaleSheet(url: string, width: number): Promise<SheetImage> {
  let full: ImageBitmap;
  try {
    // Из файла: картинка распаковывается в фоновом потоке, а не посреди кадра
    full = await createImageBitmap(await (await fetch(url)).blob());
  } catch {
    // Совсем старый браузер — обычная картинка
    const image = await loadImage(url);
    return width >= image.naturalWidth ? image : downscale(image, width, Math.round((width * image.naturalHeight) / image.naturalWidth));
  }
  // Лист и так не больше нужного — увеличивать незачем, браузер растянет его при отрисовке
  if (width >= full.width) return full;

  const height = Math.round((width * full.height) / full.width);
  try {
    const scaled = await createImageBitmap(full, { resizeWidth: width, resizeHeight: height, resizeQuality: 'high' });
    full.close();
    return scaled;
  } catch {
    const scaled = downscale(full, width, height);
    full.close();
    return scaled;
  }
}

// Закрыть самые давние ненужные копии, пока они вместе не влезут в UNUSED_BUDGET
function evictUnused() {
  const unused = [...cache.entries()]
    .filter(([, entry]) => entry.refs === 0)
    .sort(([, a], [, b]) => b.releasedAt - a.releasedAt);
  let bytes = 0;
  for (const [key, { ready }] of unused) {
    if (!ready) continue;
    bytes += sheetBytes(ready);
    if (bytes <= UNUSED_BUDGET) continue;
    cache.delete(key);
    if (ready instanceof ImageBitmap) ready.close();
  }
}

// Копия листа шириной width пикселей экрана. Пока юнит её рисует, она не закроется; потом — releaseSheet
export function acquireSheet(url: string, width: number): Promise<SheetImage> {
  const key = `${url}|${width}`;
  let entry = cache.get(key);
  if (!entry) {
    const created: Entry = { image: scaleSheet(url, width), ready: null, refs: 0, releasedAt: 0 };
    created.image.then(image => {
      created.ready = image;
      evictUnused();
    }, () => {
      // Не загрузился — следующая попытка начнёт заново
      if (cache.get(key) === created) cache.delete(key);
    });
    cache.set(key, created);
    entry = created;
  }
  entry.refs++;
  return entry.image;
}

export function releaseSheet(url: string, width: number) {
  const entry = cache.get(`${url}|${width}`);
  if (!entry || entry.refs === 0) return;
  entry.refs--;
  if (entry.refs === 0) {
    entry.releasedAt = performance.now();
    evictUnused();
  }
}
