import { useCallback, useMemo, useSyncExternalStore } from 'react'

const URL_CHANGE_EVENT = 'urlstatechange'

function subscribe(onChange: () => void) {
  window.addEventListener('popstate', onChange)        // кнопки «назад/вперёд»
  window.addEventListener(URL_CHANGE_EVENT, onChange)  // наши pushState
  return () => {
    window.removeEventListener('popstate', onChange)
    window.removeEventListener(URL_CHANGE_EVENT, onChange)
  }
}

const getSearch = () => window.location.search

/** Query-string как источник состояния. pushState → «назад» возвращает прошлый фильтр. */
export function useUrlState() {
  const search = useSyncExternalStore(subscribe, getSearch)
  const params = useMemo(() => new URLSearchParams(search), [search])

  const update = useCallback((patch: Record<string, string | undefined>) => {
    const next = new URLSearchParams(window.location.search)
    for (const [key, value] of Object.entries(patch)) {
      if (value === undefined) next.delete(key)
      else next.set(key, value)
    }
    const qs = next.toString()
    if (qs === window.location.search.slice(1)) return
    window.history.pushState(null, '', qs ? `?${qs}` : window.location.pathname)
    window.dispatchEvent(new Event(URL_CHANGE_EVENT))
  }, [])

  return [params, update] as const
}