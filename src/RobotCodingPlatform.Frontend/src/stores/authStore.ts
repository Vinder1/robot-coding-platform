import { auth } from '@/api/auth';
import type { NullableUser } from '@/types/player';
import { defineStore } from 'pinia';
import { ref } from 'vue';
import { profile } from '@/api/profile'

function empty_user() {
  return { id: '', name: '', email: '', role: '', createdAt: '' }
}
function empty_nullable_user() {
  return { user: empty_user(), hasUser: false }
}

export const useAuthStore = defineStore('auth', () => {
  const isLoggedIn = ref(false);
  const showLoginModal = ref(false);
  const showRegisterModal = ref(false);
  const user = ref<NullableUser>(empty_nullable_user());

  async function initialize() {
    isLoggedIn.value = await auth.isSignedInWithRefresh()
    if (isLoggedIn.value) {
      try {
        const profileData = await fetchUserProfile();
        if (profileData) {
          user.value = profileData;
        }
      } catch (error) {
        console.error('Ошибка получения профиля:', error);
      }
    }
  }

  async function fetchUserProfile(): Promise<NullableUser> {
    try {
      const data = await profile.getProfile();
      if (data == null){
        return empty_nullable_user();
      }
      return {
        user: data, hasUser: true,
      };
    } catch (error) {
      console.error('Ошибка получения профиля:', error);
      return empty_nullable_user();
    }
  }

  function reset() {
    isLoggedIn.value = false;
    showLoginModal.value = false;
    showRegisterModal.value = false;
    user.value = empty_nullable_user();
  }

  return {
    isLoggedIn,
    showLoginOverlay: showLoginModal,
    showRegisterOverlay: showRegisterModal,
    user,
    initialize,
    reset
  };
});
