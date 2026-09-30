import { setupServer } from 'msw/node'

// Обработчики задаются в каждом тесте через server.use(...)
export const server = setupServer()