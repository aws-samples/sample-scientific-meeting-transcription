<!--
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 -->

<template>
  <v-card outlined elevation="5">
    <v-card-title class="d-flex align-center mt-2">
      <v-icon class="mr-2">mdi-format-pilcrow</v-icon>
      Custom Vocabularies
      <v-spacer></v-spacer>
      <v-menu>
        <template v-slot:activator="{ props }">
          <v-btn
            color="primary"
            v-bind="props"
            :disabled="!selectedCustomVocabularyId"
            class="ml-2"
          >
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
                !selectedCustomVocabulary?.can_perform?.includes(
                  action.requiredPermission
                )
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
        Add Custom Vocabulary
        <v-icon right>mdi-plus</v-icon>
      </v-btn>
      <v-btn color="secondary" class="ml-2" @click="fetchCustomVocabularies()">
        <v-icon right>mdi-refresh</v-icon>
      </v-btn>
    </v-card-title>

    <v-data-table
      :headers="headers"
      :items="customVocabularies"
      :search="search"
      :loading="loading"
      :items-per-page="10"
      :server-items-length="vocabPagination.total_records"
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
      <template v-slot:item.current_step="{ item }">
        <v-tooltip :disabled="item.current_step !== 'PublishFailed'" location="top">
          <template v-slot:activator="{ props }">
            <v-chip
              v-bind="item.current_step === 'PublishFailed' ? props : {}"
              :color="getStatusColor(item.current_step)"
              text-color="white"
              small
            >
              <v-progress-circular
                :width="3"
                v-if="item.current_step === 'Publishing'"
                indeterminate
                :size="20"
                class="mr-1"
              />
              {{ item.current_step }}
            </v-chip>
          </template>
          <span>{{ item.publish_error }}</span>
        </v-tooltip>
      </template>
      <template v-slot:item.language_code="{ item }">
        {{ languageCodes.find((lang) => lang.id === item.language_code)?.name }}
      </template>
      <template v-slot:item.created_at="{ item }">
        {{ dateFormatter.format(item.created_at) }}
      </template>

      <template v-slot:item.updated_at="{ item }">
        {{ dateFormatter.format(item.updated_at) }}
      </template>

      <template v-slot:item.select="{ item }">
        <v-radio-group
          v-model="selectedCustomVocabularyId"
          hide-details
          class="ma-0 pa-0"
        >
          <v-radio :value="item.id" hide-details class="ma-0 pa-0" />
        </v-radio-group>
      </template>
    </v-data-table>
  </v-card>

  <!-- Custom Vocabulary Dialog -->
  <v-dialog
    persistent
    v-model="dialog"
    overlay-color="black"
    :overlay-opacity="0.5"
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
                  v-model="editedItem.vocabulary_name"
                  label="Vocabulary Name"
                  required
                  :rules="[(v) => !!v || 'Vocabulary Name is required']"
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
              <v-col cols="12" class="py-1">
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
          @click="saveCustomVocabulary"
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

  <!-- Vocabulary Phrases Dialog -->
  <v-dialog
    persistent
    v-model="phrasesDialog"
    overlay-color="black"
    :overlay-opacity="0.5"
    max-width="80%"
  >
    <v-card>
      <v-card-title class="pt-6 pb-2 pr-6 d-flex align-center">
        <v-icon class="mr-2">mdi-text-box</v-icon>
        <span class="text-h5"
          >Phrases for {{ selectedCustomVocabulary?.vocabulary_name }}</span
        >
        <v-spacer></v-spacer>
        <v-btn color="primary" @click="openPhraseDialog()">
          Add Phrase
          <v-icon right>mdi-plus</v-icon>
        </v-btn>
      </v-card-title>

      <v-card-text class="pb-0">
        <v-data-table
          :headers="phraseHeaders"
          :items="phrases"
          :loading="phrasesLoading"
          :items-per-page="10"
          :server-items-length="phrasesPagination.total_records"
          @update:options="handlePhrasesPageChange"
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
            <v-icon small class="mr-2" @click="openPhraseDialog(item)">mdi-pencil</v-icon>
            <v-icon small @click="deletePhrase(item)">mdi-delete</v-icon>
          </template>
        </v-data-table>
      </v-card-text>

      <v-card-actions class="d-flex justify-end px-10 pb-8">
        <v-btn
          color="primary"
          @click="closePhrasesDialog"
          variant="flat"
          class="text-white"
        >
          Close
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <!-- Phrase Dialog -->
  <v-dialog
    persistent
    v-model="phraseDialog"
    overlay-color="black"
    :overlay-opacity="0.5"
    max-width="50%"
  >
    <v-card>
      <v-card-title class="pt-6 pb-2 pr-6">
        <v-icon class="mr-2">mdi-text-box</v-icon>
        <span class="text-h5">{{ phraseFormTitle }}</span>
      </v-card-title>

      <v-card-text class="pb-0">
        <v-form ref="phraseFormRef">
          <v-container class="py-0">
            <v-row dense>
              <v-col cols="6" class="py-1">
                <v-text-field
                  v-model="editedPhrase.phrase"
                  label="Phrase"
                  required
                  :rules="[
                    // 1. Basic required check
                    (v) => !!v || 'Phrase is required',
                    (v) => {
                      // Skip if it's a valid acronym
                      if (v.match(/^([A-Z]\.)+(-s)?$/)) return true;

                      // For non-acronyms, check if it's a valid word with mixed case
                      // Allows: letters (upper/lowercase), hyphens between words
                      // Examples: Hello, hello-World, My-Phrase, hello-world
                      return (
                        v.match(/^[A-Za-z]+(-[A-Za-z]+)*$/) ||
                        'Words must be hyphen-separated and can start with upper or lowercase letters'
                      );
                    },
                    // 2. Character validation (most basic to most specific)
                    (v) =>
                      /^[A-Za-z.-]+$/.test(v) ||
                      'Only letters, periods, and hyphens are allowed',

                    // 3. No spaces rule
                    (v) =>
                      !v.includes(' ') ||
                      'Spaces are not allowed. Use hyphens (-) to separate words',

                    // 4. Number check
                    (v) =>
                      !v.match(/\d/) ||
                      'Numbers must be spelled out (e.g., zero, one, etc.)',

                    // 5. Acronym formatting rules
                    (v) => {
                      // Skip if no periods or uppercase letters (not an acronym)
                      if (!v.includes('.') && !v.match(/[A-Z]/)) return true;
                      if (v.match(/^[A-Za-z]+(-[A-Za-z]+)*$/)) return true;

                      // If it has periods, must be properly formatted acronym
                      return (
                        v.match(/^([A-Z]\.)+(-s)?$/) ||
                        'Acronyms must be formatted as A.B.C. or A.B.C.-s for plurals'
                      );
                    },

                    // 6. Non-acronym word formatting
                    (v) => {
                      // Skip if it's a valid acronym
                      if (v.match(/^([A-Z]\.)+(-s)?$/)) return true;

                      // For non-acronyms, no trailing periods allowed
                      return !v.endsWith('.') || 'Only acronyms can end with a period';
                    },

                    // 7. Multiple uppercase letters check
                    (v) => {
                      // Skip if it's a valid acronym
                      if (v.match(/^([A-Z]\.)+(-s)?$/)) return true;

                      // For non-acronyms, no consecutive uppercase letters
                      return (
                        !v.match(/[A-Z]{2,}/) ||
                        'Multiple uppercase letters must be separated by periods (e.g., D.B.)'
                      );
                    },
                  ]"
                  density="comfortable"
                  variant="outlined"
                  placeholder="e.g., Los-Angeles, A.B.C., Dynamo-D.B."
                  class="custom-field"
                ></v-text-field>
              </v-col>
              <v-col cols="6" class="py-1 d-flex align-center justify-left mb-4">
                <a
                  :href="'https://docs.aws.amazon.com/transcribe/latest/dg/custom-vocabulary-create-table.html'"
                  target="_blank"
                >
                  AWS Transcribe Phrase Documentation
                </a>
              </v-col>
              <v-col cols="6" class="py-1">
                <v-text-field
                  v-model="editedPhrase.display_as"
                  label="Display As"
                  rows="3"
                  density="comfortable"
                  variant="outlined"
                  class="custom-field"
                  placeholder="e.g., Andorra la Vella"
                  hint="Optional. Defines how the phrase will appear in transcription output. Can include spaces and numbers."
                  persistent-hint
                ></v-text-field>
              </v-col>
              <v-col cols="6" class="py-1">
                <v-select
                  v-model="editedPhrase.status"
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
          @click="savePhrase()"
          :loading="phraseSaving"
          :disabled="phraseSaving"
          variant="flat"
          class="text-white mr-4"
        >
          Save
        </v-btn>
        <v-btn
          color="primary"
          @click="closePhraseDialog"
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
import { CustomVocabularyService } from "@services/customVocabulary.service";
import { languageCodes } from "@/src/types/common";
import {
  CustomVocabularyRequest,
  CustomVocabularyResponse,
  VocabularyPhraseRequest,
  VocabularyPhraseResponse,
} from "@/src/types/customVocabulary";
import { StatusEnum } from "@/src/types/common";
import { useStore } from "vuex";
import { CustomVocabularyCanPerformEnum } from "@/src/types/customVocabulary";

export default {
  name: "CustomVocabularies",
  setup() {
    const formValid = ref(true);
    const formRef = ref();
    const phraseFormRef = ref();
    const customVocabularies = ref<CustomVocabularyResponse[]>([]);
    const phrases = ref<VocabularyPhraseResponse[]>([]);
    const loading = ref(false);
    const phrasesLoading = ref(false);
    const search = ref("");
    const dialog = ref(false);
    const phrasesDialog = ref(false);
    const phraseDialog = ref(false);
    const editedIndex = ref(-1);
    const phraseEditedIndex = ref(-1);
    const store = useStore();
    const teamId = computed(() => store.getters.selectedTeamId);
    const selectedCustomVocabulary = ref<CustomVocabularyResponse | null>(null);
    const snackbar = ref({
      show: false,
      message: "",
      color: "success",
    });

    const selectedCustomVocabularyId = ref<string>();
    const selectedAction = ref("");

    // Watch for changes to selectedCustomVocabularyId and update selectedCustomVocabulary
    watch(selectedCustomVocabularyId, (newId) => {
      if (newId) {
        selectedCustomVocabulary.value =
          customVocabularies.value.find((vocabulary) => vocabulary.id === newId) || null;
      } else {
        selectedCustomVocabulary.value = null;
      }
    });

    const publishLoading = ref(false);
    const actionItems = [
      {
        title: "Edit Vocabulary",
        value: "edit",
        icon: "mdi-pencil",
        requiredPermission: CustomVocabularyCanPerformEnum.Edit,
      },
      {
        title: "Delete Vocabulary",
        value: "delete",
        icon: "mdi-delete",
        requiredPermission: CustomVocabularyCanPerformEnum.Delete,
      },
      {
        value: "separator",
      },
      {
        title: "View Phrases",
        value: "view",
        icon: "mdi-eye",
        requiredPermission: CustomVocabularyCanPerformEnum.EditPhrases,
      },
      {
        title: "Publish Vocabulary",
        value: "publish",
        icon: "mdi-publish",
        requiredPermission: CustomVocabularyCanPerformEnum.Publish,
        loading: computed(() => publishLoading.value),
      },
    ];
    const getStatusColor = (status: string) => {
      switch (status) {
        case "Created":
          return "info"; // blue for initial state
        case "Publishing":
          return "warning"; // orange/yellow for in-progress
        case "Published":
          return "success"; // green for successful completion
        case "PublishFailed":
          return "error"; // red for failure
        default:
          return "grey"; // fallback color
      }
    };

    const executeSelectedAction = () => {
      if (!selectedAction.value || !selectedCustomVocabularyId.value) return;

      const selectedCustomVocabulary = customVocabularies.value.find(
        (vocabulary) => vocabulary.id === selectedCustomVocabularyId.value
      );
      if (!selectedCustomVocabulary) return;

      switch (selectedAction.value) {
        case "view":
          viewPhrases(selectedCustomVocabulary);
          break;
        case "edit":
          openDialog(selectedCustomVocabulary);
          break;
        case "delete":
          deleteCustomVocabulary(selectedCustomVocabulary);
          break;
        case "publish":
          publishVocabulary(selectedCustomVocabulary);
          break;
      }
      selectedAction.value = "";
    };

    const headers = [
      { title: "", key: "select", sortable: false, width: "50px" },
      { title: "Name", key: "vocabulary_name" },
      { title: "Description", key: "description" },
      { title: "Publish Status", key: "current_step" },
      { title: "Language", key: "language_code" },
      { title: "Status", key: "status" },
      { title: "Created At", key: "created_at" },
      { title: "Updated At", key: "updated_at" },
    ];

    const phraseHeaders = [
      { title: "Phrase", key: "phrase" },
      { title: "Display As", key: "display_as" },
      { title: "Status", key: "status" },
      { title: "Created At", key: "created_at" },
      { title: "Updated At", key: "updated_at" },
      { title: "Actions", key: "actions", sortable: false },
    ];

    const statusOptions = Object.values(StatusEnum);

    const editedItem = ref<CustomVocabularyRequest>({
      vocabulary_name: "",
      description: "",
      status: StatusEnum.Active,
    });

    const editedPhrase = ref<VocabularyPhraseRequest>({
      phrase: "",
      display_as: "",
      status: StatusEnum.Active,
    });

    const defaultVocabularyItem: CustomVocabularyRequest = {
      vocabulary_name: "",
      description: "",
      language_code: "en-US",
      status: StatusEnum.Active,
    };

    const defaultPhraseItem: VocabularyPhraseRequest = {
      phrase: "",
      display_as: "",
      status: StatusEnum.Active,
    };

    const formTitle = computed(() => {
      return editedIndex.value === -1
        ? "New Custom Vocabulary"
        : "Edit Custom Vocabulary";
    });

    const phraseFormTitle = computed(() => {
      return phraseEditedIndex.value === -1 ? "New Phrase" : "Edit Phrase";
    });

    // Pagination state for vocabularies
    const vocabPagination = ref({
      page_index: 1,
      total_pages: 1,
      has_previous_page: false,
      has_next_page: false,
      total_records: 0,
    });

    const fetchCustomVocabularies = async (pageIndex = 1, pageSize = 10) => {
      try {
        loading.value = true;
        const response = await CustomVocabularyService.getCustomVocabularies(
          teamId.value,
          pageIndex,
          pageSize
        );
        customVocabularies.value = response.records || [];

        // Store pagination information
        vocabPagination.value = {
          page_index: response.page_index,
          total_pages: response.total_pages,
          has_previous_page: response.has_previous_page,
          has_next_page: response.has_next_page,
          total_records: response.total_records,
        };

        selectedCustomVocabulary.value = null;
        selectedCustomVocabularyId.value = "";
      } catch (error) {
        showError("Failed to fetch custom vocabularies");
      } finally {
        loading.value = false;
      }
    };

    // Pagination state for phrases
    const phrasesPagination = ref({
      page_index: 1,
      total_pages: 1,
      has_previous_page: false,
      has_next_page: false,
      total_records: 0,
    });

    const fetchPhrases = async (pageIndex: number, pageSize: number) => {
      if (!selectedCustomVocabulary.value?.id) return;
      try {
        phrasesLoading.value = true;
        currentPhrasePage.value = pageIndex;
        currentPhrasePageLength.value = pageSize;
        const response = await CustomVocabularyService.getVocabularyPhrases(
          teamId.value,
          selectedCustomVocabulary.value.id,
          currentPhrasePage.value,
          currentPhrasePageLength.value
        );
        phrases.value = response.records || [];

        // Store pagination information
        phrasesPagination.value = {
          page_index: response.page_index,
          total_pages: response.total_pages,
          has_previous_page: response.has_previous_page,
          has_next_page: response.has_next_page,
          total_records: response.total_records,
        };
      } catch (error) {
        showError("Failed to fetch phrases");
      } finally {
        phrasesLoading.value = false;
      }
    };

    const handlePageChange = (options: any) => {
      const page = options.page || 1;
      const itemsPerPage = options.itemsPerPage || 10;
      fetchCustomVocabularies(page, itemsPerPage);
    };

    const handlePhrasesPageChange = (options: any) => {
      const page = options.page || 1;
      const itemsPerPage = options.itemsPerPage || 10;
      fetchPhrases(page, itemsPerPage);
    };

    const openDialog = (item?: CustomVocabularyResponse) => {
      if (item) {
        editedIndex.value = customVocabularies.value.indexOf(item);
        editedItem.value = { ...item };
      } else {
        editedIndex.value = -1;
        editedItem.value = { ...defaultVocabularyItem };
      }
      dialog.value = true;
      setTimeout(() => {
        const titleInput = document.querySelector('.v-dialog input[type="text"]');
        if (titleInput) {
          (titleInput.previousElementSibling as HTMLElement)?.focus();
        }
      }, 100);
    };

    const closeDialog = () => {
      dialog.value = false;
      selectedCustomVocabularyId.value = null;
      setTimeout(() => {
        editedItem.value = { ...defaultVocabularyItem };
        editedIndex.value = -1;
      }, 300);
    };

    // Keep track of current phrase page
    const currentPhrasePage = ref(1);
    const currentPhrasePageLength = ref(10);

    const viewPhrases = (item: CustomVocabularyResponse) => {
      console.log("Viewing phrases for vocabulary:", item.vocabulary_name);
      selectedCustomVocabulary.value = item;
      phrasesDialog.value = true;
      fetchPhrases(currentPhrasePage.value, currentPhrasePageLength.value);
    };

    const closePhrasesDialog = () => {
      phrasesDialog.value = false;
      selectedCustomVocabularyId.value = null;
      phrases.value = [];
    };

    const openPhraseDialog = (item?: VocabularyPhraseResponse) => {
      if (item) {
        phraseEditedIndex.value = phrases.value.indexOf(item);
        editedPhrase.value = { ...item };
      } else {
        phraseEditedIndex.value = -1;
        editedPhrase.value = { ...defaultPhraseItem };
      }
      phraseDialog.value = true;
      setTimeout(() => {
        const titleInput = document.querySelector(
          '.v-overlay__content input[type="text"]'
        );
        if (titleInput) {
          (titleInput.previousElementSibling as HTMLElement)?.focus();
        }
      }, 100);
    };

    const closePhraseDialog = () => {
      phraseDialog.value = false;
      setTimeout(() => {
        editedPhrase.value = { ...defaultPhraseItem };
        phraseEditedIndex.value = -1;
      }, 300);
    };

    const saving = ref(false);
    const phraseSaving = ref(false);

    const saveCustomVocabulary = async () => {
      try {
        saving.value = true;
        if (editedIndex.value > -1) {
          const vocabulary = customVocabularies.value[editedIndex.value];
          await CustomVocabularyService.updateCustomVocabulary(
            teamId.value,
            vocabulary.id!,
            editedItem.value
          );
          showSuccess("Custom vocabulary updated successfully");
        } else {
          await CustomVocabularyService.createCustomVocabulary(
            teamId.value,
            editedItem.value
          );
          showSuccess("Custom vocabulary created successfully");
        }
        await fetchCustomVocabularies();
        closeDialog();
      } catch (error) {
        showError("Failed to save custom vocabulary");
      } finally {
        saving.value = false;
        selectedCustomVocabularyId.value = null;
      }
    };

    const savePhrase = async () => {
      if (!selectedCustomVocabulary.value?.id) return;

      // Check if form is valid before proceeding
      const { valid } = await phraseFormRef.value.validate();
      if (!valid) {
        showError("Please fix the validation errors before saving");
        return;
      }

      try {
        phraseSaving.value = true;
        if (phraseEditedIndex.value > -1) {
          const phrase = phrases.value[phraseEditedIndex.value];
          console.log("Updating phrase:", phrase);
          await CustomVocabularyService.updateVocabularyPhrase(
            teamId.value,
            selectedCustomVocabulary.value.id,
            phrase.id,
            editedPhrase.value
          );
          showSuccess("Phrase updated successfully");
        } else {
          await CustomVocabularyService.createVocabularyPhrase(
            teamId.value,
            selectedCustomVocabulary.value.id,
            editedPhrase.value
          );
          showSuccess("Phrase created successfully");
        }
        // Fetch the updated phrases from the server to ensure type consistency
        await fetchPhrases(currentPhrasePage.value, currentPhrasePageLength.value);
        closePhraseDialog();
      } catch (error) {
        showError("Failed to save phrase");
      } finally {
        phraseSaving.value = false;
      }
    };

    const deleteCustomVocabulary = async (item: CustomVocabularyResponse) => {
      if (!confirm("Are you sure you want to delete this custom vocabulary?")) return;

      try {
        loading.value = true;
        await CustomVocabularyService.deleteCustomVocabulary(teamId.value, item.id!);
        await fetchCustomVocabularies();
        showSuccess("Custom vocabulary deleted successfully");
      } catch (error) {
        showError("Failed to delete custom vocabulary");
      } finally {
        loading.value = false;
        selectedCustomVocabularyId.value = null;
      }
    };

    const deletePhrase = async (item: VocabularyPhraseResponse) => {
      if (
        !selectedCustomVocabulary.value?.id ||
        !confirm("Are you sure you want to delete this phrase?")
      )
        return;

      try {
        loading.value = true;

        await CustomVocabularyService.deleteVocabularyPhrase(
          teamId.value,
          selectedCustomVocabulary.value.id,
          item.id
        );
        // Remove the phrase from the local array instead of fetching again
        phrases.value = phrases.value.filter((p) => p.id !== item.id);
        showSuccess("Phrase deleted successfully");
        loading.value = false;
      } catch (error) {
        showError("Failed to delete phrase");
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

    const publishVocabulary = async (item: CustomVocabularyResponse) => {
      if (!confirm("Are you sure you want to publish this custom vocabulary?")) return;

      try {
        loading.value = true;
        publishLoading.value = true;
        await CustomVocabularyService.publishCustomVocabulary(teamId.value, item.id!);
        showSuccess("Custom vocabulary published successfully");
        await fetchCustomVocabularies();
      } catch (error) {
        showError("Failed to publish custom vocabulary");
      } finally {
        publishLoading.value = false;
        loading.value = false;
        selectedCustomVocabularyId.value = undefined;
      }
    };

    const handleAction = (action: any, item: CustomVocabularyResponse) => {
      switch (action.value) {
        case "view":
          viewPhrases(item);
          break;
        case "edit":
          openDialog(item);
          break;
        case "delete":
          deleteCustomVocabulary(item);
          break;
        case "publish":
          publishVocabulary(item);
          break;
      }
    };

    return {
      customVocabularies,
      phrases,
      loading,
      phrasesLoading,
      search,
      headers,
      phraseHeaders,
      dialog,
      phrasesDialog,
      phraseDialog,
      editedIndex,
      phraseEditedIndex,
      editedItem,
      editedPhrase,
      selectedCustomVocabulary,
      formTitle,
      phraseFormTitle,
      snackbar,
      statusOptions,
      handlePageChange,
      handlePhrasesPageChange,
      openDialog,
      closeDialog,
      viewPhrases,
      closePhrasesDialog,
      openPhraseDialog,
      closePhraseDialog,
      saveCustomVocabulary,
      savePhrase,
      deleteCustomVocabulary,
      deletePhrase,
      fetchCustomVocabularies,
      handleAction,
      saving,
      selectedCustomVocabularyId,
      actionItems,
      phraseSaving,
      formValid,
      dateFormatter,
      executeSelectedAction,
      selectedAction,
      languageCodes,
      getStatusColor,
      phraseFormRef,
      vocabPagination,
      phrasesPagination,
    };
  },
};
</script>
