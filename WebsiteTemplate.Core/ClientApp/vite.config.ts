import {defineConfig} from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
    plugins: [vue()],
    server: {
        port: 9284,
        proxy: {
            '/api': 'http://127.0.0.1:5284',
            '/swagger': 'http://127.0.0.1:5284',
        },
    },
})
