import { useRecentSales } from '@/api/queries'
import type { RecentSale, SaleStatus } from '@/api/types'
import { Panel } from '@/components/Panel'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip'
import { formatDateTime, formatMoney } from '@/lib/format'
import type { PeriodParams } from '@/lib/period'
import { cn } from '@/lib/utils'

const STATUS: Record<SaleStatus, { label: string; className: string }> = {
  paid: { label: 'Оплачена', className: 'border-emerald-200 bg-emerald-50 text-emerald-700' },
  cancelled: { label: 'Отменена', className: 'border-zinc-200 bg-zinc-100 text-zinc-600' },
  refunded: { label: 'Возврат', className: 'border-amber-200 bg-amber-50 text-amber-700' },
}

type Props = { period: PeriodParams; className?: string }

export function RecentSales({ period, className }: Props) {
  const query = useRecentSales(period, 10)
  const items = query.data?.items ?? []

  return (
    <Panel
      title="Последние продажи"
      description="10 последних сделок за период · зачёркнутые суммы не входят в выручку"
      isLoading={query.isLoading}
      isFetching={query.isFetching}
      isStale={query.isPlaceholderData}
      error={query.error}
      onRetry={() => query.refetch()}
      isEmpty={items.length === 0}
      emptyMessage="За выбранный период сделок не было"
      skeleton={
        <div className="space-y-2">
          {Array.from({ length: 6 }, (_, i) => <Skeleton key={i} className="h-10 w-full" />)}
        </div>
      }
      className={className}
    >
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="w-36">Дата (МСК)</TableHead>
            <TableHead>Менеджер</TableHead>
            <TableHead>Клиент</TableHead>
            <TableHead>Товары</TableHead>
            <TableHead className="w-28">Статус</TableHead>
            <TableHead className="w-36 text-right">Сумма</TableHead>
            <TableHead className="w-36 text-right">Вал. прибыль</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {items.map((s) => {
            const counted = s.status === 'paid'
            return (
              <TableRow key={s.id}>
                <TableCell className="whitespace-nowrap text-muted-foreground tabular-nums">
                  {formatDateTime(s.soldAt)}
                </TableCell>
                <TableCell className="font-medium">{s.managerName}</TableCell>
                <TableCell>
                  <p className="font-medium">{s.customerCompany}</p>
                  <p className="text-xs text-muted-foreground">{s.customerName}</p>
                </TableCell>
                <TableCell><ItemsCell items={s.items} /></TableCell>
                <TableCell>
                  <Badge variant="outline" className={STATUS[s.status].className}>
                    {STATUS[s.status].label}
                  </Badge>
                </TableCell>
                <TableCell
                  className={cn('text-right font-medium tabular-nums', !counted && 'text-muted-foreground line-through')}
                >
                  {formatMoney(s.amount)}
                </TableCell>
                <TableCell
                  className={cn(
                    'text-right tabular-nums',
                    !counted && 'text-muted-foreground line-through',
                    counted && s.grossProfit < 0 && 'text-red-600',
                  )}
                >
                  {formatMoney(s.grossProfit)}
                </TableCell>
              </TableRow>
            )
          })}
        </TableBody>
      </Table>
    </Panel>
  )
}

function ItemsCell({ items }: { items: RecentSale['items'] }) {
  if (items.length === 0) return <span className="text-muted-foreground">—</span>
  const [first, ...rest] = items

  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <span className="cursor-default text-sm">
          {first.productName}
          <span className="text-muted-foreground"> × {first.quantity}</span>
          {rest.length > 0 && (
            <span className="ml-1.5 rounded bg-muted px-1.5 py-0.5 text-xs text-muted-foreground">
              +{rest.length}
            </span>
          )}
        </span>
      </TooltipTrigger>
      <TooltipContent>
        <ul className="space-y-0.5">
          {items.map((i, idx) => (
            <li key={idx} className="flex justify-between gap-6 tabular-nums">
              <span>{i.productName}</span>
              <span>{i.quantity} × {formatMoney(i.unitPrice)}</span>
            </li>
          ))}
        </ul>
      </TooltipContent>
    </Tooltip>
  )
}