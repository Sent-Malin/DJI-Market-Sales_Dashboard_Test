import type { ReactNode } from 'react'
import { motion, type Variants } from 'motion/react'
import { Banknote, Crown, Percent, Receipt, ShoppingCart, TrendingUp, type LucideIcon } from 'lucide-react'
import { useSummary } from '@/api/queries'
import type { Metric, Summary } from '@/api/types'
import { AnimatedNumber } from '@/components/AnimatedNumber'
import { DeltaBadge } from '@/components/DeltaBadge'
import { ErrorState } from '@/components/states'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { Card } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip'
import { formatMoney, formatNumber, formatPercent, initials } from '@/lib/format'
import type { PeriodParams } from '@/lib/period'
import { cn } from '@/lib/utils'

const container: Variants = {
  hidden: {},
  show: { transition: { staggerChildren: 0.05 } },
}

const item: Variants = {
  hidden: { opacity: 0, y: 8 },
  show: { opacity: 1, y: 0, transition: { duration: 0.3, ease: 'easeOut' } },
}

const GRID = 'grid grid-cols-3 gap-4 xl:grid-cols-6'

export function KpiCards({ period }: { period: PeriodParams }) {
  const summary = useSummary(period)

  if (summary.isLoading) return <KpiSkeleton />

  if (summary.error) {
    return (
      <Card>
        <ErrorState error={summary.error} onRetry={() => summary.refetch()} />
      </Card>
    )
  }

  const d = summary.data
  if (!d) return null

  return (
    <motion.div
      className={cn(GRID, 'transition-opacity duration-200', summary.isPlaceholderData && 'opacity-60')}
      variants={container}
      initial="hidden"
      animate="show"
    >
      <KpiCard label="Выручка" icon={Banknote} metric={d.revenue} format={formatMoney} />
      <KpiCard label="Валовая прибыль" icon={TrendingUp} metric={d.grossProfit} format={formatMoney} />
      <KpiCard label="Маржинальность" icon={Percent} metric={d.margin} format={formatPercent} deltaKind="points" />
      <KpiCard
        label="Продажи"
        icon={ShoppingCart}
        metric={d.salesCount}
        format={formatNumber}
        footnote={
          <p className="text-xs text-muted-foreground">
            Возвраты: {d.refunds.count}
            {d.refunds.count > 0 && ` (${formatMoney(d.refunds.amount)})`} · Отмены: {d.cancelledCount}
          </p>
        }
      />
      <KpiCard label="Средний чек" icon={Receipt} metric={d.averageCheck} format={formatMoney} />
      <BestManagerCard manager={d.bestManager} />
    </motion.div>
  )
}

type KpiCardProps = {
  label: string
  icon: LucideIcon
  metric: Metric
  format: (v: number) => string
  deltaKind?: 'relative' | 'points'
  footnote?: ReactNode
}

function KpiCard({ label, icon: Icon, metric, format, deltaKind = 'relative', footnote }: KpiCardProps) {
  return (
    <motion.div variants={item}>
      <Card className="h-full gap-2 px-5 py-4">
        <div className="flex items-center justify-between text-sm text-muted-foreground">
          <span>{label}</span>
          <Icon className="size-4" />
        </div>

        <div className="text-2xl font-semibold tracking-tight">
          {metric.value == null ? '—' : <AnimatedNumber value={metric.value} format={format} />}
        </div>

        <div className="flex items-center gap-2 text-xs text-muted-foreground">
          <Tooltip>
            <TooltipTrigger asChild>
              <span className="cursor-default">
                <DeltaBadge change={metric.change} kind={deltaKind} />
              </span>
            </TooltipTrigger>
            <TooltipContent>
              Предыдущий период: {metric.previous == null ? '—' : format(metric.previous)}
            </TooltipContent>
          </Tooltip>
          <span>к пред. периоду</span>
        </div>

        {footnote}
      </Card>
    </motion.div>
  )
}

function BestManagerCard({ manager }: { manager: Summary['bestManager'] }) {
  return (
    <motion.div variants={item}>
      <Card className="h-full gap-2 px-5 py-4">
        <div className="flex items-center justify-between text-sm text-muted-foreground">
          <span>Лучший менеджер</span>
          <Crown className="size-4 text-amber-500" />
        </div>

        {manager ? (
          <>
            <div className="flex items-center gap-3">
              <Avatar className="size-9">
                {manager.avatarUrl && <AvatarImage src={manager.avatarUrl} alt={manager.fullName} />}
                <AvatarFallback className="bg-primary/10 text-xs font-semibold text-primary">
                  {initials(manager.fullName)}
                </AvatarFallback>
              </Avatar>
              <div className="min-w-0">
                <p className="truncate font-semibold leading-tight">{manager.fullName}</p>
                <p className="text-xs text-muted-foreground tabular-nums">
                  GP {formatMoney(manager.grossProfit)}
                </p>
              </div>
            </div>
            <div className="flex items-center gap-2 text-xs text-muted-foreground">
              <DeltaBadge change={manager.change} />
              <span>прибыль к пред. периоду</span>
            </div>
          </>
        ) : (
          <p className="text-sm text-muted-foreground">Нет оплаченных продаж за период</p>
        )}
      </Card>
    </motion.div>
  )
}

function KpiSkeleton() {
  return (
    <div className={GRID}>
      {Array.from({ length: 6 }, (_, i) => (
        <Card key={i} className="gap-3 px-5 py-4">
          <Skeleton className="h-4 w-24" />
          <Skeleton className="h-8 w-32" />
          <Skeleton className="h-4 w-20" />
        </Card>
      ))}
    </div>
  )
}