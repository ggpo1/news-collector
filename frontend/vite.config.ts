import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

const aiProxyTimeoutMs = 30 * 60 * 1000;

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5190,
    proxy: {
      '/api': {
        target: 'http://localhost:5182',
        changeOrigin: true,
        timeout: aiProxyTimeoutMs,
        proxyTimeout: aiProxyTimeoutMs,
      },
      '/health': 'http://localhost:5182',
    },
  },
});
