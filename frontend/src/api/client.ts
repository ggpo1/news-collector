import type { ArticleEnrichmentResult, NewsItemDetail, NewsItemList, PagedResult, Source } from './types';

const API_BASE = import.meta.env.VITE_API_BASE ?? '';

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE}${path}`, init);

  if (!response.ok) {
    throw new Error(`API error ${response.status}: ${response.statusText}`);
  }

  return response.json() as Promise<T>;
}

export function getSources(): Promise<Source[]> {
  return request<Source[]>('/api/sources');
}

export function getNews(params: {
  page: number;
  pageSize: number;
  sourceId?: string;
  hasContent?: boolean;
}): Promise<PagedResult<NewsItemList>> {
  const search = new URLSearchParams({
    page: String(params.page),
    pageSize: String(params.pageSize),
  });

  if (params.sourceId) {
    search.set('sourceId', params.sourceId);
  }

  if (params.hasContent !== undefined) {
    search.set('hasContent', String(params.hasContent));
  }

  return request<PagedResult<NewsItemList>>(`/api/news?${search}`);
}

export function getNewsById(id: string): Promise<NewsItemDetail> {
  return request<NewsItemDetail>(`/api/news/${id}`);
}

export function enrichNewsContent(id: string): Promise<ArticleEnrichmentResult> {
  return request<ArticleEnrichmentResult>(`/api/news/${id}/enrich-content`, {
    method: 'POST',
  });
}
