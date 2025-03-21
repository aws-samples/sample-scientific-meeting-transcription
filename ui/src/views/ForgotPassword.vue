<!--
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 -->

<template>
  <v-container fluid class="fill-height">
    <v-row align="center" justify="center">
      <v-col cols="12" sm="8" md="6" lg="4">
        <v-card class="elevation-12">
          <v-toolbar color="primary" dark flat>
            <v-toolbar-title>Forgot Password</v-toolbar-title>
          </v-toolbar>
          <v-card-text>
            <v-form
              ref="formRef"
              v-model="formValid"
              @submit.prevent="handleForgotPassword"
            >
              <v-text-field
                v-model="username"
                label="Username"
                name="username"
                prepend-icon="mdi-account"
                type="text"
                required
                :rules="[(v) => !!v || 'Username is required']"
                density="comfortable"
                variant="outlined"
                class="custom-field"
              />
              <v-alert v-if="error" type="error" class="mt-3">
                {{ error }}
              </v-alert>
              <v-alert v-if="success" type="success" class="mt-3">
                {{ success }}
              </v-alert>
            </v-form>
          </v-card-text>
          <v-card-actions>
            <v-btn variant="text" color="primary" @click="() => router.push('/login')">
              Back to Login
            </v-btn>
            <v-spacer />
            <v-btn color="primary" @click="handleForgotPassword" :loading="loading">
              Reset Password
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script>
import { ref } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";

export default {
  name: "ForgotPassword",
  setup() {
    const router = useRouter();
    const username = ref("");
    const error = ref("");
    const success = ref("");
    const loading = ref(false);

    const handleForgotPassword = async () => {
      try {
        loading.value = true;
        error.value = "";
        success.value = "";

        await axios.post("/auth/forgot-password", {
          username: username.value,
        });

        success.value = "Password reset instructions have been sent to your email.";
      } catch (err) {
        error.value =
          err.response?.data?.detail || "Failed to process request. Please try again.";
      } finally {
        loading.value = false;
      }
    };

    return {
      router,
      username,
      error,
      success,
      loading,
      handleForgotPassword,
    };
  },
};
</script>
