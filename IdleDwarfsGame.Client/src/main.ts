import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import i18n from './i18n'
import { vTooltip } from './directives/tooltip'
import { installUiScale } from './composables/UseUiScale'

installUiScale()

createApp(App).use(router).use(i18n).directive('tooltip', vTooltip).mount('#app')
