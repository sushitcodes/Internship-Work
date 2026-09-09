import { createSlice, PayloadAction } from "@reduxjs/toolkit";

interface AuthState {
  email: string | null;
  roles: string[];
}

const initialState: AuthState = {
  email: null,
  roles: [],
  // DELETE — no longer read from localStorage; see /me flow below
};

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    setCredentials: (
      state,
      action: PayloadAction<{ email: string; roles: string[] }>,
    ) => {
      state.email = action.payload.email;
      state.roles = action.payload.roles;
    },
    logout: (state) => {
      state.email = null;
      state.roles = [];
    },
  },
});

export const { setCredentials, logout } = authSlice.actions;
export default authSlice.reducer;
