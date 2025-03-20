<!--
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
 -->

<template>
  <v-card outlined elevation="5" style="font-size: 0.9rem">
    <v-card-title class="d-flex align-center mt-2">
      <v-icon class="mr-2">mdi-calendar</v-icon>
      Meetings
      <v-spacer></v-spacer>
      <v-menu>
        <template v-slot:activator="{ props }">
          <v-btn
            color="primary"
            v-bind="props"
            class="ml-2"
            :disabled="
              !selectedMeetingId ||
              selectedMeeting?.can_perform?.includes(CanPerformEnum.Nothing)
            "
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
                !selectedMeeting?.can_perform?.includes(action.requiredPermission)
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
      <v-btn color="primary" class="ml-2" @click="openMeetingDialog()">
        Add Meeting
        <v-icon right>mdi-plus</v-icon>
      </v-btn>
      <v-btn color="secondary" class="ml-2" @click="fetchMeetings()">
        <v-icon right>mdi-refresh</v-icon>
      </v-btn>
    </v-card-title>

    <v-data-table
      :headers="meetingTableHeaders"
      :items="meetings"
      :search="search"
      loading-text="Loading meetings..."
      :loading="loading"
      :items-per-page="pageSize"
      :page="currentPage"
      @update:options="handlePageChange"
    >
      <template v-slot:item.date="{ item }">
        {{ dateFormatter.format(item.date) }}
      </template>

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
        <v-tooltip
          :text="getStepTooltip(item)"
          location="top"
          v-if="shouldShowErrorTooltip(item)"
        >
          <template v-slot:activator="{ props }">
            <v-chip
              :color="getProgressColor(item.current_step!)"
              v-bind="props"
              text-color="white"
              small
            >
              <template v-slot:prepend>
                <v-progress-circular
                  v-if="isProcessingState(item.current_step)"
                  :width="3"
                  class="mr-1"
                  indeterminate
                  :size="20"
                />
              </template>
              <v-icon
                v-if="
                  item.custom_vocabulary &&
                  item.custom_vocabulary?.current_step !==
                    CustomVocabularyProgressEnum.Published &&
                  item.current_step !== MeetingSetupProgressEnum.Sealed &&
                  item.current_step !== MeetingSetupProgressEnum.Sealing
                "
              >
                mdi-alert-box-outline
              </v-icon>
              {{ item.current_step }}
            </v-chip>
          </template>
        </v-tooltip>

        <v-chip
          :color="
           item.custom_vocabulary &&
           item.custom_vocabulary?.current_step !== CustomVocabularyProgressEnum.Published &&
           item.current_step !== MeetingSetupProgressEnum.Sealed
             ? 'error'
             : getProgressColor(item.current_step!)
         "
          v-if="!shouldShowErrorTooltip(item)"
          text-color="white"
          small
        >
          <template v-slot:prepend>
            <v-progress-circular
              v-if="isProcessingState(item.current_step)"
              :width="3"
              class="mr-1"
              indeterminate
              :size="20"
            />
          </template>
          <v-icon
            v-if="
              item.custom_vocabulary &&
              item.custom_vocabulary?.current_step !==
                CustomVocabularyProgressEnum.Published &&
              item.current_step !== MeetingSetupProgressEnum.Sealed &&
              item.current_step !== MeetingSetupProgressEnum.Sealing
            "
          >
            mdi-alert-box-outline
          </v-icon>
          {{ item.current_step }}
        </v-chip>
      </template>

      <template v-slot:item.created_at="{ item }">
        {{ dateFormatter.format(item.created_at) }}
      </template>

      <template v-slot:item.updated_at="{ item }">
        {{ dateFormatter.format(item.updated_at) }}
      </template>

      <template v-slot:item.title="{ item }">
        <v-tooltip :text="item.description" max-width="400" location="right">
          <template v-slot:activator="{ props }">
            <span v-bind="props">{{ item.title }}</span>
          </template>
        </v-tooltip>
      </template>

      <template v-slot:item.select="{ item }">
        <v-radio-group v-model="selectedMeetingId" hide-details class="ma-0 pa-0">
          <v-radio :value="item.id" hide-details class="ma-0 pa-0" />
        </v-radio-group>
      </template>
      <!-- Actions are now handled by the top dropdown -->
    </v-data-table>
  </v-card>

  <v-dialog v-model="dialog" overlay-color="black" overlay-opacity="5" max-width="50%">
    <v-card>
      <v-card-title class="pt-6 pb-2 pr-6">
        <v-icon class="mr-2">mdi-calendar</v-icon>
        <span class="text-h5">{{ formTitle }}</span>
      </v-card-title>

      <v-card-text class="pb-0">
        <v-form ref="formRef" v-model="formValid">
          <v-container class="py-0">
            <v-row dense>
              <v-col cols="8" class="py-1">
                <v-text-field
                  v-model="editedItem.title"
                  label="Title"
                  required
                  :rules="[(v) => !!v || 'Title is required']"
                  density="comfortable"
                  variant="outlined"
                  class="custom-field"
                ></v-text-field>
              </v-col>
              <v-col cols="12" md="4" class="py-1">
                <v-select
                  v-model="editedItem.status"
                  label="Status"
                  :items="statusOptions"
                  required
                  :rules="[(v) => !!v || 'Status is required']"
                  density="comfortable"
                  variant="outlined"
                  class="custom-field"
                />
              </v-col>

              <v-col cols="12" md="12" class="py-1">
                <v-textarea
                  v-model="editedItem.description"
                  placeholder="Leave empty to be generated by the GenAI model"
                  label="Description"
                  density="comfortable"
                  variant="outlined"
                  class="custom-field"
                ></v-textarea>
              </v-col>

              <v-col cols="12" md="4" class="py-1">
                <v-text-field
                  v-model="editedItem.date"
                  label="Date"
                  type="datetime-local"
                  required
                  :rules="[(v) => !!v || 'Date is required']"
                  density="comfortable"
                  variant="outlined"
                  class="custom-field"
                ></v-text-field>
              </v-col>

              <v-col cols="12" md="4" class="py-1">
                <v-select
                  v-model="editedItem.prompt_set_id"
                  label="Prompt Set"
                  :items="promptSets"
                  item-title="prompt_set_name"
                  item-value="id"
                  :rules="[(v) => !!v || 'Prompt Set is required']"
                  :loading="loadingPromptSets"
                  :error-messages="promptSetsError"
                  required
                  density="comfortable"
                  variant="outlined"
                  class="custom-field"
                ></v-select>
              </v-col>
              <v-col cols="12" md="4" class="py-1">
                <v-select
                  v-model="editedItem.custom_vocabulary_id"
                  label="Custom Vocabulary"
                  :items="[
                    { vocabulary_name: 'No Custom Vocabulary', id: null },
                    ...customVocabularies,
                  ]"
                  item-title="vocabulary_name"
                  item-value="id"
                  density="comfortable"
                  :loading="loadingCustomVocabularies"
                  :error-messages="customVocabulariesError"
                  variant="outlined"
                  class="custom-field"
                ></v-select>
              </v-col>

              <v-col cols="12" md="4" class="py-1">
                <v-select
                  v-model="editedItem.custom_model_id"
                  label="Custom Model"
                  :items="[{ model_name: 'General Model', id: null }, ...customModels]"
                  item-title="model_name"
                  :disabled="editedIndex > -1"
                  item-value="id"
                  density="comfortable"
                  :loading="loadingCustomModels"
                  :error-messages="customModelsError"
                  variant="outlined"
                  class="custom-field"
                ></v-select>
              </v-col>
              <v-col cols="12" md="8" class="py-1">
                <v-checkbox
                  v-model="editedItem.include_in_model_training"
                  :disabled="editedItem.custom_model_id == null"
                  label="Include In Model Training Once Sealed"
                ></v-checkbox>
              </v-col>
            </v-row>
          </v-container>
        </v-form>
      </v-card-text>

      <v-card-actions class="d-flex justify-end px-10 pb-8">
        <v-btn
          color="primary"
          @click="saveMeeting"
          :loading="saving"
          :disabled="saving || !formValid"
          variant="flat"
          class="text-white mr-4"
        >
          Save
        </v-btn>
        <v-btn
          color="primary"
          @click="closeMeetingDialog"
          variant="flat"
          class="text-white"
        >
          Cancel
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

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
        <span class="text-h5">Upload Meeting Recording</span>
      </v-card-title>

      <v-card-text class="pb-0">
        <v-container class="py-0">
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
              accept=".mp4,mp3"
            />
            <v-icon size="48" color="primary" class="mb-2" :disabled="isUploading"
              >mdi-cloud-upload</v-icon
            >
            <div class="text-h6" :disabled="isUploading">
              Drag and drop your file here
            </div>
            <div class="text-subtitle-1" :disabled="isUploading">or click to browse</div>
            <div class="text-caption mt-2" :disabled="isUploading">
              Supported formats: .mp4,mp3
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
        </v-container>
      </v-card-text>

      <v-card-actions class="d-flex justify-end px-10 pb-8">
        <v-btn
          color="primary"
          @click="closeUploadDialog"
          :loading="isUploading"
          variant="flat"
          class="text-white"
        >
          Cancel
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <PromptResponsesDialog
    v-model="promptResponsesDialog"
    :prompt-responses="promptResponses"
    :loading="loadingPromptResponses"
    @export-prompt-responses="exportPromptResponses"
  />

  <v-dialog persistent v-model="meetingNotes" max-width="90%">
    <v-card style="max-height: 90vh; display: flex; flex-direction: column">
      <v-card-title class="d-flex justify-space-between align-center">
        <div><v-icon class="mr-2">mdi-file-edit-outline</v-icon> Meeting Notes</div>
        <div
          key="notes_controls"
          class="d-flex justify-end justify-content-end"
          style="width: 100%"
          v-if="!isLoadingMeetingNotesObjects"
        >
          <div class="mb-12" style="height: 3px; min-width: 500px">
            <div class="confidence-chart-container">
              <div class="confidence-chart">
                <v-tooltip location="top">
                  <template v-slot:activator="{ props }">
                    <div
                      class="confidence-segment very-high"
                      v-bind="props"
                      :style="{ width: (confidenceStats.veryHigh || 0.1) + '%' }"
                    >
                      {{
                        confidenceStats.veryHigh > 5
                          ? confidenceStats.veryHigh.toFixed(1) + "%"
                          : ""
                      }}
                    </div>
                  </template>
                  Very High (98-100%): {{ confidenceStats.veryHigh.toFixed(1) }}%
                </v-tooltip>
                <v-tooltip location="top">
                  <template v-slot:activator="{ props }">
                    <div
                      class="confidence-segment high"
                      v-bind="props"
                      :style="{ width: (confidenceStats.high || 0.1) + '%' }"
                    >
                      {{
                        confidenceStats.high > 5
                          ? confidenceStats.high.toFixed(1) + "%"
                          : ""
                      }}
                    </div>
                  </template>
                  High (90-98%): {{ confidenceStats.high.toFixed(1) }}%
                </v-tooltip>
                <v-tooltip location="top">
                  <template v-slot:activator="{ props }">
                    <div
                      class="confidence-segment medium"
                      v-bind="props"
                      :style="{ width: (confidenceStats.medium || 0.1) + '%' }"
                    >
                      {{
                        confidenceStats.medium > 5
                          ? confidenceStats.medium.toFixed(1) + "%"
                          : ""
                      }}
                    </div>
                  </template>
                  Medium (60-90%): {{ confidenceStats.medium.toFixed(1) }}%
                </v-tooltip>
                <v-tooltip location="top">
                  <template v-slot:activator="{ props }">
                    <div
                      class="confidence-segment low"
                      v-bind="props"
                      :style="{ width: (confidenceStats.low || 0.1) + '%' }"
                    >
                      {{
                        confidenceStats.low > 5
                          ? confidenceStats.low.toFixed(1) + "%"
                          : ""
                      }}
                    </div>
                  </template>
                  Low (40-60%): {{ confidenceStats.low.toFixed(1) }}%
                </v-tooltip>
                <v-tooltip location="top">
                  <template v-slot:activator="{ props }">
                    <div
                      class="confidence-segment very-low"
                      v-bind="props"
                      :style="{ width: (confidenceStats.veryLow || 0.1) + '%' }"
                    >
                      {{
                        confidenceStats.veryLow > 5
                          ? confidenceStats.veryLow.toFixed(1) + "%"
                          : ""
                      }}
                    </div>
                  </template>
                  Very Low (0-20%): {{ confidenceStats.veryLow.toFixed(1) }}%
                </v-tooltip>
              </div>
            </div>
          </div>

          <div class="d-flex justify-content-end">
            <v-divider :thickness="3" vertical class="rd-4 mr-4 ml-4" />

            <div style="display: flex; align-items: center; gap: 10px; font-size: 12px">
              <v-btn
                @click="confidenceThreshold--"
                variant="flat"
                color="info"
                size="small"
                :disabled="confidenceThreshold <= 1"
              >
                <v-icon>mdi-minus</v-icon>
              </v-btn>
              <h3>Confidence: {{ confidenceThreshold }}%</h3>
              <v-btn
                @click="confidenceThreshold++"
                variant="flat"
                size="small"
                color="info"
                :disabled="confidenceThreshold >= 100"
              >
                <v-icon>mdi-plus</v-icon>
              </v-btn>
            </div>
            <v-divider :thickness="3" vertical class="rd-4 mr-4 ml-4" />

            <div style="display: flex; align-items: center; gap: 10px; font-size: 12px">
              <v-menu offset-y>
                <template v-slot:activator="{ props }">
                  <v-btn variant="flat" color="info" size="small" v-bind="props">
                    <v-icon>mdi-keyboard</v-icon>
                  </v-btn>
                </template>
                <v-card min-width="300" class="shortcuts-card">
                  <v-card-title class="d-flex align-center">
                    <v-icon class="mr-2">mdi-keyboard</v-icon>
                    Keyboard Shortcuts
                  </v-card-title>
                  <v-divider></v-divider>
                  <v-list density="compact">
                    <v-list-subheader>Mouse Operations</v-list-subheader>
                    <v-list-item>
                      <v-list-item-title class="d-flex align-center">
                        <span class="shortcut-key">Click</span>
                        <span class="ml-auto">Select a word</span>
                      </v-list-item-title>
                    </v-list-item>
                    <v-list-item>
                      <v-list-item-title class="d-flex align-center">
                        <span class="shortcut-key">Ctrl/⌘ + Click</span>
                        <span class="ml-auto">Multiple selection</span>
                      </v-list-item-title>
                    </v-list-item>
                    <v-list-item>
                      <v-list-item-title class="d-flex align-center">
                        <span class="shortcut-key">Shift + Click</span>
                        <span class="ml-auto">Edit word(s)</span>
                      </v-list-item-title>
                    </v-list-item>
                    <v-list-item>
                      <v-list-item-title class="d-flex align-center">
                        <span class="shortcut-key">Alt/⌥ + Click</span>
                        <span class="ml-auto">Add to vocabulary</span>
                      </v-list-item-title>
                    </v-list-item>

                    <v-divider class="my-2"></v-divider>
                    <v-list-subheader>Keyboard Operations</v-list-subheader>
                    <v-list-item>
                      <v-list-item-title class="d-flex align-center">
                        <span class="shortcut-key">Delete</span>
                        <span class="ml-auto">Delete selected words</span>
                      </v-list-item-title>
                    </v-list-item>
                    <v-list-item>
                      <v-list-item-title class="d-flex align-center">
                        <span class="shortcut-key">Esc</span>
                        <span class="ml-auto">Clear selection</span>
                      </v-list-item-title>
                    </v-list-item>
                  </v-list>
                </v-card>
              </v-menu>
            </div>
          </div>
        </div>
      </v-card-title>
      <v-card-text
        v-if="isLoadingMeetingNotesObjects"
        class="d-flex flex-column justify-center align-center"
      >
        <v-card-title class="text-center mb-4">{{
          meetingNotesLoadingMessage
        }}</v-card-title>
        <v-progress-linear
          :value="downloadProgressMeetingObject"
          :key="downloadProgressMeetingObject"
          :buffer-value="downloadProgressMeetingObject"
          color="primary"
          height="25"
          striped
          class="w-100"
        >
          <template v-slot:default>
            <strong>{{ Math.ceil(downloadProgressMeetingObject) }}%</strong>
          </template>
        </v-progress-linear>
      </v-card-text>
      <v-divider :thickness="1" class="rd-4 mr-4 ml-4" />

      <div class="d-flex" style="flex: 1; overflow: hidden">
        <div
          style="width: 70%; border-right: 1px solid rgba(0, 0, 0, 0.12); overflow: auto"
        >
          <v-card-text
            v-if="!isLoadingMeetingNotesObjects"
            style="padding: 10px; height: 100%"
            @keydown="handleKeyDown"
          >
            <div v-for="segment in meetingNotesObject?.segments">
              <div class="speaker-label clickable" @click="openSpeakerDialog(segment)">
                Speaker: {{ segment.speaker_label }}
                <span class="timestamp" v-if="segment.end_time">
                  ({{ formatPlayerSecondsToTime(segment.start_time) }} -
                  {{ formatPlayerSecondsToTime(segment.end_time) }})
                </span>
              </div>
              <div class="segment-content">
                <span
                  v-for="word in getSegmentWords(segment)"
                  :key="word.word_id"
                  class="clickable notewords"
                  :class="getWordClasses(word)"
                  @mousedown.prevent
                  @click="(event) => showConfidenceDebounced(word, word.word_id, event)"
                >
                  {{ word.content.trim() }}
                </span>
              </div>
            </div>
          </v-card-text>
        </div>
        <div
          style="width: 30%; overflow-y: auto; padding: 20px"
          v-if="!isLoadingMeetingNotesObjects"
        >
          <!-- Word Information Section -->
          <div v-if="selectedWords.length === 1" class="mb-4 word-info pa-3">
            <div class="d-flex justify-left">
              <strong class="mr-2">Word:</strong>
              {{ selectedWords[0].word.content?.trim() }}
            </div>
            <div class="d-flex justify-left">
              <strong class="mr-2">Confidence:</strong>
              {{ Math.round(selectedWords[0].word.confidence * 100) }}%
            </div>
            <div class="d-flex justify-left">
              <strong class="mr-2">Time Range: </strong>
              {{ formatPlayerSecondsToTime(selectedWords[0].word.start_time) }} -
              {{ formatPlayerSecondsToTime(selectedWords[0].word.end_time) }}
            </div>
          </div>

          <!-- Changes Table -->
          <div class="changes-table">
            <!-- Single word selected - Show history for that word -->
            <div v-if="selectedWords.length === 1">
              <h4 class="mb-2">
                Changes History for "{{ selectedWords[0].word.content?.trim() }}"
              </h4>
              <v-table density="compact" hover>
                <thead>
                  <tr>
                    <th>Original</th>
                    <th>Changed To</th>
                    <th>By</th>
                    <th>Time</th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="change in meetingNotesObject?.changes?.filter(
                      (change) => change.word_id === selectedWords[0].word_id
                    )"
                    :key="change.timestamp"
                    class="highlight-change"
                  >
                    <td>{{ change.original }}</td>
                    <td>{{ change.change }}</td>
                    <td>{{ change.user }}</td>
                    <td>{{ new Date(change.timestamp).toLocaleString() }}</td>
                  </tr>
                </tbody>
              </v-table>
              <div
                v-if="
                  !meetingNotesObject?.changes?.some(
                    (change) => change.word_id === selectedWords[0].word_id
                  )
                "
                class="pa-3 text-center text-subtitle-1"
              >
                No changes for this word.
              </div>
            </div>

            <!-- Multiple words selected - Show words and confidence -->
            <div v-else-if="selectedWords.length > 1">
              <h4 class="mb-2">Selected Words</h4>
              <v-table density="compact" hover>
                <thead>
                  <tr>
                    <th>Word</th>
                    <th>Confidence</th>
                    <th>Start Time</th>
                    <th>End Time</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(selected, index) in selectedWords" :key="index">
                    <td>{{ selected.word.content?.trim() }}</td>
                    <td>{{ Math.round(selected.word.confidence * 100) }}%</td>
                    <td>{{ formatPlayerSecondsToTime(selected.word.start_time) }}</td>
                    <td>{{ formatPlayerSecondsToTime(selected.word.end_time) }}</td>
                  </tr>
                </tbody>
              </v-table>
            </div>

            <!-- No words selected - Show meeting statistics -->
            <div v-else>
              <h4 class="mb-2">Meeting Statistics</h4>
              <v-table density="compact" hover>
                <tbody>
                  <tr>
                    <td class="font-weight-bold">Total Words:</td>
                    <td>
                      {{
                        meetingNotesObject?.words?.filter(
                          (w) => w.word_type === WordType.Pronunciation
                        ).length || 0
                      }}
                    </td>
                  </tr>
                  <tr>
                    <td class="font-weight-bold">Recording Length:</td>
                    <td>
                      {{
                        meetingNotesObject?.words?.length
                          ? formatPlayerSecondsToTime(
                              meetingNotesObject.segments[
                                meetingNotesObject.segments.length - 1
                              ].end_time
                            )
                          : "00:00:00"
                      }}
                    </td>
                  </tr>
                  <tr>
                    <td class="font-weight-bold">Number of Changes:</td>
                    <td>
                      {{ meetingNotesObject?.changes?.length || 0 }}
                    </td>
                  </tr>
                  <tr>
                    <td class="font-weight-bold">Number of Speakers:</td>
                    <td>
                      {{
                        new Set(
                          meetingNotesObject?.segments?.map(
                            (segment) => segment.speaker_label
                          )
                        ).size || 0
                      }}
                    </td>
                  </tr>
                  <tr>
                    <td class="font-weight-bold">Confidence Level Distribution:</td>
                    <td>
                      <div>
                        Very High (98-100%): {{ confidenceStats.veryHigh.toFixed(1) }}%
                      </div>
                      <div>High (90-98%): {{ confidenceStats.high.toFixed(1) }}%</div>
                      <div>Medium (60-90%): {{ confidenceStats.medium.toFixed(1) }}%</div>
                      <div>Low (40-60%): {{ confidenceStats.low.toFixed(1) }}%</div>
                      <div>
                        Very Low (0-20%): {{ confidenceStats.veryLow.toFixed(1) }}%
                      </div>
                    </td>
                  </tr>
                </tbody>
              </v-table>
            </div>
          </div>
        </div>
      </div>
      <v-card-actions
        class="d-flex mr-4 ml-4"
        style="border-top: 1px solid rgba(0, 0, 0, 0.12)"
        v-if="!isLoadingMeetingNotesObjects"
      >
        <div class="d-flex flex-wrap w-100">
          <div class="d-flex align-center flex-grow-1">
            <b class="mr-1">Current Version: </b> {{ meetingNotesObject?.version }}
            <v-divider vertical :thickness="3" class="mr-4 ml-4" />
            <b class="mr-1">Last Updated By:</b> {{ meetingNotesObject?.last_edited_by }}
            <v-divider :thickness="3" vertical class="mr-4 ml-4" />
            <b class="mr-1">Last Updated At: </b
            >{{ dateFormatter.format(meetingNotesObject?.last_edited_at) }}
          </div>
          <v-divider
            :thickness="3"
            vertical
            class="mr-4 ml-4"
            v-if="!isLoadingMeetingNotesObjects"
          />

          <div class="d-flex">
            <v-btn
              :loading="
                downloadProgressMeetingObject > 0 && downloadProgressMeetingObject < 100
              "
              :disabled="!sound"
              @click="togglePlayback"
              color="info"
              variant="flat"
              class="mr-2"
            >
              <v-icon>{{ isPlayingAudio ? "mdi-pause" : "mdi-play" }}</v-icon>
            </v-btn>

            <v-btn
              :disabled="!sound || !isPlayingAudio"
              @click="stopPlayback"
              color="info"
              variant="flat"
              class="mr-2"
            >
              <v-icon>mdi-stop</v-icon>
            </v-btn>
            <v-divider :thickness="1" vertical class="mr-4" />
            <div class="text-center grey d-flex flex-column align-center justify-center">
              {{ formatPlayerSecondsToTime(currentTime) }}
            </div>
          </div>
        </div>

        <div class="d-flex align-center justify-end">
          <v-divider
            :thickness="3"
            vertical
            class="mr-4 ml-4"
            v-if="!isLoadingMeetingNotesObjects"
          />

          <v-btn
            v-if="!isLoadingMeetingNotesObjects"
            prepend-icon="mdi-undo"
            variant="flat"
            color="secondary"
            class="mr-4"
            :disabled="!canUndo"
            @click="undoWordUpdate"
          >
            Undo
          </v-btn>

          <v-btn
            v-if="!isLoadingMeetingNotesObjects"
            prepend-icon="mdi-redo"
            color="secondary"
            variant="flat"
            :disabled="!canRedo"
            @click="redoWordUpdate"
          >
            Redo
          </v-btn>
          <v-divider
            :thickness="3"
            vertical
            class="mr-4 ml-4"
            v-if="!isLoadingMeetingNotesObjects"
          />

          <v-btn
            color="primary"
            v-if="!isLoadingMeetingNotesObjects"
            @click="saveMeetingNotes"
            :loading="isSavingMeetingNotes"
            :disabled="isSavingMeetingNotes"
            variant="flat"
            class="text-white mr-4"
          >
            Save
          </v-btn>
          <v-btn
            color="primary"
            v-if="!isLoadingMeetingNotesObjects"
            @click="closeMeetingNotesDialog"
            variant="flat"
            class="text-white"
            :disabled="isSavingMeetingNotes"
          >
            Cancel
          </v-btn>
        </div>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <v-dialog
    persistent
    v-model="wordDialog"
    max-width="500px"
    @update:model-value="closeWordDialog"
  >
    <v-card>
      <v-card-title>
        {{ selectedWords.length > 1 ? "Edit Multiple Words" : "Edit Word" }}
      </v-card-title>

      <v-card-text style="padding: 2px; padding-left: 20px; padding-right: 20px">
        <div class="mb-4">
          <div class="text-subtitle-1 mb-2">
            {{ selectedWords.length > 1 ? "Selected words:" : "Selected word:" }}
          </div>
          <v-chip
            v-for="(selected, index) in selectedWords"
            :key="index"
            class="ma-1"
            color="primary"
            variant="outlined"
          >
            {{ selected.word.content.trim() }}
            <span class="confidence-badge ml-2">
              {{ Math.round(selected.word.confidence * 100) }}%
            </span>
          </v-chip>
        </div>

        <v-text-field
          v-model="alternativeWord"
          :label="'Replace selected words'"
          :placeholder="getSelectedWordsText()"
          variant="outlined"
          auto-grow
          rows="2"
        ></v-text-field>
      </v-card-text>

      <v-card-actions>
        <v-btn color="primary" variant="text" @click="updateWord">Save</v-btn>
        <v-btn color="error" variant="text" @click="closeWordDialog">Cancel</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <!-- Speaker Update Dialog -->
  <v-dialog persistent v-model="speakerUpdateDialog" max-width="500px">
    <v-card>
      <v-card-title>Update Speaker Name</v-card-title>
      <v-card-text style="col-8" variant="compact">
        <v-row>
          <v-col cols="8">
            <v-text-field
              v-model="editedSpeaker.name"
              label="Speaker Name"
              placeholder="Enter new speaker name"
              variant="outlined"
            ></v-text-field>
          </v-col>
          <v-col style="col-4">
            <v-checkbox
              v-model="editedSpeaker.update_all"
              label="Update All"
              color="primary"
              hide-details
            ></v-checkbox>
          </v-col>
        </v-row>
      </v-card-text>
      <v-card-actions>
        <v-btn color="error" variant="text" @click="closeSpeakerDialog">Cancel</v-btn>
        <v-btn color="primary" @click="updateSpeakerName">Save</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <!-- Custom Vocabulary Dialog -->
  <v-dialog persistent v-model="customVocabularyDialog" max-width="500px">
    <v-card>
      <v-card-title>Add to Custom Vocabulary</v-card-title>
      <v-card-text>
        <v-form ref="phraseFormRef">
          <div class="mb-4">
            Vocabulary:
            {{
              customVocabularies.find((v) => v.id === selectedVocabularyId)
                ?.vocabulary_name
            }}
          </div>
          <v-text-field
            v-model="selectedPhrase"
            label="Word/Phrase"
            placeholder="e.g., Los-Angeles, A.B.C., Dynamo-D.B."
            variant="outlined"
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
                !v.match(/\d/) || 'Numbers must be spelled out (e.g., zero, one, etc.)',

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
            class="mb-4"
          ></v-text-field>
          <v-text-field
            v-model="displayAs"
            label="Display As"
            density="comfortable"
            variant="outlined"
            class="custom-field"
            placeholder="e.g., Andorra la Vella"
            hint="Optional. Defines how the phrase will appear in transcription output. Can include spaces and numbers."
            persistent-hint
          ></v-text-field>
        </v-form>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn
          color="error"
          variant="text"
          @click="closeCustomVocabularyDialog"
          :loading="savingCustomvocabulary"
          >Cancel</v-btn
        >
        <v-btn
          color="primary"
          @click="saveToCustomVocabulary"
          :loading="savingCustomvocabulary"
          >Add</v-btn
        >
      </v-card-actions>
    </v-card>
  </v-dialog>

  <!-- Documents Dialog -->
  <v-dialog v-model="documentsDialog" max-width="90%" scrollable>
    <MeetingDocuments
      :meetingId="selectedMeeting.id"
      :meetingState="selectedMeeting.current_step"
    />
  </v-dialog>

  <!-- Analytics Dialog -->
  <v-dialog persistent v-model="analyticsDialog" max-width="90%" scrollable>
    <v-card class="analytics-card">
      <v-card-title class="d-flex align-center mt-2" max-width="100%">
        <v-icon class="mr-2">mdi-chart-bar</v-icon>
        Meeting Analytics
      </v-card-title>
      <v-card-text class="pa-0">
        <div v-if="isLoadingAnalytics" class="analytics-loading pa-4">
          <v-progress-linear
            indeterminate
            color="primary"
            height="6"
            class="mb-4"
          ></v-progress-linear>
          <div class="text-center text-subtitle-1">Loading analytics data...</div>
        </div>
        <v-container v-else class="fill-height fill-width mb-5" fluid>
          <!-- Meeting Info -->
          <v-col cols="12">
            <v-card variant="outlined" class="mb-4 analytics-stat-card">
              <v-card-title>Meeting Overview</v-card-title>
              <v-card-text>
                <v-row>
                  <v-col cols="4">
                    <div>
                      <strong>Title:</strong>
                      {{ meetingAnalytics?.meetingMetadata?.meetingTitle }}
                    </div>
                    <div>
                      <strong>Date:</strong>
                      {{
                        meetingAnalytics?.meetingMetadata?.dateTime
                          ? new Date(
                              meetingAnalytics.meetingMetadata.dateTime
                            ).toLocaleString()
                          : "No date available"
                      }}
                    </div>
                    <div>
                      <strong>Duration:</strong>
                      {{
                        meetingAnalytics?.meetingMetadata?.meetingDuration
                          ? Math.round(
                              meetingAnalytics?.meetingMetadata?.meetingDuration / 60
                            )
                          : "No duration available"
                      }}
                      minutes
                    </div>
                    <div>
                      <strong>Type:</strong>
                      {{ meetingAnalytics?.meetingMetadata?.meetingType }}
                    </div>
                    <div>
                      <strong>Format:</strong>
                      {{ meetingAnalytics?.meetingMetadata?.virtualOrInPersonOrHybrid }}
                    </div>
                  </v-col>
                  <v-col cols="8">
                    <div>
                      <strong>Summary:</strong>
                      <span class="summary-text">{{
                        meetingAnalytics?.meetingSummary.overview.summary
                      }}</span>
                    </div>
                    <div>
                      <strong>Problem:</strong>
                      <span class="summary-highlight">{{
                        meetingAnalytics?.meetingSummary.overview.problem
                      }}</span>
                    </div>
                    <div>
                      <strong>Solution:</strong>
                      <span class="summary-highlight">{{
                        meetingAnalytics?.meetingSummary.overview.solution
                      }}</span>
                    </div>
                    <div>
                      <strong>Action Item:</strong>
                      {{ meetingAnalytics?.meetingSummary.overview.actionItems }}
                    </div>
                  </v-col>
                </v-row>
              </v-card-text>
            </v-card>
          </v-col>

          <!-- Speaker Analytics -->
          <v-col cols="6">
            <v-card variant="outlined" class="mb-4 analytics-stat-card" height="350px">
              <v-card-title>Speaking Time Distribution</v-card-title>
              <v-card-text>
                <apexchart
                  type="pie"
                  height="300"
                  :options="speakerTimeChartOptions"
                  :series="speakerTimeChartSeries.series"
                />
              </v-card-text>
            </v-card>
          </v-col>
          <v-col cols="6">
            <v-card variant="outlined" class="mb-4 analytics-stat-card" height="350px">
              <v-card-title>Speaking Time Percentage</v-card-title>
              <v-card-text>
                <apexchart
                  type="bar"
                  height="300"
                  :options="speakerContributionChartOptions"
                  :series="speakerContributionSeries"
                />
              </v-card-text>
            </v-card>
          </v-col>

          <!-- Engagement Metrics -->

          <v-col
            cols="12"
            v-if="meetingAnalytics?.engagementAnalytics?.participationMetrics"
          >
            <v-card variant="outlined" class="mb-4 analytics-stat-card">
              <v-card-title>Engagement Metrics</v-card-title>
              <v-card-text>
                <v-row>
                  <v-col cols="3">
                    <v-card variant="outlined" class="analytics-stat-card">
                      <v-card-text class="text-center">
                        <div class="analytics-stat-value">
                          {{
                            meetingAnalytics?.engagementAnalytics?.participationMetrics
                              .activeParticipants
                          }}/{{
                            meetingAnalytics?.engagementAnalytics?.participationMetrics
                              ?.totalParticipants
                          }}
                        </div>
                        <div class="analytics-stat-label">Active Participants</div>
                      </v-card-text>
                    </v-card>
                  </v-col>
                  <v-col cols="3">
                    <v-card variant="outlined" class="analytics-stat-card">
                      <v-card-text class="text-center">
                        <div class="analytics-stat-value">
                          {{
                            meetingAnalytics?.engagementAnalytics?.participationMetrics
                              ?.participantParticipationRate
                          }}%
                        </div>
                        <div class="analytics-stat-label">Participation Rate</div>
                      </v-card-text>
                    </v-card>
                  </v-col>
                  <v-col cols="3">
                    <v-card variant="outlined" class="analytics-stat-card">
                      <v-card-text class="text-center">
                        <div class="analytics-stat-value">
                          {{
                            Math.round(
                              meetingAnalytics?.engagementAnalytics?.participationMetrics
                                ?.averageSpeakingTimeForEachParticipant / 60
                            )
                          }}
                          min
                        </div>
                        <div class="analytics-stat-label">Avg Speaking Time</div>
                      </v-card-text>
                    </v-card>
                  </v-col>
                  <v-col cols="3">
                    <v-card variant="outlined" class="analytics-stat-card">
                      <v-card-text class="text-center">
                        <div class="analytics-stat-value">
                          {{
                            meetingAnalytics?.engagementAnalytics?.participationMetrics
                              ?.totalQuestionsAsked
                          }}/{{
                            meetingAnalytics?.engagementAnalytics?.participationMetrics
                              ?.totalQuestionsAnswered
                          }}
                        </div>
                        <div class="analytics-stat-label">Questions Asked/Answered</div>
                      </v-card-text>
                    </v-card>
                  </v-col>
                </v-row>
              </v-card-text>
            </v-card>
          </v-col>

          <!-- Agenda Coverage -->
          <v-col cols="12" v-if="meetingAnalytics?.agendaCoverage">
            <v-card variant="outlined" class="mb-4 analytics-stat-card">
              <v-card-title>Agenda Coverage</v-card-title>
              <v-card-text>
                <v-row>
                  <v-col cols="12">
                    <v-row>
                      <v-col cols="4">
                        <v-card variant="outlined" class="analytics-stat-card">
                          <v-card-text class="text-center">
                            <div class="analytics-stat-value">
                              {{
                                meetingAnalytics?.agendaCoverage?.summary
                                  ?.totalItemsCovered
                              }}
                            </div>
                            <div class="analytics-stat-label">Total Items</div>
                          </v-card-text>
                        </v-card>
                      </v-col>
                      <v-col cols="4">
                        <v-card variant="outlined" class="analytics-stat-card">
                          <v-card-text class="text-center">
                            <div class="analytics-stat-value">
                              {{
                                meetingAnalytics?.agendaCoverage?.summary
                                  ?.completedTopicItems
                              }}
                            </div>
                            <div class="analytics-stat-label">Completed Items</div>
                          </v-card-text>
                        </v-card>
                      </v-col>
                      <v-col cols="4">
                        <v-card variant="outlined" class="analytics-stat-card">
                          <v-card-text class="text-center">
                            <div class="analytics-stat-value">
                              {{
                                meetingAnalytics?.agendaCoverage?.summary
                                  .completionTopicRate
                              }}%
                            </div>
                            <div class="analytics-stat-label">Completion Rate</div>
                          </v-card-text>
                        </v-card>
                      </v-col>
                    </v-row>
                  </v-col>
                </v-row>
                <div class="table-wrapper">
                  <v-data-table
                    v-if="
                      meetingAnalytics?.agendaCoverage?.items &&
                      meetingAnalytics?.agendaCoverage?.items.length > 0
                    "
                    :items="meetingAnalytics.agendaCoverage.items"
                    :headers="[
                      {
                        title: 'Topic',
                        key: 'agendaTopic',
                        sortable: false,
                        width: '25%',
                      },
                      {
                        title: 'Duration',
                        key: 'topicDuration',
                        sortable: false,
                        width: '15%',
                      },
                      {
                        title: 'Completion %',
                        key: 'TopicCompletionPercentage',
                        sortable: false,
                        width: '15%',
                      },
                      {
                        title: 'Notes',
                        key: 'topicNotes',
                        sortable: false,
                        width: '45%',
                      },
                    ]"
                    class="analytics-table"
                    density="comfortable"
                    hover
                    fixed-header
                    hide-default-footer
                    disable-pagination
                    disable-sort
                  >
                    <template v-slot:item.agendaTopic="{ item }">
                      <div class="text-wrap">
                        <span class="agenda-label">{{ item.agendaTopic }}</span>
                      </div>
                    </template>
                    <template v-slot:item.topicDuration="{ item }">
                      {{ Math.round(item.topicDuration / 60) }} min
                    </template>
                    <template v-slot:item.TopicCompletionPercentage="{ item }">
                      {{ item.TopicCompletionPercentage }}%
                    </template>
                    <template v-slot:item.topicNotes="{ item }">
                      <div class="text-wrap">{{ item.topicNotes }}</div>
                    </template>
                  </v-data-table>
                </div>
              </v-card-text>
            </v-card>
          </v-col>

          <!-- Action Items -->
          <v-col cols="12" v-if="meetingAnalytics?.actionItems">
            <v-card variant="outlined" class="mb-4 analytics-stat-card">
              <v-card-title>Action Items</v-card-title>
              <v-card-text>
                <v-row>
                  <v-col cols="12">
                    <v-row>
                      <v-col cols="6">
                        <v-card variant="outlined" class="analytics-stat-card">
                          <v-card-text class="text-center">
                            <div class="analytics-stat-value">
                              {{
                                meetingAnalytics?.actionItems?.summary?.totalActionItems
                              }}
                            </div>
                            <div class="analytics-stat-label">Total Action Items</div>
                          </v-card-text>
                        </v-card>
                      </v-col>
                      <v-col cols="6">
                        <v-card variant="outlined" class="analytics-stat-card">
                          <v-card-text class="text-center">
                            <div class="analytics-stat-value">
                              {{
                                meetingAnalytics?.actionItems?.summary
                                  ?.assignedActionItems
                              }}
                            </div>
                            <div class="analytics-stat-label">Assigned Items</div>
                          </v-card-text>
                        </v-card>
                      </v-col>
                    </v-row>
                  </v-col>
                </v-row>
                <div class="table-wrapper">
                  <v-data-table
                    v-if="
                      meetingAnalytics?.actionItems?.items &&
                      meetingAnalytics?.actionItems?.items.length > 0
                    "
                    :items="meetingAnalytics.actionItems.items"
                    :headers="[
                      {
                        title: 'Assignee',
                        key: 'actionItemAssignee',
                        sortable: false,
                        width: '15%',
                      },
                      {
                        title: 'Description',
                        key: 'actionItemDescription',
                        sortable: false,
                        width: '40%',
                      },
                      {
                        title: 'Due Date',
                        key: 'actionItemDueDate',
                        sortable: false,
                        width: '15%',
                      },
                      {
                        title: 'Priority',
                        key: 'actionItemPriority',
                        sortable: false,
                        width: '15%',
                      },
                      {
                        title: 'Status',
                        key: 'actionItemStatus',
                        sortable: false,
                        width: '15%',
                      },
                    ]"
                    class="analytics-table"
                    density="comfortable"
                    hover
                    fixed-header
                    hide-default-footer
                    disable-pagination
                    disable-sort
                  >
                    <template v-slot:item.actionItemAssignee="{ item }">
                      <div class="text-wrap">{{ item.actionItemAssignee }}</div>
                    </template>
                    <template v-slot:item.actionItemDescription="{ item }">
                      <div class="text-wrap">{{ item.actionItemDescription }}</div>
                    </template>
                    <template v-slot:item.actionItemDueDate="{ item }">
                      {{ new Date(item.actionItemDueDate).toLocaleDateString() }}
                    </template>
                    <template v-slot:item.actionItemPriority="{ item }">
                      <v-chip
                        :color="getPriorityColor(item.actionItemPriority)"
                        class="status-chip"
                        size="small"
                      >
                        {{ item.actionItemPriority }}
                      </v-chip>
                    </template>
                    <template v-slot:item.actionItemStatus="{ item }">
                      <v-chip
                        :color="getStatusColor(item.actionItemStatus)"
                        class="status-chip"
                        size="small"
                      >
                        {{ item.actionItemStatus }}
                      </v-chip>
                    </template>
                  </v-data-table>
                </div>
              </v-card-text>
            </v-card>
          </v-col>

          <!-- Meeting Effectiveness -->
          <v-col cols="12" v-if="meetingAnalytics?.effectiveness">
            <v-card variant="outlined" class="mb-4 analytics-stat-card">
              <v-card-title>Meeting Effectiveness</v-card-title>
              <v-card-text>
                <v-row>
                  <v-col cols="12">
                    <v-row>
                      <v-col cols="6">
                        <v-card
                          variant="outlined"
                          class="analytics-stat-card"
                          height="350px"
                        >
                          <v-card-title>Time Management</v-card-title>
                          <v-card-text>
                            <apexchart
                              type="pie"
                              height="300"
                              :options="timeManagementChartOptions"
                              :series="[
                                meetingAnalytics?.effectiveness?.timeManagement
                                  .productiveTime,
                                meetingAnalytics?.effectiveness?.timeManagement?.idleTime,
                              ]"
                            />
                          </v-card-text>
                        </v-card>
                      </v-col>
                      <v-col cols="6">
                        <v-card
                          variant="outlined"
                          class="analytics-stat-card"
                          height="100%"
                        >
                          <v-card-title>Objectives</v-card-title>
                          <v-card-text>
                            <v-list>
                              <v-list-item
                                v-for="(objective, index) in meetingAnalytics
                                  ?.effectiveness?.objectives"
                                :key="index"
                              >
                                <v-list-item-title>
                                  <v-icon
                                    :color="
                                      objective?.meetingObjectiveAchieved
                                        ? 'success'
                                        : 'error'
                                    "
                                    class="mr-2"
                                  >
                                    {{
                                      objective?.meetingObjectiveAchieved
                                        ? "mdi-check-circle"
                                        : "mdi-close-circle"
                                    }}
                                  </v-icon>
                                  {{ objective?.meetingObjectiveDescription }}
                                </v-list-item-title>
                                <v-list-item-subtitle
                                  v-if="objective?.meetingObjectiveNotes"
                                >
                                  {{ objective?.meetingObjectiveNotes }}
                                </v-list-item-subtitle>
                              </v-list-item>
                            </v-list>
                          </v-card-text>
                        </v-card>
                      </v-col>
                    </v-row>
                  </v-col>
                </v-row>
              </v-card-text>
            </v-card>
          </v-col>
        </v-container>
      </v-card-text>
      <v-card-actions class="d-flex justify-end px-10 pb-8">
        <v-btn
          color="primary"
          @click="closeAnalyticsDialog"
          variant="flat"
          class="text-white"
        >
          Close
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <v-snackbar v-model="snackbar.show" :color="snackbar.color" timeout="3000">
    {{ snackbar.message }}
  </v-snackbar>
</template>

<style scoped>
.full-size {
  height: 100% !important;
  width: 100% !important;
  flex: 1 1 auto !important;
  display: flex;
  flex-direction: column;
}

.font-weight-bold,
.font-weight-bold + td {
  padding-left: 0 !important;
  padding-right: 0 !important;
}

.playing-word {
  border: 1px solid black;
  color: white !important;
  background-color: #002886 !important;
}

.errorRow {
  background-color: #ffebee !important;
}

.selectedWord {
  padding: 1px !important;
  background-color: #e0e0e0;
  border-radius: 2px;
  border: 1px solid #330d0d;
}

.low-confidence {
  cursor: pointer;
  background-color: #ffd7d7;
  padding: 0px 0;
  border-radius: 3px;
}

.editedWord {
  font-style: italic;
  color: #eb4a0f;
  text-decoration: green wavy underline;
}

.known-phrase {
  background-color: #ffa50066;
  border-radius: 3px;
  padding: 0 2px;
  padding-left: 0;
  margin: 0 2px;
}

.known-phrase[data-type="punctuation"] {
  padding: 0;
  margin: 0;
}

.scroller {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-start;
  gap: 4px;
  padding: 4px;
}

.meeting-notes-content {
  display: inline-block;
}

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

.v-list-item--disabled {
  opacity: 0.3;
  pointer-events: none;
}

.segment {
  margin-top: 0rem;
}

.no-padding {
  padding: 0px !important;
  margin: 0px !important;
}

.segment-content {
  margin: 0;
  margin-bottom: 10px;
  padding-left: 0.5rem;
  padding-top: 0.5rem;
  border-left: 3px solid #e3f2fd;
}

.meeting-notes {
  width: 100%;
  margin: 20px;
}

.transcript-container {
  min-height: 200px;
  max-height: calc(90vh - 180px);
  padding: 20px;
  border-radius: 8px;
  line-height: 1.6;
}

.word-info {
  background-color: #f5f5f5;
  border-radius: 4px;
}

.notewords {
  word-break: keep-all;
  overflow-wrap: always;
  /* Changed from anywhere */
  padding: 3px 3px;
  white-space: nowrap;
}

.changes-table {
  .v-table {
    background: transparent;

    th {
      font-weight: bold;
      background-color: #f5f5f5;
      white-space: nowrap;
    }

    td {
      font-size: 0.875rem;
    }
  }
}

.highlight-change {
  background-color: #e3f2fd !important;
}

.v-card {
  width: 100%;
}

.table-wrapper {
  width: 100%;
  margin: 16px 0;
  background: white;
  border-radius: 4px;
  overflow-x: auto;
  display: block;

  &:first-child {
    margin-top: 0;
  }

  &:last-child {
    margin-bottom: 0;
  }

  :deep(.v-data-table) {
    display: table !important;
    width: 100%;
  }

  :deep(.v-table) {
    display: table !important;
    width: 100%;
  }
}

.analytics-table {
  :deep(.v-data-table) {
    display: table !important;
    width: 100%;
    border-collapse: collapse;
  }

  :deep(.v-table) {
    display: table !important;
    width: 100%;
    border-collapse: collapse;
  }

  :deep(.v-table__wrapper) {
    width: 100%;

    table {
      display: table !important;
      width: 100%;
      border-collapse: collapse;
      table-layout: fixed;
    }
  }

  :deep(.v-data-table-header) {
    display: table-header-group !important;
    background-color: #f5f5f5;
  }

  :deep(.v-data-table-headers__row) {
    display: table-row !important;
  }

  :deep(.v-data-table-header > th) {
    display: table-cell !important;
    padding: 12px;
    text-align: left;
    border-bottom: thin solid rgba(0, 0, 0, 0.12);
    font-weight: 500;
  }

  :deep(.v-data-table__tbody) {
    display: table-row-group !important;
  }

  :deep(.v-data-table__row) {
    display: table-row !important;
  }

  :deep(.v-data-table__cell) {
    display: table-cell !important;
    padding: 12px;
    text-align: left;
    border-bottom: thin solid rgba(0, 0, 0, 0.12);
    vertical-align: middle;
    line-height: 1.2;
  }

  :deep(.v-data-table__row:hover) {
    background-color: #f5f5f5;
  }

  :deep(.v-data-table-footer) {
    display: none;
  }
}

.text-wrap {
  white-space: normal;
  overflow-wrap: break-word;
  word-wrap: break-word;
  /* For legacy browser support */
  word-break: keep-all;
  /* Prevents breaking words */
  max-width: 100%;
  hyphens: none;
  /* Prevents hyphenation */
}

.analytics-loading {
  min-height: 200px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  background: rgba(255, 255, 255, 0.9);
  width: 100%;
}

.confidence-slider {
  max-width: 300px;
  min-width: 200px;
}

.confidence-label {
  font-size: 0.9rem;
  color: rgba(0, 0, 0, 0.6);
}

.width-100 {
  width: 100%;
}

.segment {
  margin-bottom: 1.5rem;
}

.speaker-label {
  font-weight: 600;
  color: #2196f3;
  font-size: 0.9rem;
  padding-bottom: 1.5rem;
  padding: 2px 8px;
  background-color: #e3f2fd;
  border-radius: 4px;
  display: inline-block;
}

.clickable {
  cursor: pointer;
}

.clickable:hover {
  opacity: 0.5;
  color: white;
  background-color: #3d3d3d;
}

.edited-word {
  background-color: #d4edda;
  padding: 2px 2px;
  border-radius: 3px;
}

.edited-word:hover {
  background-color: #0e0e0e;
}

.edited-status {
  color: #28a745;
  font-size: 0.9em;
  display: flex;
  align-items: center;
  margin-top: 8px;
}

.original-content {
  color: #666;
  font-size: 0.9em;
  font-style: italic;
  margin-top: 4px;
}
</style>

<script lang="ts">
import VueApexCharts from "vue3-apexcharts";
import { type ApexOptions } from "apexcharts";
import "@assets/styles/meetings.css";
import { copyToClipboard } from "@/src/utils/clipboard";
import "@assets/styles/analytics.css";
import "@assets/styles/confidence-chart.css";
import { ref, computed, watch, Ref, onMounted, onUnmounted, reactive } from "vue";
import { useRouter } from 'vue-router';
import { dateFormatter } from "@utils/dateFormatter";
import { MeetingService } from "@services/meeting.service";
import { CustomVocabularyProgressEnum } from "@/src/types/customVocabulary";
import Dashboard from "@views/Dashboard.vue";
import {
  AuthUser,
  getCurrentUser
} from 'aws-amplify/auth';
import axios from 'axios';
import {
  CanPerformEnum,
  type MeetingRequest,
  type MeetingResponse,
  MeetingSetupProgressEnum,
  S3PresignedUrlsType,
} from "@/src/types/meeting";
import { StatusEnum } from "@/src/types/common";
import { useStore } from "vuex";
import { PromptSetService } from "@services/promptSet.service";
import { type PromptSetResponse } from "@/src/types/promptSet";
import { CustomModelService } from "@services/customModel.service";
import { type CustomModelResponse, CustomModelSetupProgressEnum } from "@/src/types/customModel";
import { type MeetingPromptResponse } from "@/src/types/meeting";
import { ScriptChangeType, type MeetingNotesType, type SpeakerWordType, type SpeakerSegmentType, type ConfidenceStats, type NotesWordChanges, WordType } from "@/src/types/meetingNotes";
import { CustomVocabularyResponse, VocabularyPhraseResponse } from "@/src/types/customVocabulary";
import { CustomVocabularyService } from "@services/customVocabulary.service";
import { MeetingAnalytics, speakerContributionChartOptionsInterface, SpeakerTimeChartOptionsInterface } from "@/src/types/meetingAnalytics";
import { Howl } from "howler";
import MeetingDocuments from "./MeetingDocuments.vue";
import PromptResponsesDialog from "./PromptResponsesDialog.vue";

export default {
  name: "Meetings",
  components: {
    Dashboard,
    MeetingDocuments,
    PromptResponsesDialog
  },


  setup() {
    // Helper function to determine if a meeting is in a processing state
    const isProcessingState = (state) => {
      let shouldDisplay =  [
        MeetingSetupProgressEnum.Transcribing,
        MeetingSetupProgressEnum.PromptProcessing,
        MeetingSetupProgressEnum.Sealing
      ].includes(state);
      return shouldDisplay;
    };

    interface SelectedWord {
      word: SpeakerWordType;
      word_id: number;
    }

    const currentTime = ref(0);
    const isWordPlaying = (word: SpeakerWordType): boolean => {
      if (!sound.value || !isPlayingAudio.value || !word.start_time || !word.end_time) {
        return false;
      }
      const currentPlayTime = currentTime.value;
      return currentPlayTime >= word.start_time && currentPlayTime <= word.end_time;
    };

    const phraseFormRef = ref();

    const selectedAction = ref("");
    const selectedMeetingId = ref<string>();
    const selectedWords = ref<SelectedWord[]>([]);

    const meetingNotesLoadingMessage = ref<string>();

    const actionItems = [
      {
        title: "Edit Meeting",
        value: "edit",
        icon: "mdi-pencil",
        requiredPermission: CanPerformEnum.Edit
      },
      {
        title: "Delete Meeting",
        value: "delete",
        icon: "mdi-delete",
        requiredPermission: CanPerformEnum.Delete
      },
      {
        title: "Upload Recording",
        value: "upload",
        icon: "mdi-upload",
        requiredPermission: CanPerformEnum.Upload
      },

      {
        title: "Start Transcribing",
        value: "transcribe",
        icon: "mdi-transcribe",
        requiredPermission: CanPerformEnum.Transcribe
      },
      {
        value: "separator",
      },
      {
        title: "Generate Prompt Notes",
        value: "generate",
        icon: "mdi-note-plus",
        requiredPermission: CanPerformEnum.GenerateNotes
      },
      {
        title: "View Prompt Notes",
        value: "view-prompts",
        icon: "mdi-text-box",
        requiredPermission: CanPerformEnum.ViewPromptNotes
      },
      {
        value: "separator",
      },
      {
        title: "View Notes",
        value: "view-notes",
        icon: "mdi-note-text",
        requiredPermission: CanPerformEnum.ReviewNotes
      },
      {
        title: "Seal Meeting",
        value: "seal-meeting",
        icon: "mdi-seal",
        requiredPermission: CanPerformEnum.Seal
      },
      {
        title: "Export Notes",
        value: "export",
        icon: "mdi-file-export",
        requiredPermission: CanPerformEnum.ExportNotes
      },
      {
        title: "View Meeting Analytics",
        value: "view-analytics",
        icon: "mdi-chart-bar",
        requiredPermission: CanPerformEnum.ViewAnalytics
      },
      {
        title: "View Documents",
        value: "view-documents",
        icon: "mdi-file-document-multiple",
        requiredPermission: CanPerformEnum.ViewDocuments
      }
    ];

    const executeSelectedAction = async () => {
      if (!selectedAction.value) {
        showError("No action selected");
        return;
      }

      if (!selectedMeetingId.value) {
        showError("Please select a meeting first");
        return;
      }

      // Find the selected meeting
      const selectedMeeting = meetings.value.find(
        (meeting) => meeting.id === selectedMeetingId.value
      );

      if (!selectedMeeting) {
        showError("Selected meeting not found");
        return;
      }

      try {
        // Execute the action
        switch (selectedAction.value) {
          case "upload":
            await uploadFile(selectedMeeting);
            break;
          case "transcribe":
            await startTranscriptionProcessing(selectedMeeting);
            break;
          case "generate":
            await startPromptProcessing(selectedMeeting);
            break;
          case "edit":
            await openMeetingDialog(selectedMeeting);
            break;
          case "delete":
            await deleteMeeting(selectedMeeting);
            break;
          case "view-notes":
            await openMeetingNotesDialog(selectedMeeting);
            break;
          case "view-prompts":
            await viewPromptResponses(selectedMeeting);
            break;
          case "seal-meeting":
            await sealMeeting(selectedMeeting);
            break;
          case "export":
            exportMeetingNotes(selectedMeeting);
            break;
          case "view-analytics":
            openAnalyticsDialog(selectedMeeting);
            break;
          case "view-documents":
            openDocumentsDialog(selectedMeeting);
            break;
          default:
            showError("Invalid action selected");
            break;
        }

        // Reset selections after successful execution
        //selectedMeetingId.value = undefined;
        selectedAction.value = "";
      } catch (error) {
        console.error("Error executing action:", error);
        showError(`Failed to execute ${selectedAction.value} action`);
      }
    };
    const handleKeyDown = (event: KeyboardEvent) => {
      if (meetingNotes.value && selectedWords.value.length > 0 && !wordDialog.value &&
        (event.key === 'Delete')) {
        if (!confirm("Are you sure you want to delete these words?")) return;
        event.preventDefault();
        deleteSelectedWords();
      }
      if (event.key === 'Escape') {
        selectedWords.value = [];
        event.preventDefault()
        event.stopPropagation()
      }
    };
    const canUndo = computed(() => {
      if (!meetingNotesObject.value?.changes || meetingNotesObject.value.changes.length === 0) {
        return false;
      }
      const lastUndoneChangeIndex = meetingNotesObject.value.changes.findIndex(change => !change.undone);
      return lastUndoneChangeIndex !== -1;
    });

    const canRedo = computed(() => {
      if (!meetingNotesObject.value?.changes || meetingNotesObject.value.changes.length === 0) {
        return false;
      }
      const lastUndoneChangeIndex = meetingNotesObject.value.changes.findIndex(change => change.undone && !change.redone);
      return lastUndoneChangeIndex !== -1;
    });
    const deleteSelectedWords = () => {
      if (selectedWords.value.length === 0) return;

      if (!meetingNotesObject.value) return;

      // Initialize changes array if it doesn't exist
      if (!meetingNotesObject.value.changes) {
        meetingNotesObject.value.changes = [];
      }

      // Sort selected words by position (ascending order)
      const sortedWords = [...selectedWords.value].sort((a, b) => a.word_id - b.word_id);
      const wordIds = sortedWords.map(w => w.word_id);

      try {
        // Track changes for each deleted word before modifying the words array
        sortedWords.forEach(({ word }) => {
          const change: NotesWordChanges = {
            word_id: word.word_id,
            type: ScriptChangeType.Delete,
            original: word.content,
            user: currentUser.value?.username!,
            change: "",
            timestamp: new Date().toISOString(),
            undone: false,
            redone: false,
            word: word
          };
          meetingNotesObject.value?.changes.push(change);
        });

        // Delete words from the words array
        if (meetingNotesObject.value.words) {
          // Filter out the deleted words
          meetingNotesObject.value.words = meetingNotesObject.value.words.filter(
            (word: SpeakerWordType) => !wordIds.includes(word.word_id)
          );

          // Update word IDs to be sequential after deletion
          meetingNotesObject.value.words.forEach((word, index) => {
            word.word_id = index;
          });

          // Update transcript text if it exists
          if (meetingNotesObject.value.transcript) {
            meetingNotesObject.value.transcript = meetingNotesObject.value.words
              .map((item: SpeakerWordType) =>
                (item.word_type === WordType.Punctuation ? '' : ' ') + item.content
              )
              .join('')
              .trim();
          }

          // Update segments
          if (meetingNotesObject.value.segments) {
            // Update each segment's words
            meetingNotesObject.value.segments = meetingNotesObject.value.segments
              .map((segment: SpeakerSegmentType) => {
                const segmentWords = meetingNotesObject.value?.words.filter(
                  (word: SpeakerWordType) => word.segment_id === segment.segment_id
                );

                if (segmentWords?.length === 0) {
                  return null; // Mark empty segments for removal
                }

                if (segmentWords) {
                  // Update segment content - fix spacing between words
                  segment.transcript = segmentWords
                    .map((word: SpeakerWordType, idx) => {
                      // For punctuation, don't add a space before
                      if (word.word_type === WordType.Punctuation) {
                        return word.content;
                      }
                      // For first word or after punctuation, don't add space before
                      if (idx === 0 || (idx > 0 && segmentWords[idx-1].word_type === WordType.Punctuation)) {
                        return word.content;
                      }
                      return ' ' + word.content;
                    })
                    .join('');

                  // Update segment timing
                  if (segmentWords.length > 0) {
                    segment.start_time = Math.min(...segmentWords.map(w => Number(w.start_time)));
                    segment.end_time = Math.max(...segmentWords.map(w => Number(w.end_time)));
                  }
                }

                return segment;
              })
              .filter((segment): segment is SpeakerSegmentType => segment !== null); // Remove empty segments
          }

          // Rebuild the segment-word mapping after deletion
          preprocessSegmentWords();

          // Recalculate confidence stats
          calculateConfidenceStats();
        }

        showSuccess("Words deleted successfully");
      } catch (error) {
        console.error("Error deleting words:", error);
        showError("Failed to delete words");
      } finally {
        // Clear selection
        selectedWords.value = [];
      }
    };

    const currentUser = ref<AuthUser | null>(null);

    onMounted(async () => {
      isMounted.value = true
      // Load pagination values from localStorage or use defaults
      currentPage.value = parseInt(localStorage.getItem('meetingsCurrentPage') || '1');
      pageSize.value = parseInt(localStorage.getItem('meetingsPageSize') || '10');
      // Load prompt responses pagination values from localStorage
      promptResponsesCurrentPage.value = parseInt(localStorage.getItem('promptResponsesCurrentPage') || '1');
      promptResponsesPageSize.value = parseInt(localStorage.getItem('promptResponsesPageSize') || '5');
      fetchMeetings(currentPage.value, pageSize.value);
      document.addEventListener('keydown', handleKeyDown);
      try {
        // Get current user using async/await pattern and store in reactive ref
        currentUser.value = await getCurrentUser();
      } catch (error) {
        console.error('Error getting current user:', error);
        showError('Failed to get current user information');
      }
    });
    onUnmounted(() => {
      document.removeEventListener('keydown', handleKeyDown);
    });
    const isMounted = ref(false)
    const promptResponsesDialog = ref(false);
    const meetingNotes = ref(false);
    const isLoadingMeetingNotesObjects = ref(false);
    const downloadProgressMeetingObject = ref(0);
    const sound = ref<Howl | null>(null);
    const isPlayingAudio = ref(false);
    const promptResponses = ref<MeetingPromptResponse[]>([]);
    const loadingPromptResponses = ref(false);

    const promptResponseHeaders = [
      { title: "Prompt", key: "prompt", sortable: true, resizable: true, width: '20%' },
      { title: "Response", key: "prompt_response", sortable: false, resizable: true },
      { title: "Created At", key: "created_at", sortable: true, resizable: true },
      { title: "Updated At", key: "updated_at", sortable: true, resizable: true },
    ];

    interface Speaker {
      name: string
      speaker_label: string
      update_all: boolean
      segment_id: number
    }

    const speakerUpdateDialog = ref(false)

    const closeSpeakerDialog = () => {
      speakerUpdateDialog.value = false
      editedSpeaker.name = ''
      editedSpeaker.speaker_label = ''
      editedSpeaker.update_all = true
      editedSpeaker.segment_id = 0
    }

    const updateSpeakerName = () => {
      if (!editedSpeaker.name) {
        return
      }

      try {
        // Update speaker labels in the transcribe object
        if (editedSpeaker.update_all) {
          if (meetingNotesObject.value?.segments) {
            meetingNotesObject.value.segments.forEach((segment: SpeakerSegmentType) => {
              if (segment.speaker_label === editedSpeaker.speaker_label) {
                segment.speaker_label = editedSpeaker.name
              }
            })
          }
        } else {
          if (meetingNotesObject.value?.segments) {
            meetingNotesObject.value.segments.filter(x => x.segment_id == editedSpeaker.segment_id).forEach((segment: SpeakerSegmentType) => {
              if (segment.segment_id === editedSpeaker.segment_id) {
                segment.speaker_label = editedSpeaker.name
              }
            })
          }
        }
        showSuccess("Speaker name updated successfully")
        closeSpeakerDialog()
      } catch (error) {
        console.error("Error updating speaker name:", error)
        showError("Failed to update speaker name")
      }
    }

    const openSpeakerDialog = (segment: SpeakerSegmentType) => {
      editedSpeaker.speaker_label = segment.speaker_label
      editedSpeaker.name = segment.speaker_label ?? ''
      editedSpeaker.update_all = true
      editedSpeaker.segment_id = segment.segment_id ?? 0
      speakerUpdateDialog.value = true
    }
    const meetings = ref<MeetingResponse[]>([]);
    const promptSets = ref<PromptSetResponse[]>([]);
    const customModels = ref<CustomModelResponse[]>([]);
    const customVocabularies = ref<CustomVocabularyResponse[]>([]);
    const promptSetsError = ref("");
    const customModelsError = ref("");
    const customVocabulariesError = ref("");
    const formValid = ref(false);

    const loadingPromptSets = ref(false);
    const loadingCustomModels = ref(false);
    const loadingCustomVocabularies = ref(false);
    const selectedMeetingNotesId = ref<string>();
    const wordDialog = ref(false);
    const editedSpeaker = reactive<Speaker>({
      name: '',
      update_all: true,
      segment_id: 0,
      speaker_label: ''
    })

    const loading = ref(false);
    const search = ref("");
    const dialog = ref(false);
    const currentPage = ref(1);
    const pageSize = ref(10);

    // Pagination state for prompt responses table
    const promptResponsesCurrentPage = ref(1);
    const promptResponsesPageSize = ref(5);
    const editedIndex = ref(-1);
    const store = useStore();
    const teamId = computed(() => store.getters.selectedTeamId);
    const selectedWord = ref<SpeakerWordType | null>(null);
    const alternativeWord = ref("");
    const selectedSegmentIndex = ref<number>(-1);
    const selectedWordIndex = ref<number>(-1);
    const confidenceThreshold = ref(98);
    const isLowConfidence = (word: SpeakerWordType): boolean => {
      if (word.word_type == WordType.Pronunciation)
        return word.confidence < confidenceThreshold.value / 100;
      return false;
    };


    const handleWordClick = (word: SpeakerWordType, segmentIndex: number, wordIndex: number, event: MouseEvent) => {
      if (isLowConfidence(word) || word.edited) {
        showConfidence(word, wordIndex, event);
      }
    };

    const undoWordUpdate = () => {
      try {
        if (!meetingNotesObject.value?.changes || meetingNotesObject.value.changes.length === 0) {
          return;
        }

        // Find the most recent change that hasn't been undone yet by searching from the end
        const lastUndoneChangeIndex = meetingNotesObject.value.changes.map((c, i) => ({ c, i }))
          .reverse()
          .find(({ c }) => !c.undone)?.i ?? -1;
        if (lastUndoneChangeIndex === -1) return;

        const change = meetingNotesObject.value.changes[lastUndoneChangeIndex];
        if (change.type === ScriptChangeType.Delete) {
          // Re-insert the word for delete operations
          const insertIndex = change.word.word_id;
          meetingNotesObject.value.words.splice(insertIndex, 0, {
            ...change.word,
            content: change.original,
            confidence: 1.0,
            edited: false
          });

          // Update surrounding word IDs
          for (let i = insertIndex + 1; i < meetingNotesObject.value.words.length; i++) {
            meetingNotesObject.value.words[i].word_id = i;
          }
        } else {
          // For update operations, find the word and restore its original content
          const currentWord = meetingNotesObject.value.words.find(w => w.word_id === change.word_id);
          if (currentWord) {
            currentWord.content = change.original;
            currentWord.confidence = 1.0;
            currentWord.edited = false;
          }
        }

        // Create a new change object with updated flags while preserving original
        const updatedChange = {
          ...change,
          undone: true,
          redone: false
        };
        meetingNotesObject.value.changes[lastUndoneChangeIndex] = updatedChange;

        showSuccess("Word update undone successfully");
      } catch (error) {
        console.error("Error undoing update:", error);
        showError("Failed to undo update");
      }
    };

    const redoWordUpdate = () => {
      try {
        if (!meetingNotesObject.value?.changes || meetingNotesObject.value.changes.length === 0) {
          return;
        }

        // Find the most recent change that has been undone but not redone
        const lastUndoneChangeIndex = meetingNotesObject.value.changes?.findIndex(change => change.undone && !change.redone);
        if (lastUndoneChangeIndex === -1) return;

        const change = meetingNotesObject.value.changes[lastUndoneChangeIndex];
        if (change.type === ScriptChangeType.Delete) {
          // Remove the word for delete operations
          const wordIndex = meetingNotesObject.value.words.findIndex(w => w.word_id === change.word_id);
          if (wordIndex !== -1) {
            meetingNotesObject.value.words.splice(wordIndex, 1);

            // Update remaining word IDs
            for (let i = wordIndex; i < meetingNotesObject.value.words.length; i++) {
              meetingNotesObject.value.words[i].word_id = i;
            }
          }
        } else {
          // For update operations, find the word and apply the change
          const currentWord = meetingNotesObject.value.words.find(w => w.word_id === change.word_id);
          if (currentWord) {
            currentWord.content = change.change;
            currentWord.confidence = 1.0;
            currentWord.edited = true;
          }
        }

        // Create new change object with updated flags while preserving original
        const updatedChange = {
          ...change,
          redone: true,
          undone: false
        };
        meetingNotesObject.value.changes[lastUndoneChangeIndex] = updatedChange;

        showSuccess("Word update redone successfully");
      } catch (error) {
        console.error("Error redoing update:", error);
        showError("Failed to redo update");
      }
    };
    const closeWordDialog = (value: boolean) => {
      selectedWords.value = [];
      alternativeWord.value = '';
      wordDialog.value = false;
    };


    const meetingNotesObject = ref<MeetingNotesType>();
    const clearMeetingNotesState = () => {
      // Clear transcript data
      meetingNotesObject.value = undefined;

      // Clear audio state
      if (sound.value) {
        sound.value.stop();
        sound.value.unload();
      }
      sound.value = null;
      isPlayingAudio.value = false;

      // Clear selection states
      selectedWords.value = [];
      alternativeWord.value = '';
      selectedSegmentIndex.value = -1;
      selectedWordIndex.value = -1;

      // Clear confidence stats
      confidenceStats.value = {
        veryHigh: 0,
        high: 0,
        medium: 0,
        low: 0,
        veryLow: 0
      };

      // Reset confidence threshold
      confidenceThreshold.value = 98;

      // Reset download progress
      downloadProgressMeetingObject.value = 0;
      isLoadingMeetingNotesObjects.value = false;

      // Reset dialog state
      meetingNotes.value = false;
      wordDialog.value = false;
      existingVocabularyPhrases.value = []
    };
    const pre_signed_urls = ref<S3PresignedUrlsType | null>(null);

    const openMeetingNotesDialog = async (item: MeetingResponse) => {
      clearMeetingNotesState();
      if (item.custom_vocabulary_id != null) {
        if (existingVocabularyPhrases.value.length === 0) {
          meetingNotesLoadingMessage.value = "Downloading Vocabulary List"
          fetchVocabularyPhrases(item.custom_vocabulary_id!)
        }
      }
      // Store the meeting's vocabulary ID
      selectedMeetingVocabularyId.value = item.custom_vocabulary_id ?? null;

      if (!teamId.value || !item.id) {
        showError("Invalid team or meeting ID");
        return;
      }
      selectedMeetingNotesId.value = item.id

      try {
        meetingNotes.value = true;
        downloadProgressMeetingObject.value = 0;
        isLoadingMeetingNotesObjects.value = true;

        // First get the pre-signed URL
        meetingNotesLoadingMessage.value = "Retrieving Meeting Notes Signed URL"

        const urls = await MeetingService.downloadMeetingNotesURL(teamId.value, item.id);
        pre_signed_urls.value = urls;
        if (!urls?.download_link || !urls?.recording_link) {
          throw new Error("Failed to get download URLs");
        }

        // Download audio file
        meetingNotesLoadingMessage.value = "Downloading Audio File";
        const audioBlob = await new Promise<Blob>((resolve, reject) => {
          const xhr = new XMLHttpRequest();
          xhr.open('GET', urls.recording_link, true);
          xhr.setRequestHeader('Content-Type', "video/mp4");
          xhr.responseType = 'blob';

          xhr.onload = () => {
            if (xhr.status === 200) {
              resolve(xhr.response);
            } else {
              reject(new Error(`Audio download failed with status ${xhr.status}`));
            }
          };

          xhr.onerror = () => {
            reject(new Error('Audio download failed'));
          };

          xhr.onprogress = (event) => {
            if (event.lengthComputable) {
              downloadProgressMeetingObject.value = Math.round((event.loaded * 100) / event.total);
            }
          };

          xhr.send();
        });

        // Create audio URL and initialize Howler
        sound.value = new Howl({
          src: [URL.createObjectURL(audioBlob)],
          format: ['mp3', 'mp4'],
          html5: true,
          onend: () => {
            isPlayingAudio.value = false;
            currentTime.value = 0;
          },
          onplay: () => {
            const updateTime = () => {
              if (sound.value && isPlayingAudio.value) {
                currentTime.value = sound.value.seek();
                requestAnimationFrame(updateTime);
              }
            };
            updateTime();
          },
          onpause: () => {
            currentTime.value = sound.value ? sound.value.seek() : 0;
          },
          onstop: () => {
            selectedWords.value = [];
            currentTime.value = 0;
          }
        });

        // Create a promise wrapper for XMLHttpRequest
        downloadProgressMeetingObject.value = 0;
        meetingNotesLoadingMessage.value = "Downloading Meeting Notes"

        meetingNotesObject.value = await new Promise<MeetingNotesType>((resolve, reject) => {
          const xhr = new XMLHttpRequest();
          xhr.open('GET', urls.download_link, true);
          xhr.setRequestHeader('Content-Type', 'application/json');
          // Add headers
          xhr.onload = () => {
            if (xhr.status === 200) {
              try {
                const parsedResponse = JSON.parse(xhr.response);
                resolve(parsedResponse);
              } catch (e) {
                reject(new Error('Invalid JSON response from server'));
              }
            } else {
              reject(new Error(`Request failed with status ${xhr.status}`));
            }
          };

          xhr.onerror = () => {
            reject(new Error('Request failed'));
          };

          xhr.onprogress = (event) => {
            if (event.lengthComputable) {
              downloadProgressMeetingObject.value = Math.round((event.loaded * 100) / event.total);
            }
          };
          xhr.send();
        });

        if (!meetingNotesObject || !meetingNotesObject.value.segments) {
          throw new Error("No data received from server or invalid data format");
        }

        try {
          calculateConfidenceStats();

        } catch (error) {
          console.error("Error processing transcript:", error);
          throw new Error(error instanceof Error ? error.message : "Failed to process transcript data");
        }

        // Ensure we have valid data before opening dialog
        if (meetingNotesObject.value.segments) {
          // Force reactivity updates
          // Preprocess segment words for faster rendering
          preprocessSegmentWords();

          // Calculate confidence statistics
          calculateConfidenceStats();
          meetingNotes.value = true;
        } else {
          throw new Error("Transcript data is empty or malformed");
        }
      } catch (error) {
        console.error("Error opening meeting notes:", error instanceof Error ? error.message : 'Unknown error');
        showError(error instanceof Error ? error.message : "Unable to load transcript data");
        meetingNotes.value = false;
      } finally {
        isLoadingMeetingNotesObjects.value = false;
        meetingNotesLoadingMessage.value = ""

      }
    };

    const showConfidence = (word: SpeakerWordType, word_id: number, event: MouseEvent) => {
      event.preventDefault();

      if (event.altKey) {
        // Handle adding to custom vocabulary
        selectedWords.value = [{ word, word_id: word_id }];
        const phrase = word.content.trim();
        addToCustomVocabulary(phrase);
        return;
      }

      if (event.shiftKey) {
        // Open word change dialog
        // Check if the clicked word is part of the current selection
        const isWordInSelection = selectedWords.value.some(w => w.word_id === word_id);

        // If multiple words are selected and clicked word is in selection,
        // keep the selection and open dialog
        if (selectedWords.value.length > 1 && isWordInSelection) {
          alternativeWord.value = getSelectedWordsText(); // Set initial value
          wordDialog.value = true;
        } else {
          // Otherwise, switch to single-select mode
          selectedWords.value = [{ word, word_id: word_id }];
          alternativeWord.value = word.content.trim(); // Set initial value for single word
          wordDialog.value = true;
        }
        return;
      }

      if (event.ctrlKey || event.metaKey) {
        // Check if there are already selected words
        if (selectedWords.value.length > 0) {
          // Get the last selected word
          const lastSelected = selectedWords.value[selectedWords.value.length - 1];

          // Check if the clicked word is adjacent (next or previous)
          const isAdjacent = Math.abs(lastSelected.word_id - word_id) === 1;

          // If not adjacent and not the same word, clear selection and start new
          if (!isAdjacent) {
            selectedWords.value = [{ word_id: word_id, word }];
            return;
          }

          // Handle selection/deselection
          const existingIndex = selectedWords.value.findIndex(
            w => w.word_id === word_id
          );

          if (existingIndex === -1) {
            // Add new word
            selectedWords.value = [
              ...selectedWords.value,
              { word, word_id }
            ].sort((a, b) => {
              // Sort by segment first, then by word index
              return a.word.word_id - b.word.word_id;
            });
          } else {
            // Remove word if it's not breaking the chain
            const wouldBreakChain = selectedWords.value.length > 1 &&
              existingIndex > 0 &&
              existingIndex < selectedWords.value.length - 1;

            if (!wouldBreakChain) {
              selectedWords.value = selectedWords.value.filter((_, index) => index !== existingIndex);
            }
          }
        } else {
          // First selection
          selectedWords.value = [{ word, word_id: word_id }];
        }
      } else {
        // Just clicking on a word selects it and shows details
        selectedWords.value = [{ word, word_id: word_id }];
      }
    };

    const selectedWordIds = computed(() =>
      selectedWords.value.map(sw => sw.word_id)
    );

    // Selected words map for O(1) lookups
    const selectedWordsMap = ref<Record<number, boolean>>({});

    // Update the isWordSelected function to use the map for faster lookups
    const isWordSelected = (wordIndex: number) => {
      return !!selectedWordsMap.value[wordIndex] || selectedWordIds.value.includes(wordIndex);
    };

    const getSelectedWordsText = () => {
      return selectedWords.value
        .map(selected => selected.word.content.trim())
        .join(' ');
    };

    const splitIntoWords = (text: string): string[] => {
      return text.trim().split(/\s+/).filter(word => word.length > 0);
    };

    const updateWord = () => {
      if (selectedWords.value.length === 0) return;
      if (!meetingNotesObject.value) return;

      const newWords = splitIntoWords(alternativeWord.value);

      // Initialize changes array if it doesn't exist
      if (!meetingNotesObject.value.changes) {
        meetingNotesObject.value.changes = [];
      }

      // Sort selected words by position
      const sortedWords = [...selectedWords.value].sort((a, b) => {
        return a.word_id - b.word_id;
      });

      const firstWord = sortedWords[0];
      const lastWord = sortedWords[sortedWords.length - 1];
      const timePerWord = (Number(lastWord.word.end_time) - Number(firstWord.word.start_time)) / newWords.length;

      try {
        // Create new word objects for replacement
        const newWordObjects = newWords.map((word, index) => {
          const startTime = Number(firstWord.word.start_time) + (timePerWord * index);
          const endTime = startTime + timePerWord;

          return {
            word_id: firstWord.word_id + index,
            segment_id: firstWord.word.segment_id,
            word_type: WordType.Pronunciation,
            content: word,
            confidence: 1.0,
            edited: true,
            start_time: startTime,
            end_time: endTime
          } as SpeakerWordType;
        });

        // Track changes for each original word and its corresponding new word(s)
        let currentNewWordIndex = 0;
        selectedWords.value.forEach(({ word, word_id }) => {
          // Calculate how many new words this original word maps to
          const wordsPerOriginal = Math.ceil(newWords.length / selectedWords.value.length);
          const startIndex = currentNewWordIndex;
          const endIndex = Math.min(currentNewWordIndex + wordsPerOriginal, newWords.length);

          // Create a change record for this word
          const change: NotesWordChanges = {
            type: ScriptChangeType.Update,
            original: word.content,
            change: newWords.slice(startIndex, endIndex).join(' '),
            timestamp: new Date().toISOString(),
            word_id: word_id,
            user: currentUser.value?.username!,
            undone: false,
            redone: false,
            word: word
          };
          meetingNotesObject.value?.changes?.push(change);

          currentNewWordIndex = endIndex;
        });

        if (meetingNotesObject.value?.words) {
          // Replace old words with new ones
          meetingNotesObject.value.words.splice(firstWord.word_id, sortedWords.length, ...newWordObjects);

          // Update segments if needed
          if (meetingNotesObject.value.segments) {
            const segmentIndex = meetingNotesObject.value.segments.findIndex((segment: SpeakerSegmentType) =>
              Number(segment.start_time) === Number(firstWord.word.start_time));

            if (segmentIndex !== -1) {
              meetingNotesObject.value.segments[segmentIndex].transcript = newWords.join(' ');
            }
          }

          showSuccess("Words updated successfully");
        }
      } catch (error) {
        console.error("Error updating words:", error);
        showError("Failed to update words");
      } finally {
        preprocessSegmentWords();
        wordDialog.value = false;
        selectedWords.value = [];
        alternativeWord.value = "";
      }
    };

    const viewPromptResponses = async (item: any) => {
      loadingPromptResponses.value = true;
      promptResponsesDialog.value = true;

      try {
        const responses = await MeetingService.GetPromptResponses(teamId.value, item.id);
        promptResponses.value = responses.records || [];
      } catch (error) {
        console.error("Error fetching prompt responses:", error);
        // You might want to add error handling/notification here
      } finally {
        loadingPromptResponses.value = false;
      }
    };
    const snackbar = ref({
      show: false,
      message: "",
      color: "success",
    });
    const shouldShowErrorTooltip = (item: MeetingResponse): boolean => {
      if (item.current_step === MeetingSetupProgressEnum.PromptsFailed ||
        item.current_step === MeetingSetupProgressEnum.TranscribeFailed ||
        item.current_step === MeetingSetupProgressEnum.SealFailed) {
        return true;
      }
      if (item.custom_vocabulary && item.custom_vocabulary?.current_step !== CustomVocabularyProgressEnum.Published && item.current_step !== MeetingSetupProgressEnum.Sealed) {
        return true;
      }
      return false;
    };

    const getRowClass = (item: MeetingResponse) => {
      if (item.custom_vocabulary?.current_step != CustomVocabularyProgressEnum.Published) {
        return { class: 'errorRow' }
      }
      return '';
    };

    const getStepTooltip = (item: MeetingResponse): string => {
      if (shouldShowErrorTooltip(item) && item.transcribe_error) {
        return item.transcribe_error;
      }
      if (item.custom_vocabulary?.current_step != CustomVocabularyProgressEnum.Published) {
        return "Custom vocabulary is not published and not ready for use";
      }
      return "";
    };

    const fetchPromptSets = async () => {
      if (!teamId.value) return;

      try {
        loadingPromptSets.value = true;
        promptSetsError.value = "";
        const response = await PromptSetService.getPromptSets(teamId.value);
        promptSets.value =
          response.records?.filter((x) => x.status == StatusEnum.Active) || [];
      } catch (error) {
        console.error("Error fetching prompt sets:", error);
        promptSetsError.value = "Failed to load prompt sets";
        promptSets.value = [];
      } finally {
        loadingPromptSets.value = false;
      }
    };

    const fetchCustomVocabularies = async () => {
      if (!teamId.value) return;

      try {
        loadingCustomVocabularies.value = true;
        customVocabulariesError.value = "";
        const response = await CustomVocabularyService.getCustomVocabularies(teamId.value);
        customVocabularies.value = response.records || [];
      } catch (error) {
        console.error("Error fetching custom models:", error);
        customVocabulariesError.value = "Failed to load custom models";
        customVocabularies.value = [];
      } finally {
        loadingCustomVocabularies.value = false;
      }
    };

    const fetchCustomModels = async () => {
      if (!teamId.value) return;

      try {
        loadingCustomModels.value = true;
        customModelsError.value = "";
        const response = await CustomModelService.getCustomModels(teamId.value);
        customModels.value =
          response.records?.filter(
            (x) =>
              x.status == StatusEnum.Active &&
              x.custom_model_progress_status == CustomModelSetupProgressEnum.ModelReady
          ) || [];
      } catch (error) {
        console.error("Error fetching custom models:", error);
        customModelsError.value = "Failed to load custom models";
        customModels.value = [];
      } finally {
        loadingCustomModels.value = false;
      }
    };
    watch(
      () => dialog.value,
      (newValue) => {
        if (newValue && teamId.value) {
          if (promptSets.value.length === 0) {
            fetchPromptSets();
          }
          if (customModels.value.length === 0) {
            fetchCustomModels();
          }
        }
      }
    );
    // Watch for changes in selectedMeetingId
    watch(selectedMeetingId, (newId) => {
      if (!newId) {
        selectedAction.value = "";
        selectedMeeting.value = null;
      } else {
        // Update selectedMeeting when ID changes
        selectedMeeting.value = meetings.value.find(meeting => meeting.id === newId) || null;
      }
    });

    const getProgressColor = (progress: MeetingSetupProgressEnum): string => {
      switch (progress) {
        case MeetingSetupProgressEnum.Created:
          return "info";
        case MeetingSetupProgressEnum.Uploaded:
          return "primary";
        case MeetingSetupProgressEnum.Transcribing:
          return "warning";
        case MeetingSetupProgressEnum.Transcribed:
          return "success";
        case MeetingSetupProgressEnum.TranscribeFailed:
          return "error";
        case MeetingSetupProgressEnum.NotesReviewed:
          return "success";
        case MeetingSetupProgressEnum.Completed:
          return "secondary";
        case MeetingSetupProgressEnum.PromptProcessing:
          return "warning";
        case MeetingSetupProgressEnum.PromptsFailed:
          return "error";
        case MeetingSetupProgressEnum.PromptProcessed:
          return "success";
        case MeetingSetupProgressEnum.Sealed:
          return "secondary";
        default:
          return "grey";
      }
    };

    const meetingTableHeaders = [
      { title: "", key: "select", sortable: false, width: "50px" },
      { title: "Title", key: "title" },
      { title: "Version", key: "meeting_notes_version" },
      { title: "# Documents", key: "meeting_documents.length" },
      { title: "Date", key: "date" },
      { title: "PromptSet", key: "promptset.prompt_set_name" },
      {
        title: "Custom Model",
        key: "custom_model.model_name",
        value: (item: MeetingResponse) =>
          item.custom_model?.model_name || "General Model",
      },
      {
        title: "Current Step",
        key: "current_step",
        value: (item: MeetingResponse) => item.current_step,
      },
      { title: "Status", key: "status" },
      { title: "Created At", key: "created_at" },
      { title: "Updated At", key: "updated_at" },
      // Actions are now handled by the top dropdown
    ];

    const statusOptions = Object.values(StatusEnum);

    const editedItem = ref<MeetingRequest>({
      title: "",
      description: "",
      date: new Date().toISOString().slice(0, 16),
      status: StatusEnum.Active,
      meeting_notes: "",
      include_in_model_training: false,
    });

    interface validType {
      valid: boolean;
      errors: string[];
      results: {
        [field: string]: {
          valid: boolean;
          errors: string[];
        };
      }
    }

    const defaultItem: MeetingRequest = {
      title: "",
      description: "",
      date: new Date().toISOString().slice(0, 16),
      current_step: MeetingSetupProgressEnum.Created,

      status: StatusEnum.Active,
      meeting_notes: "",
      include_in_model_training: false,
    };
    watch(editedItem, () => {
      formRef.value?.validate().then((valid: validType) => {
        formValid.value = valid.valid;
      });
    }, { deep: true });
    const isSavingMeetingNotes = ref(false)
    const uploadDialog = ref(false);
    const selectedFile = ref<File | null>(null);
    const fileError = ref("");
    const uploading = ref(false);
    const selectedMeeting = ref<MeetingResponse | null>(null);

    // Documents Dialog
    const documentsDialog = ref(false);

    const openDocumentsDialog = async (meeting: MeetingResponse) => {
      selectedMeeting.value = meeting;
      documentsDialog.value = true;
    };

    const closeDocumentsDialog = () => {
      documentsDialog.value = false;
      selectedMeeting.value = null;
      fetchMeetings();
    };

    // Analytics Dialog
    const analyticsDialog = ref(false);
    const isLoadingAnalytics = ref(false);
    const speakerTimeChartSeries = ref<any>([]);
    const speakerContributionSeries = ref<any>([]);



    const fileUploadSelectedMeeting = ref<MeetingResponse | null>(null);
    const isDragging = ref(false);
    const isUploading = ref(false);
    const uploadProgress = ref(0);
    const formRef = ref();
    const fileInput = ref<HTMLInputElement | null>(null);

    const uploadFile = async (item: MeetingResponse) => {
      isUploading.value = false
      fileUploadSelectedMeeting.value = item
      uploadDialog.value = true;
    };

    const closeUploadDialog = () => {
      uploadDialog.value = false;
      uploadProgress.value = 0;
      selectedMeeting.value = null;
    };

    const triggerFileInput = () => {
      if (fileInput.value) {
        fileInput.value.click();
      } else {
        console.log('fileInput is null'); // Debug log
      }
    };

    const handleFileDrop = (event: DragEvent) => {
      isDragging.value = false;
      const files = event.dataTransfer?.files;
      if (files?.length) {
        handleUpload(files[0]);
      }
    };

    const handleFileSelect = (event: Event) => {
      const files = (event.target as HTMLInputElement).files;
      if (files?.length) {
        handleUpload(files[0]);
      }
    };

    const handleUpload = async (file: File) => {
      if (!fileUploadSelectedMeeting.value) return;
      isUploading.value = true;

      try {
        const upload_url = (
          await MeetingService.createSignedUrl(teamId.value, fileUploadSelectedMeeting.value.id)
        ).pre_signed_url!;
        const xhr = new XMLHttpRequest();
        xhr.upload.onprogress = (event) => {
          isUploading.value = true;
          if (event.lengthComputable) {
            uploadProgress.value = (event.loaded / event.total) * 100;
          }
        };

        xhr.open("PUT", upload_url, true);
        xhr.setRequestHeader("Content-Type", file.type);
        xhr.send(file);

        // Add completion handlers
        await new Promise<void>((resolve, reject) => {
          xhr.onload = async () => {
            if (xhr.status === 200) {
              showSuccess("File uploaded successfully");
              const updateMeeting: MeetingRequest = {
                current_step: MeetingSetupProgressEnum.Uploaded
              }
              await MeetingService.updateMeeting(
                teamId.value,
                fileUploadSelectedMeeting?.value?.id ?? "",
                updateMeeting
              );
              await fetchMeetings();
              closeUploadDialog();
              resolve();
            } else {
              reject(new Error(`Upload failed with status ${xhr.status}`));
            }
          };

          xhr.onerror = () => {
            reject(new Error("Upload failed"));
          };
        });



      } catch (error) {
        showError("Failed to upload file: " + (error instanceof Error ? error.message : "Unknown error"));
      } finally {
        isUploading.value = false;
      }
    };

    const formTitle = computed(() => {
      return editedIndex.value === -1 ? "New Meeting" : "Edit Meeting";
    });

    const fetchMeetings = async (pageIndex = currentPage.value, itemsPerPage = pageSize.value) => {
      try {
        loading.value = true;
        const response = await MeetingService.getMeetings(
          teamId.value,
          pageIndex,
          itemsPerPage
        );
        meetings.value = response.records || [];
      } catch (error) {
        showError("Failed to fetch meetings");
      } finally {
        selectedMeetingId.value == null;
        selectedMeeting.value = null;
        loading.value = false;
      }
    };

    const handlePageChange = (options: any) => {
      if (!isMounted.value) return
      currentPage.value = options.page || 1;
      pageSize.value = options.itemsPerPage || 10;
      // Store pagination state in localStorage
      localStorage.setItem('meetingsCurrentPage', currentPage.value.toString());
      localStorage.setItem('meetingsPageSize', pageSize.value.toString());
      fetchMeetings(currentPage.value, pageSize.value);
    };

    // Handle prompt responses table pagination changes
    const handlePromptResponsesPageChange = (options: any) => {
      if (!isMounted.value) return;
      promptResponsesCurrentPage.value = options.page || 1;
      promptResponsesPageSize.value = options.itemsPerPage || 5;
      // Store pagination state in localStorage
      localStorage.setItem('promptResponsesCurrentPage', promptResponsesCurrentPage.value.toString());
      localStorage.setItem('promptResponsesPageSize', promptResponsesPageSize.value.toString());
    };

    const openMeetingDialog = (item?: MeetingResponse) => {
      if (item) {
        editedIndex.value = meetings.value.indexOf(item);
        editedItem.value = {
          ...item,
          meeting_notes: item.meeting_notes?.replace(/\\n/g, "\n") || "",
        };
        if (editedItem.value.date) {
          editedItem.value.date = new Date(editedItem.value.date)
            .toISOString()
            .slice(0, 16);
        }
      } else {
        editedIndex.value = -1;
        editedItem.value = { ...defaultItem };
      }

      setTimeout(() => {
        const titleInput = document.querySelector('.v-dialog input[type="text"]');
        if (titleInput) {
          (titleInput.previousElementSibling as HTMLElement)?.focus();
        }
      }, 100);
      dialog.value = true;
      if (promptSets.value.length === 0) {
        fetchPromptSets();
      }
      if (customModels.value.length === 0) {
        fetchCustomModels();
      }
      if (customVocabularies.value.length === 0) {
        fetchCustomVocabularies();
      }
    };
    const formatPlayerSecondsToTime = (inputSeconds: number) => {

      const totalSeconds = Math.floor(inputSeconds);

      // Calculate hours, minutes, and seconds
      const hours = Math.floor(totalSeconds / 3600);
      const minutes = Math.floor((totalSeconds % 3600) / 60);
      const seconds = totalSeconds % 60;

      // Pad with zeros if needed
      const formattedHours = String(hours).padStart(2, '0');
      const formattedMinutes = String(minutes).padStart(2, '0');
      const formattedSeconds = String(seconds).padStart(2, '0');

      return `${formattedHours}:${formattedMinutes}:${formattedSeconds}`;
    }

    const closeMeetingDialog = () => {
      dialog.value = false;
      formRef.value?.reset();
      setTimeout(() => {
        editedItem.value = { ...defaultItem };
        editedIndex.value = -1;
      }, 300);
    };

    const exportMeetingNotes = async (item: MeetingResponse) => {
      try {
        loading.value = true;
        // First download the meeting notes
        const urls = await MeetingService.downloadMeetingNotesURL(teamId.value, item.id);
        if (!urls?.download_link) {
          throw new Error("Failed to get download URL");
        }

        // Download and parse the JSON data
        const response = await new Promise<MeetingNotesType>((resolve, reject) => {
          const xhr = new XMLHttpRequest();
          xhr.open('GET', urls.download_link, true);
          xhr.setRequestHeader('Content-Type', 'application/json');

          xhr.onload = () => {
            if (xhr.status === 200) {
              try {
                const parsedResponse = JSON.parse(xhr.response);
                resolve(parsedResponse);
              } catch (e) {
                reject(new Error('Invalid JSON response from server'));
              }
            } else {
              reject(new Error(`Request failed with status ${xhr.status}`));
            }
          };

          xhr.onerror = () => {
            reject(new Error('Request failed'));
          };

          xhr.send();
        });

        // Process the downloaded transcript
        let transcriptOutput = '';
        let initial = true;

        if (response?.segments) {
          // Process segments with speaker labels
          response.segments.forEach((segment: any) => {
            if (segment.speaker_label) {
              if (initial) {
                transcriptOutput = `Speaker: ${segment.speaker_label}\n`;
              } else {
                transcriptOutput += `\n\nSpeaker: ${segment.speaker_label}\n`;
              }
              initial = false;

              // Get words for this segment by segment_id
              const segmentWords = response.words
                .filter((item: SpeakerWordType) => item.segment_id === segment.segment_id)
                .map((item: any) => item.content)
                .join(' ');

              transcriptOutput += segmentWords;
            }
          });
        } else {
          // Fallback: Process all words without speaker labels
          transcriptOutput = response.words
            .map((item: any) => item.content)
            .join(' ');
        }

        // Export to file
        const blob = new Blob([transcriptOutput], { type: 'text/plain' });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `meeting_notes_${item.title}.txt`);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);

        showSuccess("Meeting notes exported successfully");
      } catch (error) {
        console.error("Error exporting meeting notes:", error);
        showError("Failed to export meeting notes");
      }
      finally {
        loading.value = false;
      }
    };

    const exportPromptResponses = () => {
      try {
        if (!promptResponses.value || promptResponses.value.length === 0) {
          showError("No prompt responses to export");
          return;
        }

        // Create the formatted text content
        let textContent = "";

        promptResponses.value.forEach((response, index) => {
          // Add a line separator if it's not the first item
          if (index > 0) {
            textContent += "\n\n" + "-".repeat(80) + "\n\n";
          }

          // Add the prompt
          textContent += "PROMPT:\n";
          textContent += response.prompt + "\n\n";

          // Add the response
          textContent += "RESPONSE:\n";
          textContent += response.prompt_response;
        });

        // Export to file
        const blob = new Blob([textContent], { type: 'text/plain' });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `prompt_responses_${new Date().toISOString().split('T')[0]}.txt`);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);

        showSuccess("Prompt responses exported successfully");
      } catch (error) {
        console.error("Error exporting prompt responses:", error);
        showError("Failed to export prompt responses");
      }
    };

    const saveMeetingNotes = async () => {
      if (!pre_signed_urls.value?.upload_link) {
        showError("No upload URL available");
        return;
      }
      try {
        isSavingMeetingNotes.value = true;
        // Update meeting record
        if (!selectedMeetingNotesId.value) {
          throw new Error('No meeting ID selected');
        }
        const selectedMeeting = meetings.value.find(meeting => meeting.id === selectedMeetingNotesId.value)

        meetingNotesObject.value.version = selectedMeeting?.meeting_notes_version + 1;
        meetingNotesObject.value.last_edited_by = currentUser.value.username;
        meetingNotesObject.value.last_edited_at = new Date().toISOString();
        if (selectedMeeting?.meeting_notes_version != null) {
          const transcriptOutput: Ref<string> = ref('');
          meetingNotesObject.value?.segments.forEach((segment: SpeakerSegmentType, index: number) => {
            transcriptOutput.value = transcriptOutput.value + "\n\nSegment Speaker: " + segment.speaker_label + "\n"
            transcriptOutput.value = transcriptOutput.value + "Segment Duration: " + segment.start_time + "-" + segment.end_time + "\n"
            meetingNotesObject.value?.words.filter((x: SpeakerWordType) => x.segment_id == segment.segment_id).forEach((word: SpeakerWordType) => {
              transcriptOutput.value = transcriptOutput.value + word.content + " "
            });
          });

          await MeetingService.updateMeeting(teamId.value, selectedMeetingNotesId.value, {
            meeting_notes_version: selectedMeeting?.meeting_notes_version + 1,
            meeting_notes: transcriptOutput.value
          });
        }

        // Create save payload with cumulative changes
        const jsonData = JSON.stringify(meetingNotesObject.value);

        // Upload with progress tracking
        await new Promise<void>((resolve, reject) => {
          const xhr = new XMLHttpRequest();
          xhr.open('PUT', pre_signed_urls.value!.upload_link, true);
          xhr.setRequestHeader('Content-Type', 'application/json');

          xhr.onload = () => {
            if (xhr.status === 200) {
              resolve();
            } else {
              reject(new Error(`Upload failed with status ${xhr.status}`));
            }
          };

          xhr.onerror = () => {
            reject(new Error('Upload failed'));
          };

          xhr.upload.onprogress = (event) => {
            if (event.lengthComputable) {
              downloadProgressMeetingObject.value = Math.round((event.loaded * 100) / event.total);
            }
          };

          xhr.send(jsonData);
        });



        showSuccess("Meeting notes saved successfully");
        selectedMeetingNotesId.value = undefined;

      } catch (error) {
        console.error("Error saving meeting notes:", error);
        showError(error instanceof Error ? error.message : "Failed to save meeting notes");
        throw error; // Re-throw to handle in calling code
      } finally {
        isSavingMeetingNotes.value = false;
        uploadProgress.value = 0;
        closeMeetingNotesDialog();
        await fetchMeetings();
      }
    };
    const closeMeetingNotesDialog = () => {
      meetingNotes.value = false;
      selectedMeetingId.value = undefined;
      if (sound.value) {
        sound.value.stop();
        sound.value.unload();
      }
      sound.value = null;
      isPlayingAudio.value = false;
      setTimeout(() => { }, 300);
    };

    const togglePlayback = () => {
      if (!sound.value) return;

      if (isPlayingAudio.value) {
        sound.value.pause();
      } else {
        // Set start time based on selected word if any are selected
        if (selectedWords.value.length > 0) {
          const firstSelectedWord = selectedWords.value[0].word;
          sound.value.seek(firstSelectedWord.start_time);
        } else if (currentTime.value != 0) {
          sound.value.seek(currentTime.value);
        } else {
          sound.value.seek(0);
        }

        sound.value.play();

        // Start tracking current time
        const updateTime = () => {
          if (sound.value && isPlayingAudio.value) {
            currentTime.value = sound.value.seek();
            requestAnimationFrame(updateTime);
          }
        };
        updateTime();
      }
      isPlayingAudio.value = !isPlayingAudio.value;
    };

    const stopPlayback = () => {
      if (!sound.value) return;
      sound.value.stop();
      isPlayingAudio.value = false;
      currentTime.value = 0;
    };

    const saving = ref(false);

    const saveMeeting = async () => {
      const isValid = await formRef.value?.validate();

      if (!isValid) {
        return;
      }
      try {
        saving.value = true;
        const formData = {
          ...editedItem.value,
          meeting_notes: editedItem.value.meeting_notes?.replace(/\n/g, "\\n"),
        };
        if (editedIndex.value > -1) {
          const meeting = meetings.value[editedIndex.value];
          if (!meeting) {
            throw new Error("Meeting not found");
          }
          await MeetingService.updateMeeting(teamId.value, meeting.id, formData);
          showSuccess("Meeting updated successfully");
        } else {
          // Create new meeting
          await MeetingService.createMeeting(teamId.value, formData);
          showSuccess("Meeting created successfully");
        }
        await fetchMeetings();
        closeMeetingDialog();
      } catch (error) {
        if (error instanceof Error) {
          console.error("Error:", error.message);
          showError(error.message);
        } else {
          console.error("Unknown error:", error);
          showError("An unknown error occurred");
        }
      } finally {
        saving.value = false;
      }
    };
    const sealMeeting = async (item: MeetingResponse) => {
      if (!confirm("Are you sure you want to seal this meeting?")) return;
      try {
        loading.value = true;
        await MeetingService.sealMeeting(teamId.value, item.id);
        await fetchMeetings();
        showSuccess("Meeting sealed successfully");
      } catch (error) {
        showError("Failed to seal meeting");
      }
      finally {
        selectedMeetingId.value = undefined;
        loading.value = false;
      }
    };
    const deleteMeeting = async (item: MeetingResponse) => {
      if (!confirm("Are you sure you want to delete this meeting?")) return;
      loading.value = true;

      try {
        await MeetingService.deleteMeeting(teamId.value, item.id);
        await fetchMeetings();
        showSuccess("Meeting deleted successfully");
      } catch (error) {
        showError("Failed to delete meeting");
      }
      finally {
        loading.value = false;
      }
    };
    const startPromptProcessing = async (item: MeetingResponse) => {
      if (!confirm("Are you sure you want to start prompt processing for this meeting?"))
        return;

      try {
        loading.value = true;

        await MeetingService.startPromptProcessing(teamId.value, item.id);
        await fetchMeetings();
        showSuccess("Prompt processing started successfully");
      } catch (error) {
        showError("Failed to start Prompt processing");
      } finally {
        selectedMeetingId.value = undefined;
        loading.value = false;
      }
    };

    const startTranscriptionProcessing = async (item: MeetingResponse) => {
      if (!confirm("Are you sure you want to start the transcribing process for this meeting?")) {
        return;
      }

      try {
        loading.value = true;

        if (!teamId.value || !item.id) {
          throw new Error("Missing team ID or meeting ID");
        }

        await MeetingService.startTranscriptionProcessing(teamId.value, item.id);
        await fetchMeetings();
        showSuccess("Transcription processing started successfully");
      } catch (error) {
        console.error("Error starting transcription:", error);
        showError(error instanceof Error ? error.message : "Failed to start transcription processing");
        throw error;
      } finally {
        selectedMeetingId.value = undefined;
        loading.value = false;
      }
    };

    const customVocabularyDialog = ref(false);
    const selectedPhrase = ref("");
    const selectedVocabularyId = ref<string | null>(null);
    const displayAs = ref("");

    const closeCustomVocabularyDialog = () => {
      customVocabularyDialog.value = false;
      selectedPhrase.value = "";
      selectedVocabularyId.value = null;
      displayAs.value = "";
    };

    const existingVocabularyPhrases = ref<VocabularyPhraseResponse[]>([]);
    const selectedMeetingVocabularyId = ref<string | null>(null);

    const fetchVocabularyPhrases = async (vocabularyId: string) => {
      try {
        if (!existingVocabularyPhrases.value || existingVocabularyPhrases.value.length === 0) {
          const response = await CustomVocabularyService.getVocabularyPhrases(
            teamId.value,
            vocabularyId
          );
          existingVocabularyPhrases.value = response.records || [];
        }
      } catch (error) {
        console.error("Error fetching vocabulary phrases:", error);
        showError("Failed to fetch vocabulary phrases");
      }
    };

    const addToCustomVocabulary = async (phrase: string) => {
      try {
        // Only allow custom vocabulary functionality if meeting has a vocabulary ID
        if (!selectedMeetingVocabularyId.value) {
          showError("This meeting is not associated with a custom vocabulary");
          return;
        }
        // Set the vocabulary ID from the meeting's vocabulary
        selectedVocabularyId.value = selectedMeetingVocabularyId.value;

        selectedPhrase.value = phrase;
        preprocessSegmentWords();
        customVocabularyDialog.value = true;
      } catch (error) {
        console.error("Error preparing to add to custom vocabulary:", error);
        showError(error instanceof Error ? error.message : "Failed to prepare custom vocabulary dialog");
      }
    };

    const savingCustomvocabulary = ref(false)
    const saveToCustomVocabulary = async () => {
      try {
        savingCustomvocabulary.value = true
        if (!selectedVocabularyId.value) {
          throw new Error("Please select a vocabulary");
        }
        const { valid } = await phraseFormRef.value.validate();
        if (!valid) {
          showError("Please fix the validation errors before saving");
          return;
        }

        const newRecord = await CustomVocabularyService.createVocabularyPhrase(
          teamId.value,
          selectedVocabularyId.value,
          {
            phrase: selectedPhrase.value,
            display_as: displayAs.value || undefined,
            status: StatusEnum.Active
          }
        );

        // Update the existing vocabulary phrases with the new record
        existingVocabularyPhrases.value = [...existingVocabularyPhrases.value, newRecord];

        // Force a refresh of the vocabulary phrase map by making a complete replacement
        // This ensures Vue's reactivity system detects the change
        const refreshedPhrases = await CustomVocabularyService.getVocabularyPhrases(
          teamId.value,
          selectedVocabularyId.value
        );
        existingVocabularyPhrases.value = refreshedPhrases.records || [];

        showSuccess(`Added "${selectedPhrase.value}" to custom vocabulary successfully`);
        closeCustomVocabularyDialog();
      } catch (error) {
        console.error("Error adding to custom vocabulary:", error);
        showError(error instanceof Error ? error.message : "Failed to add to custom vocabulary");
      }
      finally {
        savingCustomvocabulary.value = false
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
        message: message || "An unexpected error occurred",
        color: "error",
      };
      console.error(message);  // Log the error for debugging
    };
    const CONFIDENCE_THRESHOLDS = {
      VERY_HIGH: 98,
      HIGH: 90,
      MEDIUM: 40,
      LOW: 20,
    } as const;

    const CHART_COLORS = {
      VERY_HIGH: "#2E7D32", // Dark Green
      HIGH: "#4CAF50", // Green
      MEDIUM: "#FFC107", // Yellow
      LOW: "#FF9800", // Orange
      VERY_LOW: "#F44336", // Red
    } as const;

    const CONFIDENCE_LEVELS = {
      VERY_HIGH: "(98-100%)",
      HIGH: "(90-98%)",
      MEDIUM: "(60-90%)",
      LOW: "(40-60%)",
      VERY_LOW: "(0-20%)",
    } as const;

    const confidenceChartOptions: ApexOptions = {
      chart: {
        padding: {
          left: 0,
          right: 0,
          top: 0,
          bottom: 0
        },
        type: "bar",
        stacked: true,
        stackType: "100%",
        height: 80,
        fontFamily: "inherit",
        toolbar: {
          show: false,
        },
        animations: {
          enabled: true,
          easing: "easeinout",
          speed: 800,
          animateGradually: {
            enabled: true,
            delay: 150,
          },
          dynamicAnimation: {
            enabled: true,
            speed: 350,
          },
        },
      },
      plotOptions: {
        bar: {
          horizontal: true,
          borderRadius: 2,
          barHeight: "80%",
          distributed: false,
          dataLabels: {
            position: "center",
            maxItems: 100,
            hideOverflowingLabels: true,
          },
        },
      },
      colors: [
        CHART_COLORS.VERY_HIGH,
        CHART_COLORS.HIGH,
        CHART_COLORS.MEDIUM,
        CHART_COLORS.LOW,
        CHART_COLORS.VERY_LOW,
      ],
      xaxis: {
        categories: ["Confidence"],
        labels: {
          show: false,
          formatter: function (value: string) {
            return `${value}%`;
          },
        },
        min: 0,
        max: 100,
        tickAmount: 5,
        axisBorder: {
          show: true,
        },
        axisTicks: {
          show: true,
        },
      },
      yaxis: {
        labels: {
          show: false,
        },
      },
      dataLabels: {
        enabled: true,
        formatter: function (val: number, opts?: any) {
          return val > 0 ? `${val.toFixed(1)}%` : "";
        },
        style: {
          fontSize: "12px",
          colors: ["#fff"],
        },
      },
      tooltip: {
        enabled: true,
        y: {
          formatter: function (value: number): string {
            return `${value.toFixed(1)}%`;
          },
        },
      },
      legend: {
        show: false
      },
      grid: {
        xaxis: {
          lines: {
            show: true,
          },
        },
        yaxis: {
          lines: {
            show: false,
          },
        },
        padding: {
          top: 0,
          right: 20,
          bottom: 0,
          left: 20,
        },
      },
    } as ApexOptions;

    const confidenceStats = ref<ConfidenceStats>({
      veryHigh: 0,
      high: 0,
      medium: 0,
      low: 0,
      veryLow: 0,
    });

    const calculateConfidenceStats = (): void => {
      if (!meetingNotesObject.value?.segments) {
        confidenceStats.value = { veryHigh: 0, high: 0, medium: 0, low: 0, veryLow: 0 };
        return;
      }

      const items = meetingNotesObject.value?.words.filter((item: SpeakerWordType) => item.word_type === "pronunciation") || [];
      const totalWords = items.length;

      if (totalWords === 0) {
        confidenceStats.value = { veryHigh: 0, high: 0, medium: 0, low: 0, veryLow: 0 };
        return;
      }

      const counts = items.reduce(
        (acc: any, item: any) => {
          if (!item?.confidence) return acc;

          const confidence = item.confidence * 100;

          if (confidence >= CONFIDENCE_THRESHOLDS.VERY_HIGH) {
            acc.veryHigh++;
          } else if (confidence >= CONFIDENCE_THRESHOLDS.HIGH) {
            acc.high++;
          } else if (confidence >= CONFIDENCE_THRESHOLDS.MEDIUM) {
            acc.medium++;
          } else if (confidence >= CONFIDENCE_THRESHOLDS.LOW) {
            acc.low++;
          } else {
            acc.veryLow++;
          }

          return acc;
        },
        { veryHigh: 0, high: 0, medium: 0, low: 0, veryLow: 0 }
      );

      confidenceStats.value = {
        veryHigh: Number(((counts.veryHigh / totalWords) * 100).toFixed(1)),
        high: Number(((counts.high / totalWords) * 100).toFixed(1)),
        medium: Number(((counts.medium / totalWords) * 100).toFixed(1)),
        low: Number(((counts.low / totalWords) * 100).toFixed(1)),
        veryLow: Number(((counts.veryLow / totalWords) * 100).toFixed(1)),
      };
    };

    const confidenceChartSeries = computed(() => {
      return [
        {
          name: CONFIDENCE_LEVELS.VERY_HIGH,
          data: [confidenceStats.value.veryHigh],
        },
        {
          name: CONFIDENCE_LEVELS.HIGH,
          data: [confidenceStats.value.high],
        },
        {
          name: CONFIDENCE_LEVELS.MEDIUM,
          data: [confidenceStats.value.medium],
        },
        {
          name: CONFIDENCE_LEVELS.LOW,
          data: [confidenceStats.value.low],
        },
        {
          name: CONFIDENCE_LEVELS.VERY_LOW,
          data: [confidenceStats.value.veryLow],
        },
      ];
    });
    const speakerTimeChartOptions = ref<SpeakerTimeChartOptionsInterface>({
      chartOptions: {
        chart: {
          type: 'pie',
          height: 200
        }
      },
      title: {
        text: 'Speaker Time Distribution',
      },
      labels: []
    });

    const speakerContributionChartOptions = ref<speakerContributionChartOptionsInterface>({
      title: {
        text: 'Speaker Contributions'
      },
      xaxis: {
        categories: ['Total Speaking (min)', 'Speaking Turns', 'Avg Contribution (min)']
      },
      // Optional: Add tooltips for better data representation
      tooltip: {
        y: {
          formatter: function (value: number, { seriesIndex, dataPointIndex }: any) {
            if (dataPointIndex === 0 || dataPointIndex === 2) {
              return `${value} minutes`;
            }
            return value.toString();
          }
        }
      }
    });

    // Cache segment words to avoid repeated filtering
    const segmentWordsMap = ref(new Map());

    // Preprocess words by segment for faster lookup
    const preprocessSegmentWords = () => {
      const map = new Map();
      if (meetingNotesObject.value?.segments && meetingNotesObject.value?.words) {
        meetingNotesObject.value.segments.forEach(segment => {
          if (segment.segment_id !== undefined) {
            map.set(segment.segment_id, meetingNotesObject.value?.words.filter(
              word => word.segment_id === segment.segment_id
            ));
          }
        });
      }
      segmentWordsMap.value = map;
    };

    // Get words for a segment from the cache
    const getSegmentWords = (segment: SpeakerSegmentType): SpeakerWordType[] => {
      if (segment === undefined || segment.segment_id === undefined) {
        console.log("segment_id is undefined");
        return [];
      }
      return segmentWordsMap.value.get(segment.segment_id) || [];
    };

    // For backward compatibility
    const filterWords = getSegmentWords;

    // Cache vocabulary phrases for faster lookup
    const vocabularyPhraseMap = computed(() => {
      const map = new Map();
      existingVocabularyPhrases.value.forEach(phrase => {
        if (phrase.phrase) map.set(phrase.phrase.toLowerCase(), true);
        if (phrase.display_as) map.set(phrase.display_as.toLowerCase(), true);
      });
      return map;
    });

    // Optimized function to check if word is in vocabulary
    const isKnownPhrase = (word: SpeakerWordType): boolean => {
      const content = word.content.trim().toLowerCase();
      return vocabularyPhraseMap.value.has(content);
    };

    // Debounce utility function
    const debounce = (fn: Function, delay: number) => {
      let timer: number | null = null;
      return function(this: any, ...args: any[]) {
        const context = this;
        if (timer) clearTimeout(timer);
        timer = setTimeout(() => {
          fn.apply(context, args);
          timer = null;
        }, delay) as unknown as number;
      };
    };

    // Debounced version of showConfidence
    const showConfidenceDebounced = debounce((word: SpeakerWordType, wordId: number, event: MouseEvent) => {
      showConfidence(word, wordId, event);
    }, 50);

    // Helper function for word class bindings
    const getWordClasses = (word: SpeakerWordType) => {
      return {
        selectedWord: isWordSelected(word.word_id),
        'low-confidence': isLowConfidence(word),
        editedWord: word.edited,
        'edited-word': word.edited,
        'known-phrase': isKnownPhrase(word),
        'data-type': word.word_type == WordType.Pronunciation,
        'playing-word': isWordPlaying(word),
        'no-padding': word.word_type !== WordType.Pronunciation,
      };
    };



    const formatTimestamp = (timestamp: number): string => {
      const time = parseFloat(timestamp.toString());
      const minutes = Math.floor(time / 60);
      const seconds = Math.floor(time % 60);
      return `${minutes}:${seconds.toString().padStart(2, '0')}`;
    };
    // Add new refs for analytics data
    const meetingAnalytics = ref<MeetingAnalytics | null>(null);
    const exportingPDF = ref(false);

 

    const openAnalyticsDialog = async (selectedMeeting: MeetingResponse) => {
      analyticsDialog.value = true;
      isLoadingAnalytics.value = true;
      try {
        speakerTimeChartOptions.value.labels = [];
        speakerTimeChartSeries.value = {
          series: [],
          labels: []
        };
        speakerContributionSeries.value = [];

        // Get the analytics data
        const payload = await MeetingService.downloadMeetingAnalytics(teamId.value, selectedMeeting.id);
        meetingAnalytics.value = <MeetingAnalytics>JSON.parse(payload);
        meetingAnalytics.value.meetingMetadata.meetingTitle = selectedMeeting.title ?? '';
        // Process the data for the chart
        const speakers = meetingAnalytics.value.attendance.participants.map(speaker => (speaker.id));
        speakers.forEach(speaker => {
          meetingAnalytics?.value?.attendance.participants
            .filter(x => x.id.includes(speaker))
            .map(speaker_record => {
              speakerContributionSeries.value.push({
                name: speaker_record.id,
                data: [
                  Math.round(speaker_record.speakingMetrics.totalDuration / 60), // Convert to minutes and round
                  speaker_record.speakingMetrics.speakingTurns,
                  Math.round(speaker_record.speakingMetrics.averageContributionLength / 60) // Convert to minutes and round
                ],
              },

              )
            })
        });
        meetingAnalytics?.value?.attendance.participants.map(speaker_record => {
          speakerTimeChartOptions.value.labels.push(speaker_record.id),
            speakerTimeChartSeries.value.series.push(speaker_record.speakingMetrics.totalDuration / 60)
        });



        showSuccess("Analytics loaded successfully");
        isLoadingAnalytics.value = false;
      } catch (error) {
        console.error("Error loading analytics:", error);
        showError("Failed to load analytics data");
        analyticsDialog.value = false;
      } finally {
        isLoadingAnalytics.value = false;
      }
    };

    const closeAnalyticsDialog = () => {
      analyticsDialog.value = false;
    };

    // Chart options for time management
    const timeManagementChartOptions = {
      chart: {
        height: 100,
        type: 'pie'
      },
      xaxis: {
        categories: ["Category 1", "Category 2", "Category 3"]
      },
      labels: ['Productive Time', 'Idle Time'],
      colors: ['#4CAF50', '#FF5252'],
      legend: {
        position: 'right'
      }
    };

    // Helper functions for action items
    const getPriorityColor = (priority: string): string => {
      switch (priority.toLowerCase()) {
        case 'high': return 'error';
        case 'medium': return 'warning';
        case 'low': return 'success';
        default: return 'grey';
      }
    };

    const getStatusColor = (status: string): string => {
      switch (status.toLowerCase()) {
        case 'completed': return 'success';
        case 'in progress': return 'info';
        case 'pending': return 'warning';
        case 'overdue': return 'error';
        default: return 'grey';
      }
    };

    return {
      confidenceChartOptions,
      confidenceChartSeries,
      copyToClipboard,
      confidenceStats,
      // Export new refs and functions
      meetingAnalytics,
      timeManagementChartOptions,
      getPriorityColor,
      getStatusColor,
      promptSets,
      loadingPromptSets,
      promptSetsError,
      currentPage,
      pageSize,
      customModels,
      loadingCustomModels,
      customModelsError,
      meetings,
      loading,
      search,
      meetingTableHeaders,
      dialog,
      editedIndex,
      editedItem,
      formTitle,
      snackbar,
      statusOptions,
      handlePageChange,
      openMeetingDialog,
      closeMeetingDialog,
      saveMeeting,
      deleteMeeting,
      startTranscriptionProcessing,
      uploadDialog,
      selectedFile,
      fileError,
      uploading,
      closeUploadDialog,
      isDragging,
      uploadProgress,
      formRef,
      formValid,
      fetchMeetings,
      saving,
      handleFileDrop,
      triggerFileInput,
      handleFileSelect,
      uploadFile,
      dateFormatter,
      getProgressColor,
      shouldShowErrorTooltip,
      getStepTooltip,
      startPromptProcessing,
      promptResponsesDialog,
      promptResponseHeaders,
      promptResponses,
      viewPromptResponses,
      loadingPromptResponses,
      selectedWord,
      confidenceThreshold,
      isLowConfidence,
      alternativeWord,
      handleWordClick,
      updateWord,
      undoWordUpdate,
      redoWordUpdate,
      selectedWords,
      meetingNotes,
      openMeetingNotesDialog,
      wordDialog,
      closeMeetingNotesDialog,
      saveMeetingNotes,
      selectedMeetingId,
      selectedAction,
      actionItems,
      executeSelectedAction,
      editedSpeaker,
      formatTimestamp,
      selectedMeeting,
      isLoadingMeetingNotesObjects,
      downloadProgressMeetingObject,
      sound,
      isPlayingAudio,
      togglePlayback,
      stopPlayback,
      isWordSelected,
      showConfidence,
      closeWordDialog,
      getSelectedWordsText,
      handleKeyDown,
      canUndo,
      canRedo,
      isSavingMeetingNotes,
      customVocabularies,
      loadingCustomVocabularies,
      customVocabulariesError,
      CanPerformEnum,
      isUploading,
      fileInput,
      customVocabularyDialog,
      selectedPhrase,
      selectedVocabularyId,
      displayAs,
      addToCustomVocabulary,
      saveToCustomVocabulary,
      closeCustomVocabularyDialog,
      existingVocabularyPhrases,
      meetingNotesLoadingMessage,
      // Speaker update functionality
      speakerUpdateDialog,
      closeSpeakerDialog,
      updateSpeakerName,
      openSpeakerDialog,
      analyticsDialog,
      isLoadingAnalytics,
      speakerTimeChartOptions,
      speakerTimeChartSeries,
      speakerContributionChartOptions,
      speakerContributionSeries,
      closeAnalyticsDialog,
      exportingPDF,
      phraseFormRef,
      CustomVocabularyProgressEnum,
      MeetingSetupProgressEnum,
      meetingNotesObject,
      filterWords,
      WordType,
      isWordPlaying,
      formatPlayerSecondsToTime,
      currentTime,
      documentsDialog,
      closeDocumentsDialog,
      savingCustomvocabulary,
      promptResponsesPageSize,
      promptResponsesCurrentPage,
      handlePromptResponsesPageChange,
      showConfidenceDebounced,
      getSegmentWords,
      getWordClasses,
      exportPromptResponses,
      isProcessingState
    };
  },
};
</script>
