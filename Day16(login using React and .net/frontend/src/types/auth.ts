// The data user submits when registering
export type RegisterRequest = {
  email: string;
  password: string;
};
//The data user submits when logging in
export type LoginRequest = {
  email: string;
  password: string;
};
//What the backend sends back after a successful login/register
export type AuthResponse = {
  token: string;
  email: string;
};
// Represents the currently logged-in user, stored in app state
export type User = {
  email: string;
};
