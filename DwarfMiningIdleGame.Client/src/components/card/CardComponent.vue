<template>
  <div
    ref="cardRef"
    class="tilt-card"
    @mousemove="handleMouseMove"
    @mouseleave="handleMouseLeave"
    :style="cardStyle"
  >
    <!-- Внутренний контейнер для эффекта глубины -->
    <div class="tilt-card-content">
      <slot>
        <h2>Dwarf Mining</h2>
        <p>Наведи курсор</p>
      </slot>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';

const cardRef = ref<HTMLElement | null>(null);

const rotateX = ref(0);
const rotateY = ref(0);

// Максимальный угол наклона в градусах
const maxTilt = 15; 

const handleMouseMove = (event: MouseEvent) => {
  if (!cardRef.value) return;

  const rect = cardRef.value.getBoundingClientRect();

  // Координаты мыши относительно левого верхнего угла карточки
  const x = event.clientX - rect.left;
  const y = event.clientY - rect.top;

  // Находим центр карточки
  const centerX = rect.width / 2;
  const centerY = rect.height / 2;

  // Вычисляем процент отклонения курсора от центра (от -1 до 1)
  const percentX = (x - centerX) / centerX;
  const percentY = (y - centerY) / centerY;

  // По оси Y крутим в зависимости от X (движение курсора влево-вправо)
  rotateY.value = percentX * maxTilt;
  // По оси X крутим в зависимости от Y (движение курсора вверх-вниз)
  // Знак минус нужен, чтобы верхняя часть карточки наклонялась к зрителю, когда курсор сверху
  rotateX.value = -percentY * maxTilt;
};

const handleMouseLeave = () => {
  // Плавно возвращаем карточку в исходное положение при уходе курсора
  rotateX.value = 0;
  rotateY.value = 0;
};

const cardStyle = computed(() => ({
  transform: `perspective(1000px) rotateX(${rotateX.value}deg) rotateY(${rotateY.value}deg)`,
  // Меняем transition динамически: 
  // Быстрый отклик при движении, плавная анимация возврата (когда координаты 0)
  transition: (rotateX.value === 0 && rotateY.value === 0)
    ? 'transform 0.5s cubic-bezier(0.23, 1, 0.32, 1)'
    : 'transform 0.1s linear'
}));
</script>

<style scoped>
.tilt-card {
  width: 200px;
  height: 400px;
  background: linear-gradient(135deg, #2b2b2b, #1a1a1a);
  border-radius: 16px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.4);
  border: 1px solid #3a3a3a;
  
  /* Критично для сохранения 3D-эффекта дочерних элементов */
  transform-style: preserve-3d;
  will-change: transform;
  
  /* Центрирование контента */
  display: flex;
  align-items: center;
  justify-content: center;
}

.tilt-card-content {
  color: #e0e0e0;
  text-align: center;
  pointer-events: none; /* Чтобы контент не перехватывал события мыши */
  
  /* Выдвигаем контент ближе к зрителю по оси Z */
  /* За счет transform-style: preserve-3d у родителя это создаст крутой параллакс */
  transform: translateZ(60px);
}
</style>