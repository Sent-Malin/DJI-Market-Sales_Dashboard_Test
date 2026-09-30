import { Activity } from 'lucide-react'
import { useSummary } from '@/api/queries'
import { formatRange } from '@/lib/format'
import type { PeriodParams } from '@/lib/period'
import { PeriodPicker } from './PeriodPicker'

type Props = {
  period: PeriodParams
  onPeriodChange: (next: PeriodParams) => void
}

export function DashboardHeader({ period, onPeriodChange }: Props) {
  // Тот же queryKey, что у KPI → данные из кэша, отдельного запроса нет
  const summary = useSummary(period)
  const p = summary.data?.period

  return (
    <header className="sticky top-0 z-20 border-b bg-background/80 backdrop-blur">
      <div className="mx-auto flex max-w-[1400px] items-center justify-between gap-6 px-8 py-4">
        <div className="flex items-center gap-3">
          <div className="flex size-9 items-center justify-center rounded-lg bg-primary text-primary-foreground">
            <Activity className="size-5" />
          </div>
          <div>
            <h1 className="text-lg font-semibold leading-tight">Эффективность продаж</h1>
            <p className="text-sm text-muted-foreground">
              {p ? (
                <>
                  {formatRange(p.from, p.to)}
                  <span className="mx-1.5">·</span>
                  сравнение с {formatRange(p.previousFrom, p.previousTo)}
                </>
              ) : (
                'Загрузка периода…'
              )}
            </p>
          </div>
        </div>

        <PeriodPicker value={period} onChange={onPeriodChange} />
      </div>
    </header>
  )
}