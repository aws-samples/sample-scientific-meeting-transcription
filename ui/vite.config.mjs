/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
 */

import { fileURLToPath, URL } from 'node:url'
import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'
//import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/


const aliasConfig = {
  '@': fileURLToPath(new URL('./', import.meta.url)),
  '@assets': fileURLToPath(new URL('./src/assets', import.meta.url)),
  '@views': fileURLToPath(new URL('./src/views', import.meta.url)),
  '@services': fileURLToPath(new URL('./src/services', import.meta.url)),
  '@types': fileURLToPath(new URL('./src/types', import.meta.url)),
  '@utils': fileURLToPath(new URL('./src/utils', import.meta.url)),
  '@stores': fileURLToPath(new URL('./src/stores', import.meta.url)),
  '@router': fileURLToPath(new URL('./src/router', import.meta.url)),
  '@layouts': fileURLToPath(new URL('./src/layouts', import.meta.url)),
  '@styles': fileURLToPath(new URL('./src/styles', import.meta.url)),
  '@public': fileURLToPath(new URL('./public', import.meta.url))
}

console.log('Vite Alias Configuration:')
Object.entries(aliasConfig).forEach(([key, value]) => {
  console.log(`${key} -> ${value}`)
})

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  return {
    plugins: [
      vue(),
    ],
    build: {
      outDir: 'dist',
      sourcemap: true
    },
    base: '/',
    resolve: {
      alias: aliasConfig,
      extensions: ['.mjs', '.js', '.ts', '.jsx', '.tsx', '.json', '.vue']
    }
  }
})