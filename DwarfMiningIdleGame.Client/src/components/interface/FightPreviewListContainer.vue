<script lang="ts" setup>
import FightPreviewList from "@/components/interface/FightPreviewList.vue";
import IconButton from "../common/IconButton.vue";
import IconHelm from "../icons/IconHelm.vue";
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
                <template #list-end><IconButton inner><IconHelm size="60px" primary-color="rgb(217 215 178)" secondary-color="rgb(64 255 251)"></IconHelm></IconButton></template>
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
