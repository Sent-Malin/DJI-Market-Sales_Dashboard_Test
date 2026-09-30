import { motion } from 'motion/react'
import { useTopProducts } from '@/api/queries'
import { Panel } from '@/components/Panel'
import { Skeleton } from '@/components/ui/skeleton'
import { formatMoney, formatNumber, formatPercent } from '@/lib/format'
import type { PeriodParams } from '@/lib/period'

type Props = { period: PeriodParams; className?: string }

export function TopProducts({ period, className }: Props) {
  const query = useTopProducts(period, 5)
  const items = query.data?.items ?? []
  const max = Math.max(0, ...items.map((p) => p.grossProfit))

  return (
    <Panel
      title="Лучшие товары"
      description="Топ-5 по валовой прибыли"
      isLoading={query.isLoading}
      isFetching={query.isFetching}
      isStale={query.isPlaceholderData}
      error={query.error}
      onRetry={() => query.refetch()}
      isEmpty={items.length === 0}
      emptyMessage="Нет проданных товаров за период"
      skeleton={
        <div className="space-y-3">
          {Array.from({ length: 5 }, (_, i) => <Skeleton key={i} className="h-11 w-full" />)}
        </div>
      }
      className={className}
    >
      <ol className="space-y-3.5">
        {items.map((p, i) => (
          <li key={p.productId} className="space-y-1.5">
            <div className="flex items-start justify-between gap-3">
              <div className="flex min-w-0 gap-3">
                <span className="w-4 pt-px text-sm text-muted-foreground tabular-nums">{i + 1}</span>
                <div className="min-w-0">
                  <p className="truncate text-sm font-medium">{p.name}</p>
                  <p className="text-xs text-muted-foreground">
                    {p.categoryName} · {formatNumber(p.units)} шт. · {formatMoney(p.revenue)}
                  </p>
                </div>
              </div>
              <div className="shrink-0 text-right">
                <p className="text-sm font-semibold tabular-nums">{formatMoney(p.grossProfit)}</p>
                <p className="text-xs text-muted-foreground tabular-nums">маржа {formatPercent(p.margin)}</p>
              </div>
            </div>
            <div className="ml-7 h-1.5 rounded-full bg-muted">
              <motion.div
                className="h-1.5 rounded-full"
                style={{ background: 'var(--series-profit)' }}
                initial={false}
                animate={{ width: `${max > 0 ? (Math.max(0, p.grossProfit) / max) * 100 : 0}%` }}
                transition={{ duration: 0.5, ease: 'easeOut' }}
              />
            </div>
          </li>
        ))}
      </ol>
    </Panel>
  )
}