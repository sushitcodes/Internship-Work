// Business Logic: register() ,Login() ,logout() ,getCurrentUser()
import axiosInstance from "../api/axiosInstance";
import type {
  RegisterRequest,
  LoginRequest,
  AuthResponse,
} from "../types/auth";
const TOKEN_KEY = "auth_token";
export async function register(data: RegisterRequest): Promise<AuthResponse> {
  const response = await axiosInstance.post<AuthResponse>(
    "/auth/register",
    data,
  );
  storeToken(response.data.token);
  return response.data;
}
export async function login(data: LoginRequest): Promise<AuthResponse> {
  const response = await axiosInstance.post<AuthResponse>("/auth/login", data);
  storeToken(response.data.token);
  return response.data;
}
export function logout(): void {
  localStorage.removeItem(TOKEN_KEY);
}
export function storeToken(token: string): void {
  localStorage.setItem(TOKEN_KEY, token);
}

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function isAuthenticated(): boolean {
  return getToken() !== null;
}
