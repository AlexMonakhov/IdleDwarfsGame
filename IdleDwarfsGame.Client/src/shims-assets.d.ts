// Картинки из assets в .ts-файлах: webpack отдаёт их URL строкой
declare module '*.png' {
  const url: string
  export default url
}

declare module '*.webp' {
  const url: string
  export default url
}
