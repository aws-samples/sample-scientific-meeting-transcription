// Import Vue core functionality
import { createApp } from 'vue'
import App from './App.vue'

// Import AWS Amplify configuration for authentication
import './plugins/amplify'

// Import router for navigation
import router from "@/src/router"

// Import Vuetify for UI components
import { createVuetify } from 'vuetify'
import 'vuetify/styles'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'

// Import Vuex store for state management
import store from './store'

// Import Material Design Icons
import '@mdi/font/css/materialdesignicons.css'

// Import global styles
import './assets/styles/global.css'

// Import ApexCharts for data visualization
import VueApexCharts from "vue3-apexcharts"

// Import axios configuration for API calls
import './plugins/axios'


// Configure Vuetify with components and Material Design Icons
const vuetify = createVuetify({
  components,
  directives,
  icons: {
    defaultSet: 'mdi'
  }
})

// Create the Vue application instance
const app = createApp(App)

// Initialize the Vuex store
store.dispatch('initializeStore')

// Register plugins with the Vue application
app.use(VueApexCharts); // Charts
app.use(store)         // State management
app.use(router)        // Routing
app.use(vuetify)       // UI framework

// Mount the application to the DOM
app.mount('#app')