<script lang="ts" setup>
import { onMounted, onBeforeUnmount, ref, getCurrentInstance, reactive, defineProps, watch, defineExpose, computed } from 'vue';

const props = defineProps<{
    loading?: boolean 
}>();

const onClick = (event: any) => {
    const ripple = document.createElement('span');
    ripple.classList.add('m-ripple--');
    ripple.style.left = `${event.offsetX}px`;
    ripple.style.top = `${event.offsetY}px`;
    event.currentTarget.append(ripple);
    let but = event.currentTarget; 
    setTimeout(() => {
        but.removeChild(ripple); 
    }, 1000)
}
</script>

<style lang="scss">

.m-button--{
    margin:3px;
    position: relative;
    height: 35px;
    width:auto;
    min-width: fit-content;
    z-index:10;
    overflow: hidden;
    transition: background-color 0.3s linear;
}

.m-button--default-styles-{
    border-radius: 6px;
    border:none;
    outline: none;
    padding: 0 1.5em 0 1.5em;
    background-color: var(--normal);
    color: var(--normal-text);
}

.m-primary--{
    background-color: var(--primary) !important;
    color: var(--primary-text) !important;
}

.m-secondary--{
    background-color: var(--secondary) !important;
    color: var(--primary-text) !important;
}

.m-error--{
    background-color: var(--error) !important;
    color: var(--primary-text) !important;
}

.m-warning--{
    background-color: var(--warning) !important;
    color: var(--primary-text) !important;
}

.m-button--content-{
    font-size: 13px;
    font-weight: 500;
    font-family: 'Roboto', sans-serif;
    letter-spacing: 0.09987em;
    text-transform: uppercase;
    pointer-events: none;
}

.m-button--default-shadow-{
    box-shadow: 1px 1px 5px 0.5px #ccc;
}

.m-button--:hover{
    filter: brightness(107%);
}

.m-button--:active{
    filter: brightness(105%);
    box-shadow:  0.7px 0.7px 2px #999 inset;
}

.m-button--:disabled{
    background-color: #ccc !important;
    box-shadow: none;
}

.m-loader--{
    position: absolute;
    height: 100%;
    width: 100%;
    left:0;
    top:0;
    background-color: inherit;
    border-radius: inherit;
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;  
    z-index: 9999;
}


.m-loader--default- .m-loader-dot-{
    height: 50%;
    background-color:inherit;
    aspect-ratio: 1 / 1;
    border-radius: 50%;
    margin: 0 2% 0 2%;
    
}

.m-loader--default- .m-loader-dot-::before{
    position: absolute;
    content: "";
    filter: inherit;
    left:0;
    top:0;
    width: 100%;
    height: 100%;
    background: inherit;
    border-radius: inherit;
    animation: wave 2s ease-out infinite;
}

.m-loader--default- .m-loader-dot-:nth-child(1)::before{
    animation-delay: 0.1s;
}

.m-loader--default- .m-loader-dot-:nth-child(2)::before{
    animation-delay: 0.2s;
}

.m-loader--default- .m-loader-dot-:nth-child(3)::before{
    animation-delay: 0.3s;
}

.m-loader--default- .m-loader-dot-:nth-child(4)::before{
    animation-delay: 0.4s;
}

.m-loader--default- .m-loader-dot-:nth-child(4){
    filter: brightness(90%);
}

.m-loader--default- .m-loader-dot-:nth-child(3){
    filter: brightness(95%);
}

.m-loader--default- .m-loader-dot-:nth-child(2){
    filter: brightness(110%);
}

.m-loader--default- .m-loader-dot-:nth-child(1){
    filter: brightness(120%);
}

@keyframes wave {
    50%,
    75% {
      transform: scale(2.5);
    }
    80%,
    100% {
      opacity: 0;
    }
  }

.m-ripple--{
    position: absolute;
    width: 10px;
    height: 10px;
    border-radius: 50%;
    background: inherit;
    filter: brightness(3);
    translate: -50% -50%;
    opacity: 0.2;
    animation: ripple 1s forwards;
    overflow: hidden;
    pointer-events: none;
}

@keyframes ripple {
    0%{
       opacity: 0.2; 
    }
    50%,
    75% {
      transform: scale(20);
    }
    80%,
    100% {
      opacity: 0;
    }
  }
</style>

<template>
    <button 
        class="m-button-- m-button--default-styles- m-primary--" type="button" @click="onClick">
        <span class="m-button--content-">
            <slot></slot>
        </span> 
        <span v-if="props.loading" class="m-loader-- m-loader--default-">
                <span class="m-loader-dot-"></span>
                <span class="m-loader-dot-"></span>
                <span class="m-loader-dot-"></span>
                <span class="m-loader-dot-"></span>
        </span>    
    </button>
</template>