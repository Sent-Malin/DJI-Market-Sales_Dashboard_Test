import { AnimatePresence, motion } from 'motion/react'
import { ChevronDown, ChevronUp } from 'lucide-react'
import { useManagerRating } from '@/api/queries'
import type { ManagerRating as RatingRow, RatingMetric } from '@/api/types'
import { DeltaBadge } from '@/components/DeltaBadge'
import { Panel } from '@/components/Panel'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { Skeleton } from '@/components/ui/skeleton'
import { ToggleGroup, ToggleGroupItem } from '@/components/ui/toggle-group'
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip'
import { useRatingMetric } from '@/hooks/useDashboardParams'
import { formatMoney, formatNumber, formatPercent, initials } from '@/lib/format'
import type { PeriodParams } from '@/lib/period'
import { cn } from '@/lib/utils'

const COLS =
  'grid grid-cols-[32px_minmax(160px,1fr)_52px_104px_112px_104px_60px_88px] items-center gap-2 px-2'

const TABLE_MIN_WIDTH = 'min-w-[784px]' // 768 колонки + 16 padding строки

const METRIC: Record<RatingMetric, { label: string; color: string }> = {
  grossProfit: { label: 'валовой прибыли', color: 'var(--series-profit)' },
  averageCheck: { label: 'среднему чеку', color: 'var(--series-revenue)' },
}

function metricValue(row: RatingRow, metric: RatingMetric) {
  if (row.salesCount === 0) return null
  return metric === 'grossProfit' ? row.grossProfit : row.averageCheck
}

type Props = { period: PeriodParams; className?: string }

export function ManagerRating({ period, className }: Props) {
  const { metric, setMetric } = useRatingMetric()
  const query = useManagerRating(period, metric)

  const items = query.data?.items ?? []
  // Подсвечиваем режим, по которому отсортированы данные НА ЭКРАНЕ.
  // Пока новый режим грузится (keepPreviousData), это ещё старый.
  const shown = query.data?.sortBy ?? metric
  const max = Math.max(0, ...items.map((r) => metricValue(r, shown) ?? 0))
  const isEmpty = items.every((r) => r.salesCount === 0)

  return (
    <Panel
      title="Рейтинг менеджеров"
      description={`Места по ${METRIC[shown].label}; равные значения делят место`}
      actions={
        <ToggleGroup
          type="single"
          size="sm"
          variant="outline"
          value={metric}
          onValueChange={(v) => v && setMetric(v as RatingMetric)}
        >
          <ToggleGroupItem value="grossProfit" className="px-3">Валовая прибыль</ToggleGroupItem>
          <ToggleGroupItem value="averageCheck" className="px-3">Средний чек</ToggleGroupItem>
        </ToggleGroup>
      }
      isLoading={query.isLoading}
      isFetching={query.isFetching}
      isStale={query.isPlaceholderData}
      error={query.error}
      onRetry={() => query.refetch()}
      isEmpty={isEmpty}
      emptyMessage="Ни у одного менеджера нет оплаченных продаж за период"
      skeleton={<RowsSkeleton />}
      className={className}
    >
      <div className="overflow-x-auto">
        <div role="table" aria-label="Рейтинг менеджеров" className={cn('text-sm', TABLE_MIN_WIDTH)}>
          <div role="row" className={cn(COLS, 'border-b pb-2 text-xs font-medium text-muted-foreground')}>
            <span role="columnheader">#</span>
            <span role="columnheader">Менеджер</span>
            <span role="columnheader" className="text-right">Продаж</span>
            <span role="columnheader" className="text-right">Выручка</span>
            <span role="columnheader" className={cn('text-right', shown === 'grossProfit' && 'text-foreground')}>
              Вал. прибыль
            </span>
            <span role="columnheader" className={cn('text-right', shown === 'averageCheck' && 'text-foreground')}>
              Ср. чек
            </span>
            <span role="columnheader" className="text-right">Маржа</span>
            <span role="columnheader" className="text-right">К пред.</span>
          </div>

          <div className="relative">
            <AnimatePresence initial={false} mode="popLayout">
              {items.map((row) => (
                <motion.div
                  key={row.managerId}
                  role="row"
                  layout="position"
                  initial={{ opacity: 0 }}
                  animate={{ opacity: 1 }}
                  exit={{ opacity: 0 }}
                  transition={{
                    layout: { type: 'spring', stiffness: 500, damping: 40 },
                    opacity: { duration: 0.2 },
                  }}
                  className={cn(COLS, 'rounded-md border-b py-2.5 last:border-0 hover:bg-muted/50')}
                >
                  <span role="cell"><RankBadge rank={row.rank} /></span>

                  <span role="cell" className="flex min-w-0 items-center gap-2.5">
                    <Avatar className="size-8 shrink-0">
                      {row.avatarUrl && <AvatarImage src={row.avatarUrl} alt={row.fullName} />}
                      <AvatarFallback className="bg-primary/10 text-xs font-semibold text-primary">
                        {initials(row.fullName)}
                      </AvatarFallback>
                    </Avatar>
                    <span className="min-w-0">
                      <span className="block truncate font-medium" title={row.fullName}>
                        {row.fullName}
                        {!row.isActive && (
                          <span className="ml-1.5 text-xs font-normal text-muted-foreground">(неактивен)</span>
                        )}
                      </span>
                      <span className="block truncate text-xs text-muted-foreground">
                        {row.team} · {row.position}
                      </span>
                    </span>
                  </span>

                  <span role="cell" className="text-right tabular-nums">{formatNumber(row.salesCount)}</span>
                  <span role="cell" className="text-right tabular-nums">{formatMoney(row.revenue)}</span>
                  <MetricCell
                    value={row.salesCount === 0 ? null : row.grossProfit}
                    max={max}
                    active={shown === 'grossProfit'}
                    color={METRIC.grossProfit.color}
                  />
                  <MetricCell
                    value={row.averageCheck}
                    max={max}
                    active={shown === 'averageCheck'}
                    color={METRIC.averageCheck.color}
                  />
                  <span role="cell" className="text-right tabular-nums">{formatPercent(row.margin)}</span>

                  <span role="cell" className="flex flex-col items-end gap-1">
                    <DeltaBadge change={shown === 'grossProfit' ? row.grossProfitChange : row.averageCheckChange} />
                    <RankChange rank={row.rank} change={row.rankChange} />
                  </span>
                </motion.div>
              ))}
            </AnimatePresence>
          </div>
        </div>
      </div>
    </Panel>
  )
}

function RankBadge({ rank }: { rank: number | null }) {
  if (rank == null) return <span className="text-muted-foreground">—</span>
  const podium =
    rank === 1 ? 'bg-amber-100 text-amber-800'
      : rank === 2 ? 'bg-zinc-200 text-zinc-700'
        : rank === 3 ? 'bg-orange-100 text-orange-800'
          : 'text-muted-foreground'
  return (
    <span className={cn('inline-flex size-7 items-center justify-center rounded-full text-xs font-semibold tabular-nums', podium)}>
      {rank}
    </span>
  )
}

function MetricCell({ value, max, active, color }: { value: number | null; max: number; active: boolean; color: string }) {
  const width = active && value != null && max > 0 ? (Math.max(0, value) / max) * 100 : 0
  return (
    <span role="cell" className="text-right">
      <span className={cn('tabular-nums', active && 'font-semibold')}>{formatMoney(value)}</span>
      {active && (
        <span className="mt-1 block h-1 rounded-full bg-muted">
          <motion.span
            className="ml-auto block h-1 rounded-full"
            style={{ background: color }}
            initial={false}
            animate={{ width: `${width}%` }}
            transition={{ duration: 0.5, ease: 'easeOut' }}
          />
        </span>
      )}
    </span>
  )
}

function RankChange({ rank, change }: { rank: number | null; change: number | null }) {
  if (rank == null) return null
  if (change == null) {
    return <span className="text-[10px] font-medium uppercase tracking-wide text-blue-600">новый</span>
  }

  const previous = rank + change
  const content =
    change > 0 ? (
      <span className="inline-flex items-center text-xs text-emerald-600"><ChevronUp className="size-3" />{change}</span>
    ) : change < 0 ? (
      <span className="inline-flex items-center text-xs text-red-600"><ChevronDown className="size-3" />{Math.abs(change)}</span>
    ) : (
      <span className="text-xs text-muted-foreground">без изм.</span>
    )

  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <span className="cursor-default">{content}</span>
      </TooltipTrigger>
      <TooltipContent>Место в прошлом периоде: {previous}</TooltipContent>
    </Tooltip>
  )
}

function RowsSkeleton() {
  return (
    <div className="space-y-2">
      {Array.from({ length: 6 }, (_, i) => (
        <Skeleton key={i} className="h-11 w-full" />
      ))}
    </div>
  )
}