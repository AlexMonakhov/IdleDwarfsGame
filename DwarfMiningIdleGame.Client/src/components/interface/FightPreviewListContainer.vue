<script lang="ts" setup>
import FightPreviewList from "@/components/interface/FightPreviewList.vue";
import IconButton from "../common/IconButton.vue";
import IconHelm from "../icons/IconHelm.vue";
import CanvasComponent from "../canvas/CanvasComponent.vue";
import CanvasLayer from "../canvas/CanvasLayer.vue";
interface Props {
    count?: number;
    height?: string;
}

const props = withDefaults(defineProps<Props>(), {
  count: 0,
  height: '30%'
})
</script>

<template>
    <ul class="list-wrapper">
        <li v-for="n in props.count" :key="n" class="list" :style="{height: props.height}">
            
            <FightPreviewList :stage="n">
                
                <template #list-start>{{ n }}</template>
                <template #list-middle>hue</template>
                <template #list-end>
                    <CanvasLayer :page="false">
                    <CanvasComponent 
                    :effects="[{ func: 'god-rays-radiance', padding: 40, isUnder: true },{ func: 'particles', padding: 100, isUnder: false }]"
                    >
                        <IconButton inner><IconHelm size="60px" primary-color="rgb(217 215 178)" secondary-color="rgb(64 255 251)"></IconHelm></IconButton>
                    </CanvasComponent>
                    </CanvasLayer>
                </template>
            </FightPreviewList>
            
        </li>
    </ul>
</template>

<style scoped>
.list-wrapper{
    width:100%;
    padding:0;
}

.list{
    list-style: none;
    margin-bottom: 25px;
}

.button{
    width: 80%;
    height: 80%;
    background: #2d2177;
    border: none;
    border-radius: 10px;
    cursor: pointer;
}
</style>
