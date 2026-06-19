import { clearAuthToken, getAuthToken } from './auth-storage';
import type {
  AiNewsRewriteResult,
  ArticleEnrichmentResult,
  CreateNewsRewritePayload,
  CreateSourcePayload,
  CreateUserPayload,
  CurrentUser,
  LinkType,
  LoginResponse,
  InvitationCode,
  CreateInvitationCodePayload,
  RegisterPayload,
  ValidateInvitationResponse,
  NewsItemDetail,
  NewsItemList,
  NewsLink,
  NewsRewrite,
  PagedResult,
  RelatedNews,
  Source,
  UpdateNewsRewritePayload,
  UpdateSourcePayload,
  UpdateUserPayload,
  UserAccount,
} from './types';
import { getVisitorId } from './visitor-id';

const API_BASE = import.meta.env.VITE_API_BASE ?? '';

type UnauthorizedHandler = () => void;

let unauthorizedHandler: UnauthorizedHandler | null = null;

export function setUnauthorizedHandler(handler: UnauthorizedHandler | null) {
  unauthorizedHandler = handler;
}

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

function buildHeaders(init?: RequestInit): Headers {
  const headers = new Headers(init?.headers);
  headers.set('X-Visitor-Id', getVisitorId());

  const token = getAuthToken();
  if (token) {
    headers.set('Authorization', `Bearer ${token}`);
  }

  return headers;
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE}${path}`, {
    ...init,
    headers: buildHeaders(init),
  });

  if (response.status === 401) {
    unauthorizedHandler?.();
    throw new Error('Требуется авторизация');
  }

  if (!response.ok) {
    throw new Error(await readApiError(response));
  }

  return response.json() as Promise<T>;
}

async function requestVoid(path: string, init?: RequestInit): Promise<void> {
  const response = await fetch(`${API_BASE}${path}`, {
    ...init,
    headers: buildHeaders(init),
  });

  if (response.status === 401) {
    unauthorizedHandler?.();
    throw new Error('Требуется авторизация');
  }

  if (!response.ok) {
    throw new Error(await readApiError(response));
  }
}

export function login(loginName: string, password: string): Promise<LoginResponse> {
  return fetch(`${API_BASE}/api/auth/login`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'X-Visitor-Id': getVisitorId(),
    },
    body: JSON.stringify({ login: loginName, password }),
  }).then(async (response) => {
    if (!response.ok) {
      throw new Error(await readApiError(response));
    }

    return response.json() as Promise<LoginResponse>;
  });
}

async function postAnonymous<T>(path: string, body: unknown): Promise<T> {
  const response = await fetch(`${API_BASE}${path}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'X-Visitor-Id': getVisitorId(),
    },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    throw new Error(await readApiError(response));
  }

  return response.json() as Promise<T>;
}

export function validateInvitation(code: string): Promise<ValidateInvitationResponse> {
  return postAnonymous<ValidateInvitationResponse>('/api/auth/validate-invitation', { code });
}

export function register(payload: RegisterPayload): Promise<LoginResponse> {
  return postAnonymous<LoginResponse>('/api/auth/register', payload);
}

export function getInvitationCodes(): Promise<InvitationCode[]> {
  return request<InvitationCode[]>('/api/invitation-codes');
}

export function createInvitationCode(payload: CreateInvitationCodePayload): Promise<InvitationCode> {
  return request<InvitationCode>('/api/invitation-codes', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
}

export function getCurrentUser(): Promise<CurrentUser> {
  return request<CurrentUser>('/api/auth/me');
}

export function logoutLocally() {
  clearAuthToken();
}

export function getUsers(): Promise<UserAccount[]> {
  return request<UserAccount[]>('/api/users');
}

export function createUser(payload: CreateUserPayload): Promise<UserAccount> {
  return request<UserAccount>('/api/users', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
}

export function updateUser(id: string, payload: UpdateUserPayload): Promise<UserAccount> {
  return request<UserAccount>(`/api/users/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
}

export function getSources(): Promise<Source[]> {
  return request<Source[]>('/api/sources');
}

export function createSource(payload: CreateSourcePayload): Promise<Source> {
  return request<Source>('/api/sources', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
}

export function updateSource(id: string, payload: UpdateSourcePayload): Promise<Source> {
  return request<Source>(`/api/sources/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
}

export function deleteSource(id: string): Promise<void> {
  return requestVoid(`/api/sources/${id}`, { method: 'DELETE' });
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

export function deleteNewsRewrite(id: string): Promise<void> {
  return requestVoid(`/api/news-rewrites/${id}`, { method: 'DELETE' });
}
