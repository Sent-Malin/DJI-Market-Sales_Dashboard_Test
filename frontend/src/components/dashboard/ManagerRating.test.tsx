import { screen, waitFor, within } from '@testing-library/react'
import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'
import type { ManagerRatingResponse } from '@/api/types'
import { makePeriod, makeRatingRow } from '@/test/fixtures'
import { server } from '@/test/server'
import { renderWithProviders } from '@/test/utils'
import { ManagerRating } from './ManagerRating'

const period = makePeriod('2026-09-01', '2026-09-30', '2026-08-02', '2026-08-31')

const byProfit: ManagerRatingResponse = {
  period,
  sortBy: 'grossProfit',
  items: [makeRatingRow(1, 'Анна Соколова', 1), makeRatingRow(2, 'Игорь Петров', 2)],
}

const byCheck: ManagerRatingResponse = {
  period,
  sortBy: 'averageCheck',
  items: [makeRatingRow(2, 'Игорь Петров', 1), makeRatingRow(1, 'Анна Соколова', 2)],
}

const firstRowName = () => within(screen.getAllByRole('row')[1]).getByTitle(/./).getAttribute('title')

describe('Переключение рейтинга', () => {
  it('запрашивает рейтинг по среднему чеку, сохраняет режим в URL и меняет порядок', async () => {
    const requested: (string | null)[] = []
    server.use(
      http.get('*/api/dashboard/managers', ({ request }) => {
        const sortBy = new URL(request.url).searchParams.get('sortBy')
        requested.push(sortBy)
        return HttpResponse.json(sortBy === 'averageCheck' ? byCheck : byProfit)
      }),
    )

    const { user } = renderWithProviders(<ManagerRating period={{ period: '30d' }} />)

    await screen.findByText('Анна Соколова')
    expect(firstRowName()).toBe('Анна Соколова')

    await user.click(screen.getByText('Средний чек'))

    await waitFor(() => expect(firstRowName()).toBe('Игорь Петров'))
    expect(window.location.search).toContain('rank=averageCheck')
    expect(requested).toEqual(['grossProfit', 'averageCheck'])
  })
})