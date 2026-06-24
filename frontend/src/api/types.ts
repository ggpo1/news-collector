export type SourceType = 'Rss' | 'Html' | 'Api';

export interface Category {
  id: string;
  slug: string;
  name: string;
  sortOrder: number;
}

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
  categoryName: string | null;
  toneCoefficient: number | null;
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
  categoryId: string | null;
  categoryName: string | null;
  toneCoefficient: number | null;
  toneAnalyzedAt: string | null;
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

export interface SecondDayAngles {
  newsId: string;
  newsTitle: string;
  historicalParallels: HistoricalParallel[];
  stakeholders: StakeholderAngle[];
  numberContradictions: NumberContradiction[];
  suggestedAngles: SuggestedAngle[];
}

export interface HistoricalParallel {
  newsId: string;
  title: string;
  sourceName: string;
  publishedAt: string | null;
  parallelSummary: string;
  structuralSimilarity: string;
}

export interface StakeholderAngle {
  name: string;
  entityType: string;
  role: string;
  reason: string;
}

export interface NumberContradiction {
  metric: string;
  values: SourceValue[];
  factCheckAngle: string;
}

export interface SourceValue {
  sourceName: string;
  value: string;
}

export interface SuggestedAngle {
  title: string;
  rationale: string;
  angleType: string;
}

export interface TelegramBot {
  id: string;
  name: string;
  botTokenMasked: string;
  isActive: boolean;
  containerStatus: string;
  containerName: string | null;
  containerError: string | null;
  channelCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface TelegramChannel {
  id: string;
  telegramBotId: string;
  botName: string;
  name: string;
  chatId: string;
  isActive: boolean;
  categoryIds: string[];
  sourceIds: string[];
  categoryNames: string[];
  sourceNames: string[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateTelegramBotPayload {
  name: string;
  botToken: string;
  isActive: boolean;
}

export interface UpdateTelegramBotPayload {
  name: string;
  botToken?: string | null;
  isActive: boolean;
}

export interface CreateTelegramChannelPayload {
  telegramBotId: string;
  name: string;
  chatId: string;
  isActive: boolean;
  categoryIds: string[];
  sourceIds: string[];
}

export interface UpdateTelegramChannelPayload extends CreateTelegramChannelPayload {}

export interface TelegramSendResult {
  deliveryId: string;
  status: string;
  message: string;
}
