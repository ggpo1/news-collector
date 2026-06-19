export type SourceType = 'Rss' | 'Html' | 'Api';

export interface Source {
  id: string;
  name: string;
  type: SourceType | number;
  url: string;
  isActive: boolean;
  fetchIntervalMinutes: number;
  lastFetchedAt: string | null;
  contentFetchEnabled: boolean;
  contentSelector: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateSourcePayload {
  name: string;
  type: SourceType;
  url: string;
  isActive: boolean;
  fetchIntervalMinutes: number;
  contentFetchEnabled: boolean;
  contentSelector: string | null;
}

export interface UpdateSourcePayload extends CreateSourcePayload {}

export interface NewsItemList {
  id: string;
  sourceId: string;
  sourceName: string;
  title: string;
  summary: string | null;
  url: string;
  publishedAt: string | null;
  fetchedAt: string;
  hasContent: boolean;
}

export interface NewsItemDetail {
  id: string;
  sourceId: string;
  sourceName: string;
  externalId: string;
  title: string;
  summary: string | null;
  content: string | null;
  url: string;
  publishedAt: string | null;
  fetchedAt: string;
  contentFetchedAt: string | null;
  contentHash: string | null;
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface ArticleEnrichmentResult {
  status: 'enriched' | 'already_enriched' | 'not_supported' | 'failed' | 'not_found';
  item: NewsItemDetail | null;
  message: string | null;
}

export type LinkType = 'SameTopic' | 'Duplicate' | 'Related';

export type LinkMethod = 'Manual' | 'TitleSimilarity' | 'Embedding';

export interface RelatedNews {
  linkId: string;
  linkType: LinkType;
  linkMethod: LinkMethod;
  confidence: number;
  news: NewsItemList;
}

export interface NewsLink {
  id: string;
  linkType: LinkType;
  linkMethod: LinkMethod;
  confidence: number;
  createdAt: string;
  newsLow: NewsItemList;
  newsHigh: NewsItemList;
}

export type UserRole = 'ChiefEditor' | 'Editor';

export interface CurrentUser {
  id: string;
  login: string;
  displayName: string;
  role: UserRole;
}

export interface LoginResponse {
  accessToken: string;
  user: CurrentUser;
}

export interface UserAccount {
  id: string;
  login: string;
  displayName: string;
  role: UserRole;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateUserPayload {
  login: string;
  password: string;
  displayName: string;
  role: UserRole;
}

export interface UpdateUserPayload {
  displayName: string;
  role: UserRole;
  isActive: boolean;
  password: string | null;
}

export interface ValidateInvitationResponse {
  role: UserRole;
}

export interface RegisterPayload {
  invitationCode: string;
  login: string;
  password: string;
  displayName: string;
}

export interface InvitationCode {
  code: string;
  role: UserRole;
  createdAt: string;
  createdByUserId: string;
  createdByLogin: string;
  usedAt: string | null;
  usedByUserId: string | null;
  usedByLogin: string | null;
}

export interface CreateInvitationCodePayload {
  role: UserRole;
}

export interface NewsRewrite {
  id: string;
  sourceNewsId: string;
  sourceNewsSourceId: string;
  sourceNewsSourceName: string;
  sourceNewsTitle: string;
  sourceNewsUrl: string;
  sourceNewsPublishedAt: string | null;
  authorId: string;
  authorLogin: string;
  authorDisplayName: string;
  title: string;
  summary: string | null;
  content: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateNewsRewritePayload {
  sourceNewsId: string;
  title: string;
  summary: string | null;
  content: string | null;
}

export interface UpdateNewsRewritePayload {
  title: string;
  summary: string | null;
  content: string | null;
}

export interface AiNewsRewriteResult {
  title: string;
  summary: string | null;
  content: string;
}
