import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { existsSync, readFileSync } from 'node:fs'
import { fileURLToPath } from 'node:url'

const certificatePath = fileURLToPath(new URL('./.certs/reservatucitaya.pem', import.meta.url))
const certificateKeyPath = fileURLToPath(new URL('./.certs/reservatucitaya.key', import.meta.url))

export default defineConfig(({ command }) => {
  const hasDevelopmentCertificate = existsSync(certificatePath) && existsSync(certificateKeyPath)

  if (command === 'serve' && !hasDevelopmentCertificate) {
    throw new Error(
      'Falta el certificado HTTPS local. Exporta uno en .certs/reservatucitaya.pem y .certs/reservatucitaya.key.',
    )
  }

  return {
    plugins: [react()],
    server: {
      host: 'localhost',
      port: 5173,
      https: hasDevelopmentCertificate
        ? {
            cert: readFileSync(certificatePath),
            key: readFileSync(certificateKeyPath),
          }
        : undefined,
    },
  }
})
