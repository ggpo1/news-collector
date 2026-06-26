import { UNCATEGORIZED_FILTER } from '../components/category-select/category-select';
import type { NewsToneFilterValue } from '../components/tone-select/tone-select';
import { PATHS } from './paths';

const TONE_FILTER_VALUES: NewsToneFilterValue[] = [
  'positive',
  'negative',
  'neutral',
  'strong',
  'unanalyzed',
];

export interface NewsRouteState {
  page: number;
  sourceId: string | null;
  categoryId: string | null;
  uncategorized: boolean;
  tone: NewsToneFilterValue | null;
  newsId: string | null;
}

export function parseNewsPageParam(pageParam: string | undefined): number | null {
  if (!pageParam) {
    return null;
  }

  const page = Number.parseInt(pageParam, 10);
  if (!Number.isFinite(page) || page < 1) {
    return null;
  }

  return page;
}

export function parseNewsToneParam(value: string | null): NewsToneFilterValue | null {
  if (!value) {
    return null;
  }

  return TONE_FILTER_VALUES.includes(value as NewsToneFilterValue)
    ? (value as NewsToneFilterValue)
    : null;
}

export function parseNewsRoute(
  pageParam: string | undefined,
  searchParams: URLSearchParams,
): NewsRouteState {
  const categoryParam = searchParams.get('categoryId');
  const uncategorized = categoryParam === UNCATEGORIZED_FILTER;

  return {
    page: parseNewsPageParam(pageParam) ?? 1,
    sourceId: searchParams.get('sourceId'),
    categoryId: uncategorized || !categoryParam ? null : categoryParam,
    uncategorized,
    tone: parseNewsToneParam(searchParams.get('tone')),
    newsId: searchParams.get('newsId'),
  };
}

export function buildNewsPath(state: NewsRouteState): string {
  const page = state.page > 1 ? state.page : 1;
  const params = new URLSearchParams();

  if (state.sourceId) {
    params.set('sourceId', state.sourceId);
  }

  if (state.uncategorized) {
    params.set('categoryId', UNCATEGORIZED_FILTER);
  } else if (state.categoryId) {
    params.set('categoryId', state.categoryId);
  }

  if (state.tone) {
    params.set('tone', state.tone);
  }

  if (state.newsId) {
    params.set('newsId', state.newsId);
  }

  const query = params.toString();
  const base = `${PATHS.news}/${page}`;

  return query ? `${base}?${query}` : base;
}

export function isNewsSectionPath(pathname: string): boolean {
  return pathname === PATHS.news || pathname.startsWith(`${PATHS.news}/`);
}
