<template>
  <canvas ref="canvasRef" class="vortex-canvas" />
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

const canvasRef = ref(null)

let canvas
let ctx
let animationId
let time = 0

let width = window.innerWidth
let height = window.innerHeight

const center = {
  x: width / 2,
  y: height / 2
}

const sparkles = []

class Sparkle {
  constructor() {
    this.reset()
  }

  reset() {
    this.angle = Math.random() * Math.PI * 2
    this.radius = 80 + Math.random() * 350
    this.size = 1 + Math.random() * 3
    this.speed = 0.002 + Math.random() * 0.005
    this.alpha = 0.3 + Math.random() * 0.7
  }

  update() {
    this.angle += this.speed
  }

  draw() {
    const x = center.x + Math.cos(this.angle) * this.radius
    const y = center.y + Math.sin(this.angle) * this.radius

    ctx.beginPath()
    ctx.fillStyle = `rgba(255,255,255,${this.alpha})`
    ctx.shadowBlur = 15
    ctx.shadowColor = '#aaf6ff'
    ctx.arc(x, y, this.size, 0, Math.PI * 2)
    ctx.fill()
  }
}

function createSparkles() {
  sparkles.length = 0
  for (let i = 0; i < 80; i++) {
    sparkles.push(new Sparkle())
  }
}

function drawGlow() {
  const gradient = ctx.createRadialGradient(
    center.x,
    center.y,
    100,
    center.x,
    center.y,
    420
  )

  gradient.addColorStop(0, 'rgba(0,0,0,0)')
  gradient.addColorStop(0.4, 'rgba(0,180,255,0.15)')
  gradient.addColorStop(0.7, 'rgba(0,255,255,0.12)')
  gradient.addColorStop(1, 'rgba(0,0,0,0)')

  ctx.beginPath()
  ctx.fillStyle = gradient
  ctx.arc(center.x, center.y, 420, 0, Math.PI * 2)
  ctx.fill()
}

function drawCore() {
  const gradient = ctx.createRadialGradient(
    center.x,
    center.y,
    20,
    center.x,
    center.y,
    170
  )

  gradient.addColorStop(0, '#000')
  gradient.addColorStop(0.35, '#050c35')
  gradient.addColorStop(0.7, '#00dfff')
  gradient.addColorStop(1, 'rgba(0,0,0,0)')

  ctx.beginPath()
  ctx.fillStyle = gradient
  ctx.arc(center.x, center.y, 170, 0, Math.PI * 2)
  ctx.fill()
}

function drawSpiralArm(offset, opacity) {
  ctx.beginPath()

  for (let a = 0; a < Math.PI * 5; a += 0.03) {
    const radius = 40 + a * 45
    const wave = Math.sin(a * 3 + time + offset) * 18

    const x =
      center.x +
      Math.cos(a + time * 0.4 + offset) * (radius + wave)

    const y =
      center.y +
      Math.sin(a + time * 0.4 + offset) * (radius + wave)

    if (a === 0) ctx.moveTo(x, y)
    else ctx.lineTo(x, y)
  }

  ctx.strokeStyle = `rgba(0,220,255,${opacity})`
  ctx.lineWidth = 28
  ctx.lineCap = 'round'
  ctx.shadowBlur = 30
  ctx.shadowColor = '#00dfff'
  ctx.stroke()
}

function drawSpirals() {
  ctx.save()
  ctx.globalCompositeOperation = 'lighter'

  drawSpiralArm(0, 0.08)
  drawSpiralArm(Math.PI * 0.7, 0.06)
  drawSpiralArm(Math.PI * 1.3, 0.05)

  ctx.restore()
}

function animate() {
  ctx.fillStyle = 'rgba(5, 5, 25, 0.08)'
  ctx.fillRect(0, 0, width, height)

  drawGlow()
  drawSpirals()
  drawCore()

  sparkles.forEach((s) => {
    s.update()
    s.draw()
  })

  time += 0.02

  animationId = requestAnimationFrame(animate)
}

function resize() {
  width = window.innerWidth
  height = window.innerHeight

  canvas.width = width
  canvas.height = height

  center.x = width / 2
  center.y = height / 2
}

onMounted(() => {
  canvas = canvasRef.value
  ctx = canvas.getContext('2d')

  resize()
  createSparkles()
  animate()

  window.addEventListener('resize', resize)
})

onUnmounted(() => {
  cancelAnimationFrame(animationId)
  window.removeEventListener('resize', resize)
})
</script>

<style scoped>
.vortex-canvas {
  inset: 0;
  width: 100%;
  height: 100%;
  pointer-events: none;
}
</style>