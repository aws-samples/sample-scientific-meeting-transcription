// src/store/index.ts
import { createStore } from 'vuex';


interface TeamSelection {
  id: string | null;
  name: string;
}

export interface RootState {
  user: any | null;
  isAuthenticated: boolean;
  selectedTeam: TeamSelection | null;
  chatMessages: Array<{
    type: 'user' | 'bot';
    text: string;
    timestamp: Date;
  }>;
  selectedModelId: string | null;
  selectedMeetingId: string | null;
}

const store = createStore({
  state: {
    user: null,
    isAuthenticated: false,
    selectedTeam: {
      id: null,
      name: ''
    },
    chatMessages: [],
    selectedModelId: null,
    selectedMeetingId: null
  } as RootState ,

  mutations: {
    SET_USER(state: RootState, user: any) {
      state.user = user;
      state.isAuthenticated = !!user;
    },
    SET_SELECTED_MODEL(state: RootState, modelId: string | null) {
      state.selectedModelId = modelId;
      if (modelId) {
        localStorage.setItem('selectedModelId', modelId);
      } else {
        localStorage.removeItem('selectedModelId');
      }
    },
    SET_SELECTED_MEETING(state: RootState, meetingId: string | null) {
      state.selectedMeetingId = meetingId;
      if (meetingId) {
        localStorage.setItem('selectedMeetingId', meetingId);
      } else {
        localStorage.removeItem('selectedMeetingId');
      }
    },
    SET_SELECTED_TEAM(state, payload: TeamSelection) {
      state.selectedTeam = payload
      if (payload.id) {
        localStorage.setItem('selectedTeam', JSON.stringify(payload))
      } else {
        localStorage.removeItem('selectedTeam')
      }
    },
    ADD_CHAT_MESSAGE(state, message: { type: 'user' | 'bot'; text: string; timestamp: Date }) {
      state.chatMessages.push(message)
      localStorage.setItem('chatMessages', JSON.stringify(state.chatMessages))
    },
    CLEAR_CHAT_MESSAGES(state) {
      state.chatMessages = []
      localStorage.removeItem('chatMessages')
    },
    RESTORE_CHAT_MESSAGES(state, messages: Array<{ type: 'user' | 'bot'; text: string; timestamp: Date }>) {
      state.chatMessages = messages
    }
  },

  getters: {
    isAuthenticated: (state: RootState) => state.isAuthenticated,
    currentUser: (state: RootState) => state.user,
    selectedTeam: state => state.selectedTeam,
    selectedTeamId: state => state.selectedTeam.id,
    selectedTeamName: state => state.selectedTeam.name
  },

  actions: {
    resetAndInitializeStore({ commit }): void {
      // Clear all state
      commit('SET_USER', null);
      commit('SET_SELECTED_TEAM', { id: null, name: '' });
      commit('SET_SELECTED_MODEL', null);
      commit('SET_SELECTED_MEETING', null);
      commit('CLEAR_CHAT_MESSAGES');
      
      // Reinitialize store with saved data if any
      const savedTeam = localStorage.getItem('selectedTeam')
      if (savedTeam) {
        try {
          const teamData = JSON.parse(savedTeam)
          commit('SET_SELECTED_TEAM', teamData);
        } catch (e) {
          console.error('Failed to parse saved team data:', e);
          localStorage.removeItem('selectedTeam');
        }
      }
    },

    initializeStore({ commit }) {
      const savedTeam = localStorage.getItem('selectedTeam')
      if (savedTeam) {
        try {
          const teamData = JSON.parse(savedTeam)
          commit('SET_SELECTED_TEAM', teamData)
        } catch (e) {
          console.error('Error parsing saved team data:', e)
          localStorage.removeItem('selectedTeam')
        }
      }

      const savedModelId = localStorage.getItem('selectedModelId')
      if (savedModelId) {
        commit('SET_SELECTED_MODEL', savedModelId)
      }

      const savedMeetingId = localStorage.getItem('selectedMeetingId')
      if (savedMeetingId) {
        commit('SET_SELECTED_MEETING', savedMeetingId)
      }
      
      const savedMessages = localStorage.getItem('chatMessages')
      if (savedMessages) {
        try {
          const messagesData = JSON.parse(savedMessages)
          // Convert string timestamps back to Date objects
          const messages = messagesData.map(msg => ({
            ...msg,
            timestamp: new Date(msg.timestamp)
          }))
          commit('RESTORE_CHAT_MESSAGES', messages)
        } catch (e) {
          console.error('Error parsing saved chat messages:', e)
          localStorage.removeItem('chatMessages')
        }
      }
    },
    login({ commit }, user: any): void {
      commit('SET_USER', user);
      localStorage.removeItem('selectedTeam');
      commit('SET_SELECTED_TEAM', { id: null, name: null });
    },
    logout({ commit }): void {
      commit('SET_USER', null);
      commit('SET_SELECTED_TEAM', { id: null, name: null });
      localStorage.removeItem('selectedTeam');
    },
    setSelectedTeam({ commit }, payload: TeamSelection): void {
      commit('SET_SELECTED_TEAM', payload)
    },
  },

  strict: process.env.NODE_ENV !== 'production'
});

store.dispatch('initializeStore')

export default store;
