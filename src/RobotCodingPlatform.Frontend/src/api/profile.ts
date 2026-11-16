import type { User } from "@/types/player";
import { api } from "./api";

export const profile = {
  async getProfile(): Promise<User | null> {
    try {
      const response = await api.get('/me');
      if (response.data) {
        return response.data;
      }
    } catch (error) {
      console.error('Ошибка получения профиля:', error);
    }
    return null;
  }
}
