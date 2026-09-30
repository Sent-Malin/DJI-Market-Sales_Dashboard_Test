import { useState } from 'react'
import { Area, AreaChart, Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { useTimeseries } from '@/api/queries'
import type { Granularity, TimeseriesPoint } from '@/api/types'
import { Panel } from '@/components/Panel'
import { Skeleton } from '@/components/ui/skeleton'
import { ToggleGroup, ToggleGroupItem } from '@/components/ui/toggle-group'
import { formatCompactMoney, formatDate, formatMoney, formatNumber } from '@/lib/format'
import type { PeriodParams } from '@/lib/period'

type View = 'money' | 'count'

const AXIS_TICK = { fill: 'var(--muted-foreground)', fontSize: 12 }
const CHART_HEIGHT = 300

const SERIES = {
  revenue: { label: 'Выручка', color: 'var(--series-revenue)' },
  grossProfit: { label: 'Валовая прибыль', color: 'var(--series-profit)' },
  salesCount: { label: 'Продажи', color: 'var(--series-sales)' },
} as const

function tickLabel(date: string, g: Granularity) {
  return g === 'month' ? formatDate(date, 'LLL yy') : formatDate(date, 'd MMM')
}

function bucketLabel(date: string, g: Granularity) {
  if (g === 'day') return formatDate(date, 'd MMMM yyyy, EEEE')
  if (g === 'week') return `Неделя с ${formatDate(date, 'd MMMM')}`
  return formatDate(date, 'LLLL yyyy')
}

type Props = { period: PeriodParams; className?: string }

export function TrendChart({ period, className }: Props) {
  const [view, setView] = useState<View>('money')
  const query = useTimeseries(period)

  const points = query.data?.points ?? []
  const granularity = query.data?.period.granularity ?? 'day'
  // Сервер отдаёт непрерывную шкалу; «пусто» = во всех точках ноль продаж
  const isEmpty = points.every((p) => p.salesCount === 0)

  const legend = view === 'money' ? [SERIES.revenue, SERIES.grossProfit] : [SERIES.salesCount]

  return (
    <Panel
      title="Динамика"
      description={
        <span className="flex items-center gap-4">
          {legend.map((s) => (
            <span key={s.label} className="inline-flex items-center gap-1.5">
              <span className="size-2 rounded-full" style={{ background: s.color }} />
              {s.label}
            </span>
          ))}
        </span>
      }
      actions={
        <ToggleGroup
          type="single"
          size="sm"
          variant="outline"
          value={view}
          onValueChange={(v) => v && setView(v as View)}
        >
          <ToggleGroupItem value="money" className="px-3">Выручка и прибыль</ToggleGroupItem>
          <ToggleGroupItem value="count" className="px-3">Кол-во продаж</ToggleGroupItem>
        </ToggleGroup>
      }
      isLoading={query.isLoading}
      isFetching={query.isFetching}
      isStale={query.isPlaceholderData}
      error={query.error}
      onRetry={() => query.refetch()}
      isEmpty={isEmpty}
      emptyMessage="За выбранный период нет оплаченных продаж"
      skeleton={<Skeleton className="w-full" style={{ height: CHART_HEIGHT }} />}
      className={className}
    >
      <div style={{ height: CHART_HEIGHT }}>
        <ResponsiveContainer width="100%" height="100%">
          {view === 'money' ? (
            <AreaChart data={points} margin={{ top: 8, right: 8, left: 0, bottom: 0 }}>
              <defs>
                <linearGradient id="fillRevenue" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor={SERIES.revenue.color} stopOpacity={0.25} />
                  <stop offset="95%" stopColor={SERIES.revenue.color} stopOpacity={0} />
                </linearGradient>
                <linearGradient id="fillProfit" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor={SERIES.grossProfit.color} stopOpacity={0.25} />
                  <stop offset="95%" stopColor={SERIES.grossProfit.color} stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid vertical={false} strokeDasharray="3 3" stroke="var(--border)" />
              <XAxis
                dataKey="date"
                tickFormatter={(d: string) => tickLabel(d, granularity)}
                tick={AXIS_TICK}
                tickLine={false}
                axisLine={false}
                minTickGap={24}
              />
              <YAxis
                tickFormatter={(v: number) => formatCompactMoney(v)}
                tick={AXIS_TICK}
                tickLine={false}
                axisLine={false}
                width={80}
              />
              <Tooltip
                cursor={{ stroke: 'var(--border)' }}
                content={({ active, payload }) =>
                  active ? (
                    <ChartTooltip
                      point={payload?.[0]?.payload as TimeseriesPoint | undefined}
                      granularity={granularity}
                    />
                  ) : null
                }
              />
              <Area
                type="monotone"
                dataKey="revenue"
                stroke={SERIES.revenue.color}
                strokeWidth={2}
                fill="url(#fillRevenue)"
                animationDuration={500}
              />
              <Area
                type="monotone"
                dataKey="grossProfit"
                stroke={SERIES.grossProfit.color}
                strokeWidth={2}
                fill="url(#fillProfit)"
                animationDuration={500}
              />
            </AreaChart>
          ) : (
            <BarChart data={points} margin={{ top: 8, right: 8, left: 0, bottom: 0 }}>
              <CartesianGrid vertical={false} strokeDasharray="3 3" stroke="var(--border)" />
              <XAxis
                dataKey="date"
                tickFormatter={(d: string) => tickLabel(d, granularity)}
                tick={AXIS_TICK}
                tickLine={false}
                axisLine={false}
                minTickGap={24}
              />
              <YAxis allowDecimals={false} tick={AXIS_TICK} tickLine={false} axisLine={false} width={40} />
              <Tooltip
                cursor={{ fill: 'var(--muted)' }}
                content={({ active, payload }) =>
                  active ? (
                    <ChartTooltip
                      point={payload?.[0]?.payload as TimeseriesPoint | undefined}
                      granularity={granularity}
                    />
                  ) : null
                }
              />
              <Bar
                dataKey="salesCount"
                fill={SERIES.salesCount.color}
                radius={[4, 4, 0, 0]}
                maxBarSize={32}
                animationDuration={500}
              />
            </BarChart>
          )}
        </ResponsiveContainer>
      </div>
    </Panel>
  )
}

function ChartTooltip({ point, granularity }: { point?: TimeseriesPoint; granularity: Granularity }) {
  if (!point) return null
  return (
    <div className="min-w-48 rounded-lg border bg-popover px-3 py-2 text-xs shadow-md">
      <p className="mb-1.5 font-medium">{bucketLabel(point.date, granularity)}</p>
      <TooltipRow color={SERIES.revenue.color} label={SERIES.revenue.label} value={formatMoney(point.revenue)} />
      <TooltipRow color={SERIES.grossProfit.color} label={SERIES.grossProfit.label} value={formatMoney(point.grossProfit)} />
      <TooltipRow color={SERIES.salesCount.color} label={SERIES.salesCount.label} value={formatNumber(point.salesCount)} />
    </div>
  )
}

function TooltipRow({ color, label, value }: { color: string; label: string; value: string }) {
  return (
    <div className="flex items-center justify-between gap-4 py-0.5">
      <span className="inline-flex items-center gap-1.5 text-muted-foreground">
        <span className="size-2 rounded-full" style={{ background: color }} />
        {label}
      </span>
      <span className="font-medium tabular-nums">{value}</span>
    </div>
  )
}