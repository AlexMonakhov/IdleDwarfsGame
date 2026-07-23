<template>
  <InterfaceView></InterfaceView>
  <router-view></router-view>
</template>

<script lang="ts" setup>
import GemButton from './components/common/GemButton.vue';
import router from './router';
import InterfaceView from './views/InterfaceView.vue';
</script>

<style lang="less">
#app {
  font-family:  Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  background: #0c5461;
}

@font-face {
  font-family: RunesFont;
  src: url('/public/viking-younger-space-viking-runes-bold.otf.woff2') format('woff2');
  font-weight: bold;
  font-style: normal;
}

:root{
    --normal:#f5f5f5;
    --primary: #18c0c0;
    --secondary: #ec40fc;
    --error: #c44f4f;
    --warning: #e6b940;
    --primary-text: #fff;
    --normal-text: #000;   
    --metalic-background: -webkit-repeating-linear-gradient(left, hsla(0,0%,100%,0) 0%, hsla(0,0%,100%,0)   6%, hsla(0,0%,100%, .1) 7.5%),
                          -webkit-repeating-linear-gradient(left, hsla(0,0%,  0%,0) 0%, hsla(0,0%,  0%,0)   4%, hsla(0,0%,  0%,.03) 4.5%),
                          -webkit-repeating-linear-gradient(left, hsla(0,0%,100%,0) 0%, hsla(0,0%,100%,0) 1.2%, hsla(0,0%,100%,.15) 2.2%),
                          linear-gradient(180deg, hsl(0, 0%, 63%)  0%, hsl(0,0%,90%) 50%, hsl(0,0%,78%) 55%,hsl(0,0%,70%)100%);
    --golden-background:  -webkit-repeating-linear-gradient(left, hsla(0,0%,100%,0) 0%, hsla(0,0%,100%,0)   6%, hsla(0,0%,100%, .1) 7.5%),
                          -webkit-repeating-linear-gradient(left, hsla(0,0%,  0%,0) 0%, hsla(0,0%,  0%,0)   4%, hsla(0,0%,  0%,.03) 4.5%),
                          -webkit-repeating-linear-gradient(left, hsla(0,0%,100%,0) 0%, hsla(0,0%,100%,0) 1.2%, hsla(0,0%,100%,.15) 2.2%),
                          linear-gradient(135deg, #fbe08d 0%, #d89020 40%, #fbe08d 60%, #8b5a19 100%); 
    --wood-background: linear-gradient(to right, #464449 0%, #585556 10%, #464449 20%, #464449 30%, #585556 50%, #464449 60%, #464449 70%, #585556 80%, #464449 90%);
    --rune-text-background: linear-gradient(to bottom, #dae5ff 20%, #2cb1cb 35%, #ffffff 50%, #97c1c9 65%, #083966 100%);
    --metalic-round-background: 
    // -webkit-radial-gradient(  50%   0%,  8% 50%, hsla(0,0%,100%,.5) 0%, hsla(0,0%,100%,0) 100%), -- top blink
    // -webkit-radial-gradient(  50% 100%, 12% 50%, hsla(0,0%,100%,.6) 0%, hsla(0,0%,100%,0) 100%), -- bottom blink
    -webkit-radial-gradient(   0%  50%, 50%  7%, hsla(0,0%,100%,.5) 0%, hsla(0,0%,100%,0) 100%),
    -webkit-radial-gradient( 100%  50%, 50%  5%, hsla(0,0%,100%,.5) 0%, hsla(0,0%,100%,0) 100%),
    
    -webkit-repeating-radial-gradient( 50% 50%, 100% 100%, hsla(0,0%,100%,0) 0%, hsla(0,0%,100%,0)   6%, hsla(0,0%,100%,.1) 7.5%),
    -webkit-repeating-radial-gradient( 50% 50%, 100% 100%, hsla(0,0%,100%,0) 0%, hsla(0,0%,100%,0) 1.2%, hsla(0,0%,100%,.2) 2.2%),
    
    -webkit-radial-gradient( 50% 50%, 200% 50%, hsla(0,0%,90%,1) 5%, hsla(0,0%,85%,1) 30%, hsla(0,0%,60%,1) 100%);
    --blue-gem-color: #2196f3;
    --green-gem-color: #00c853;
    --pink-gem-color: #e91e63;
    --red-gem-color: #d40808;
    --yellow-gem-color: #ffb300;
  }

nav {
  text-align: start;
  a {
    font-weight: bold;
    color: #2c3e50;

    &.router-link-exact-active {
      color: #42b983;
    }
  }
  position: fixed;
  z-index: 1;
  display: flex;
  column-gap: 5px;
  margin: 10px;
}

.main{
  display: flex;
  height: calc(100vh - 20px);
  width: 100%;
  justify-content: center;
  align-items: center;
  overflow: hidden;
}

body {
  margin: 0;
  padding: 0;
}
</style>
