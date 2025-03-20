<!--
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
 -->

<template>
  <v-container fluid class="fill-height pa-0">
    <v-card elevation="5" class="chatbot-card">
      <!-- Header - Static -->
      <v-card-title class="chat-header d-flex align-center justify-space-between">
        <div class="d-flex align-center">
          <v-icon size="small" class="mr-2">mdi-assistant</v-icon>
          Meeting Assistant
        </div>
        <v-select
          v-model="selectedMeeting"
          :items="[{ title: 'All Meetings', id: null }, ...meetings]"
          :loading="meetingsLoading"
          item-title="title"
          item-value="id"
          width="150"
          label="Meeting"
          density="compact"
          variant="outlined"
          hide-details
          class="meeting-select"
          @update:model-value="handleMeetingChange"
        >
        </v-select>
      </v-card-title>

      <!-- Messages - Scrollable -->
      <div class="chat-container" ref="chatContainer">
        <v-overlay class="align-center justify-center" contained persistent>
          <v-progress-circular indeterminate color="primary"></v-progress-circular>
        </v-overlay>
        <div
          v-for="(message, index) in messages"
          :key="index"
          :class="['message', message.type === 'user' ? 'user-message' : 'bot-message']"
        >
          <v-img
            v-if="message.type === 'bot'"
            src="/public/bedrock-color.svg"
            class="message-icon no-flex"
            :width="30"
            :height="30"
            alt="Bedrock Icon"
          />
          <div class="message-wrapper">
            <div class="message-content">
              <span style="white-space: pre-wrap">{{ message.text }}</span>
            </div>
            <div class="message-timestamp">
              {{ formatTimestamp(message.timestamp) }}
            </div>
          </div>
          <v-icon
            v-if="message.type === 'user'"
            class="message-icon user-icon"
            size="large"
          >
            mdi-account-arrow-right
          </v-icon>
        </div>
        <div v-if="isLoading" class="loader"></div>
      </div>

      <!-- Input Form - Static -->
      <div class="chat-input-container">
        <v-form @submit.prevent="sendMessage">
          <v-textarea
            v-model="newMessage"
            label="Question"
            rows="3"
            auto-grow
            hide-details
            :disabled="isLoading"
            class="chat-input"
            @keydown.enter.prevent="sendMessage"
            variant="outlined"
            density="comfortable"
          >
          </v-textarea>
          <v-btn
            color="primary"
            class="mt-3"
            @click="sendMessage"
            :disabled="!newMessage.trim() || isLoading"
            min-width="120"
          >
            Send
            <v-icon class="ml-2" right>mdi-cube-send</v-icon>
          </v-btn>
        </v-form>
      </div>
    </v-card>
  </v-container>
</template>

<script lang="ts">
import { computed, ref, defineComponent, onMounted } from "vue";
import { MeetingService } from "@services/meeting.service";
import { MeetingResponse, MeetingSetupProgressEnum } from "@/src/types/meeting";
import { useStore } from "vuex";
import { ChatbotRequest, ModelType } from "@/src/types/meetingassistant";
import { ChatbotService } from "@services/meetingassistant.service";

interface Message {
  type: "user" | "bot";
  text: string;
  timestamp: Date;
}

interface SnackbarState {
  show: boolean;
  message: string;
  color: "success" | "error";
}

export default defineComponent({
  name: "MeetingAssistant",

  setup() {
    onMounted(() => {
      fetchMeetings();
    });
    const isLoading = ref<boolean>(false);
    const newMessage = ref<string>("");
    const chatContainer = ref<HTMLElement | null>(null);
    const selectedModel = computed({
      get: () => store.state.selectedModelId || "anthropic.claude-3-sonnet-20240229-v1:0",
      set: (value) => store.commit("SET_SELECTED_MODEL", value),
    });

    const selectedMeeting = computed({
      get: () => store.state.selectedMeetingId,
      set: (value) => store.commit("SET_SELECTED_MEETING", value),
    });
    const meetingsLoading = ref<boolean>(false);
    const meetings = ref<MeetingResponse[]>([]);

    const store = useStore();
    const teamId = computed(() => store.getters.selectedTeamId);
    const messages = computed(() => store.state.chatMessages);
    const snackbar = ref<SnackbarState>({
      show: false,
      message: "",
      color: "success",
    });
    const botMessage = ref<ChatbotRequest>({
      meeting_id: "",
      question: "",
    });

    const fetchMeetings = async (
      pageIndex: number = 1,
      pageSize: number = 100
    ): Promise<void> => {
      try {
        meetingsLoading.value = true;
        const response = await MeetingService.getMeetings(
          teamId.value,
          pageIndex,
          pageSize
        );
        meetings.value =
          response.records?.filter(
            (meeting) => meeting.current_step === MeetingSetupProgressEnum.Sealed
          ) || [];
      } catch (error) {
        showError("Failed to fetch meetings");
      } finally {
        meetingsLoading.value = false;
      }
    };

    const showError = (message: string): void => {
      snackbar.value = {
        show: true,
        message,
        color: "error",
      };
    };

    const handleMeetingChange = async (meetingId: number): Promise<void> => {
      if (!meetingId) return;
      store.commit("CLEAR_CHAT_MESSAGES");
      try {
      } catch (error) {
        console.error("Error fetching meeting messages:", error);
      }
    };

    const formatTimestamp = (timestamp: Date): string => {
      return new Intl.DateTimeFormat("en-US", {
        hour: "numeric",
        minute: "numeric",
        second: "numeric",
        hour12: true,
      }).format(timestamp);
    };

    const sendMessage = async (): Promise<void> => {
      if (!newMessage.value.trim()) return;

      const userMessage = newMessage.value.trim();
      store.commit("ADD_CHAT_MESSAGE", {
        type: "user",
        text: userMessage,
        timestamp: new Date(),
      });
      try {
        isLoading.value = true;

        setTimeout(() => {
          if (chatContainer.value) {
            chatContainer.value.scrollTop = chatContainer.value.scrollHeight;
          }
        }, 100);

        if (selectedMeeting.value) {
          botMessage.value.meeting_id = selectedMeeting.value.toString();
        } else {
          botMessage.value.meeting_id = null;
        }
        botMessage.value.question = newMessage.value.trim();
        newMessage.value = "";

        const response = await ChatbotService.submitQuestion(
          teamId.value,
          botMessage.value
        );
        store.commit("ADD_CHAT_MESSAGE", {
          type: "bot",
          text: response.result,
          timestamp: new Date(),
        });
      } catch (error) {
        store.commit("ADD_CHAT_MESSAGE", {
          type: "bot",
          text: "Sorry, there was an error processing your request.",
          timestamp: new Date(),
        });
        console.error("Error sending message to chatbot:", error);
      } finally {
        isLoading.value = false;
        newMessage.value = "";
      }

      setTimeout(() => {
        if (chatContainer.value) {
          chatContainer.value.scrollTop = chatContainer.value.scrollHeight;
        }
      }, 100);
    };

    return {
      messages,
      newMessage,
      sendMessage,
      chatContainer,
      selectedMeeting,
      meetings,
      handleMeetingChange,
      meetingsLoading,
      isLoading,
      formatTimestamp
    };
  },
});
</script>

<style scoped>
.loader {
  justify-content: center;
  margin: 0 auto;
  margin-bottom: 15px;
  width: 100px; /* Increased width to accommodate 5 dots */
  aspect-ratio: 3; /* Adjusted for better spacing */
  --_g: no-repeat
    radial-gradient(circle closest-side, rgb(var(--v-theme-primary)) 90%, #0000);
  background: var(--_g) 0% 50%, var(--_g) 25% 50%, var(--_g) 50% 50%, var(--_g) 75% 50%,
    var(--_g) 100% 50%;
  background-size: calc(100% / 5) 50%; /* Changed from /3 to /5 */
  animation: l3 1s infinite linear;
}
@keyframes l3 {
  20% {
    background-position: 0% 0%, 25% 50%, 50% 50%, 75% 50%, 100% 50%;
  }
  40% {
    background-position: 0% 100%, 25% 0%, 50% 50%, 75% 50%, 100% 50%;
  }
  60% {
    background-position: 0% 50%, 25% 100%, 50% 0%, 75% 50%, 100% 50%;
  }
  80% {
    background-position: 0% 50%, 25% 50%, 50% 100%, 75% 0%, 100% 50%;
  }
  100% {
    background-position: 0% 50%, 25% 50%, 50% 50%, 75% 100%, 100% 0%;
  }
}

.chatbot-card {
  display: flex;
  flex-direction: column;
  height: 100%;
  width: 100%;
  position: relative;
  overflow: hidden;
}

.chat-header {
  flex: 0 0 auto;
  z-index: 1;
  margin-top: 10px;
  background: rgb(var(--v-theme-surface));
  border-bottom: 1px solid rgba(0, 0, 0, 0.12);
}

.chat-container {
  flex: 1 1 auto;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 16px 0;
  position: absolute;
  top: 64px; /* Height of the header */
  bottom: 210px; /* Height of the input container */
  left: 0;
  right: 0;
  min-height: 0;
}

.chat-input-container {
  position: absolute;
  bottom: 30px;
  left: 0;
  right: 0;
  padding: 16px;
  background: rgb(var(--v-theme-surface));
  border-top: 1px solid rgba(0, 0, 0, 0.12);
  z-index: 1;
}

.message {
  display: flex;
  align-items: flex-start;
  margin: 8px 0;
  gap: 8px;
  width: 100%;
  padding: 0 16px;
}

.user-message {
  display: flex;
  flex-direction: row-reverse;
  margin-left: auto;
  width: fit-content;
  max-width: 70%;
}

.message-wrapper {
  display: flex;
  flex-direction: column;
  max-width: 100%;
}

.user-message .message-wrapper {
  align-items: flex-end;
}

.bot-message .message-wrapper {
  align-items: flex-start;
  max-width: 70%;
  flex: 1 !important;
}

.no-flex {
  display: block !important;
}
.message-content {
  padding: 8px 12px;
  border-radius: 8px;
  word-wrap: break-word;
  white-space: pre-wrap; /* This will preserve newlines */
}

.user-message .message-content {
  background-color: rgb(var(--v-theme-primary));
  color: white;
}

.user-icon {
  margin-right: 0;
  padding-bottom: 15px;
  margin-left: 8px;
}
.message-timestamp {
  font-size: 0.75rem;
  color: rgba(var(--v-theme-on-surface), 0.6);
  margin-top: 4px;
  padding: 0 12px;
}
.message-icon.v-responsive {
  display: block !important;
  flex: 0 0 auto !important;
}

.user-message .message-timestamp {
  text-align: right;
  color: rgba(54, 67, 184, 0.7);
}

.model-select {
  max-width: 400px;
  margin-left: auto;
  margin-right: 10px;
}
.meeting-select {
  max-width: 400px;
  margin-left: 16px;
}

.message-icon {
  align-self: center;
  flex-shrink: 0;
  padding-bottom: 15px;
  width: 30px;
}

.bot-message .message-content {
  align-self: flex-start;
  border: 1px solid rgba(0, 0, 0, 0.23);
  background-color: rgba(156, 179, 201, 0.12);
}

.bot-message .message-icon {
  color: rgb(var(--v-theme-primary));
}

.chat-input {
  margin-top: 8px;
  border-radius: 8px;
}

/* Scrollbar styling */
.chat-container::-webkit-scrollbar {
  width: 8px;
}

.chat-container::-webkit-scrollbar-track {
  background: rgba(var(--v-theme-surface));
}

.chat-container::-webkit-scrollbar-thumb {
  background: rgba(var(--v-theme-primary), 0.2);
  border-radius: 4px;
}

.chat-container::-webkit-scrollbar-thumb:hover {
  background: rgba(var(--v-theme-primary), 0.4);
}
</style>
