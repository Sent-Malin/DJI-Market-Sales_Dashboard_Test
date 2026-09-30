import { useSummary } from '@/api/queries'
import { Panel } from '@/components/Panel'
import { Button } from '@/components/ui/button'
import { usePeriod } from '@/hooks/useDashboardParams'
import { formatDelta, formatMoney, formatRange } from '@/lib/format'
import { PRESETS } from '@/lib/period'

export default function App() {
  const { period, setPeriod } = usePeriod()
  const summary = useSummary(period)

  return (
    <div className="min-h-screen bg-muted/40 p-8">
      <div className="mx-auto max-w-[1400px] space-y-6">
        <div className="flex gap-2">
          {PRESETS.map((p) => (
            <Button
              key={p.value}
              size="sm"
              variant={period.period === p.value ? 'default' : 'outline'}
              onClick={() => setPeriod({ period: p.value })}
            >
              {p.label}
            </Button>
          ))}
        </div>

        <Panel
          title="Выручка — проверка связки"
          description={summary.data && formatRange(summary.data.period.from, summary.data.period.to)}
          isLoading={summary.isLoading}
          isFetching={summary.isFetching}
          isStale={summary.isPlaceholderData}
          error={summary.error}
          onRetry={() => summary.refetch()}
          isEmpty={summary.data?.salesCount.value === 0}
          className="max-w-sm"
        >
          <div className="flex items-baseline gap-3">
            <span className="text-3xl font-semibold tabular-nums">
              {formatMoney(summary.data?.revenue.value)}
            </span>
            <span className="text-sm text-muted-foreground">
              {formatDelta(summary.data?.revenue.change)}
            </span>
          </div>
        </Panel>
      </div>
    </div>
  )
}