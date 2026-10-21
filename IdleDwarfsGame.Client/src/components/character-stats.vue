<script lang="ts" setup>
import { computed, defineProps, ref, watch } from 'vue';

const props = defineProps<{
  currentHp: number;
  maxHp: number;
  attack?: number;
  armor?: number;
}>();



const hpFillStyle = computed(() => ({
    width: `${(props.currentHp/props.maxHp * 100)}%`
}) )




</script>

<style lang="scss">
.hp{
    background: #763b53;
    height: 18px;
    color: whitesmoke;
    position: relative;
    border-radius: 5px;
    border: 1px solid #552d2c;
    &-fill{
        position: absolute;
        background: linear-gradient(45deg, transparent, #ff1d00);    
        border-radius: 5px;
        height: 100%;
        left: 0;
        bottom: 0;
        transition: width 0.8s;
    }
    &-numbers{
        position: absolute;
        background: transparent;    
        border-radius: 5px;
        line-height: 20px;
        font-weight: 600;
        width: 100%;
        height: 100%;
        left: 0;
        bottom: 0;
        text-align: center;
    }
}


</style>

<template>
    <div>
        <div class="icons">
            <div class="attack">{{ attack }}</div>
            <div class="armor">{{ armor }}</div>
        </div>
        <div class="hp">       
            <div class="hp-fill" :style="hpFillStyle"></div>
            <div class="hp-numbers">{{ currentHp }}</div>
        </div>
    </div>   
</template>