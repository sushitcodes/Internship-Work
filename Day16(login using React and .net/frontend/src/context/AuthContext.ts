// app wide auth state, built on top of auth Service
import { createContext } from "react";
import type { LoginRequest, RegisterRequest } from "../types/auth";

export type AuthContextType = {
  isLoggedIn: boolean;
  register: (data: RegisterRequest) => Promise<void>;
  login: (data: LoginRequest) => Promise<void>;
  logout: () => void;
};

export const AuthContext = createContext<AuthContextType | undefined>(
  undefined,
);
