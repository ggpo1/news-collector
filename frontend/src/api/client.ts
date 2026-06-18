import type {
  AiNewsRewriteResult,
  ArticleEnrichmentResult,
  CreateNewsRewritePayload,
  LinkType,
  NewsItemDetail,
  NewsItemList,
  NewsLink,
  NewsRewrite,
  PagedResult,
  RelatedNews,
  Source,
  UpdateNewsRewritePayload,
} from './types';
import { getVisitorId } from './visitor-id';

const API_BASE = import.meta.env.VITE_API_BASE ?? '';

async function readApiError(response: Response): Promise<string> {
  try {
    const body = (await response.json()) as { error?: string };
    if (body.error) {
      return body.error;
    }
  } catch {
    // Response body is not JSON.
  }

  return `API error ${response.status}: ${response.statusText}`;
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const headers = new Headers(init?.headers);
  headers.set('X-Visitor-Id', getVisitorId());

  const response = await fetch(`${API_BASE}${path}`, {
    ...init,
    headers,
  });

  if (!response.ok) {
    throw new Error(await readApiError(response));
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

export function aiRewriteNews(id: string): Promise<AiNewsRewriteResult> {
  return request<AiNewsRewriteResult>(`/api/news/${id}/ai-rewrite`, {
    method: 'POST',
  });
}

export function getRelatedNews(id: string): Promise<RelatedNews[]> {
  return request<RelatedNews[]>(`/api/news/${id}/related`);
}

export function getNewsLinks(params: {
  page: number;
  pageSize: number;
  linkType?: LinkType;
}): Promise<PagedResult<NewsLink>> {
  const search = new URLSearchParams({
    page: String(params.page),
    pageSize: String(params.pageSize),
  });

  if (params.linkType) {
    search.set('linkType', params.linkType);
  }

  return request<PagedResult<NewsLink>>(`/api/news-links?${search}`);
}

export function getNewsRewrites(params: {
  page: number;
  pageSize: number;
  sourceNewsId?: string;
}): Promise<PagedResult<NewsRewrite>> {
  const search = new URLSearchParams({
    page: String(params.page),
    pageSize: String(params.pageSize),
  });

  if (params.sourceNewsId) {
    search.set('sourceNewsId', params.sourceNewsId);
  }

  return request<PagedResult<NewsRewrite>>(`/api/news-rewrites?${search}`);
}

export function createNewsRewrite(payload: CreateNewsRewritePayload): Promise<NewsRewrite> {
  return request<NewsRewrite>('/api/news-rewrites', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
}

export function updateNewsRewrite(id: string, payload: UpdateNewsRewritePayload): Promise<NewsRewrite> {
  return request<NewsRewrite>(`/api/news-rewrites/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
}

export async function deleteNewsRewrite(id: string): Promise<void> {
  const headers = new Headers();
  headers.set('X-Visitor-Id', getVisitorId());

  const response = await fetch(`${API_BASE}/api/news-rewrites/${id}`, {
    method: 'DELETE',
    headers,
  });

  if (!response.ok) {
    throw new Error(await readApiError(response));
  }
}
