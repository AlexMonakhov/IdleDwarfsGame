<template>
  <div class="canvas-wrapper" ref="wrapperRef">
    <canvas ref="canvasRef"></canvas>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'

const wrapperRef = ref(null)
const canvasRef = ref(null)

let animationId
let ctx
let width, height, centerX, centerY
const particles = []



const PARTICLE_COUNT = 200 // Немного уменьшили для оптимизации хвостов
const HOLE_RADIUS = 70
const COLORS = ['#00f0ff', '#9d00ff', '#ffffff']

class Particle {
  constructor() {
    this.history = []
    this.reset(true)
  }

  reset(initial = false) {
    this.angle = Math.random() * Math.PI * 2
    const maxRadius = Math.max(width, height) / 1.5
    this.distance = initial ? (Math.random() * maxRadius) + HOLE_RADIUS : maxRadius
    
    this.spinSpeed = (Math.random() * 0.01) + 0.005
    this.pullSpeed = (Math.random() * 2) + 0.5
    
    this.radius = Math.random() * 1.5 + 0.5
    this.color = COLORS[Math.floor(Math.random() * COLORS.length)]
    
    // Очищаем историю при перерождении (попадании в дыру)
    this.history = [] 
  }

  update() {
    this.angle += this.spinSpeed
    this.distance -= this.pullSpeed

    if (this.distance < HOLE_RADIUS * 3) {
      this.spinSpeed += 0.001
      this.pullSpeed += 0.1
    }

    // Вычисляем текущие координаты (x, y)
    this.x = centerX + Math.cos(this.angle) * this.distance
    this.y = centerY + Math.sin(this.angle) * this.distance

    // Сохраняем текущую позицию в историю
    this.history.push({ x: this.x, y: this.y })

    // Ограничиваем длину хвоста (например, 15 кадров)
    if (this.history.length > 15) {
      this.history.shift()
    }

    if (this.distance < HOLE_RADIUS - 5) {
      this.reset()
    }
  }

  draw() {
    // 1. Рисуем исчезающий хвост
    if (this.history.length > 1) {
      for (let i = 0; i < this.history.length - 1; i++) {
        ctx.beginPath()
        ctx.moveTo(this.history[i].x, this.history[i].y)
        ctx.lineTo(this.history[i + 1].x, this.history[i + 1].y)
        
        // Чем дальше точка в истории, тем прозрачнее линия
        ctx.globalAlpha = i / this.history.length 
        ctx.strokeStyle = this.color
        ctx.lineWidth = this.radius
        ctx.stroke()
      }
      ctx.globalAlpha = 1.0 // Сбрасываем прозрачность для отрисовки головы
    }

    // 2. Рисуем голову частицы
    ctx.beginPath()
    ctx.arc(this.x, this.y, this.radius, 0, Math.PI * 2)
    ctx.fillStyle = this.color
    ctx.shadowBlur = 5
    ctx.shadowColor = this.color
    ctx.fill()
    ctx.shadowBlur = 0
  }
}

const handleResize = () => {
  if (!wrapperRef.value || !canvasRef.value) return
  
  width = canvasRef.value.width = wrapperRef.value.clientWidth
  height = canvasRef.value.height = wrapperRef.value.clientHeight
  centerX = width / 2
  centerY = height / 2
}

function drawGlow() {
  const gradient = ctx.createRadialGradient(
    centerX,
    centerY,
    100,
    centerX,
    centerY,
    420
  )

  gradient.addColorStop(0, 'rgba(0,0,0,0)')
  gradient.addColorStop(0.4, 'rgba(0,180,255,0.15)')
  gradient.addColorStop(0.7, 'rgba(0,255,255,0.12)')
  gradient.addColorStop(1, 'rgba(0,0,0,0)')

  ctx.beginPath()
  ctx.fillStyle = gradient
  ctx.arc(centerX, centerY, 420, 0, Math.PI * 2)
  ctx.fill()
}

function drawCore() {
  const gradient = ctx.createRadialGradient(
    centerX,
    centerY,
    20,
    centerX,
    centerY,
    170
  )

  gradient.addColorStop(0, '#000')
  gradient.addColorStop(0.35, '#050c35')
  gradient.addColorStop(0.7, '#00dfff')
  gradient.addColorStop(1, 'rgba(0,0,0,0)')

  ctx.beginPath()
  ctx.fillStyle = gradient
  ctx.arc(centerX, centerY, 170, 0, Math.PI * 2)
  ctx.fill()
}

const animate = () => {
  // ПОЛНОСТЬЮ очищаем канвас каждый кадр. Больше никаких вечных следов на фоне!
  ctx.clearRect(0, 0, width, height)

  // Рисуем светящийся ореол вихря за черной дырой
  drawCore();

  // Обновляем и рисуем частицы
  particles.forEach(p => {
    p.update()
    p.draw()
  })

  animationId = requestAnimationFrame(animate)
}

onMounted(() => {
  ctx = canvasRef.value.getContext('2d')
  
  window.addEventListener('resize', handleResize)
  handleResize()

  for (let i = 0; i < PARTICLE_COUNT; i++) {
    particles.push(new Particle())
  }

  animate()
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
  cancelAnimationFrame(animationId)
})
</script>

<style scoped>
.canvas-wrapper {
  width: 100%;
  height: 100vh;
  background-color: #0a0514; 
  overflow: hidden;
  display: flex;
  justify-content: center;
  align-items: center;
}

canvas {
  display: block;
}
</style>