<!--
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 -->

<template>
  <v-card outlined elevation="5">
    <v-card-title class="d-flex align-center mt-2">
      <v-icon class="mr-2">mdi-account-group</v-icon>
      Teams
      <v-spacer></v-spacer>
      <v-menu>
        <template v-slot:activator="{ props }">
          <v-btn
            color="primary"
            v-bind="props"
            :disabled="!selectedTableTeamId"
            class="ml-2"
          >
            Actions
            <v-icon right>mdi-chevron-down</v-icon>
          </v-btn>
        </template>
        <v-list>
          <v-list-item
            v-for="(action, index) in actionItems"
            :key="index"
            @click="executeAction(action.value)"
          >
            <v-list-item-title>
              <v-icon :icon="action.icon" class="mr-2" small></v-icon>
              {{ action.title }}
            </v-list-item-title>
          </v-list-item>
        </v-list>
      </v-menu>
      <v-btn color="primary" class="ml-2" @click="openDialog()">
        Add Team
        <v-icon right>mdi-plus</v-icon>
      </v-btn>
      <v-btn color="secondary" class="ml-2" @click="fetchTeams()">
        <v-icon right>mdi-refresh</v-icon>
      </v-btn>
    </v-card-title>

    <v-data-table
      :headers="headers"
      :items="teams"
      :search="search"
      :loading="loading"
      loading-text="Loading teams..."
      :items-per-page="pageSize"
      :page="currentPage"
      @update:options="handlePageChange"
    >
      <!-- Add item class to style the selected row -->
      <template v-slot:item="{ item, props }">
        <tr v-bind="props" :class="{ 'selected-row': selectedTeamId === item.id }">
          <!-- Render each cell -->
          <td>
            <v-radio-group v-model="selectedTableTeamId" hide-details class="ma-0 pa-0">
              <v-radio :value="item.id" hide-details class="ma-0 pa-0" />
            </v-radio-group>
          </td>
          <td>
            {{ item.team }}
            <v-chip v-if="selectedTeamId === item.id" color="primary" class="ml-2">
              Selected
            </v-chip>
          </td>
          <td>{{ item.idp_group }}</td>
          <td>
            <v-chip
              :color="item.status === 'Active' ? 'success' : 'error'"
              text-color="white"
              small
            >
              {{ item.status }}
            </v-chip>
          </td>
          <td>{{ dateFormatter.format(item.created_at) }}</td>
          <td>{{ dateFormatter.format(item.updated_at) }}</td>
        </tr>
      </template>
    </v-data-table>
  </v-card>

  <v-dialog v-model="dialog" overlay-color="black" overlay-opacity="5" max-width="50%">
    <v-card>
      <v-card-title class="pt-6 pb-2 pr-6">
        <v-icon class="mr-2">mdi-account-group</v-icon>
        <span class="text-h5">{{ formTitle }}</span>
      </v-card-title>

      <v-card-text class="pb-0">
        <v-form ref="formRef" v-model="formValid">
          <v-container class="py-0">
            <v-row dense>
              <v-col cols="12" class="py-1">
                <v-text-field
                  v-model="editedItem.team"
                  label="Team Name"
                  required
                  :rules="[(v) => !!v || 'Team Name is required']"
                  density="comfortable"
                  variant="outlined"
                  class="custom-field"
                ></v-text-field>
              </v-col>
              <v-col cols="12" class="py-1" md="6">
                <v-text-field
                  v-model="editedItem.idp_group"
                  label="IDP Group"
                  density="comfortable"
                  variant="outlined"
                  class="custom-field"
                ></v-text-field>
              </v-col>
              <v-col cols="12" class="py-1" md="6">
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
          @click="saveTeam"
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

  <v-snackbar v-model="snackbar.show" :color="snackbar.color" timeout="3000">
    {{ snackbar.message }}
  </v-snackbar>
</template>

<script lang="ts">
import { ref, computed, watch } from "vue";
import { dateFormatter } from "@utils/dateFormatter";
import { TeamService } from "@services/team.service";
import type { TeamRequest, TeamResponse } from "@/src/types/team";
import { StatusEnum } from "@/src/types/common";
import Dashboard from "./Dashboard.vue";
import { useStore } from "vuex";

export default {
  components: {
    Dashboard,
    name: "Teams",
  },
  setup() {
    const formValid = ref(false);
    const store = useStore();
    const teams = ref<TeamResponse[]>([]);
    const loading = ref(false);
    const search = ref("");
    const dialog = ref(false);
    const currentPage = ref(1);
    const pageSize = ref(10);
    const editedIndex = ref(-1);
    const snackbar = ref({
      show: false,
      message: "",
      color: "success",
    });
    const selectedTeamId = computed(() => store.getters.selectedTeamId);
    const selectedTeam = computed(() => store.getters.selectedTeam);
    const selectedTableTeamId = ref<string>();

    const actionItems = [
      { title: "Select Team", value: "select", icon: "mdi-check-circle" },
      { title: "Edit Team", value: "edit", icon: "mdi-pencil" },
      { title: "Delete Team", value: "delete", icon: "mdi-delete" },
    ];

    const handleActionSelect = (action: string, item: TeamResponse) => {
      if (!action) return;

      switch (action) {
        case "select":
          selectTeam(item);
          break;
        case "edit":
          openDialog(item);
          break;
        case "delete":
          deleteTeam(item);
          break;
      }
    };

    const headers = [
      { title: "", key: "select", sortable: false, width: "50px" },
      { title: "Team Name", key: "team" },
      { title: "IDP Group", key: "idp_group" },
      { title: "Selected", key: "selected" },
      { title: "Status", key: "status" },
      { title: "Created At", key: "created_at" },
      { title: "Updated At", key: "updated_at" },
      // Actions are now handled by the top dropdown
    ];

    const selectTeam = (team: TeamResponse) => {
      if (selectedTeamId.value === team.id) {
        store.dispatch("setSelectedTeam", { id: null, name: null });
        showSuccess("Team selection cleared");
        return;
      } else {
        const newTeamId = selectedTeamId.value === team.id ? null : team.id;
        const newTeam = selectedTeam.value === team.team ? null : team.team;
        store.dispatch("setSelectedTeam", { id: newTeamId, name: newTeam });
        showSuccess(newTeamId ? `Selected team: ${newTeam}` : "Team selection cleared");
      }
    };

    const statusOptions = Object.values(StatusEnum);

    const editedItem = ref<TeamRequest>({
      team: "",
      idp_group: "",
      status: StatusEnum.Active,
    });

    const defaultItem = {
      team: "",
      idp_group: "",
      status: StatusEnum.Active,
    };

    const formTitle = computed(() => {
      return editedIndex.value === -1 ? "New Team" : "Edit Team";
    });

    const refreshData = async () => {
      try {
        loading.value = true;
        await fetchTeams(1, pageSize.value); // Reset to first page when refreshing
        showSuccess("Data refreshed successfully");
      } catch (error) {
        showError("Failed to refresh data");
      } finally {
        loading.value = false;
      }
    };

    const fetchTeams = async (
      pageIndex = currentPage.value,
      itemsPerPage = pageSize.value
    ) => {
      try {
        loading.value = true;
        const response = await TeamService.getTeams(pageIndex, itemsPerPage);
        teams.value = response.records || [];
        // Update pagination values
        currentPage.value = pageIndex;
        pageSize.value = itemsPerPage;
      } catch (error) {
        throw error; // Let the calling function handle the error
      } finally {
        loading.value = false;
      }
    };

    const handlePageChange = (options: any) => {
      const newPage = options.page || 1;
      const newPageSize = options.itemsPerPage || 10;
      if (newPage !== currentPage.value || newPageSize !== pageSize.value) {
        fetchTeams(newPage, newPageSize);
      }
    };

    const openDialog = (item?: TeamResponse) => {
      if (item) {
        editedIndex.value = teams.value.indexOf(item);
        editedItem.value = { ...item };
      } else {
        editedIndex.value = -1;
        editedItem.value = { ...defaultItem };
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
      setTimeout(() => {
        editedItem.value = { ...defaultItem };
        editedIndex.value = -1;
      }, 300);
    };

    const saving = ref(false);

    const saveTeam = async () => {
      try {
        saving.value = true;
        if (editedIndex.value > -1) {
          // Update existing team
          const team = teams.value[editedIndex.value];
          await TeamService.updateTeam(team.id!, editedItem.value);
          showSuccess("Team updated successfully");
        } else {
          // Create new team
          await TeamService.createTeam(editedItem.value);
          showSuccess("Team created successfully");
        }
        await fetchTeams();
        closeDialog();
      } catch (error) {
        showError("Failed to save team");
      } finally {
        saving.value = false;
      }
    };

    const deleteTeam = async (item: TeamResponse) => {
      if (!confirm("Are you sure you want to delete this team?")) return;

      try {
        await TeamService.deleteTeam(item.id!);
        await fetchTeams();
        showSuccess("Team deleted successfully");
      } catch (error) {
        showError("Failed to delete team");
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

    // Initial fetch
    fetchTeams();

    const executeAction = (action: string) => {
      if (!action || !selectedTableTeamId.value) {
        showError("Please select a team first");
        return;
      }

      // Find the selected team
      const selectedTeamItem = teams.value.find(
        (team) => team.id === selectedTableTeamId.value
      );

      if (!selectedTeamItem) {
        showError("Selected team not found");
        return;
      }

      // Execute the action
      switch (action) {
        case "select":
          selectTeam(selectedTeamItem);
          break;
        case "edit":
          openDialog(selectedTeamItem);
          break;
        case "delete":
          deleteTeam(selectedTeamItem);
          break;
        default:
          showError("Invalid action selected");
          break;
      }
    };

    return {
      teams,
      loading,
      refreshData,
      currentPage,
      pageSize,
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
      saveTeam,
      deleteTeam,
      selectTeam,
      selectedTeamId,
      selectedTeam,
      fetchTeams,
      saving,
      formValid,
      dateFormatter,
      selectedTableTeamId,
      actionItems,
      executeAction,
    };
  },
};
</script>

<style scoped>
:deep(.selected-team) {
  background-color: #4caf50 !important;
  color: white !important;
}

:deep(.v-btn.selected-team:hover) {
  background-color: #388e3c !important;
}

.action-select {
  max-width: 150px;
}

:deep(.v-radio-group) {
  display: flex;
  justify-content: center;
}
</style>
