import { execFileSync } from 'node:child_process'
import { existsSync, mkdirSync, readFileSync } from 'node:fs'
import { resolve } from 'node:path'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

function developmentHttps() {
  const certificateDirectory = resolve(process.cwd(), '.certs')
  const certificatePath = resolve(certificateDirectory, 'reservatucitaya.pem')
  const keyPath = resolve(certificateDirectory, 'reservatucitaya.key')

  if (!existsSync(certificatePath) || !existsSync(keyPath)) {
    mkdirSync(certificateDirectory, { recursive: true })
    execFileSync('dotnet', [
      'dev-certs', 'https',
      '--export-path', certificatePath,
      '--format', 'Pem',
      '--no-password',
    ], { stdio: 'inherit' })
  }

  return {
    cert: readFileSync(certificatePath),
    key: readFileSync(keyPath),
  }
}

export default defineConfig(({ command }) => ({
  plugins: [react()],
  server: {
    host: 'localhost',
    port: 5173,
    strictPort: true,
    ...(command === 'serve' ? { https: developmentHttps() } : {}),
  },
}))
