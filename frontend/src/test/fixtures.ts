import type { ManagerRating, Metric, Period, Summary, TopProductsResponse } from '@/api/types'

export function makePeriod(from: string, to: string, previousFrom: string, previousTo: string): Period {
  return { from, to, previousFrom, previousTo, granularity: 'day' }
}

const metric = (value: number): Metric => ({ value, previous: value * 0.8, change: 0.25 })

export function makeSummary(period: Period): Summary {
  return {
    period,
    revenue: metric(1_000_000),
    grossProfit: metric(250_000),
    margin: { value: 0.25, previous: 0.24, change: 0.01 },
    salesCount: metric(10),
    averageCheck: metric(100_000),
    refunds: { count: 0, amount: 0, previousCount: 0, previousAmount: 0 },
    cancelledCount: 0,
    bestManager: null,
  }
}

export function makeRatingRow(managerId: number, fullName: string, rank: number): ManagerRating {
  return {
    rank,
    rankChange: 0,
    managerId,
    fullName,
    team: 'Малый бизнес',
    position: 'Менеджер',
    avatarUrl: null,
    isActive: true,
    salesCount: 10,
    revenue: 1_000_000,
    grossProfit: 200_000,
    averageCheck: 100_000,
    margin: 0.2,
    grossProfitChange: 0.1,
    averageCheckChange: 0.05,
  }
}

export function makeTopProducts(): TopProductsResponse {
  return {
    period: makePeriod('2026-09-01', '2026-09-30', '2026-08-02', '2026-08-31'),
    items: [
      {
        productId: 1,
        name: 'DJI Mavic 3 Pro',
        categoryName: 'Дроны',
        units: 3,
        revenue: 750_000,
        grossProfit: 150_000,
        margin: 0.2,
      },
    ],
  }
}