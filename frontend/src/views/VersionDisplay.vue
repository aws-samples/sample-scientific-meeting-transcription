<template>
  <div class="version-display">
    <span class="version-text">v{{ versionInfo.version }} |  Build Date: {{ versionInfo.buildDate }}</span>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import versionService, { VersionInfo } from '../services/versionService';

const versionInfo = ref<VersionInfo>({ version: '1.0.0', buildDate: '' });

onMounted(async () => {
  try {
    versionInfo.value = await versionService.getVersion();
  } catch (error) {
    console.error('Failed to load version info:', error);
  }
});
</script>

<style scoped>
.version-display {
  font-size: 0.9rem;
}

.version-text {
  opacity: 0.7;
}
</style>
