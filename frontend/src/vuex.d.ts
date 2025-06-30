import { Store } from 'vuex'
import { RootState } from './store'
import { Router, RouteLocationNormalizedLoaded } from 'vue-router'

declare module '@vue/runtime-core' {
  interface ComponentCustomProperties {
    $store: Store<RootState>
    $router: Router
    $route: RouteLocationNormalizedLoaded
  }
}

declare module 'vuex/dist/vuex.esm-bundler.js' {
  export * from 'vuex'
  export function useStore<T = RootState>(): Store<T>
}