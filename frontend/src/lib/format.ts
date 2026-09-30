import { format, parseISO } from 'date-fns'
import { ru } from 'date-fns/locale'

const EMPTY = '—'
const BUSINESS_TZ = 'Europe/Moscow'

const moneyFmt = new Intl.NumberFormat('ru-RU', { style: 'currency', currency: 'RUB', maximumFractionDigits: 0 })
const compactFmt = new Intl.NumberFormat('ru-RU', { notation: 'compact', maximumFractionDigits: 1 })
const intFmt = new Intl.NumberFormat('ru-RU', { maximumFractionDigits: 0 })
const percentFmt = new Intl.NumberFormat('ru-RU', { style: 'percent', minimumFractionDigits: 1, maximumFractionDigits: 1 })
const dateTimeFmt = new Intl.DateTimeFormat('ru-RU', {
  day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit', timeZone: BUSINESS_TZ,
})

type Num = number | null | undefined

export const formatMoney = (v: Num) => (v == null ? EMPTY : moneyFmt.format(v))
export const formatCompactMoney = (v: Num) => (v == null ? EMPTY : `${compactFmt.format(v)} ₽`)
export const formatNumber = (v: Num) => (v == null ? EMPTY : intFmt.format(v))
export const formatPercent = (v: Num) => (v == null ? EMPTY : percentFmt.format(v))

/** relative: 0.123 → «+12,3 %»; points: 0.012 → «+1,2 п.п.» */
export function formatDelta(v: Num, kind: 'relative' | 'points' = 'relative') {
  if (v == null) return EMPTY
  const sign = v > 0 ? '+' : v < 0 ? '−' : ''
  const abs = Math.abs(v * 100).toLocaleString('ru-RU', { maximumFractionDigits: 1 })
  return kind === 'points' ? `${sign}${abs} п.п.` : `${sign}${abs} %`
}

/** DateOnly с backend (YYYY-MM-DD) → «29 сент.» */
export const formatDate = (iso: string, pattern = 'd MMM') => format(parseISO(iso), pattern, { locale: ru })

export function formatRange(from: string, to: string) {
  return from === to ? formatDate(from, 'd MMM yyyy') : `${formatDate(from)} – ${formatDate(to, 'd MMM yyyy')}`
}

/** Момент продажи (UTC) → время бизнеса, как и на сервере */
export const formatDateTime = (isoUtc: string) => dateTimeFmt.format(new Date(isoUtc))

export function initials(fullName: string) {
  return fullName
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((w) => w[0]!.toUpperCase())
    .join('')
}