import { type ObjectDirective, shallowReactive } from 'vue'

// Реестр якорей обучения: v-tutorial="'warehouse.tab.chests'" отмечает элемент, шаг сценария ссылается на него
// по имени. Селекторы ломались бы при каждой правке вёрстки, имя — нет.
// Под одним именем может быть несколько элементов — тогда якорь это все видимые из них вместе (здание города:
// картинка и табличка). Невидимые не в счёт: KeepAlive держит скрытую вкладку смонтированной, только вынимает
// её из документа
const anchors = shallowReactive(new Map<string, Set<HTMLElement>>())

const register = (id: string, el: HTMLElement) => {
  const set = anchors.get(id) ?? new Set<HTMLElement>()
  set.add(el)
  anchors.set(id, set)
}

const unregister = (id: string, el: HTMLElement) => {
  const set = anchors.get(id)
  if (!set) return
  set.delete(el)
  if (!set.size) anchors.delete(id)
}

const isVisible = (el: HTMLElement) => {
  if (!el.isConnected) return false
  const rect = el.getBoundingClientRect()
  return rect.width > 0 && rect.height > 0
}

export const findAnchors = (id: string): HTMLElement[] => [...anchors.get(id) ?? []].filter(isVisible)

export const findAnchor = (id: string): HTMLElement | null => findAnchors(id)[0] ?? null

// Видимая часть элемента: его рамка, обрезанная прокручиваемыми и обрезающими родителями (список в окне)
// и самим окном браузера
export const visibleRect = (el: HTMLElement): DOMRect => {
  const rect = el.getBoundingClientRect()
  let left = Math.max(rect.left, 0)
  let top = Math.max(rect.top, 0)
  let right = Math.min(rect.right, window.innerWidth)
  let bottom = Math.min(rect.bottom, window.innerHeight)
  for (let parent = el.parentElement; parent && parent !== document.body; parent = parent.parentElement) {
    const style = getComputedStyle(parent)
    if (style.overflowX === 'visible' && style.overflowY === 'visible') continue
    const clip = parent.getBoundingClientRect()
    left = Math.max(left, clip.left)
    top = Math.max(top, clip.top)
    right = Math.min(right, clip.right)
    bottom = Math.min(bottom, clip.bottom)
  }
  return new DOMRect(left, top, Math.max(0, right - left), Math.max(0, bottom - top))
}

// Элемент обрезан (часть за краем списка или окна) — его стоит прокрутить в видимую зону
export const isClipped = (el: HTMLElement) => {
  const full = el.getBoundingClientRect()
  const shown = visibleRect(el)
  return shown.width * shown.height < full.width * full.height - 1
}

// Докрутить якорь в видимую зону: по горизонтали — к центру, по вертикали — ровно настолько, чтобы влез.
// Двигаем только настоящие прокрутки (overflow auto/scroll) — scrollIntoView докручивал и контейнеры с overflow: hidden:
// башня у правого края города не может встать по центру, и остаток уходил в .ui-stage — вся сцена уезжала влево.
// scrollTo сам упирается в границы прокрутки
const isScroller = (style: CSSStyleDeclaration, axis: 'x' | 'y') =>
  /(auto|scroll)/.test(axis === 'x' ? style.overflowX : style.overflowY)

// Каждую ось двигает только ближайшая прокрутка: внешняя считала бы по старому положению, пока внутренняя ещё едет
export const scrollAnchorIntoView = (el: HTMLElement) => {
  let doneX = false
  let doneY = false
  for (let parent = el.parentElement; parent && parent !== document.body; parent = parent.parentElement) {
    const style = getComputedStyle(parent)
    const canX: boolean = !doneX && isScroller(style, 'x') && parent.scrollWidth > parent.clientWidth
    const canY: boolean = !doneY && isScroller(style, 'y') && parent.scrollHeight > parent.clientHeight
    if (!canX && !canY) continue
    doneX ||= canX
    doneY ||= canY
    const box = parent.getBoundingClientRect()
    const rect = el.getBoundingClientRect()
    // Прокрутка в пикселях контейнера, а rect — в пикселях окна: учитываем масштаб интерфейса
    const scale = box.width / (parent.offsetWidth || box.width) || 1
    let left = parent.scrollLeft
    let top = parent.scrollTop
    if (canX) left += (rect.left + rect.width / 2 - (box.left + box.width / 2)) / scale
    if (canY) {
      if (rect.top < box.top) top -= (box.top - rect.top) / scale
      else if (rect.bottom > box.bottom) top += (rect.bottom - box.bottom) / scale
    }
    parent.scrollTo({ left, top, behavior: 'smooth' })
    if (doneX && doneY) return
  }
}

// Общая рамка видимых частей всех элементов якоря (в пикселях окна); null — якоря на экране нет
export const anchorRect = (id: string): DOMRect | null => {
  const rects = findAnchors(id).map(visibleRect).filter(r => r.width > 0 && r.height > 0)
  if (!rects.length) return null
  const left = Math.min(...rects.map(r => r.left))
  const top = Math.min(...rects.map(r => r.top))
  const right = Math.max(...rects.map(r => r.right))
  const bottom = Math.max(...rects.map(r => r.bottom))
  return new DOMRect(left, top, right - left, bottom - top)
}

// Клик пришёлся в якорь (в любой его элемент)
export const isInAnchor = (id: string, node: Node) => findAnchors(id).some(el => el.contains(node))

// Имя якоря держим на элементе, чтобы при смене значения снять старое
const ANCHOR_KEY = Symbol('tutorialAnchor')
type AnchoredElement = HTMLElement & { [ANCHOR_KEY]?: string }

const bind = (el: AnchoredElement, id: string | null | undefined) => {
  const previous = el[ANCHOR_KEY]
  if (previous === (id ?? undefined)) return
  if (previous) unregister(previous, el)
  el[ANCHOR_KEY] = id ?? undefined
  if (id) register(id, el)
}

// Пустое значение — якоря нет (удобно для общих компонентов вроде ModalComponent)
export const vTutorial: ObjectDirective<HTMLElement, string | null | undefined> = {
  mounted: (el, { value }) => bind(el, value),
  updated: (el, { value }) => bind(el, value),
  unmounted: el => bind(el, null),
}
