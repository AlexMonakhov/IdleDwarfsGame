// Вырезание однотонного фона (белого, серого или хромакея) вокруг персонажа с тёмным контуром.
// Общее для спрайт-листов (sprite-sheets.js) и иконок из портретов (portraits.js)

const lum = (d, i) => 0.299 * d[i] + 0.587 * d[i + 1] + 0.114 * d[i + 2];
const sat = (d, i) => Math.max(d[i], d[i + 1], d[i + 2]) - Math.min(d[i], d[i + 1], d[i + 2]);

// Хромакей (зелёный/синий фон): насколько ключевой канал фона выше остальных.
// Для пикселя тот же перевес даёт прозрачность; light-пиксели (белая борода, меч) почти не теряют непрозрачности,
// в отличие от color-to-alpha, который вычитает из них зелёный и красит в розовый
function chromaKey(paper) {
  const key = paper.indexOf(Math.max(...paper));
  const others = [0, 1, 2].filter(c => c !== key);
  const excess = (d, i) => d[i + key] - Math.max(d[i + others[0]], d[i + others[1]]);
  const full = excess(paper, 0);
  const lo = full * 0.2, hi = full * 0.8;
  return {
    alpha: (d, i) => Math.min(1, Math.max(0, (hi - excess(d, i)) / (hi - lo))),
    // Цвет без примеси фона (край контура — тёмный, а не светлая кайма), затем убрать отсвет фона:
    // ключевой канал не выше самого яркого из остальных
    unmix: (d, i, a, out) => {
      for (let c = 0; c < 3; c++) out[i + c] = Math.min(255, Math.max(0, (d[i + c] - (1 - a) * paper[c]) / a));
      out[i + key] = Math.min(out[i + key], Math.max(out[i + others[0]], out[i + others[1]]));
    },
  };
}

// Цвет бумаги — медиана по краю кадра
function paperColor(png) {
  const { width: W, height: H, data: d } = png;
  const ch = [[], [], []];
  const add = p => { for (let c = 0; c < 3; c++) ch[c].push(d[p * 4 + c]); };
  for (let x = 0; x < W; x++) { add(x); add((H - 1) * W + x); }
  for (let y = 0; y < H; y++) { add(y * W); add(y * W + W - 1); }
  return ch.map(v => v.sort((a, b) => a - b)[v.length >> 1]);
}

// Квадратная дилатация маски на r пикселей (два прохода)
function dilate(mask, W, H, r) {
  const tmp = new Uint8Array(mask.length);
  const out = new Uint8Array(mask.length);
  for (let y = 0; y < H; y++) {
    let last = -Infinity;
    for (let x = 0; x < W; x++) { if (mask[y * W + x]) last = x; if (x - last <= r) tmp[y * W + x] = 1; }
    last = Infinity;
    for (let x = W - 1; x >= 0; x--) { if (mask[y * W + x]) last = x; if (last - x <= r) tmp[y * W + x] = 1; }
  }
  for (let x = 0; x < W; x++) {
    let last = -Infinity;
    for (let y = 0; y < H; y++) { if (tmp[y * W + x]) last = y; if (y - last <= r) out[y * W + x] = 1; }
    last = Infinity;
    for (let y = H - 1; y >= 0; y--) { if (tmp[y * W + x]) last = y; if (last - y <= r) out[y * W + x] = 1; }
  }
  return out;
}

function floodFill(seeds, passable, W, H) {
  const filled = new Uint8Array(W * H);
  const stack = [];
  const push = p => { if (!filled[p] && passable(p)) { filled[p] = 1; stack.push(p); } };
  seeds.forEach(push);
  while (stack.length) {
    const p = stack.pop(), x = p % W;
    if (x > 0) push(p - 1);
    if (x < W - 1) push(p + 1);
    if (p >= W) push(p - W);
    if (p < W * (H - 1)) push(p + W);
  }
  return filled;
}

// Всё, что обведено тёмным контуром персонажа, остаётся непрозрачным (белая борода не пропадёт).
// Снаружи контура фон убирается через color-to-alpha от цвета бумаги: шлейфы и свечение атак
// становятся полупрозрачными, а не исчезают целиком. Фон может быть любым однотонным:
// на сером светлее фона — свет, темнее — тень (на белом светлые эффекты неотличимы от бумаги).
// Цветной фон (хромакей) вырезается по перевесу ключевого канала, см. chromaKey.
// lightOutline — контуром считается и светлая обводка, лишь бы заметно темнее фона (золотой меч, серые грани
// кристаллов короны); только для анимаций без эффектов: у атак так обвелись бы шлейфы и стали непрозрачными целиком.
// Результат — RGBA с премультиплицированным цветом
function cutout(png, paper, lightOutline) {
  const { width: W, height: H, data: d } = png;
  const n = W * H;
  const s = W / 768;
  const R = Math.max(1, Math.round(2 * s));
  const paperLum = 0.299 * paper[0] + 0.587 * paper[1] + 0.114 * paper[2];
  const whitePaper = paperLum > 230;
  const chroma = sat(paper, 0) > 60 ? chromaKey(paper) : null;

  const dark = new Uint8Array(n);
  // Контур после сжатия видео светлее чёрного; на сером фоне порог — заметно темнее бумаги
  const darkBelow = Math.min(130, paperLum - 35);
  for (let p = 0; p < n; p++) {
    const i = p * 4, l = lum(d, i);
    // На сером фоне белые кристаллы сами служат обводкой: серые грани между ними иначе не отличить от фона
    if (l < darkBelow || (lightOutline && (l < paperLum - 60 || (!whitePaper && l > paperLum + 60)))) dark[p] = 1;
  }
  // На цветном фоне тёмное, но с цветом фона (тень от сжатия по краю кадра), — не контур
  if (chroma) for (let p = 0; p < n; p++) if (dark[p] && chroma.alpha(d, p * 4) < 0.5) dark[p] = 0;
  // запечатываем мелкие разрывы контура
  const sealed = dilate(dark, W, H, R);

  const border = [];
  for (let x = 0; x < W; x++) border.push(x, (H - 1) * W + x);
  for (let y = 0; y < H; y++) border.push(y * W, y * W + W - 1);
  const outside = floodFill(border, p => !sealed[p], W, H);

  // Дыры внутри силуэта (между рукой и мечом и т.п.) — крупные пятна чистой бумаги
  const paperLike = p => {
    const i = p * 4;
    if (outside[p] || sealed[p]) return false;
    if (chroma) return chroma.alpha(d, i) < 0.1;
    // Светлее белой бумаги ничего не бывает; на сером фоне светлее — это уже персонаж (белые кристаллы)
    const near = whitePaper ? (v, c) => d[i + c] >= v - 6 : (v, c) => Math.abs(d[i + c] - v) <= 12;
    return paper.every(near) && sat(d, i) <= 8;
  };
  // Просвет обведён тёмным контуром почти целиком, а белая грань кристалла (борода, меч) — светлыми линиями граней
  const seen = new Uint8Array(n);
  const minHole = 150 * s * s;
  for (let p = 0; p < n; p++) {
    if (seen[p] || !paperLike(p)) continue;
    seen[p] = 1;
    const pts = [p];
    let edge = 0, edgeDark = 0;
    for (let j = 0; j < pts.length; j++) {
      const q = pts[j], x = q % W;
      for (const nb of [x > 0 ? q - 1 : -1, x < W - 1 ? q + 1 : -1, q - W, q + W]) {
        if (nb < 0 || nb >= n || seen[nb]) continue;
        if (paperLike(nb)) { seen[nb] = 1; pts.push(nb); } else { edge++; if (sealed[nb]) edgeDark++; }
      }
    }
    if (pts.length >= minHole && (chroma || edgeDark >= edge * 0.7)) pts.forEach(q => { outside[q] = 1; });
  }

  // Полоса запечатки со стороны фона тоже прозрачная, иначе вокруг контура останется белая кайма
  const nearOutside = dilate(outside, W, H, R + 1);
  const out = new Uint8ClampedArray(n * 4);
  const DEAD = 8;
  for (let p = 0; p < n; p++) {
    const i = p * 4;
    const inside = !outside[p] && !(sealed[p] && !dark[p] && nearOutside[p]);
    if (chroma) {
      const a = inside ? 1 : chroma.alpha(d, i);
      if (a <= 0) continue;
      chroma.unmix(d, i, a, out);
      for (let c = 0; c < 3; c++) out[i + c] *= a;
      out[i + 3] = a * 255;
      continue;
    }
    if (inside) {
      out[i] = d[i]; out[i + 1] = d[i + 1]; out[i + 2] = d[i + 2]; out[i + 3] = 255;
      continue;
    }
    // Минимальная непрозрачность, при которой цвет пикселя получается смешением бумаги с каким-то цветом 0..255.
    // Светлее белой бумаги — только шум сжатия, его не трогаем
    let a = 0;
    for (let c = 0; c < 3; c++) {
      const delta = d[i + c] - paper[c];
      const range = delta < 0 ? paper[c] : 255 - paper[c];
      if (range > DEAD * 2) a = Math.max(a, Math.max(0, Math.abs(delta) - DEAD) / (range - DEAD));
    }
    // На сером фоне даже яркие цвета дают a ~0.8: всё заметно отличное от бумаги — непрозрачное
    // (борода там, где контур сливается со шлейфом), плавный спад остаётся только у краёв свечения
    a = Math.min(1, whitePaper ? a : a / 0.6);
    if (a <= 0) continue;
    // На белой бумаге светлое восстановить нельзя, поэтому серое (тени, сглаживание контура) вычитаем из бумаги —
    // остаётся тёмным, а насыщенное (шлейфы, свечение) оставляем светлым, как свет: иначе на тёмном фоне боя выйдет грязно-синим
    const glow = whitePaper ? Math.min(1, Math.max(0, (sat(d, i) - 12) / 40)) : 0;
    const alpha = Math.min(1, a * (1 + 1.5 * glow));
    for (let c = 0; c < 3; c++) {
      const ink = Math.min(255, Math.max(0, d[i + c] - (1 - a) * paper[c]) / a);
      out[i + c] = (ink * (1 - glow) + d[i + c] * glow) * alpha;
    }
    out[i + 3] = alpha * 255;
  }
  return { W, H, px: out };
}

module.exports = { paperColor, cutout };
