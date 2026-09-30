import { Inbox, RotateCw, TriangleAlert } from 'lucide-react'
import { ApiError } from '@/api/client'
import { Button } from '@/components/ui/button'

function describe(error: unknown) {
  if (error instanceof ApiError) {
    if (error.status === 0) return error.message
    if (error.status >= 500) return 'Ошибка на сервере. Попробуйте повторить через минуту.'
    return error.message // 4xx — текст валидации с backend
  }
  return 'Непредвиденная ошибка.'
}

export function ErrorState({ error, onRetry }: { error: unknown; onRetry?: () => void }) {
  return (
    <div role="alert" className="flex flex-col items-center justify-center gap-3 py-10 text-center">
      <div className="rounded-full bg-destructive/10 p-3 text-destructive">
        <TriangleAlert className="size-5" />
      </div>
      <div className="space-y-1">
        <p className="text-sm font-medium">Не удалось загрузить данные</p>
        <p className="max-w-xs text-sm text-muted-foreground">{describe(error)}</p>
      </div>
      {onRetry && (
        <Button variant="outline" size="sm" onClick={onRetry}>
          <RotateCw className="size-4" />
          Повторить
        </Button>
      )}
    </div>
  )
}

export function EmptyState({ message = 'Нет данных за выбранный период' }: { message?: string }) {
  return (
    <div className="flex flex-col items-center justify-center gap-3 py-10 text-center">
      <div className="rounded-full bg-muted p-3 text-muted-foreground">
        <Inbox className="size-5" />
      </div>
      <p className="max-w-xs text-sm text-muted-foreground">{message}</p>
    </div>
  )
}