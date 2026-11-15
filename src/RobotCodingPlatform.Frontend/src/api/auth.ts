import { api } from "./api";

export const auth = {
  isSignedInLight() {
    return this.getToken() != null;
  },

  async isSignedInWithRefresh() {
    if (this.getToken() != null)
      return true;
    await this.refresh();
    return this.getToken() != null
  },

  getToken() {
    const token = localStorage.getItem("token");
    return token;
  },

  async refresh() {
    const response = await api.post('/auth/refresh/');
    this.setToken(response.data.token);
    return response.data.token;
  },

  setToken(value: string) {
    localStorage.setItem("token", value);
  },

  clear() {
    localStorage.removeItem("token");
  }
}
