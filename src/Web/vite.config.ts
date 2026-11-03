import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

// `npm run dev` forwards API calls to the services on their local ports
// (the same ports whether they run with `dotnet run` or Docker).
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api/policies': 'http://localhost:5001',
      '/api/trips': 'http://localhost:5002',
      '/api/billing': 'http://localhost:5003',
    },
  },
});
