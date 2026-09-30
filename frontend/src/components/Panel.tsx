import type { ReactNode } from 'react'
import { LoaderCircle } from 'lucide-react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { cn } from '@/lib/utils'
import { EmptyState, ErrorState } from './states'

type PanelProps = {
  title: string
  description?: ReactNode
  actions?: ReactNode
  /** первая загрузка, данных ещё нет */
  isLoading: boolean
  /** любой запрос в полёте (в т.ч. смена периода) */
  isFetching?: boolean
  /** показаны данные прошлого периода, пока грузится новый */
  isStale?: boolean
  error?: unknown
  onRetry?: () => void
  isEmpty?: boolean
  emptyMessage?: string
  skeleton?: ReactNode
  className?: string
  children?: ReactNode
}

/** Единая оболочка блока: все состояния из ТЗ обрабатываются одинаково во всех блоках */
export function Panel({
  title, description, actions,
  isLoading, isFetching, isStale, error, onRetry,
  isEmpty, emptyMessage, skeleton, className, children,
}: PanelProps) {
  let body: ReactNode
  if (isLoading) body = skeleton ?? <DefaultSkeleton />
  else if (error) body = <ErrorState error={error} onRetry={onRetry} />
  else if (isEmpty) body = <EmptyState message={emptyMessage} />
  else body = <div className={cn('transition-opacity duration-200', isStale && 'opacity-60')}>{children}</div>

  return (
    <Card className={cn('gap-4', className)} aria-busy={isFetching}>
      <CardHeader className="flex flex-row items-start justify-between gap-4">
        <div className="space-y-1">
          <CardTitle className="flex items-center gap-2 text-base">
            {title}
            {isFetching && !isLoading && (
              <LoaderCircle className="size-3.5 animate-spin text-muted-foreground" aria-label="Обновление" />
            )}
          </CardTitle>
          {description && <CardDescription>{description}</CardDescription>}
        </div>
        {actions}
      </CardHeader>
      <CardContent>{body}</CardContent>
    </Card>
  )
}

function DefaultSkeleton() {
  return (
    <div className="space-y-3">
      <Skeleton className="h-4 w-2/3" />
      <Skeleton className="h-4 w-1/2" />
      <Skeleton className="h-32 w-full" />
    </div>
  )
}