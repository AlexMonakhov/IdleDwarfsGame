import { computed } from 'vue';
// import { useInventoryStore, ItemType } from '@/stores/inventoryStore';

export function useInventoryBadge(type: 'helm' | 'armor') {
//   const store = useInventoryStore();

  //const count = computed(() => store.getNewCountByType(type));
  const count = computed(() => {
    if(type === 'helm') return 11;
    if(type === 'armor') return 100;
     return 0;
  })


  return {
    count
  };
}