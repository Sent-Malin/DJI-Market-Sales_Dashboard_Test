type ProblemDetails = {
  title?: string
  detail?: string
  status?: number
  errors?: Record<string, string[]>
}

export class ApiError extends Error {
  readonly status: number
  readonly errors?: Record<string, string[]>

  constructor(status: number, message: string, errors?: Record<string, string[]>) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.errors = errors
  }
}

type QueryParams = Record<string, string | number | undefined>

export async function apiGet<T>(path: string, params: QueryParams, signal?: AbortSignal): Promise<T> {
  const qs = new URLSearchParams()
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== '') qs.set(key, String(value))
  }

  let response: Response
  try {
    response = await fetch(`/api${path}?${qs}`, { signal, headers: { Accept: 'application/json' } })
  } catch (e) {
    if (e instanceof DOMException && e.name === 'AbortError') throw e // отмена — не ошибка
    throw new ApiError(0, 'Сервер недоступен. Проверьте соединение.')
  }

  if (!response.ok) {
    let problem: ProblemDetails | undefined
    try {
      problem = (await response.json()) as ProblemDetails
    } catch {
      // тело не JSON — оставляем общий текст
    }
    const validation = problem?.errors ? Object.values(problem.errors).flat().join(' ') : undefined
    throw new ApiError(
      response.status,
      validation ?? problem?.detail ?? problem?.title ?? `Ошибка ${response.status}`,
      problem?.errors,
    )
  }

  return (await response.json()) as T
}