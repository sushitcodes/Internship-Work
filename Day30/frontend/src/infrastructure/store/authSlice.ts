import { createSlice, PayloadAction } from "@reduxjs/toolkit";

interface AuthState {
  email: string | null;
  roles: string[];
  /** ISO string of when the current access token expires. Null when not logged in. */
  expiresAt: string | null;
}

const initialState: AuthState = {
  email: null,
  roles: [],
  expiresAt: null,
  // DELETE — no longer read from localStorage; see /me flow below
};

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    setCredentials: (
      state,
      action: PayloadAction<{ email: string; roles: string[]; expiresAt?: string }>,
    ) => {
      state.email = action.payload.email;
      state.roles = action.payload.roles;
      if (action.payload.expiresAt) {
        state.expiresAt = action.payload.expiresAt;
      }
    },
    logout: (state) => {
      state.email = null;
      state.roles = [];
      state.expiresAt = null;
    },
  },
});

export const { setCredentials, logout } = authSlice.actions;
export default authSlice.reducer;
