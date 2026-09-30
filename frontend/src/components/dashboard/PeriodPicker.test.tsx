import { screen, waitFor } from '@testing-library/react'
import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'
import { usePeriod } from '@/hooks/useDashboardParams'
import { makePeriod, makeSummary } from '@/test/fixtures'
import { server } from '@/test/server'
import { renderWithProviders } from '@/test/utils'
import { DashboardHeader } from './DashboardHeader'

function HeaderHarness() {
  const { period, setPeriod } = usePeriod()
  return <DashboardHeader period={period} onPeriodChange={setPeriod} />
}

describe('Смена периода', () => {
  it('обновляет URL, запрашивает данные за новый период и показывает его даты', async () => {
    const requested: string[] = []
    server.use(
      http.get('*/api/dashboard/summary', ({ request }) => {
        const period = new URL(request.url).searchParams.get('period') ?? ''
        requested.push(period)
        return HttpResponse.json(
          period === '7d'
            ? makeSummary(makePeriod('2026-09-24', '2026-09-30', '2026-09-17', '2026-09-23'))
            : makeSummary(makePeriod('2026-09-01', '2026-09-30', '2026-08-02', '2026-08-31')),
        )
      }),
    )

    const { user } = renderWithProviders(<HeaderHarness />)

    // По умолчанию — 30 дней
    expect(await screen.findByText(/^1 сент\. – 30 сент\. 2026/)).toBeInTheDocument()
    expect(requested).toEqual(['30d'])

    await user.click(screen.getByText('7 дней'))

    await waitFor(() => expect(window.location.search).toBe('?period=7d'))
    expect(await screen.findByText(/^24 сент\. – 30 сент\. 2026/)).toBeInTheDocument()
    expect(requested).toEqual(['30d', '7d'])
  })
})