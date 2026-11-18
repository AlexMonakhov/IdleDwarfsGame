import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import i18n from './i18n'
import { vTooltip } from './directives/tooltip'
import { vTutorial } from './tutorial/anchors'
import { installUiScale } from './composables/UseUiScale'
import { installUiSounds } from './services/soundService/uiSounds'

installUiScale()
installUiSounds()

createApp(App).use(router).use(i18n).directive('tooltip', vTooltip).directive('tutorial', vTutorial).mount('#app')
