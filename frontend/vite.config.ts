import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5181,
    proxy: {
      '/api': 'http://localhost:5182',
      '/health': 'http://localhost:5182',
    },
  },
});
