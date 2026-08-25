import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path"; // ← Add this import

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
  },
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"), // ← Add this
    },
  },
});
