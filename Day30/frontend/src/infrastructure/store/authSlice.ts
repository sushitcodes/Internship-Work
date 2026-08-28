import { createSlice, PayloadAction } from "@reduxjs/toolkit";

interface AuthState {
  email: string | null;
  role: string | null;
}

const initialState: AuthState = {
  email: null,
  role: null,
  // DELETE — no longer read from localStorage; see /me flow below
};

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    setCredentials: (
      state,
      action: PayloadAction<{ email: string; role: string }>,
    ) => {
      state.email = action.payload.email;
      state.role = action.payload.role;
      // DELETE — no more localStorage.setItem("token", ...); nothing to store
    },
    logout: (state) => {
      state.email = null;
      state.role = null;
    },
  },
});

export const { setCredentials, logout } = authSlice.actions;
export default authSlice.reducer;
