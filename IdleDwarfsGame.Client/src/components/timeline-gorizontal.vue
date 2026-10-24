<script lang="ts" setup>
import { computed, onMounted, ref } from 'vue';

const seconds = ref(172805);

const timeleft = computed<string>(() => formatTime(seconds.value))

onMounted( () => {
    startCountdown();
})



function formatTime(seconds) {
  const days = Math.floor(seconds / (3600 * 24));
  seconds %= 3600 * 24; // Remaining seconds after removing full days
  
  const hours = Math.floor(seconds / 3600);
  seconds %= 3600; // Remaining seconds after removing full hours
  
  const minutes = Math.floor(seconds / 60);
  seconds %= 60; // Remaining seconds after removing full minutes
  
  const remainingSeconds = seconds; // The leftover seconds

  // Format the output string
  return `${days}d : ${String(hours).padStart(2, '0')}h : ${String(minutes).padStart(2, '0')}m : ${String(remainingSeconds).padStart(2, '0')}s`;
}

function startCountdown(): void {
  // This will run every second (1000 milliseconds)
  const intervalId = setInterval(() => {
    if (seconds.value <= 0) {
      clearInterval(intervalId); // Stop the countdown when seconds reach 0
    } else {
      seconds.value--; // Update the reactive reference
    }
  }, 1000); // 1000ms = 1 second
}
</script>


<template>
    <div class="timeline">{{ timeleft }}{{ seconds }}</div>
</template>


<style lang="scss">
.timeline {
    width: 100px;
    height: 40px;
}
</style>