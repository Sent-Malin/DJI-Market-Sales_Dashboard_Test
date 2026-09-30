export type Granularity = 'day' | 'week' | 'month'
export type SaleStatus = 'paid' | 'cancelled' | 'refunded'
export type RatingMetric = 'grossProfit' | 'averageCheck'

export type Period = {
  from: string          // YYYY-MM-DD, включительно
  to: string
  previousFrom: string
  previousTo: string
  granularity: Granularity
}

/** change: относительное изменение (0.15 = +15%); для маржи — абсолютная разница долей */
export type Metric = {
  value: number | null
  previous: number | null
  change: number | null
}

export type Summary = {
  period: Period
  revenue: Metric
  grossProfit: Metric
  margin: Metric
  salesCount: Metric
  averageCheck: Metric
  refunds: { count: number; amount: number; previousCount: number; previousAmount: number }
  cancelledCount: number
  bestManager: {
    id: number
    fullName: string
    avatarUrl: string | null
    grossProfit: number
    change: number | null
  } | null
}

export type ManagerRating = {
  rank: number | null
  rankChange: number | null
  managerId: number
  fullName: string
  team: string
  position: string
  avatarUrl: string | null
  isActive: boolean
  salesCount: number
  revenue: number
  grossProfit: number
  averageCheck: number | null
  margin: number | null
  grossProfitChange: number | null
  averageCheckChange: number | null
}

export type ManagerRatingResponse = { period: Period; sortBy: RatingMetric; items: ManagerRating[] }

export type TimeseriesPoint = { date: string; revenue: number; grossProfit: number; salesCount: number }
export type TimeseriesResponse = { period: Period; points: TimeseriesPoint[] }

export type CategoryStats = {
  categoryId: number
  name: string
  units: number
  revenue: number
  grossProfit: number
  margin: number | null
  share: number
}
export type CategoriesResponse = { period: Period; items: CategoryStats[] }

export type ProductStats = {
  productId: number
  name: string
  categoryName: string
  units: number
  revenue: number
  grossProfit: number
  margin: number | null
}
export type TopProductsResponse = { period: Period; items: ProductStats[] }

export type RecentSale = {
  id: number
  soldAt: string        // ISO UTC
  status: SaleStatus
  managerId: number
  managerName: string
  customerName: string
  customerCompany: string
  items: { productName: string; quantity: number; unitPrice: number }[]
  amount: number
  grossProfit: number
}
export type RecentSalesResponse = { period: Period; items: RecentSale[] }