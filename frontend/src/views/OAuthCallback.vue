<template>
  <div class="oauth-callback">
    <p>Processing authentication, please wait...</p>
  </div>
</template>

<script>
import { defineComponent, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { signInWithRedirect } from 'aws-amplify/auth';

export default defineComponent({
  name: 'OAuthCallback',
  setup() {
    const router = useRouter();

    onMounted(async () => {
      try {
        // This will handle the OAuth callback automatically
        const { isSignedIn } = await signInWithRedirect();
        if (isSignedIn) {
          router.push('/');
        } else {
          router.push('/login');
        }
      } catch (error) {
        console.error('OAuth callback error:', error);
        router.push('/login');
      }
    });
  }
});
</script>