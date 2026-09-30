import { keepPreviousData, useQuery } from '@tanstack/react-query'
import type { PeriodParams } from '@/lib/period'
import { apiGet } from './client'
import type {
  CategoriesResponse,
  ManagerRatingResponse,
  RatingMetric,
  RecentSalesResponse,
  Summary,
  TimeseriesResponse,
  TopProductsResponse,
} from './types'

// Период входит в queryKey → смена периода = новый запрос, старый ответ остаётся в кэше.
// keepPreviousData: пока грузится новый период, показываем старые данные, а не skeleton.

export function useSummary(period: PeriodParams) {
  return useQuery({
    queryKey: ['summary', period],
    queryFn: ({ signal }) => apiGet<Summary>('/dashboard/summary', period, signal),
    placeholderData: keepPreviousData,
  })
}

export function useManagerRating(period: PeriodParams, sortBy: RatingMetric) {
  return useQuery({
    queryKey: ['managers', period, sortBy],
    queryFn: ({ signal }) =>
      apiGet<ManagerRatingResponse>('/dashboard/managers', { ...period, sortBy }, signal),
    placeholderData: keepPreviousData,
  })
}

export function useTimeseries(period: PeriodParams) {
  return useQuery({
    queryKey: ['timeseries', period],
    queryFn: ({ signal }) => apiGet<TimeseriesResponse>('/dashboard/timeseries', period, signal),
    placeholderData: keepPreviousData,
  })
}

export function useCategories(period: PeriodParams) {
  return useQuery({
    queryKey: ['categories', period],
    queryFn: ({ signal }) => apiGet<CategoriesResponse>('/dashboard/categories', period, signal),
    placeholderData: keepPreviousData,
  })
}

export function useTopProducts(period: PeriodParams, limit = 5) {
  return useQuery({
    queryKey: ['topProducts', period, limit],
    queryFn: ({ signal }) =>
      apiGet<TopProductsResponse>('/dashboard/products/top', { ...period, limit }, signal),
    placeholderData: keepPreviousData,
  })
}

export function useRecentSales(period: PeriodParams, limit = 10) {
  return useQuery({
    queryKey: ['recentSales', period, limit],
    queryFn: ({ signal }) =>
      apiGet<RecentSalesResponse>('/dashboard/sales/recent', { ...period, limit }, signal),
    placeholderData: keepPreviousData,
  })
}