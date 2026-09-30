import { useState } from 'react'
import { differenceInCalendarDays, format, parseISO, subMonths } from 'date-fns'
import { ru } from 'date-fns/locale'
import type { DateRange } from 'react-day-picker'
import { CalendarDays } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Calendar } from '@/components/ui/calendar'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import { ToggleGroup, ToggleGroupItem } from '@/components/ui/toggle-group'
import { formatRange } from '@/lib/format'
import { PRESETS, type PeriodParams, type PeriodPreset } from '@/lib/period'
import { cn } from '@/lib/utils'

const MAX_RANGE_DAYS = 731 // синхронно с PeriodResolver.MaxRangeDays на backend
const toIso = (d: Date) => format(d, 'yyyy-MM-dd')

type Props = {
  value: PeriodParams
  onChange: (next: PeriodParams) => void
}

export function PeriodPicker({ value, onChange }: Props) {
  const [open, setOpen] = useState(false)
  const [draft, setDraft] = useState<DateRange | undefined>()

  const isCustom = value.period === 'custom'
  const rangeDays = draft?.from && draft.to ? differenceInCalendarDays(draft.to, draft.from) + 1 : 0
  const tooLong = rangeDays > MAX_RANGE_DAYS
  const canApply = rangeDays > 0 && !tooLong

  function handleOpenChange(next: boolean) {
    // При открытии календаря подставляем текущий custom-диапазон как черновик
    if (next) {
      setDraft(
        isCustom && value.from && value.to
          ? { from: parseISO(value.from), to: parseISO(value.to) }
          : undefined,
      )
    }
    setOpen(next)
  }

  function apply() {
    if (!canApply || !draft?.from || !draft.to) return
    onChange({ period: 'custom', from: toIso(draft.from), to: toIso(draft.to) })
    setOpen(false)
  }

  return (
    <div className="flex items-center gap-2">
      <ToggleGroup
        type="single"
        variant="outline"
        size="sm"
        value={isCustom ? '' : value.period}
        onValueChange={(v) => {
          if (v) onChange({ period: v as PeriodPreset }) // '' = попытка снять выбор — игнорируем
        }}
      >
        {PRESETS.map((p) => (
          <ToggleGroupItem key={p.value} value={p.value} className="px-3">
            {p.label}
          </ToggleGroupItem>
        ))}
      </ToggleGroup>

      <Popover open={open} onOpenChange={handleOpenChange}>
        <PopoverTrigger asChild>
          <Button variant={isCustom ? 'default' : 'outline'} size="sm" className="gap-2">
            <CalendarDays className="size-4" />
            {isCustom && value.from && value.to ? formatRange(value.from, value.to) : 'Период…'}
          </Button>
        </PopoverTrigger>
        <PopoverContent align="end" className="w-auto p-0">
          <Calendar
            mode="range"
            numberOfMonths={2}
            locale={ru}
            selected={draft}
            onSelect={setDraft}
            defaultMonth={draft?.from ?? subMonths(new Date(), 1)}
            disabled={{ after: new Date() }}
          />
          <div className="flex items-center justify-between gap-4 border-t p-3">
            <p className={cn('text-sm', tooLong ? 'text-destructive' : 'text-muted-foreground')}>
              {tooLong
                ? 'Диапазон не больше 2 лет'
                : draft?.from && draft.to
                  ? formatRange(toIso(draft.from), toIso(draft.to))
                  : 'Выберите начало и конец'}
            </p>
            <Button size="sm" onClick={apply} disabled={!canApply}>
              Применить
            </Button>
          </div>
        </PopoverContent>
      </Popover>
    </div>
  )
}