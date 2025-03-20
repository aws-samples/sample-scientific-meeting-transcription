import { createApp } from 'vue'
import App from './App.vue'
import './plugins/amplify'
import router from "@/src/router"
import { createVuetify } from 'vuetify'
import 'vuetify/styles'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import store from './store'
import '@mdi/font/css/materialdesignicons.css'
import './assets/styles/global.css'
import VueApexCharts from "vue3-apexcharts"
import './plugins/axios'



const vuetify = createVuetify({
  components,
  directives,
  icons: {
    defaultSet: 'mdi'
  }
})

const app = createApp(App)

store.dispatch('initializeStore')

app.use(VueApexCharts);
app.use(store)
app.use(router)
app.use(vuetify)

app.mount('#app')