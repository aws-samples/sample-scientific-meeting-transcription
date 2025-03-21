<!--
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 -->

<template>
  <v-container fluid class="fill-height bg-grey-lighten-4">
    <v-row align="center" justify="center">
      <v-col cols="12" sm="8" md="6" lg="4">
        <v-card class="elevation-12 login-card">
          <!-- Logo and text header -->
          <div class="text-center pa-6 d-flex align-center justify-center">
            <v-img
              src="/exscribo.png"
              alt="Exscribo Logo"
              max-height="40"
              max-width="40"
              contain
              class="mr-3"
            />
            <span class="text-h4 font-weight-medium text-primary">Exscribo</span>
          </div>
          <v-divider></v-divider>

          <v-card-text class="pt-6">
            <v-form @submit.prevent="handleLogin">
              <v-text-field
                v-model="username"
                label="Username"
                name="username"
                prepend-icon="mdi-account"
                type="text"
                required
                variant="outlined"
                class="rounded-lg mb-4 custom-field"
                density="comfortable"
              />
              <v-text-field
                v-model="password"
                label="Password"
                name="password"
                prepend-icon="mdi-lock"
                type="password"
                required
                variant="outlined"
                class="rounded-lg custom-field"
                density="comfortable"
              />
              <v-alert
                v-if="error"
                type="error"
                class="mt-3"
                variant="tonal"
                density="comfortable"
              >
                {{ error }}
              </v-alert>
            </v-form>
          </v-card-text>

          <v-card-actions>
            <v-card-text class="text-center pb-6" style="width: 100%">
              <v-btn
                block
                variant="elevated"
                color="primary"
                @click="handleLogin"
                :loading="loading"
                class="rounded-lg"
                size="large"
                min-height="44"
              >
                Login
              </v-btn>
            </v-card-text>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import { AuthService } from "@services/auth.service";
import type { CognitoLoginRequest } from "@/src/types/auth.ts";
import { useStore } from "vuex";

export default {
  name: "Login",
  setup() {
    const store = useStore();
    const router = useRouter();
    const username = ref("");
    const password = ref("");
    const error = ref("");
    const loading = ref(false);

    setTimeout(() => {
      const titleInput = document.querySelector('input[type="text"]');
      if (titleInput) {
        (titleInput.previousElementSibling as HTMLElement)?.focus();
      }
    }, 100);

    const handleLogin = async () => {
      try {
        store.dispatch("resetAndInitializeStore");

        loading.value = true;
        error.value = "";
        const credentials: CognitoLoginRequest = {
          username: username.value,
          password: password.value,
        };

        if (await AuthService.login(credentials)) {
          localStorage.removeItem("selectedTeam");
          router.push("/teams");
        } else {
          error.value = "Login failed. Please try again.";
          loading.value = false;
        }
      } catch (err) {
        if (err instanceof Error) {
          if (err.name === "UserNotConfirmedException") {
            error.value = "Please confirm your account first.";
          } else if (err.name === "NotAuthorizedException") {
            error.value = "Incorrect username or password.";
          } else if (err.name === "UserNotFoundException") {
            error.value = "User does not exist.";
          } else {
            error.value = err.message || "Login failed. Please try again.";
          }
        }
      } finally {
        loading.value = false;
      }
    };

    const handleForgotPassword = () => {
      router.push("/forgot-password");
    };

    return {
      username,
      password,
      error,
      loading,
      handleLogin,
      handleForgotPassword,
    };
  },
};
</script>

<style scoped>
.bg-grey-lighten-4 {
  background-color: rgb(246, 246, 246) !important;
  min-height: 100vh;
  display: flex;
  align-items: center;
}

.login-card {
  border-radius: 16px;
  max-width: 100%;
  margin: auto;
}

.v-img {
  transition: transform 0.3s ease;
}

.text-h4 {
  letter-spacing: -0.5px;
  transition: color 0.3s ease;
}

/* Custom field styling */
.custom-field {
  border-radius: 8px;
}

:deep(.custom-field .v-field__outline) {
  --v-field-border-width: 1px;
}

:deep(.custom-field .v-field--focused .v-field__outline) {
  border-width: 2px;
}

:deep(.custom-field .v-label) {
  font-size: 0.875rem;
}

:deep(.custom-field .v-field--focused .v-label),
:deep(.custom-field .v-field--active .v-label) {
  color: rgb(var(--v-theme-primary));
}

/* Form field animations */
.v-text-field {
  transition: transform 0.2s ease;
}

.v-text-field:focus-within {
  transform: translateY(-2px);
}

/* Button styling */
.v-btn {
  transition: all 0.2s ease;
  letter-spacing: 0.5px;
  font-weight: 500;
}

.v-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
}

/* Optional: Add a subtle hover effect to the header */
.text-center:hover .v-img {
  transform: scale(1.05);
}

/* Responsive adjustments */
@media (max-width: 600px) {
  .login-card {
    margin: 16px;
  }

  .text-h4 {
    font-size: 1.5rem !important;
  }
}
</style>
