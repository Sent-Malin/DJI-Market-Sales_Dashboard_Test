import { ArrowDownRight, ArrowUpRight, Minus } from 'lucide-react'
import { formatDelta } from '@/lib/format'
import { cn } from '@/lib/utils'

type Props = {
  change: number | null
  kind?: 'relative' | 'points'
  className?: string
}

export function DeltaBadge({ change, kind = 'relative', className }: Props) {
  if (change == null) {
    return <span className={cn('text-xs text-muted-foreground', className)}>—</span>
  }

  const up = change > 0
  const down = change < 0
  const Icon = up ? ArrowUpRight : down ? ArrowDownRight : Minus

  return (
    <span
      className={cn(
        'inline-flex items-center gap-0.5 rounded-md px-1.5 py-0.5 text-xs font-medium tabular-nums',
        up && 'bg-emerald-500/10 text-emerald-700',
        down && 'bg-red-500/10 text-red-700',
        !up && !down && 'bg-muted text-muted-foreground',
        className,
      )}
    >
      <Icon className="size-3" />
      {formatDelta(change, kind)}
    </span>
  )
}