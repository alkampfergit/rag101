import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";

// The frontend is built straight into the API's wwwroot folder:
// the API serves it, and no dev server is used.
export default defineConfig({
  plugins: [react(), tailwindcss()],
  build: {
    outDir: "../src/Rag101.Api/wwwroot",
    emptyOutDir: true,
  },
});
