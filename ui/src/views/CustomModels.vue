<!--
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
 -->

<template>
  <v-card outlined elevation="5">
    <v-card-title class="d-flex align-center mt-2">
      <v-icon class="mr-2">mdi-brain</v-icon>
      Custom Models
      <v-spacer></v-spacer>
      <v-menu>
        <template v-slot:activator="{ props }">
          <v-btn color="primary" v-bind="props" :disabled="!selectedModelId" class="ml-2">
            Actions
            <v-icon right>mdi-chevron-down</v-icon>
          </v-btn>
        </template>

        <v-list>
          <template v-for="action in actionItems" :key="action.value">
            <v-divider v-if="action.value === 'separator'"></v-divider>
            <v-list-item
              v-else
              @click="
                selectedAction = action.value;
                executeSelectedAction();
              "
              :disabled="
                action.requiredPermission &&
                !selectedModel?.can_perform?.includes(action.requiredPermission)
              "
            >
              <template v-slot:prepend>
                <v-icon :icon="action.icon"></v-icon>
              </template>
              <v-list-item-title>{{ action.title }}</v-list-item-title>
            </v-list-item>
          </template>
        </v-list>
      </v-menu>
      <v-btn color="primary" class="ml-2" @click="openDialog()">
        Custom Model
        <v-icon right>mdi-plus</v-icon>
      </v-btn>
      <v-btn color="secondary" class="ml-2" @click="fetchCustomModels()">
        <v-icon right>mdi-refresh</v-icon>
      </v-btn>
    </v-card-title>

    <v-data-table
      :headers="headers"
      :items="customModels"
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

      <template v-slot:item.custom_model_progress_stats="{ item }">
        <v-chip
          :color="getProgressColor(item.custom_model_progress_status)"
          text-color="white"
          small
        >
          <v-progress-circular
            :width="3"
            v-if="
              item.custom_model_progress_status ===
                CustomModelSetupProgressEnum.TrainingQueued ||
              item.custom_model_progress_status ===
                CustomModelSetupProgressEnum.TrainingStarted
            "
            indeterminate
            :size="20"
            class="mr-1"
          />
          {{ item.custom_model_progress_status }}
        </v-chip>
      </template>

      <template v-slot:item.select="{ item }">
        <v-radio-group v-model="selectedModelId" hide-details class="ma-0 pa-0">
          <v-radio :value="item.id" hide-details class="ma-0 pa-0" />
        </v-radio-group>
      </template>
    </v-data-table>
  </v-card>

  <v-dialog
    persistent
    v-model="dialog"
    overlay-color="black"
    overlay-opacity="5"
    max-width="50%"
  >
    <v-card>
      <v-card-title class="pt-6 pb-2 pr-6">
        <v-icon class="mr-2">mdi-brain</v-icon>
        <span class="text-h5">{{ formTitle }}</span>
      </v-card-title>

      <v-card-text class="pb-0">
        <v-form ref="formRef" v-model="formValid">
          <v-container class="py-0">
            <v-row dense>
              <v-col cols="12" class="py-1">
                <v-text-field
                  v-model="editedItem.model_name"
                  label="Model Name"
                  required
                  :rules="[(v) => !!v || 'Model Name is required']"
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
              <v-col cols="12" md="6" class="py-1">
                <v-select
                  v-model="editedItem.language_code"
                  :items="languageCodes"
                  label="Language Code"
                  item-title="name"
                  item-value="id"
                  required
                  :rules="[(v) => !!v || 'Language Code is required']"
                  density="comfortable"
                  variant="outlined"
                  class="custom-field"
                ></v-select>
              </v-col>
              <v-col cols="12" md="6" class="py-1">
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
  
            </v-row>
          </v-container>
        </v-form>
      </v-card-text>

      <v-card-actions class="d-flex justify-end px-10 pb-8">
        <v-btn
          color="primary"
          @click="saveModel"
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

  <!-- Upload Dialog -->
  <v-dialog
    persistent
    v-model="uploadDialog"
    overlay-color="black"
    overlay-opacity="5"
    max-width="50%"
  >
    <v-card>
      <v-card-title class="pt-6 pb-2 pr-6">
        <v-icon class="mr-2">mdi-upload</v-icon>
        <span class="text-h5">Upload Training Data</span>
      </v-card-title>

      <v-card-text class="pb-0">
        <v-container class="py-0">
          <div
            class="upload-area pa-4 rounded-lg"
            :class="{ dragover: isDragging }"
            @dragenter.prevent="isDragging = true"
            @dragover.prevent="isDragging = true"
            @dragleave.prevent="isDragging = false"
            @drop.prevent="handleFileDrop"
            @click="triggerFileInput"
          >
            <input
              type="file"
              ref="fileInput"
              style="display: none"
              @change="handleFileSelect"
              accept=".txt"
            />
            <v-icon size="48" color="primary" class="mb-2">mdi-cloud-upload</v-icon>
            <div class="text-h6">Drag and drop your file here</div>
            <div class="text-subtitle-1">or click to browse</div>
            <div class="text-caption mt-2">Supported formats: .txt</div>
          </div>

          <v-progress-linear
            v-if="uploadProgress > 0"
            :value="uploadProgress"
            color="primary"
            height="25"
            class="mt-4"
          >
            <template v-slot:default> {{ Math.round(uploadProgress) }}% </template>
          </v-progress-linear>
        </v-container>
      </v-card-text>

      <v-card-actions class="d-flex justify-end px-10 pb-8">
        <v-btn
          color="primary"
          @click="closeUploadDialog"
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

<style scoped>
.upload-area {
  border: 2px dashed #1976d2;
  text-align: center;
  cursor: pointer;
  transition: all 0.3s ease;
}

.upload-area:hover {
  background-color: rgba(25, 118, 210, 0.05);
}

.upload-area.dragover {
  background-color: rgba(25, 118, 210, 0.1);
  border-color: #1565c0;
}
</style>

<script lang="ts">
import { ref, computed, onMounted, watch } from "vue";
import { dateFormatter } from "@utils/dateFormatter";
import { CustomModelService } from "@services/customModel.service";
import {
  CustomModelRequest,
  CustomModelResponse,
  CustomModelSetupProgressEnum,
  CustomModelCanPerformEnum,
} from "@/src/types/customModel";
import { StatusEnum } from "@/src/types/common";
import { useStore } from "vuex";

export default {
  name: "CustomModels",
  setup() {
    const formValid = ref(true);
    const customModels = ref<CustomModelResponse[]>([]);
    const loading = ref(false);
    const search = ref("");
    const dialog = ref(false);
    const currentPage = ref(1);
    const pageSize = ref(10);
    const editedIndex = ref(-1);
    const store = useStore();
    const teamId = computed(() => store.getters.selectedTeamId);
    const snackbar = ref({
      show: false,
      message: "",
      color: "success",
    });

    const selectedModelId = ref<string>();
    const selectedModel = ref<CustomModelResponse | null>(null);

    // Watch for changes to selectedModelId and update selectedModel
    watch(selectedModelId, (newId) => {
      if (newId) {
        selectedModel.value =
          customModels.value.find((model) => model.id === newId) || null;
      } else {
        selectedModel.value = null;
      }
    });

    const actionItems = [
      {
        title: "Edit Model",
        value: "edit",
        icon: "mdi-pencil",
        requiredPermission: CustomModelCanPerformEnum.Edit,
      },
      {
        title: "Delete Model",
        value: "delete",
        icon: "mdi-delete",
        requiredPermission: CustomModelCanPerformEnum.Delete,
      },
      {
        value: "separator",
      },
      {
        title: "Upload Training Data",
        value: "upload",
        icon: "mdi-upload",
        requiredPermission: CustomModelCanPerformEnum.Upload,
      },
      {
        title: "Start Training",
        value: "train",
        icon: "mdi-play",
        requiredPermission: CustomModelCanPerformEnum.Train,
      },
    ];

    const languageCodes = [
      {
        id: "en-US",
        name: "English (United States)",
      },
      {
        id: "hi-IN",
        name: "Hindi (India)",
      },
      {
        id: "es-US",
        name: "Spanish (United States)",
      },
      {
        id: "en-GB",
        name: "English (United Kingdom)",
      },
      {
        id: "en-AU",
        name: "English (Australia)",
      },
      {
        id: "de-DE",
        name: "German (Germany)",
      },
      {
        id: "ja-JP",
        name: "Japanese (Japan)",
      },
    ];

    const headers = [
      { title: "", key: "select", sortable: false, width: "50px" },
      { title: "Model Name", key: "model_name" },
      { title: "Description", key: "description" },
      { title: "Language", key: "language_code" },
      { title: "Status", key: "status" },
      { title: "Progress", key: "custom_model_progress_stats" },
      { title: "Created At", key: "created_at" },
      { title: "Updated At", key: "updated_at" },
      // Actions are now handled by the top dropdown
    ];

    const statusOptions = Object.values(StatusEnum);

    const uploadDialog = ref(false);
    const isDragging = ref(false);
    const uploadProgress = ref(0);
    const fileInput = ref<HTMLInputElement | null>(null);

    const editedItem = ref<CustomModelRequest>({
      model_name: "",
      description: "",
      status: StatusEnum.Active,
      language_code: "en-US",
    });

    const defaultItem: CustomModelRequest = {
      model_name: "",
      description: "",
      status: StatusEnum.Active,
      language_code: "en-US",
    };

    const formTitle = computed(() => {
      return editedIndex.value === -1 ? "New Custom Model" : "Edit Custom Model";
    });

    const getProgressColor = (progress: CustomModelSetupProgressEnum): string => {
      switch (progress) {
        case CustomModelSetupProgressEnum.Created:
          return "info";
        case CustomModelSetupProgressEnum.S3SignedUrlCreated:
          return "warning";
        case CustomModelSetupProgressEnum.TrainingStarted:
          return "primary";
        case CustomModelSetupProgressEnum.ModelReady:
          return "success";
        case CustomModelSetupProgressEnum.TrainingFailed:
          return "error";
        default:
          return "grey";
      }
    };

    const fetchCustomModels = async (
      pageIndex = currentPage.value,
      itemsPerPage = pageSize.value
    ) => {
      try {
        loading.value = true;
        const response = await CustomModelService.getCustomModels(
          teamId.value,
          pageIndex,
          itemsPerPage
        );
        customModels.value = response.records || [];
      } catch (error) {
        showError("Failed to fetch custom models");
      } finally {
        loading.value = false;
        selectedModelId.value = null;
      }
    };
    const isMounted = ref(false);

    const handlePageChange = (options: any) => {
      if (!isMounted.value) return;

      currentPage.value = options.page || 1;
      pageSize.value = options.itemsPerPage || 10;
      fetchCustomModels(currentPage.value, pageSize.value);
    };

    const openDialog = (item?: CustomModelResponse) => {
      if (item) {
        editedIndex.value = customModels.value.indexOf(item);
        editedItem.value = { ...item };
      } else {
        editedIndex.value = -1;
        editedItem.value = { ...defaultItem };
      }
      dialog.value = true;
      // Set focus on model name field when dialog opens
      setTimeout(() => {
        const titleInput = document.querySelector('.v-dialog input[type="text"]');
        if (titleInput) {
          (titleInput.previousElementSibling as HTMLElement)?.focus();
        }
      }, 100);
    };

    const closeDialog = () => {
      dialog.value = false;
      selectedModelId.value = null;
      setTimeout(() => {
        editedItem.value = { ...defaultItem };
        editedIndex.value = -1;
      }, 300);
    };

    const saving = ref(false);

    const saveModel = async () => {
      try {
        saving.value = true;
        if (editedIndex.value > -1) {
          // Update existing model
          const model = customModels.value[editedIndex.value];
          await CustomModelService.updateCustomModel(
            teamId.value,
            model.id,
            editedItem.value
          );
          showSuccess("Custom model updated successfully");
        } else {
          // Create new model
          await CustomModelService.createCustomModel(teamId.value, editedItem.value);
          showSuccess("Custom model created successfully");
        }
        await fetchCustomModels();
        closeDialog();
      } catch (error) {
        showError(error);
      } finally {
        saving.value = false;
      }
    };

    const deleteModel = async (item: CustomModelResponse) => {
      if (!confirm("Are you sure you want to delete this custom model?")) return;

      try {
        loading.value = true;
        await CustomModelService.deleteCustomModel(teamId.value, item.id);
        await fetchCustomModels();
        showSuccess("Custom model deleted successfully");
      } catch (error) {
        showError(error);
      } finally {
        loading.value = false;
      }
    };

    const createSignedUrl = async (item: CustomModelResponse) => {
      selectedModel.value = item;
      uploadDialog.value = true;
    };

    const closeUploadDialog = () => {
      uploadDialog.value = false;
      uploadProgress.value = 0;
      selectedModelId.value = null;
    };

    const triggerFileInput = () => {
      fileInput.value?.click();
    };

    const handleFileDrop = async (event: DragEvent) => {
      isDragging.value = false;
      const files = event.dataTransfer?.files;
      if (files?.length) {
        handleUpload(files[0]);
      }
    };

    const handleFileSelect = async (event: Event) => {
      const files = (event.target as HTMLInputElement).files;
      if (files?.length) {
        handleUpload(files[0]);
      }
      //update model with the correct status after upload completes
    };

    const handleUpload = async (file: File) => {
      if (!selectedModel.value) return;

      try {
        const upload_url = (
          await CustomModelService.createSignedUrl(teamId.value, selectedModel.value.id)
        ).presigned_url!;

        const xhr = new XMLHttpRequest();
        xhr.upload.onprogress = (event) => {
          if (event.lengthComputable) {
            uploadProgress.value = (event.loaded / event.total) * 100;
          }
        };

        xhr.onload = async () => {
          if (xhr.status === 200) {
            showSuccess("File uploaded successfully");
          } else {
            showError("Upload failed");
          }
        };

        xhr.onerror = () => {
          showError("Upload failed");
        };

        xhr.open("PUT", upload_url, true);
        xhr.setRequestHeader("Content-Type", file.type);
        xhr.send(file);
      } catch (error) {
        showError("Failed to get upload URL");
      } finally {
        if (!selectedModel.value) return;
        console.log(selectedModel.value);
        selectedModel.value.custom_model_progress_status =
          CustomModelSetupProgressEnum.TrainingDataUploaded;
        await CustomModelService.updateCustomModel(
          teamId.value,
          selectedModel.value.id,
          selectedModel.value
        );

        await fetchCustomModels();
        closeUploadDialog();
        uploadDialog.value = false;
      }
    };

    const startTraining = async (item: CustomModelResponse) => {
      try {
        loading.value = true;
        await CustomModelService.startModelTraining(teamId.value, item.id);
        await fetchCustomModels();
        showSuccess("Model training started successfully");
      } catch (error) {
        showError(error);
      } finally {
        loading.value = false;
        selectedModelId.value = null;
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
    onMounted(() => {
      isMounted.value = true;
      fetchCustomModels();
    });

    const selectedAction = ref("");
    const executeSelectedAction = () => {
      if (!selectedAction.value || !selectedModelId.value) return;

      // Find the selected model
      const selectedModel = customModels.value.find(
        (model) => model.id === selectedModelId.value
      );
      if (!selectedModel) return;

      // Execute the action
      switch (selectedAction.value) {
        case "upload":
          createSignedUrl(selectedModel);
          break;
        case "train":
          startTraining(selectedModel);
          break;
        case "edit":
          openDialog(selectedModel);
          break;
        case "delete":
          deleteModel(selectedModel);
          break;
      }

      // Reset selections
      selectedAction.value = "";
    };

    const handleAction = (action: string, item: CustomModelResponse) => {
      if (!action) return;
      switch (action) {
        case "upload":
          createSignedUrl(item);
          break;
        case "train":
          startTraining(item);
          break;
        case "edit":
          openDialog(item);
          break;
        case "delete":
          deleteModel(item);
          break;
      }
    };

    return {
      customModels,
      loading,
      search,
      headers,
      dialog,
      editedIndex,
      editedItem,
      formTitle,
      snackbar,
      statusOptions,
      handlePageChange,
      openDialog,
      closeDialog,
      saveModel,
      deleteModel,
      createSignedUrl,
      startTraining,
      getProgressColor,
      fetchCustomModels,
      languageCodes,
      saving,
      uploadDialog,
      isDragging,
      uploadProgress,
      closeUploadDialog,
      triggerFileInput,
      handleFileDrop,
      handleFileSelect,
      fileInput,
      formValid,
      dateFormatter,
      CustomModelSetupProgressEnum,
      handleAction,
      selectedModelId,
      actionItems,
      selectedAction,
      executeSelectedAction,
      pageSize,
      currentPage,
      selectedModel,
    };
  },
};
</script>
