<!--
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
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
