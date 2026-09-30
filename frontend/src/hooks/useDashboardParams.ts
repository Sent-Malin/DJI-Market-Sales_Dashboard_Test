import { useCallback, useMemo } from 'react'
import type { RatingMetric } from '@/api/types'
import { DEFAULT_PERIOD, isIsoDate, isPreset, type PeriodParams } from '@/lib/period'
import { useUrlState } from './useUrlState'

export function usePeriod() {
  const [params, update] = useUrlState()

  const period = useMemo<PeriodParams>(() => {
    const raw = params.get('period')
    if (!isPreset(raw)) return DEFAULT_PERIOD
    if (raw !== 'custom') return { period: raw }

    const from = params.get('from')
    const to = params.get('to')
    // битый custom в URL → безопасный дефолт, а не 400 на каждом блоке
    return isIsoDate(from) && isIsoDate(to) ? { period: 'custom', from, to } : DEFAULT_PERIOD
  }, [params])

  const setPeriod = useCallback(
    (next: PeriodParams) =>
      update({
        period: next.period,
        from: next.period === 'custom' ? next.from : undefined,
        to: next.period === 'custom' ? next.to : undefined,
      }),
    [update],
  )

  return { period, setPeriod }
}

export function useRatingMetric() {
  const [params, update] = useUrlState()
  const metric: RatingMetric = params.get('rank') === 'averageCheck' ? 'averageCheck' : 'grossProfit'
  const setMetric = useCallback((m: RatingMetric) => update({ rank: m }), [update])
  return { metric, setMetric }
}