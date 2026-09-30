export type PeriodPreset = 'today' | '7d' | '30d' | 'thisMonth' | 'lastMonth' | 'custom'

/** type, а не interface: нужна неявная index signature для передачи в apiGet */
export type PeriodParams = {
  period: PeriodPreset
  from?: string // YYYY-MM-DD, только для custom
  to?: string
}

export const DEFAULT_PERIOD: PeriodParams = { period: '30d' }

export const PRESETS: { value: Exclude<PeriodPreset, 'custom'>; label: string }[] = [
  { value: 'today', label: 'Сегодня' },
  { value: '7d', label: '7 дней' },
  { value: '30d', label: '30 дней' },
  { value: 'thisMonth', label: 'Этот месяц' },
  { value: 'lastMonth', label: 'Прошлый месяц' },
]

const PRESET_VALUES: readonly string[] = ['today', '7d', '30d', 'thisMonth', 'lastMonth', 'custom']
const ISO_DATE = /^\d{4}-\d{2}-\d{2}$/

export function isPreset(value: string | null): value is PeriodPreset {
  return value !== null && PRESET_VALUES.includes(value)
}

export function isIsoDate(value: string | null | undefined): value is string {
  return !!value && ISO_DATE.test(value)
}