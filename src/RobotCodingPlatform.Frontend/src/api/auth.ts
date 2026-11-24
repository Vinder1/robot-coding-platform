import { api } from "./api";

export const auth = {
  isSignedInLight() {
    return this.getToken() != null;
  },

  async isSignedInWithRefresh() {
    if (this.getToken() != null)
      return true;
    try {
      await this.refresh();
    } catch {
      return false;
    }
    return this.getToken() != null
  },

  getToken() {
    const token = localStorage.getItem("token");
    return token;
  },

  async refresh() {
    const response = await api.post('/auth/refresh/');
    const token = response.data?.token;
    if (token) this.setToken(token);
    return token;
  },

  setToken(value: string) {
    localStorage.setItem("token", value);
  },

  clear() {
    localStorage.removeItem("token");
  }
}
