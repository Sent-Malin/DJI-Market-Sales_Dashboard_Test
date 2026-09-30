import { motion } from 'motion/react'
import { useCategories } from '@/api/queries'
import { Panel } from '@/components/Panel'
import { Skeleton } from '@/components/ui/skeleton'
import { formatCompactMoney, formatMoney, formatPercent } from '@/lib/format'
import type { PeriodParams } from '@/lib/period'

// Цвет привязан к id категории, а не к позиции: при смене периода порядок меняется, цвет — нет
const PALETTE = [
  'var(--series-revenue)',
  'var(--series-profit)',
  'oklch(0.769 0.188 70.08)',   // amber-500
  'oklch(0.627 0.265 303.9)',   // purple-500
  'oklch(0.645 0.246 16.439)',  // rose-500
  'oklch(0.715 0.143 215.221)', // cyan-500
]
const colorOf = (categoryId: number) => PALETTE[(categoryId - 1) % PALETTE.length]

type Props = { period: PeriodParams; className?: string }

export function CategoryBreakdown({ period, className }: Props) {
  const query = useCategories(period)
  const items = query.data?.items ?? []
  const isEmpty = items.every((c) => c.revenue === 0)

  return (
    <Panel
      title="Категории"
      description="Доля в выручке, прибыль и маржа"
      isLoading={query.isLoading}
      isFetching={query.isFetching}
      isStale={query.isPlaceholderData}
      error={query.error}
      onRetry={() => query.refetch()}
      isEmpty={isEmpty}
      emptyMessage="Нет продаж по категориям за период"
      skeleton={
        <div className="space-y-3">
          <Skeleton className="h-2.5 w-full" />
          {Array.from({ length: 4 }, (_, i) => <Skeleton key={i} className="h-10 w-full" />)}
        </div>
      }
      className={className}
    >
      <div className="flex h-2.5 overflow-hidden rounded-full bg-muted">
        {items.map((c) => (
          <motion.div
            key={c.categoryId}
            style={{ background: colorOf(c.categoryId) }}
            initial={false}
            animate={{ width: `${c.share * 100}%` }}
            transition={{ duration: 0.5, ease: 'easeOut' }}
          />
        ))}
      </div>

      <ul className="mt-4 divide-y">
        {items.map((c) => (
          <li key={c.categoryId} className="flex items-center justify-between gap-3 py-2.5">
            <div className="flex min-w-0 items-center gap-2">
              <span className="size-2.5 shrink-0 rounded-full" style={{ background: colorOf(c.categoryId) }} />
              <span className="truncate text-sm font-medium">{c.name}</span>
              <span className="text-xs text-muted-foreground tabular-nums">{formatPercent(c.share)}</span>
            </div>
            <div className="text-right">
              <p className="text-sm font-medium tabular-nums">{formatMoney(c.revenue)}</p>
              <p className="text-xs text-muted-foreground tabular-nums">
                GP {formatCompactMoney(c.grossProfit)} · маржа {formatPercent(c.margin)}
              </p>
            </div>
          </li>
        ))}
      </ul>
    </Panel>
  )
}