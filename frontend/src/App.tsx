import { Info } from 'lucide-react'
import { useSummary } from '@/api/queries'
import { CategoryBreakdown } from '@/components/dashboard/CategoryBreakdown'
import { DashboardHeader } from '@/components/dashboard/DashboardHeader'
import { KpiCards } from '@/components/dashboard/KpiCards'
import { ManagerRating } from '@/components/dashboard/ManagerRating'
import { RecentSales } from '@/components/dashboard/RecentSales'
import { TopProducts } from '@/components/dashboard/TopProducts'
import { TrendChart } from '@/components/dashboard/TrendChart'
import { TooltipProvider } from '@/components/ui/tooltip'
import { usePeriod } from '@/hooks/useDashboardParams'

export default function App() {
  const { period, setPeriod } = usePeriod()
  const summary = useSummary(period)
  const s = summary.data
  const noSales = s?.salesCount.value === 0

  return (
    <TooltipProvider delayDuration={200}>
      <div className="min-h-screen bg-muted/40">
        <DashboardHeader period={period} onPeriodChange={setPeriod} />

        <main className="mx-auto max-w-[1400px] space-y-6 px-8 py-6">
          {noSales && s && (
            <div className="flex items-center gap-3 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-900">
              <Info className="size-4 shrink-0" />
              <span>
                За выбранный период нет оплаченных продаж
                {(s.cancelledCount > 0 || s.refunds.count > 0) &&
                  ` (отмен: ${s.cancelledCount}, возвратов: ${s.refunds.count})`}
                . Попробуйте выбрать более длинный период.
              </span>
            </div>
          )}

          <KpiCards period={period} />

          <div className="grid grid-cols-12 gap-6">
            <TrendChart period={period} className="col-span-8" />
            <CategoryBreakdown period={period} className="col-span-4" />
            <ManagerRating period={period} className="col-span-8 min-w-0" />
            <TopProducts period={period} className="col-span-4" />
            <RecentSales period={period} className="col-span-12" />
          </div>
        </main>
      </div>
    </TooltipProvider>
  )
}