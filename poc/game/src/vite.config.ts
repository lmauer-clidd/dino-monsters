import { defineConfig } from 'vite';
import { fileURLToPath } from 'url';
import path from 'path';

const __dirname = path.dirname(fileURLToPath(import.meta.url));

export default defineConfig({
  base: './',
  resolve: {
    alias: {
      '@scenes': path.resolve(__dirname, 'scenes'),
      '@entities': path.resolve(__dirname, 'entities'),
      '@systems': path.resolve(__dirname, 'systems'),
      '@ui': path.resolve(__dirname, 'ui'),
      '@data': path.resolve(__dirname, 'data'),
      '@utils': path.resolve(__dirname, 'utils'),
    },
  },
  build: {
    target: 'ES2020',
    outDir: 'dist',
    assetsInlineLimit: 0,
  },
  server: {
    port: 3000,
    open: true,
  },
});
