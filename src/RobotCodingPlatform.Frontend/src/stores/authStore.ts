import { auth } from '@/api/auth';
import type { User } from '@/types/player';
import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useAuthStore = defineStore('auth', () => {
  const isLoggedIn = ref(false);
  const showLoginModal = ref(false);
  const showRegisterModal = ref(false);
  const player = ref<User>({
    id: "unknown",
    name: "unknown",
    email: "unknown",
    role: "unknown",
  });

  function initialize() {
      isLoggedIn.value = auth.isSignedInLight();
  }

  function reset() {
    isLoggedIn.value = false;
    showLoginModal.value = false;
    showRegisterModal.value = false;
    player.value = {
        id: "unknown",
        name: "unknown",
        email: "unknown",
        role: "unknown",
    };
  }

  return {
    isLoggedIn,
    showLoginOverlay: showLoginModal,
    showRegisterOverlay: showRegisterModal,
    user: player,
    initialize,
    reset
  };
});
