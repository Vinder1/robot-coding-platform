import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios";
import { auth } from "./auth";

const url = '/api';

export const api = axios.create({
  headers: {
    'Content-Type': 'application/json',
  },
  baseURL: url,
});

api.interceptors.request.use((config) => {
  const token = auth.getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

interface CustomAxiosRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as CustomAxiosRequestConfig;
    const isRefreshRequest = originalRequest?.url?.includes('/auth/refresh') ?? false;

    if (
      error.response?.status === 401 &&
      originalRequest &&
      !originalRequest._retry &&
      !isRefreshRequest
    ) {
      originalRequest._retry = true;

      try {
        const newToken = await auth.refresh();
        originalRequest.headers.Authorization = `Bearer ${newToken}`;
        return api(originalRequest);
      } catch (refreshError) {
        auth.clear();
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);
