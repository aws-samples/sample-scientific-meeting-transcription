/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
 */

/// <reference types="vite/client" />
/// <reference types="vite/types/importMeta.d.ts" />

declare module 'vue3-virtual-scroller' {
  import { Plugin } from 'vue'
  const VueVirtualScroller: Plugin
  export default VueVirtualScroller
  export const RecycleScroller: any
  export const DynamicScroller: any
  export const DynamicScrollerItem: any
}


interface ImportMetaEnv {
  readonly VITE_COGNITO_REGION: string
  readonly VITE_COGNITO_USER_POOL_ID: string
  readonly VITE_COGNITO_CLIENT_ID: string
  readonly VITE_COGNITO_IDENTITY_POOL_ID: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
