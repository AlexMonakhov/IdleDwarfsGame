import type { Ref } from 'vue'
import { useInterface } from '@/composables/UseInterface'
import { useGuild } from '@/composables/UseGuild'
import { usePvp } from '@/composables/UsePvp'
import { useSocial } from '@/composables/UseSocial'
import type { TutorialModalId } from './types'

// Флаги открытия окон разделов: по ним сценарий сам открывает нужное окно (действие openModal)
const {
  inventoryOpen, upgradeOpen, forgeOpen, shopOpen, miningOpen, expeditionsOpen, questsOpen, eventOpen,
  mailOpen, collectionOpen, campaignOpen, towerOpen
} = useInterface()
const { guildOpen } = useGuild()
const { pvpOpen } = usePvp()
const { friendsOpen, delveOpen } = useSocial()

export const MODAL_FLAGS: Record<TutorialModalId, Ref<boolean>> = {
  campaign: campaignOpen,
  tower: towerOpen,
  mail: mailOpen,
  collection: collectionOpen,
  warehouse: inventoryOpen,
  altar: upgradeOpen,
  forge: forgeOpen,
  shop: shopOpen,
  mine: miningOpen,
  expeditions: expeditionsOpen,
  quests: questsOpen,
  event: eventOpen,
  pvp: pvpOpen,
  guild: guildOpen,
  friends: friendsOpen,
  delve: delveOpen,
}
