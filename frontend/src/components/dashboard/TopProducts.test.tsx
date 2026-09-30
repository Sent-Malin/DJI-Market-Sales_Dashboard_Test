import { screen } from '@testing-library/react'
import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'
import { makeTopProducts } from '@/test/fixtures'
import { server } from '@/test/server'
import { renderWithProviders } from '@/test/utils'
import { TopProducts } from './TopProducts'

describe('Состояние ошибки API', () => {
  it('показывает ошибку сервера и восстанавливается по кнопке «Повторить»', async () => {
    let fail = true
    server.use(
      http.get('*/api/dashboard/products/top', () =>
        fail
          ? HttpResponse.json({ title: 'Internal Server Error', status: 500 }, { status: 500 })
          : HttpResponse.json(makeTopProducts()),
      ),
    )

    const { user } = renderWithProviders(<TopProducts period={{ period: '30d' }} />)

    expect(await screen.findByText('Не удалось загрузить данные')).toBeInTheDocument()
    expect(screen.getByText(/Ошибка на сервере/)).toBeInTheDocument()

    fail = false
    await user.click(screen.getByRole('button', { name: /Повторить/ }))

    expect(await screen.findByText('DJI Mavic 3 Pro')).toBeInTheDocument()
    expect(screen.queryByText('Не удалось загрузить данные')).not.toBeInTheDocument()
  })
})