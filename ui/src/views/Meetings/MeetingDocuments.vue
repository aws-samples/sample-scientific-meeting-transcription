<!--
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 -->

<template>
  <v-card outlined elevation="5" style="font-size: 0.9rem">
    <v-card-title class="d-flex align-center mt-2">
      <v-icon class="mr-2">mdi-file-document-multiple</v-icon>
      Meeting Documents
      <v-spacer></v-spacer>
      <v-btn
        color="primary"
        class="ml-2"
        @click="openDocumentDialog()"
        :disabled="!canEditDocuments"
      >
        Add Document
        <v-icon right>mdi-plus</v-icon>
      </v-btn>
      <v-btn color="secondary" class="ml-2" @click="fetchDocuments()">
        <v-icon right>mdi-refresh</v-icon>
      </v-btn>
    </v-card-title>

    <v-data-table
      :headers="documentTableHeaders"
      :items="documents"
      :search="search"
      :loading="loading"
      :items-per-page="pageSize"
      :page="currentPage"
      @update:options="handlePageChange"
    >
      <template v-slot:item.created_at="{ item }">
        {{ dateFormatter.format(item.created_at) }}
      </template>

      <template v-slot:item.updated_at="{ item }">
        {{ dateFormatter.format(item.updated_at) }}
      </template>

      <template v-slot:item.actions="{ item }">
        <v-btn
          small
          @click="confirmDelete(item)"
          variant="text"
          density="compact"
          icon
          flat
          color="default"
        >
          <v-icon size="small">mdi-delete</v-icon>
        </v-btn>
      </template>

      <template v-slot:no-data>
        <div class="pa-4 text-center">No documents available for this meeting</div>
      </template>
    </v-data-table>
  </v-card>

  <!-- Document Dialog -->
  <v-dialog v-model="dialog" overlay-color="black" overlay-opacity="5" max-width="50%">
    <v-card>
      <v-card-title class="pt-6 pb-2 pr-6">
        <v-icon class="mr-2">mdi-file-document</v-icon>
        <span class="text-h5">{{ formTitle }}</span>
      </v-card-title>

      <v-card-text class="pb-0">
        <v-form ref="formRef" v-model="formValid">
          <v-container class="py-0">
            <v-row dense>
              <!-- Filename field removed as it will be auto-populated from the selected file -->

              <v-col cols="12" class="py-1">
                <v-text-field
                  v-model="editedItem.description"
                  label="Description"
                  density="comfortable"
                  variant="outlined"
                  class="custom-field"
                ></v-text-field>
              </v-col>

              <v-col cols="12" class="py-1" v-if="!editingExistingDocument">
                <div
                  class="upload-area pa-4 rounded-lg"
                  :class="{ dragover: isDragging }"
                  :disabled="isUploading"
                  @dragenter.prevent="isDragging = true"
                  @dragover.prevent="isDragging = true"
                  @dragleave.prevent="isDragging = false"
                  @drop.prevent="handleFileDrop"
                  @click="triggerFileInput"
                >
                  <input
                    :disabled="isUploading"
                    type="file"
                    ref="fileInput"
                    style="display: none"
                    @change="handleFileSelect"
                  />
                  <v-icon size="48" color="primary" class="mb-2" :disabled="isUploading"
                    >mdi-cloud-upload</v-icon
                  >
                  <div class="text-h6" :disabled="isUploading">
                    Drag and drop your file here
                  </div>
                  <div class="text-subtitle-1" :disabled="isUploading">
                    or click to browse
                  </div>
                  <div class="text-caption mt-2">
                    File size limits: 2.5MB for images, 50MB for other documents
                  </div>
                  <div v-if="selectedFile" class="text-subtitle-2 mt-2">
                    Selected: {{ selectedFile.name }} ({{
                      formatFileSize(selectedFile.size)
                    }})
                  </div>
                </div>

                <v-progress-linear
                  v-if="uploadProgress > 0"
                  :value="uploadProgress"
                  :key="uploadProgress"
                  :buffer-value="uploadProgress"
                  color="primary"
                  height="25"
                  class="mt-4"
                  striped
                >
                  <template v-slot:default> {{ Math.round(uploadProgress) }}% </template>
                </v-progress-linear>
              </v-col>
            </v-row>
          </v-container>
        </v-form>
      </v-card-text>

      <v-card-actions class="d-flex justify-end px-10 pb-8">
        <v-btn
          color="primary"
          @click="saveDocument"
          :loading="saving"
          :disabled="saving || !formValid || (!editingExistingDocument && !selectedFile)"
          variant="flat"
          class="text-white mr-4"
        >
          Save
        </v-btn>
        <v-btn
          color="primary"
          @click="closeDocumentDialog"
          variant="flat"
          class="text-white"
        >
          Cancel
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <!-- Delete Confirmation Dialog -->
  <v-dialog v-model="deleteDialog" max-width="400px">
    <v-card>
      <v-card-title class="text-h5">Delete Document</v-card-title>
      <v-card-text>
        Are you sure you want to delete this document?
        <div class="mt-3 font-weight-bold">{{ selectedDocument?.filename }}</div>
        <div class="mt-1 text-caption">{{ selectedDocument?.description }}</div>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="blue-darken-1" variant="text" @click="deleteDialog = false"
          >Cancel</v-btn
        >
        <v-btn
          color="error"
          variant="flat"
          @click="deleteDocument"
          :loading="deleting"
          :disabled="deleting"
          >Delete</v-btn
        >
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

.upload-area:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>

<script lang="ts">
import { ref, computed, onMounted, watch } from "vue";
import { dateFormatter } from "@utils/dateFormatter";
import { MeetingDocumentsService } from "@services/meetingDocuments.service";
import { useStore } from "vuex";
import {
  MeetingDocumentRequestType,
  MeetingDocumentResponseType,
} from "@/src/types/meetingDocument";
import axios from "axios";
import { MeetingSetupProgressEnum } from "../types/meeting";

export default {
  name: "MeetingDocuments",
  props: {
    meetingId: {
      type: String,
      required: true,
    },
    meetingState: {
      type: String,
      required: true,
    },
  },
  emits: ["close"],

  setup(props, { emit }) {
    const store = useStore();
    const teamId = computed(() => store.getters.selectedTeamId);

    const canEditDocuments = computed(() => {
      console.log("meetingState", props.meetingState !== "Sealed");
      return props.meetingState !== "Sealed";
    });

    // Table data
    const documents = ref<MeetingDocumentResponseType[]>([]);
    const loading = ref(false);
    const search = ref("");
    const currentPage = ref(1);
    const pageSize = ref(10);
    const totalItems = ref(0);
    const isMounted = ref(false);

    // Dialog controls
    const dialog = ref(false);
    const deleteDialog = ref(false);
    const editingExistingDocument = ref(false);
    const selectedDocument = ref<MeetingDocumentResponseType | null>(null);

    // Form controls
    const formRef = ref();
    const formValid = ref(false);
    const editedItem = ref<MeetingDocumentRequestType>({
      description: "",
      filename: "",
    });
    const defaultItem: MeetingDocumentRequestType = {
      description: "",
      filename: "",
    };

    // File upload
    const fileInput = ref<HTMLInputElement | null>(null);
    const selectedFile = ref<File | null>(null);
    const isDragging = ref(false);
    const isUploading = ref(false);
    const uploadProgress = ref(0);
    const saving = ref(false);
    const deleting = ref(false);

    // Notifications
    const snackbar = ref({
      show: false,
      message: "",
      color: "success",
    });

    // Table headers
    const documentTableHeaders = computed(() => {
      const headers = [
        { title: "Filename", key: "filename" },
        { title: "Description", key: "description" },
        { title: "Created At", key: "created_at", sortable: true },
      ];

      // Only add actions column if the meeting is not sealed
      if (props.meetingState !== "Sealed") {
        headers.push({ title: "Actions", key: "actions", sortable: false });
      }

      return headers;
    });

    const formTitle = computed(() => {
      return editingExistingDocument.value ? "Edit Document" : "Upload New Document";
    });

    // Fetch documents from API
    const fetchDocuments = async (
      pageIndex = currentPage.value,
      itemsPerPage = pageSize.value
    ) => {
      if (!teamId.value || !props.meetingId) {
        showError("Team ID or Meeting ID not available");
        return;
      }

      try {
        loading.value = true;
        const response = await MeetingDocumentsService.listMeetingDocuments(
          teamId.value,
          props.meetingId,
          pageIndex,
          itemsPerPage
        );

        documents.value = response.records || [];
        totalItems.value = response.total_records || 0;
      } catch (error) {
        console.error("Error fetching documents:", error);
        showError("Failed to fetch documents");
      } finally {
        loading.value = false;
      }
    };

    // Handle pagination changes
    const handlePageChange = (options: any) => {
      if (!isMounted.value) return;
      currentPage.value = options.page || 1;
      pageSize.value = options.itemsPerPage || 10;
      fetchDocuments(currentPage.value, pageSize.value);
    };

    // Format file size for display
    const formatFileSize = (bytes: number): string => {
      if (bytes === 0) return "0 Bytes";
      const k = 1024;
      const sizes = ["Bytes", "KB", "MB", "GB"];
      const i = Math.floor(Math.log(bytes) / Math.log(k));
      return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
    };

    // File selection handlers
    const triggerFileInput = () => {
      if (fileInput.value) {
        fileInput.value.click();
      }
    };

    const handleFileDrop = (event: DragEvent) => {
      isDragging.value = false;
      const files = event.dataTransfer?.files;
      if (files?.length) {
        const file = files[0];
        const fileExtension = file.name.split(".").pop()?.toLowerCase() || "";
        const isImage = ["png", "jpg", "jpeg"].includes(fileExtension);
        const maxSizeInBytes = isImage ? 2.5 * 1024 * 1024 : 50 * 1024 * 1024; // 2.5MB for images, 50MB for others

        if (file.size > maxSizeInBytes) {
          const maxSizeInMB = isImage ? "2.5MB" : "50MB";
          showError(`File size exceeds the limit (${maxSizeInMB})`);
          return;
        }

        selectedFile.value = file;
        // Automatically set the filename from the dropped file
        editedItem.value.filename = file.name;
      }
    };

    const handleFileSelect = (event: Event) => {
      const files = (event.target as HTMLInputElement).files;
      if (files?.length) {
        const file = files[0];
        const fileExtension = file.name.split(".").pop()?.toLowerCase() || "";
        const isImage = ["png", "jpg", "jpeg"].includes(fileExtension);
        const maxSizeInBytes = isImage ? 2.5 * 1024 * 1024 : 50 * 1024 * 1024; // 2.5MB for images, 50MB for others

        if (file.size > maxSizeInBytes) {
          const maxSizeInMB = isImage ? "2.5MB" : "50MB";
          showError(`File size exceeds the limit (${maxSizeInMB})`);
          return;
        }

        selectedFile.value = file;
        // Automatically set the filename from the selected file
        editedItem.value.filename = file.name;
      }
    };

    // Dialog management
    const openDocumentDialog = (item?: MeetingDocumentResponseType) => {
      if (item) {
        editingExistingDocument.value = true;
        selectedDocument.value = item;
        editedItem.value = {
          description: item.description || "",
          filename: item.filename || "",
        };
      } else {
        editingExistingDocument.value = false;
        selectedDocument.value = null;
        editedItem.value = { ...defaultItem };
        selectedFile.value = null;
        uploadProgress.value = 0;
      }

      dialog.value = true;
    };

    const closeDocumentDialog = () => {
      dialog.value = false;
      setTimeout(() => {
        editedItem.value = { ...defaultItem };
        selectedFile.value = null;
        uploadProgress.value = 0;
        formRef.value?.reset();
      }, 300);
    };

    // Document actions
    const saveDocument = async () => {
      try {
        saving.value = true;

        if (editingExistingDocument.value) {
          // Update existing document (just metadata)
          if (!selectedDocument.value) {
            throw new Error("No document selected for editing");
          }

          await MeetingDocumentsService.createMeetingDocument(
            teamId.value,
            props.meetingId,
            editedItem.value
          );

          showSuccess("Document updated successfully");
        } else {
          // Create new document
          if (!selectedFile.value) {
            throw new Error("No file selected for upload");
          }

          // First create the document record
          const newDocument = await MeetingDocumentsService.createMeetingDocument(
            teamId.value,
            props.meetingId,
            editedItem.value
          );
          if (!newDocument.id) {
            showError("Failed to create document record");
            throw new Error("Failed to create document record");
          }
          // Get upload URL
          const uploadUrl = await MeetingDocumentsService.getMeetingDocumentUploadUrl(
            teamId.value,
            props.meetingId,
            newDocument.id
          );

          // Upload the file
          await uploadFile(uploadUrl, selectedFile.value, newDocument);
          showSuccess("Document uploaded successfully");
        }

        closeDocumentDialog();
        fetchDocuments();
      } catch (error) {
        console.error("Error saving document:", error);
        showError(error instanceof Error ? error.message : "Failed to save document");
      } finally {
        saving.value = false;
      }
    };

    const uploadFile = async (
      url: string,
      file: File,
      newDocument: MeetingDocumentResponseType
    ): Promise<void> => {
      return new Promise((resolve, reject) => {
        isUploading.value = true;

        const xhr = new XMLHttpRequest();
        xhr.upload.onprogress = (event) => {
          if (event.lengthComputable) {
            uploadProgress.value = (event.loaded / event.total) * 100;
          }
        };

        xhr.open("PUT", url, true);
        xhr.setRequestHeader("Content-Type", newDocument.mimetype);

        xhr.onload = () => {
          isUploading.value = false;
          if (xhr.status === 200) {
            resolve();
          } else {
            reject(new Error(`Upload failed with status ${xhr.status}`));
          }
        };

        xhr.onerror = () => {
          isUploading.value = false;
          reject(new Error("Upload failed"));
        };

        xhr.send(file);
      });
    };

    const downloadDocument = async (item: MeetingDocumentResponseType) => {
      try {
        // Get the download URL
        const url = await MeetingDocumentsService.getMeetingDocumentUploadUrl(
          teamId.value,
          props.meetingId,
          item.id
        );

        // Create a link and click it to download
        const link = document.createElement("a");
        link.href = url;
        link.download = item.filename || "document";
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        showSuccess("Document download started");
      } catch (error) {
        console.error("Error downloading document:", error);
        showError("Failed to download document");
      }
    };

    const editDocument = (item: MeetingDocumentResponseType) => {
      openDocumentDialog(item);
    };

    const confirmDelete = (item: MeetingDocumentResponseType) => {
      selectedDocument.value = item;
      deleteDialog.value = true;
    };

    const deleteDocument = async () => {
      if (!selectedDocument.value) return;

      try {
        deleting.value = true;
        await MeetingDocumentsService.deleteMeetingDocument(
          teamId.value,
          props.meetingId,
          selectedDocument.value.id
        );

        showSuccess("Document deleted successfully");
        fetchDocuments();
        deleteDialog.value = false;
      } catch (error) {
        console.error("Error deleting document:", error);
        showError("Failed to delete document");
      } finally {
        deleting.value = false;
      }
    };

    // Notifications
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
        message: message || "An unexpected error occurred",
        color: "error",
      };
    };

    // Lifecycle hooks
    onMounted(() => {
      console.log("MeetingDocuments mounted");
      isMounted.value = true;
      fetchDocuments();
    });

    return {
      // Data
      documents,
      loading,
      search,
      currentPage,
      pageSize,
      totalItems,
      dialog,
      deleteDialog,
      editingExistingDocument,
      selectedDocument,
      formRef,
      formValid,
      editedItem,
      fileInput,
      selectedFile,
      isDragging,
      isUploading,
      uploadProgress,
      saving,
      deleting,
      snackbar,
      documentTableHeaders,
      formTitle,

      // Methods
      fetchDocuments,
      handlePageChange,
      formatFileSize,
      triggerFileInput,
      handleFileDrop,
      handleFileSelect,
      openDocumentDialog,
      closeDocumentDialog,
      saveDocument,
      downloadDocument,
      editDocument,
      confirmDelete,
      deleteDocument,
      dateFormatter,
      canEditDocuments,
    };
  },
};
</script>
