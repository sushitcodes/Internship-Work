import { useState, type ReactNode } from "react";
import * as authServices from "../services/authServices";
import type { LoginRequest, RegisterRequest } from "../types/auth";
import { AuthContext } from "./AuthContext";

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isLoggedIn, setIsLoggedIn] = useState(() =>
    authServices.isAuthenticated(),
  );

  const login = async (data: LoginRequest) => {
    await authServices.login(data);
    setIsLoggedIn(true);
  };

  const register = async (data: RegisterRequest) => {
    await authServices.register(data);
    setIsLoggedIn(true);
  };

  const logout = () => {
    authServices.logout();
    setIsLoggedIn(false);
  };

  return (
    <AuthContext.Provider value={{ isLoggedIn, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}
