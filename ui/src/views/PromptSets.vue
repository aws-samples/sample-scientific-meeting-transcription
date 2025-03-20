<!--
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
 -->

<template>
  <v-card outlined elevation="5">
    <v-card-title class="d-flex align-center mt-2">
      <v-icon class="mr-2">mdi-text-box-multiple</v-icon>
      Promptsets
      <v-spacer></v-spacer>
      <v-menu>
        <template v-slot:activator="{ props }">
          <v-btn
            color="primary"
            v-bind="props"
            :disabled="!selectedPromptSetId"
            class="ml-2"
          >
            Actions
            <v-icon right>mdi-chevron-down</v-icon>
          </v-btn>
        </template>
        <v-list>
          <v-list-item
            v-for="action in actionItems"
            :key="action.value"
            @click="
              selectedAction = action.value;
              executeSelectedAction();
            "
          >
            <template v-slot:prepend>
              <v-icon :icon="action.icon"></v-icon>
            </template>
            <v-list-item-title>{{ action.title }}</v-list-item-title>
          </v-list-item>
        </v-list>
      </v-menu>
      <v-btn color="primary" class="ml-2" @click="openDialog()">
        Add Promptset
        <v-icon right>mdi-plus</v-icon>
      </v-btn>
      <v-btn color="secondary" class="ml-2" @click="fetchPromptSets()">
        <v-icon right>mdi-refresh</v-icon>
      </v-btn>
    </v-card-title>

    <v-data-table
      :headers="headers"
      :items="promptSets"
      :search="search"
      :loading="loading"
      :items-per-page="pageSize"
      :page="currentPage"
      @update:options="handlePageChange"
    >
      <template v-slot:item.status="{ item }">
        <v-chip
          :color="item.status === 'Active' ? 'success' : 'error'"
          text-color="white"
          small
        >
          {{ item.status }}
        </v-chip>
      </template>

      <template v-slot:item.created_at="{ item }">
        {{ dateFormatter.format(item.created_at) }}
      </template>

      <template v-slot:item.updated_at="{ item }">
        {{ dateFormatter.format(item.updated_at) }}
      </template>

      <template v-slot:item.select="{ item }">
        <v-radio-group v-model="selectedPromptSetId" hide-details class="ma-0 pa-0">
          <v-radio :value="item.id" hide-details class="ma-0 pa-0" />
        </v-radio-group>
      </template>
      <!-- Actions are now handled by the top dropdown -->
    </v-data-table>
  </v-card>

  <!-- Prompt Set Dialog -->
  <v-dialog
    persistent
    v-model="dialog"
    overlay-color="black"
    overlay-opacity="5"
    max-width="50%"
  >
    <v-card>
      <v-card-title class="pt-6 pb-2 pr-6">
        <v-icon class="mr-2">mdi-text-box-multiple</v-icon>
        <span class="text-h5">{{ formTitle }}</span>
      </v-card-title>

      <v-card-text class="pb-0">
        <v-form ref="formRef" v-model="formValid">
          <v-container class="py-0">
            <v-row dense>
              <v-col cols="12" class="py-1">
                <v-text-field
                  v-model="editedItem.prompt_set_name"
                  label="Prompt Set Name"
                  required
                  :rules="[(v) => !!v || 'Prompt Set Name is required']"
                  density="comfortable"
                  variant="outlined"
                  class="custom-field"
                ></v-text-field>
              </v-col>
              <v-col cols="12" class="py-1">
                <v-textarea
                  v-model="editedItem.description"
                  label="Description"
                  rows="3"
                  density="comfortable"
                  variant="outlined"
                  class="custom-field"
                ></v-textarea>
              </v-col>
              <v-col cols="12" md="4" class="py-1">
                <v-select
                  v-model="editedItem.status"
                  :items="statusOptions"
                  label="Status"
                  required
                  :rules="[(v) => !!v || 'Status is required']"
                  density="comfortable"
                  variant="outlined"
                  class="custom-field"
                ></v-select>
              </v-col>
              <v-col cols="12" md="8" class="py-1">
                <v-checkbox
                  v-model="editedItem.create_prompts_from_description"
                  label="Generate Prompts from Description using GenAI"
                  required
                  density="comfortable"
                  variant="outlined"
                  class="custom-field"
                  :disabled="!hasEnoughWords"
                ></v-checkbox>
              </v-col>
            </v-row>
          </v-container>
        </v-form>
      </v-card-text>

      <v-card-actions class="d-flex justify-end px-10 pb-8">
        <v-btn
          color="primary"
          @click="savePromptSet"
          :loading="saving"
          :disabled="saving"
          variant="flat"
          class="text-white mr-4"
        >
          Save
        </v-btn>
        <v-btn color="primary" @click="closeDialog" variant="flat" class="text-white">
          Cancel
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <!-- Prompts Dialog -->
  <v-dialog
    persistent
    v-model="promptsDialog"
    overlay-color="black"
    overlay-opacity="5"
    max-width="80%"
  >
    <v-card>
      <v-card-title class="pt-6 pb-2 pr-6 d-flex align-center">
        <v-icon class="mr-2">mdi-text-box</v-icon>
        <span class="text-h5">Prompts for {{ selectedPromptSet?.prompt_set_name }}</span>
        <v-spacer></v-spacer>
        <v-btn color="primary" @click="openPromptDialog()">
          Add Prompt
          <v-icon right>mdi-plus</v-icon>
        </v-btn>
      </v-card-title>

      <v-card-text class="pb-0">
        <v-data-table
          :headers="promptHeaders"
          :items="prompts"
          :loading="promptsLoading"
          @update:options="handlePromptsPageChange"
        >
          <template v-slot:item.status="{ item }">
            <v-chip
              :color="item.status === 'Active' ? 'success' : 'error'"
              text-color="white"
              small
            >
              {{ item.status }}
            </v-chip>
          </template>
          <template v-slot:item.created_at="{ item }">
            {{ dateFormatter.format(item.created_at) }}
          </template>

          <template v-slot:item.updated_at="{ item }">
            {{ dateFormatter.format(item.updated_at) }}
          </template>

          <template v-slot:item.actions="{ item }">
            <v-icon small class="mr-2" @click="movePromptUp(item)" title="Move Up"
              >mdi-arrow-up-bold</v-icon
            >
            <v-icon small class="mr-2" @click="movePromptDown(item)" title="Move Down"
              >mdi-arrow-down-bold</v-icon
            >
            <v-icon small class="mr-2" @click="openPromptDialog(item)" title="Edit"
              >mdi-pencil</v-icon
            >
            <v-icon small @click="deletePrompt(item)" title="Delete">mdi-delete</v-icon>
          </template>
        </v-data-table>
      </v-card-text>

      <v-card-actions class="d-flex justify-end px-10 pb-8">
        <v-btn
          color="primary"
          @click="closePromptsDialog"
          variant="flat"
          class="text-white"
        >
          Close
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <!-- Prompt Dialog -->
  <v-dialog
    persistent
    v-model="promptDialog"
    overlay-color="black"
    overlay-opacity="5"
    max-width="50%"
  >
    <v-card>
      <v-card-title class="pt-6 pb-2 pr-6">
        <v-icon class="mr-2">mdi-text-box</v-icon>
        <span class="text-h5">{{ promptFormTitle }}</span>
      </v-card-title>

      <v-card-text class="pb-0">
        <v-form ref="promptFormRef" v-model="promptFormValid">
          <v-container class="py-0">
            <v-row dense>
              <v-col cols="12" class="py-1">
                <v-textarea
                  v-model="editedPrompt.prompt"
                  label="Prompt"
                  rows="5"
                  required
                  :rules="[(v) => !!v || 'Prompt is required']"
                  density="comfortable"
                  variant="outlined"
                  class="custom-field"
                ></v-textarea>
              </v-col>
              <v-col cols="12" class="py-1">
                <v-textarea
                  v-model="editedPrompt.description"
                  label="Description"
                  rows="3"
                  density="comfortable"
                  variant="outlined"
                  class="custom-field"
                ></v-textarea>
              </v-col>
              <v-col cols="12" class="py-1">
                <v-select
                  v-model="editedPrompt.status"
                  :items="statusOptions"
                  label="Status"
                  required
                  :rules="[(v) => !!v || 'Status is required']"
                  density="comfortable"
                  variant="outlined"
                  class="custom-field"
                ></v-select>
              </v-col>
            </v-row>
          </v-container>
        </v-form>
      </v-card-text>

      <v-card-actions class="d-flex justify-end px-10 pb-8">
        <v-btn
          color="primary"
          @click="savePrompt"
          :loading="promptSaving"
          :disabled="promptSaving"
          variant="flat"
          class="text-white mr-4"
        >
          Save
        </v-btn>
        <v-btn
          color="primary"
          @click="closePromptDialog"
          variant="flat"
          class="text-white"
        >
          Cancel
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <v-snackbar v-model="snackbar.show" :color="snackbar.color" timeout="3000">
    {{ snackbar.message }}
  </v-snackbar>
</template>

<script lang="ts">
import { ref, computed, watch } from "vue";
import { dateFormatter } from "@utils/dateFormatter";
import { PromptSetService } from "@services/promptSet.service";
import {
  PromptSetRequest,
  PromptSetResponse,
  PromptRequest,
  PromptResponse,
} from "@/src/types/promptSet";
import { StatusEnum } from "@/src/types/common";
import { useStore } from "vuex";

export default {
  name: "PromptSets",
  setup() {
    const hasEnoughWords = ref(false);
    const formValid = ref(true);
    const promptFormValid = ref(true);
    const promptSets = ref<PromptSetResponse[]>([]);
    const prompts = ref<PromptResponse[]>([]);
    const loading = ref(false);
    const promptsLoading = ref(false);
    const search = ref("");
    const currentPage = ref(
      parseInt(localStorage.getItem("promptSets_currentPage") || "1")
    );
    const pageSize = ref(parseInt(localStorage.getItem("promptSets_pageSize") || "10"));
    const currentPromptPage = ref(
      parseInt(localStorage.getItem("promptSets_currentPromptPage") || "1")
    );
    const pagePromptSize = ref(
      parseInt(localStorage.getItem("promptSets_pagePromptSize") || "10")
    );

    const dialog = ref(false);
    const promptsDialog = ref(false);
    const promptDialog = ref(false);
    const editedIndex = ref(-1);
    const promptEditedIndex = ref(-1);
    const store = useStore();
    const teamId = computed(() => store.getters.selectedTeamId);
    const selectedPromptSet = ref<PromptSetResponse | null>(null);
    const snackbar = ref({
      show: false,
      message: "",
      color: "success",
    });

    const selectedPromptSetId = ref<string>();
    const selectedAction = ref("");

    const actionItems = [
      { title: "View Prompts", value: "view", icon: "mdi-eye" },
      { title: "Edit Promptset", value: "edit", icon: "mdi-pencil" },
      { title: "Delete Promptset", value: "delete", icon: "mdi-delete" },
    ];

    const executeSelectedAction = () => {
      if (!selectedAction.value || !selectedPromptSetId.value) return;

      // Find the selected promptset
      const selectedPromptSet = promptSets.value.find(
        (promptset) => promptset.id === selectedPromptSetId.value
      );
      if (!selectedPromptSet) return;

      // Execute the action
      switch (selectedAction.value) {
        case "view":
          viewPrompts(selectedPromptSet);
          break;
        case "edit":
          openDialog(selectedPromptSet);
          break;
        case "delete":
          deletePromptSet(selectedPromptSet);
          break;
      }

      // Reset selections
      selectedAction.value = "";
    };

    const headers = [
      { title: "", key: "select", sortable: false, width: "50px" },
      { title: "Name", key: "prompt_set_name" },
      { title: "Description", key: "description" },
      { title: "Status", key: "status" },
      { title: "Created At", key: "created_at" },
      { title: "Updated At", key: "updated_at" },
      // Actions are now handled by the top dropdown
    ];

    const promptHeaders = [
      { title: "Prompt", key: "prompt" },
      { title: "Order", key: "order" },
      { title: "Description", key: "description" },
      { title: "Status", key: "status" },
      { title: "Created At", key: "created_at" },
      { title: "Updated At", key: "updated_at" },
      { title: "Actions", key: "actions", sortable: false },
    ];

    const statusOptions = Object.values(StatusEnum);

    const editedItem = ref<PromptSetRequest>({
      prompt_set_name: "",
      description: "",
      status: StatusEnum.Active,
      create_prompts_from_description: false,
    });

    const editedPrompt = ref<PromptRequest>({
      prompt: "",
      description: "",
      status: StatusEnum.Active,
    });

    const defaultPromptSetItem: PromptSetRequest = {
      prompt_set_name: "",
      description: "",
      status: StatusEnum.Active,
      create_prompts_from_description: false,
    };

    const defaultPromptItem: PromptRequest = {
      prompt: "",
      description: "",
      prompt_set_id: "",
      status: StatusEnum.Active,
    };

    const formTitle = computed(() => {
      return editedIndex.value === -1 ? "New Prompt Set" : "Edit Prompt Set";
    });

    const promptFormTitle = computed(() => {
      return promptEditedIndex.value === -1 ? "New Prompt" : "Edit Prompt";
    });

    const fetchPromptSets = async (pageIndex = 1, pageSize = 10) => {
      try {
        loading.value = true;
        const response = await PromptSetService.getPromptSets(
          teamId.value,
          pageIndex,
          pageSize
        );
        promptSets.value = response.records || [];
      } catch (error) {
        showError("Failed to fetch prompt sets");
      } finally {
        loading.value = false;
      }
    };

    const fetchPrompts = async (pageIndex = 1, pageSize = 10) => {
      if (!selectedPromptSet.value?.id) return;

      try {
        promptsLoading.value = true;
        const response = await PromptSetService.getPrompts(
          teamId.value,
          selectedPromptSet.value.id,
          pageIndex,
          pageSize
        );
        prompts.value = response.records || [];
      } catch (error) {
        showError("Failed to fetch prompts");
      } finally {
        promptsLoading.value = false;
      }
    };

    const handlePageChange = (options: any) => {
      currentPage.value = options.page || 1;
      pageSize.value = options.itemsPerPage || 10;
      // Store values in localStorage
      localStorage.setItem("promptSets_currentPage", currentPage.value.toString());
      localStorage.setItem("promptSets_pageSize", pageSize.value.toString());
      fetchPromptSets(currentPage.value, pageSize.value);
    };

    const handlePromptsPageChange = (options: any) => {
      currentPromptPage.value = options.page || 1;
      pagePromptSize.value = options.itemsPerPage || 10;
      // Store values in localStorage
      localStorage.setItem(
        "promptSets_currentPromptPage",
        currentPromptPage.value.toString()
      );
      localStorage.setItem("promptSets_pagePromptSize", pagePromptSize.value.toString());
      fetchPrompts(currentPromptPage.value, pagePromptSize.value);
    };

    const openDialog = (item?: PromptSetResponse) => {
      if (item) {
        editedIndex.value = promptSets.value.indexOf(item);
        editedItem.value = { ...item };
      } else {
        editedIndex.value = -1;
        editedItem.value = { ...defaultPromptSetItem };
      }
      dialog.value = true;
      // Set focus on prompt set name field when dialog opens
      setTimeout(() => {
        const titleInput = document.querySelector('.v-dialog input[type="text"]');
        if (titleInput) {
          (titleInput.previousElementSibling as HTMLElement)?.focus();
        }
      }, 100);
    };

    const closeDialog = () => {
      dialog.value = false;
      selectedPromptSetId.value = null;
      setTimeout(() => {
        editedItem.value = { ...defaultPromptSetItem };
        editedIndex.value = -1;
      }, 300);
    };

    const viewPrompts = (item: PromptSetResponse) => {
      selectedPromptSet.value = item;
      promptsDialog.value = true;
      fetchPrompts();
    };

    const closePromptsDialog = () => {
      promptsDialog.value = false;
      selectedPromptSetId.value = null;
      prompts.value = [];
    };

    const openPromptDialog = (item?: PromptResponse) => {
      if (item) {
        promptEditedIndex.value = prompts.value.indexOf(item);
        editedPrompt.value = { ...item };
      } else {
        promptEditedIndex.value = -1;
        editedPrompt.value = {
          ...defaultPromptItem,
          prompt_set_id: selectedPromptSet.value?.id,
        };
      }
      promptDialog.value = true;
      setTimeout(() => {
        const titleInput = document.querySelector(
          '.v-overlay__content input[type="textarea"]'
        );
        if (titleInput) {
          (titleInput.previousElementSibling as HTMLElement)?.focus();
        }
      }, 100);
    };

    const closePromptDialog = () => {
      promptDialog.value = false;
      setTimeout(() => {
        editedPrompt.value = { ...defaultPromptItem };
        promptEditedIndex.value = -1;
      }, 300);
    };

    const saving = ref(false);
    const promptSaving = ref(false);

    const savePromptSet = async () => {
      try {
        saving.value = true;
        if (editedIndex.value > -1) {
          // Update existing prompt set
          const promptSet = promptSets.value[editedIndex.value];
          await PromptSetService.updatePromptSet(
            teamId.value,
            promptSet.id!,
            editedItem.value
          );
          showSuccess("Prompt set updated successfully");
        } else {
          // Create new prompt set
          await PromptSetService.createPromptSet(teamId.value, editedItem.value);
          showSuccess("Prompt set created successfully");
        }
        await fetchPromptSets();
        closeDialog();
      } catch (error) {
        showError("Failed to save prompt set");
      } finally {
        saving.value = false;
        selectedPromptSetId.value = undefined;
      }
    };

    const savePrompt = async () => {
      if (!selectedPromptSet.value?.id) return;

      try {
        promptSaving.value = true;
        const promptSetId = selectedPromptSet.value.id;
        if (promptEditedIndex.value > -1) {
          // Update existing prompt
          const prompt = prompts.value[promptEditedIndex.value];
          await PromptSetService.updatePrompt(
            teamId.value,
            selectedPromptSet.value.id,
            prompt.id,
            editedPrompt.value
          );
          showSuccess("Prompt updated successfully");
        } else {
          // Create new prompt
          await PromptSetService.createPrompt(
            teamId.value,
            selectedPromptSet.value.id,
            editedPrompt.value
          );
          showSuccess("Prompt created successfully");
        }
        await fetchPrompts();
        closePromptDialog();
      } catch (error) {
        showError("Failed to save prompt");
      } finally {
        promptSaving.value = false;
      }
    };

    const deletePromptSet = async (item: PromptSetResponse) => {
      if (!confirm("Are you sure you want to delete this prompt set?")) return;

      try {
        loading.value = true;
        await PromptSetService.deletePromptSet(teamId.value, item.id!);
        await fetchPromptSets();
        showSuccess("Prompt set deleted successfully");
        loading.value = false;
      } catch (error) {
        showError("Failed to delete prompt set");
      }
    };

    const deletePrompt = async (item: PromptResponse) => {
      if (
        !selectedPromptSet.value?.id ||
        !confirm("Are you sure you want to delete this prompt?")
      )
        return;

      try {
        promptsLoading.value = true;
        await PromptSetService.deletePrompt(
          teamId.value,
          selectedPromptSet.value.id,
          item.id
        );
        await fetchPrompts();
        showSuccess("Prompt deleted successfully");
      } catch (error) {
        showError("Failed to delete prompt");
      }
    };

    const showSuccess = (message: string) => {
      snackbar.value = {
        show: true,
        message,
        color: "success",
      };
    };

    const showError = (message: string) => {
      snackbar.value = {
        show: true,
        message,
        color: "error",
      };
    };

    const handleAction = (action: any, item: PromptSetResponse) => {
      switch (action.value) {
        case "view":
          viewPrompts(item);
          break;
        case "edit":
          openDialog(item);
          break;
        case "delete":
          deletePromptSet(item);
          break;
      }
    };

    const movePromptUp = async (item: PromptResponse) => {
      if (!selectedPromptSet.value?.id) return;

      try {
        promptsLoading.value = true;

        const result = await PromptSetService.moveUpPrompt(
          teamId.value,
          selectedPromptSet.value.id,
          item.id
        );
        await fetchPrompts();
        showSuccess(result.message || "Prompt moved up successfully");
      } catch (error) {
        showError("Failed to move prompt up");
      }
    };

    const movePromptDown = async (item: PromptResponse) => {
      if (!selectedPromptSet.value?.id) return;

      try {
        promptsLoading.value = true;

        const result = await PromptSetService.moveDownPrompt(
          teamId.value,
          selectedPromptSet.value.id,
          item.id
        );
        await fetchPrompts();
        showSuccess(result.message || "Prompt moved down successfully");
      } catch (error) {
        showError("Failed to move prompt down");
      }
    };

    watch(
      () => editedItem.value.description,
      (newDescription) => {
        const wordCount = newDescription ? newDescription.length : 0;
        const enoughWords = wordCount > 30;
        if (enoughWords) {
          hasEnoughWords.value = true;
        } else {
          hasEnoughWords.value = false;
          if (editedItem.value.create_prompts_from_description) {
            editedItem.value.create_prompts_from_description = false;
          }
        }
      }
    );

    return {
      promptSets,
      prompts,
      loading,
      promptsLoading,
      search,
      headers,
      promptHeaders,
      dialog,
      promptsDialog,
      promptDialog,
      editedIndex,
      promptEditedIndex,
      editedItem,
      editedPrompt,
      selectedPromptSet,
      formTitle,
      promptFormTitle,
      snackbar,
      statusOptions,
      handlePageChange,
      handlePromptsPageChange,
      openDialog,
      closeDialog,
      viewPrompts,
      closePromptsDialog,
      openPromptDialog,
      closePromptDialog,
      savePromptSet,
      savePrompt,
      deletePromptSet,
      deletePrompt,
      movePromptUp,
      movePromptDown,
      fetchPromptSets,
      handleAction,
      saving,
      selectedPromptSetId,
      actionItems,
      promptSaving,
      promptFormValid,
      formValid,
      dateFormatter,
      executeSelectedAction,
      selectedAction,
      pageSize,
      currentPage,
      hasEnoughWords,
    };
  },
};
</script>
