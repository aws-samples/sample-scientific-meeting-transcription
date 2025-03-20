<!--
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
 -->

<template>
  <RouterView />
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useStore } from "vuex";
import type { RootState } from "./store";
import { useRouter } from "vue-router";
import { RouterView } from "vue-router";

head: [["link", { rel: "icon", type: "image/x-icon", href: "/favicon.ico" }]];

const store = useStore();
const router = useRouter();

const handleLogout = () => {
  store.dispatch("logout");
  router.push("/login");
};
</script>
