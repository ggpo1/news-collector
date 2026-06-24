import { clearAuthToken, getAuthToken } from './auth-storage';
import type {
  AiNewsRewriteResult,
  ArticleEnrichmentResult,
  Category,
  CreateNewsRewritePayload,
  CreateSourcePayload,
  CreateTelegramBotPayload,
  CreateTelegramChannelPayload,
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
  SecondDayAngles,
  Source,
  TelegramBot,
  TelegramChannel,
  TelegramSendResult,
  TelegramDelivery,
  EditorialDashboard,
  UpdateNewsRewritePayload,
  UpdateSourcePayload,
  UpdateTelegramBotPayload,
  UpdateTelegramChannelPayload,
  UpdateUserPayload,
  UserAccount,
} from './types';
import type { EntityGraph, NamedEntityDetail } from './entities';
import { getVisitorId } from './visitor-id';

const API_BASE = import.meta.env.VITE_API_BASE ?? '';

type UnauthorizedHandler = () => void;

let unauthorizedHandler: UnauthorizedHandler | null = null;

export function setUnauthorizedHandler(handler: UnauthorizedHandler | null) {
  unauthorizedHandler = handler;
}

async function readApiError(response: Response): Promise<string> {
  if (response.status === 504) {
    return 'Превышено время ожидания ответа (504). Модель ещё генерирует текст — попробуйте снова или выберите модель полегче (например llama3.2).';
  }

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

export function deleteInvitationCode(code: string): Promise<void> {
  return requestVoid(`/api/invitation-codes/${code}`, { method: 'DELETE' });
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

export function deleteUser(id: string): Promise<void> {
  return requestVoid(`/api/users/${id}`, { method: 'DELETE' });
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

export function getCategories(): Promise<Category[]> {
  return request<Category[]>('/api/categories');
}

export function getNews(params: {
  page: number;
  pageSize: number;
  sourceId?: string;
  categoryId?: string;
  uncategorized?: boolean;
  hasContent?: boolean;
  toneFilter?: string;
}): Promise<PagedResult<NewsItemList>> {
  const search = new URLSearchParams({
    page: String(params.page),
    pageSize: String(params.pageSize),
  });

  if (params.sourceId) {
    search.set('sourceId', params.sourceId);
  }

  if (params.categoryId) {
    search.set('categoryId', params.categoryId);
  }

  if (params.uncategorized) {
    search.set('uncategorized', 'true');
  }

  if (params.hasContent !== undefined) {
    search.set('hasContent', String(params.hasContent));
  }

  if (params.toneFilter) {
    search.set('toneFilter', params.toneFilter);
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

export function generateSecondDayAngles(id: string): Promise<SecondDayAngles> {
  return request<SecondDayAngles>(`/api/news/${id}/second-day-angles`, {
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

export function getEntityGraph(params: {
  from?: string;
  to?: string;
  type?: string;
  minWeight?: number;
  maxNodes?: number;
}): Promise<EntityGraph> {
  const search = new URLSearchParams();

  if (params.from) {
    search.set('from', params.from);
  }

  if (params.to) {
    search.set('to', params.to);
  }

  if (params.type) {
    search.set('type', params.type);
  }

  if (params.minWeight !== undefined) {
    search.set('minWeight', String(params.minWeight));
  }

  if (params.maxNodes !== undefined) {
    search.set('maxNodes', String(params.maxNodes));
  }

  const query = search.toString();
  return request<EntityGraph>(query ? `/api/entities/graph?${query}` : '/api/entities/graph');
}

export function getEntityDetail(
  id: string,
  params?: { from?: string; to?: string },
): Promise<NamedEntityDetail> {
  const search = new URLSearchParams();

  if (params?.from) {
    search.set('from', params.from);
  }

  if (params?.to) {
    search.set('to', params.to);
  }

  const query = search.toString();
  return request<NamedEntityDetail>(query ? `/api/entities/${id}?${query}` : `/api/entities/${id}`);
}

export function getTelegramBots(): Promise<TelegramBot[]> {
  return request<TelegramBot[]>('/api/telegram/bots');
}

export function createTelegramBot(payload: CreateTelegramBotPayload): Promise<TelegramBot> {
  return request<TelegramBot>('/api/telegram/bots', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
}

export function updateTelegramBot(id: string, payload: UpdateTelegramBotPayload): Promise<TelegramBot> {
  return request<TelegramBot>(`/api/telegram/bots/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
}

export function deleteTelegramBot(id: string): Promise<void> {
  return requestVoid(`/api/telegram/bots/${id}`, { method: 'DELETE' });
}

export function restartTelegramBot(id: string): Promise<TelegramBot> {
  return request<TelegramBot>(`/api/telegram/bots/${id}/restart`, { method: 'POST' });
}

export function getTelegramChannels(params?: {
  sourceId?: string;
  categoryId?: string;
}): Promise<TelegramChannel[]> {
  const search = new URLSearchParams();
  if (params?.sourceId) {
    search.set('sourceId', params.sourceId);
  }
  if (params?.categoryId) {
    search.set('categoryId', params.categoryId);
  }
  const query = search.toString();
  return request<TelegramChannel[]>(query ? `/api/telegram/channels?${query}` : '/api/telegram/channels');
}

export function createTelegramChannel(payload: CreateTelegramChannelPayload): Promise<TelegramChannel> {
  return request<TelegramChannel>('/api/telegram/channels', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
}

export function updateTelegramChannel(
  id: string,
  payload: UpdateTelegramChannelPayload,
): Promise<TelegramChannel> {
  return request<TelegramChannel>(`/api/telegram/channels/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
}

export function deleteTelegramChannel(id: string): Promise<void> {
  return requestVoid(`/api/telegram/channels/${id}`, { method: 'DELETE' });
}

export function sendNewsToTelegram(newsId: string, channelId: string): Promise<TelegramSendResult> {
  return request<TelegramSendResult>(`/api/telegram/news/${newsId}/send`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ channelId }),
  });
}

export function sendRewriteToTelegram(rewriteId: string, channelId: string): Promise<TelegramSendResult> {
  return request<TelegramSendResult>(`/api/telegram/rewrites/${rewriteId}/send`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ channelId }),
  });
}

export function getTelegramDelivery(deliveryId: string): Promise<TelegramDelivery> {
  return request<TelegramDelivery>(`/api/telegram/deliveries/${deliveryId}`);
}

export function getEditorialDashboard(windowHours?: number): Promise<EditorialDashboard> {
  const params = new URLSearchParams();
  if (windowHours !== undefined) {
    params.set('windowHours', String(windowHours));
  }

  const query = params.toString();
  return request<EditorialDashboard>(`/api/editorial/dashboard${query ? `?${query}` : ''}`);
}
