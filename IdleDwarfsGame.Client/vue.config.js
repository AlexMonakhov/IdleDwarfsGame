const { defineConfig } = require('@vue/cli-service')
module.exports = defineConfig({
  transpileDependencies: true,
  // Флаги сборки vue-i18n (без них в консоли предупреждение): только Composition API
  chainWebpack: config => {
    config.plugin('define').tap(([definitions]) => {
      Object.assign(definitions, {
        __VUE_I18N_FULL_INSTALL__: JSON.stringify(true),
        __VUE_I18N_LEGACY_API__: JSON.stringify(false),
        __INTLIFY_PROD_DEVTOOLS__: JSON.stringify(false),
      })
      return [definitions]
    })
  }
})
