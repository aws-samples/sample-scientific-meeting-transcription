<!--
  // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
  -->

<template>
  <v-dialog v-model="dialogVisible" max-width="90%">
    <v-card>
      <v-card-title class="d-flex align-center mt-2">
        <v-icon class="mr-2">mdi-brain</v-icon>
        Prompt Responses
        <v-spacer></v-spacer>
        <v-btn
          color="primary"
          variant="flat"
          class="text-white"
          :disabled="promptResponses.length === 0"
          @click="exportPromptResponses"
        >
          <v-icon class="mr-1">mdi-export</v-icon>
          Export
        </v-btn>
      </v-card-title>
      <v-card-text>
        <div class="d-flex">
          <!-- Left side - Prompts Table -->
          <div class="prompt-list" style="width: 40%; border-right: 1px solid rgba(0,0,0,0.12); padding-right: 16px;">
            <v-data-table
              class="table"
              :headers="promptListHeaders"
              :items="promptResponses"
              :items-per-page="promptResponsesPageSize"
              :page="promptResponsesCurrentPage"
              @update:options="handlePromptResponsesPageChange"
              :loading="loading"
              item-value="id"
              @click:row="handlePromptClick"
              single-select
            >
              <template v-slot:no-data> No prompt responses found </template>
              <template v-slot:item="{ item, index }">
                <tr 
                  :class="{ 'selected-prompt': selectedPromptIndex === index }"
                  @click="selectPrompt(item, index)"
                >
                  <td>{{ item.prompt }}</td>
                </tr>
              </template>
            </v-data-table>
          </div>
          
          <!-- Right side - Selected Prompt Response -->
          <div class="prompt-response" style="width: 60%; padding-left: 16px;">
            <div v-if="selectedPrompt" class="pa-3">
              <div class="d-flex align-center mb-4">
                <h3>Response:</h3>
                <v-btn
                  icon="mdi-content-copy"
                  size="small"
                  variant="text"
                  class="ml-2"
                  @click="copyToClipboard(selectedPrompt.prompt_response)"
                >
                </v-btn>
              </div>
              <div class="response-content">
                <div style="white-space: pre !important; text-wrap: wrap">
                  {{ selectedPrompt.prompt_response }}
                </div>
              </div>
            </div>
            <div v-else class="d-flex justify-center align-center" style="height: 300px;">
              <v-card-text class="text-center text-subtitle-1">
                Select a prompt to view its response
              </v-card-text>
            </div>
          </div>
        </div>
      </v-card-text>
      <v-card-actions class="d-flex justify-end pa-4">
        <v-btn
          color="primary"
          @click="closeDialog"
          variant="flat"
          class="text-white"
        >
          Close
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script lang="ts">
import { ref, defineComponent, PropType, computed } from 'vue';
import { dateFormatter } from "@utils/dateFormatter";
import { copyToClipboard } from "@/src/utils/clipboard";
import { type MeetingPromptResponse } from "@/src/types/meeting";

export default defineComponent({
  name: 'PromptResponsesDialog',
  
  props: {
    modelValue: {
      type: Boolean,
      default: false
    },
    promptResponses: {
      type: Array as PropType<MeetingPromptResponse[]>,
      default: () => []
    },
    loading: {
      type: Boolean,
      default: false
    }
  },
  
  emits: ['update:modelValue', 'export-prompt-responses'],
  
  setup(props, { emit }) {
    const dialogVisible = computed({
      get: () => props.modelValue,
      set: (value) => emit('update:modelValue', value)
    });
    
    const promptResponsesCurrentPage = ref(1);
    const promptResponsesPageSize = ref(10);
    const selectedPromptIndex = ref(-1);
    const selectedPrompt = ref<MeetingPromptResponse | null>(null);

    const promptListHeaders = [
      { title: "Prompt", key: "prompt", sortable: true, width: '100%' },
    ];

    const handlePromptResponsesPageChange = (options: any) => {
      promptResponsesCurrentPage.value = options.page || 1;
      promptResponsesPageSize.value = options.itemsPerPage || 10;
      // Store pagination state in localStorage
      localStorage.setItem('promptResponsesCurrentPage', promptResponsesCurrentPage.value.toString());
      localStorage.setItem('promptResponsesPageSize', promptResponsesPageSize.value.toString());
    };

    const selectPrompt = (item: MeetingPromptResponse, index: number) => {
      selectedPromptIndex.value = index;
      selectedPrompt.value = item;
    };

    const handlePromptClick = (event: Event, { item }: { item: MeetingPromptResponse }) => {
      const index = props.promptResponses.findIndex(response => response === item);
      selectPrompt(item, index);
    };

    const closeDialog = () => {
      dialogVisible.value = false;
      selectedPromptIndex.value = -1;
      selectedPrompt.value = null;
    };

    const exportPromptResponses = () => {
      if (props.promptResponses.length === 0) return;
      emit('export-prompt-responses');
    };

    // Initialize pagination from localStorage if available
    const initFromStorage = () => {
      const storedPage = localStorage.getItem('promptResponsesCurrentPage');
      const storedSize = localStorage.getItem('promptResponsesPageSize');
      if (storedPage) promptResponsesCurrentPage.value = parseInt(storedPage);
      if (storedSize) promptResponsesPageSize.value = parseInt(storedSize);
    };
    
    initFromStorage();

    return {
      dialogVisible,
      promptListHeaders,
      promptResponsesCurrentPage,
      promptResponsesPageSize,
      handlePromptResponsesPageChange,
      selectPrompt,
      handlePromptClick,
      selectedPromptIndex,
      selectedPrompt,
      closeDialog,
      exportPromptResponses,
      dateFormatter,
      copyToClipboard,
    };
  }
});
</script>

<style scoped>
.prompt-list {
  overflow-y: auto;
  max-height: 70vh;
}

.prompt-response {
  height: 500px; /* Fixed height */
  overflow-y: auto; /* Make it scrollable */
}

.response-content {
  background-color: #f5f5f5;
  border-radius: 8px;
  padding: 16px;
  position: relative;
}

.selected-prompt {
  background-color: #e3f2fd !important; /* Light blue background */
  color: #1976d2 !important; /* Blue text */
  font-weight: 500;
}

.table tr:hover {
  cursor: pointer;
}


/* Style for the copy button */
.response-content .v-btn {
  position: absolute;
  top: 8px;
  right: 8px;
}
</style>