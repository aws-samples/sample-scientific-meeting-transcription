<!--
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 -->

<template>
  <v-app>
    <v-app-bar elevation="1">
      <v-img
        src="/exscribo.png"
        alt="Exscribo Logo"
        max-height="30"
        max-width="30"
        contain
        class="mr-3 ml-4"
      />
      <div class="d-flex align-center">
        <v-toolbar-title>Exscribo</v-toolbar-title>
        <v-divider v-if="selectedTeam.name" vertical class="mx-3"></v-divider>
        <span v-if="selectedTeam.name" class="text-subtitle-1">
          {{ selectedTeam.name }}
        </span>
      </div>
      <v-spacer></v-spacer>
    </v-app-bar>

    <v-navigation-drawer
      class="mt-1"
      expand-on-hover
      rail
      v-bind:width="325"
      @update:rail="rail = $event"
    >
      <v-list-item
        style="background-color: transparent !important"
        :title="currentUser?.username || 'User'"
        prepend-icon="mdi-account-circle"
      >
      </v-list-item>

      <v-divider></v-divider>
      <!-- Regular menu items -->
      <template v-for="item in visibleMenuItems" :key="item.title">
        <!-- Regular menu item without submenu -->
        <v-list-item
          v-if="!item.submenu"
          :to="item.path"
          :active="isCurrentRoute(item.path)"
          hover
          :prepend-icon="item.icon"
          :title="item.title"
          :class="{
            'active-text': isCurrentRoute(item.path),
          }"
        >
        </v-list-item>

        <!-- Menu item with submenu -->
        <div v-else class="mb-2">
          <v-list-item
            @click="toggleSubmenu(item.title)"
            hover
            :prepend-icon="item.icon"
            :title="item.title"
            :class="{
              'settings-active':
                item.title === 'Settings' && expandedSubmenus[item.title],
            }"
          >
            <template v-slot:append>
              <v-icon v-if="drawer" size="small">
                {{ expandedSubmenus[item.title] ? "mdi-chevron-up" : "mdi-chevron-down" }}
              </v-icon>
            </template>
          </v-list-item>

          <!-- Submenu items -->
          <div
            v-if="expandedSubmenus[item.title]"
            :class="{ 'submenu-expanded': !isRailActive }"
          >
            <v-list-item
              v-for="subItem in item.submenu"
              :key="subItem.title"
              :to="subItem.path"
              :active="isCurrentRoute(subItem.path)"
              hover
              link
              :prepend-icon="subItem.icon"
              :title="subItem.title"
              :class="{
                'active-text': isCurrentRoute(subItem.path),
              }"
            >
            </v-list-item>
          </div>
        </div>
      </template>

      <!-- Divider -->
      <v-divider style="padding: 0px"></v-divider>

      <!-- Logout item -->
      <v-list-item @click="handleLogout" hover prepend-icon="mdi-logout" title="Logout">
      </v-list-item>
    </v-navigation-drawer>
    <v-main>
      <router-view></router-view>
      <div class="version-container">
        <VersionDisplay />
      </div>
    </v-main>
  </v-app>
</template>

<script setup lang="ts">
import { useRouter, useRoute } from "vue-router";
import { ref, computed, watch, onMounted } from "vue";
import { useStore } from "vuex";
import { signOut, getCurrentUser } from "aws-amplify/auth";
import type { AuthUser } from "aws-amplify/auth";
import type { RootState } from "../store";
import VersionDisplay from "./VersionDisplay.vue";

// Component name (for debugging)
defineOptions({
  name: "Dashboard",
});
const isRailActive = computed(() => rail.value);
const store = useStore<RootState>();
const router = useRouter();
const route = useRoute();
const drawer = ref<boolean>(true);
const selectedTeamVersion = ref<number>(0);
const expandedSubmenus = ref<Record<string, boolean>>({});
const currentUser = ref<AuthUser | null>(null);
const rail = ref(false);
interface MenuItem {
  title: string;
  icon: string;
  path?: string;
  show: "always" | "team";
  submenu?: MenuItem[];
}

const toggleSubmenu = (title: string): void => {
  expandedSubmenus.value[title] = !expandedSubmenus.value[title];
};

const refreshSelectedTeam = async (): Promise<void> => {
  try {
    // First, fetch fresh data from the backend
    await store.dispatch("fetchSelectedTeam");
    // Then force recomputation by incrementing version
    selectedTeamVersion.value++;
  } catch (error) {
    console.error("Error refreshing selected team:", error);
  }
};

onMounted(async () => {
  try {
    currentUser.value = await getCurrentUser();
  } catch (error) {
    console.error("Error getting current user:", error);
  }
});

const isAuthenticated = computed(() => store.getters.isAuthenticated);
const selectedTeam = computed(() => store.getters.selectedTeam);

const menuItems: MenuItem[] = [
  { title: "Teams", icon: "mdi-account-group", path: "/teams", show: "always" },
  { title: "Meetings", icon: "mdi-calendar", path: "/meetings", show: "team" },
  {
    title: "Meeting Assistant",
    icon: "mdi-assistant",
    path: "/meeting-assistant",
    show: "team",
  },
  {
    title: "Settings",
    icon: "mdi-cog",
    show: "team",
    submenu: [
      {
        title: "Prompt Sets",
        icon: "mdi-text-box-multiple",
        path: "/prompt-sets",
        show: "team",
      },
      {
        title: "Custom Models",
        icon: "mdi-brain",
        path: "/custom-models",
        show: "team",
      },
      {
        title: "Custom Vocabularies",
        icon: "mdi-format-pilcrow",
        path: "/custom-vocabularies",
        show: "team",
      },
    ],
  },
];

const visibleMenuItems = computed(() => {
  return menuItems.filter((item) => {
    if (item.show === "always") return true;
    if (item.show === "team" && selectedTeam.value?.id) return true;
    return false;
  });
});

const isCurrentRoute = (path: string): boolean => {
  return route.path === path;
};

const handleLogout = async (): Promise<void> => {
  try {
    await signOut({ global: true });
    localStorage.removeItem("selectedTeam");
    store.dispatch("resetAndInitializeStore");
    router.push("/login");
  } catch (error) {
    console.error("Error during logout:", error);
  }
};
</script>

<style scoped>
.submenu-expanded {
  margin-left: 24px;
  border-left: 4px solid rgb(180, 178, 178);
}
/* Adjust for mobile screens */
@media (max-width: 960px) {
  .content-wrapper {
    height: calc(100vh - 70px);
    /* Subtract the mobile app bar height */
  }
}

.v-list-item {
  transition: all 0.2s ease-in-out;
  margin-bottom: 4px;
}

.v-list-item:hover {
  background-color: rgb(var(--v-theme-primary), 0.12) !important;
}

/* Active state styling */
.v-list-item--active {
  background-color: rgb(var(--v-theme-primary), 0.16) !important;
}

/* Active text and icon styling */
.active-text {
  color: rgb(var(--v-theme-primary)) !important;
  font-weight: 600;
}

.active-icon {
  color: rgb(var(--v-theme-primary)) !important;
}

/* Avatar styling */
.v-avatar {
  transition: transform 0.2s ease;
}

.v-list-item:hover .v-avatar {
  transform: scale(1.1);
}

/* Menu item styling */
.v-menu .v-list-item:hover {
  background-color: rgb(var(--v-theme-primary), 0.08) !important;
  transform: none;
}

/* Responsive adjustment for mobile */

.list-item-custom {
  border-radius: 0px !important;
  margin-bottom: 4px !important;
}

/* Active item styling simplified */

/* Optional: Add transition for the icon margin */
.v-icon {
  transition: margin 0.3s ease, color 0.2s ease;
}

.v-app-bar {
  border-bottom: 1px solid rgba(var(--v-border-color), 0.12);
}

.v-toolbar-title {
  font-weight: 500;
  letter-spacing: 0.5px;
}

/* Navigation drawer positioning has been reset to Vuetify defaults */

/* Optional: Add transition for the drawer */

/* Optional: Enhance the logout button */
.v-btn {
  text-transform: none;
  letter-spacing: 0.5px;
}

.v-btn:hover .v-icon {
  transform: translateX(2px);
}

.v-icon {
  transition: transform 0.2s ease;
}

/* Optional: Add subtle shadow to the app bar */
.v-app-bar.elevation-1 {
  box-shadow: 0 2px 4px rgba(8, 8, 8, 0.39) !important;
}

/* Optional: Style the team name */
.text-subtitle-1 {
  font-weight: 400;
  opacity: 0.9;
}

/* Optional: Style the vertical divider */
.v-divider {
  opacity: 0.3;
  height: 24px;
}

.version-container {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  padding: 6px 16px;
  background-color: #ffffff;
  border-top: 1px solid #e0e0e0;
  display: flex;
  justify-content: flex-end;
  z-index: 1;
}
</style>
