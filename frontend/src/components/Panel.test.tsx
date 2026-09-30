import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { ApiError } from '@/api/client'
import { Panel } from './Panel'

describe('Panel', () => {
  it('при первой загрузке показывает skeleton вместо содержимого', () => {
    render(
      <Panel title="Блок" isLoading skeleton={<div>skeleton-marker</div>}>
        <div>content</div>
      </Panel>,
    )

    expect(screen.getByText('skeleton-marker')).toBeInTheDocument()
    expect(screen.queryByText('content')).not.toBeInTheDocument()
  })

  it('при ошибке показывает текст с сервера и вызывает повтор', async () => {
    const onRetry = vi.fn()
    render(
      <Panel title="Блок" isLoading={false} error={new ApiError(400, 'Дата to должна быть не раньше from.')} onRetry={onRetry}>
        <div>content</div>
      </Panel>,
    )

    expect(screen.getByRole('alert')).toHaveTextContent('Дата to должна быть не раньше from.')
    await userEvent.click(screen.getByRole('button', { name: /Повторить/ }))
    expect(onRetry).toHaveBeenCalledOnce()
  })

  it('для пустых данных показывает объяснение, а не пустую рамку', () => {
    render(
      <Panel title="Блок" isLoading={false} isEmpty emptyMessage="Нет продаж за период">
        <div>content</div>
      </Panel>,
    )

    expect(screen.getByText('Нет продаж за период')).toBeInTheDocument()
    expect(screen.queryByText('content')).not.toBeInTheDocument()
  })

  it('при обновлении оставляет старые данные и показывает индикатор', () => {
    render(
      <Panel title="Блок" isLoading={false} isFetching isStale>
        <div>content</div>
      </Panel>,
    )

    expect(screen.getByText('content')).toBeInTheDocument()
    expect(screen.getByLabelText('Обновление')).toBeInTheDocument()
    expect(screen.getByText('content').parentElement).toHaveClass('opacity-60')
  })
})