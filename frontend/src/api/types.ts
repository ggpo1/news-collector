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

export interface EditorialTag {
  id: string;
  slug: string;
  name: string;
  color: string | null;
}

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
  editorialTags: EditorialTag[];
}

export interface NewsItemDetail {
  id: string;
  sourceId: string;
  sourceName: string;
  categoryId: string | null;
  categoryName: string | null;
  isCategoryManual: boolean;
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
  editorialTags: EditorialTag[];
  storyId: string | null;
}

export type StoryStatus = 'Monitoring' | 'InWork' | 'Published' | 'Closed';

export interface StoryListItem {
  id: string;
  clusterKey: string;
  title: string;
  status: StoryStatus;
  articleCount: number;
  sourceCount: number;
  firstSeenAt: string | null;
  lastActivityAt: string | null;
  updatedAt: string;
  sourceNames: string[];
}

export interface StoryTimelineItem {
  id: string;
  title: string;
  sourceName: string;
  categoryName: string | null;
  toneCoefficient: number | null;
  publishedAt: string | null;
  fetchedAt: string;
  hasContent: boolean;
  url: string;
  isPrimary: boolean;
}

export interface StoryEntity {
  id: string;
  name: string;
  type: string;
  mentionCount: number;
}

export interface StoryRewrite {
  id: string;
  sourceNewsId: string;
  sourceNewsTitle: string;
  title: string;
  authorName: string;
  updatedAt: string;
}

export interface StoryTelegramDelivery {
  id: string;
  channelName: string;
  status: string;
  createdAt: string;
  sentAt: string | null;
  newsItemId: string | null;
  newsRewriteId: string | null;
}

export interface StoryDetail {
  id: string;
  clusterKey: string;
  title: string;
  status: StoryStatus;
  articleCount: number;
  sourceCount: number;
  firstSeenAt: string | null;
  lastActivityAt: string | null;
  createdAt: string;
  updatedAt: string;
  primaryNewsItemId: string | null;
  timeline: StoryTimelineItem[];
  keyEntities: StoryEntity[];
  rewrites: StoryRewrite[];
  telegramDeliveries: StoryTelegramDelivery[];
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

export type LinkMethod = 'Manual' | 'TitleSimilarity' | 'Embedding' | 'EntityOverlap' | 'Hybrid';

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

export interface TelegramDelivery {
  id: string;
  channelId: string;
  channelName: string;
  status: string;
  errorMessage: string | null;
  createdAt: string;
  sentAt: string | null;
}

export interface EditorialBriefNews {
  id: string;
  title: string;
  sourceName: string;
  categoryName: string | null;
  toneCoefficient: number | null;
  publishedAt: string | null;
  fetchedAt: string;
  hasContent: boolean;
  url: string;
}

export interface DevelopingTopic {
  clusterKey: string;
  headline: string;
  sourceCount: number;
  articleCount: number;
  sourceNames: string[];
  primaryArticle: EditorialBriefNews;
  relatedArticles: EditorialBriefNews[];
}

export interface DuplicateGroup {
  clusterKey: string;
  headline: string;
  sourceCount: number;
  articleCount: number;
  hasDuplicateLink: boolean;
  sourceNames: string[];
  primaryArticle: EditorialBriefNews;
  relatedArticles: EditorialBriefNews[];
}

export interface EntitySpike {
  entityId: string;
  entityName: string;
  entityType: string;
  mentionsInWindow: number;
  mentionsInPreviousWindow: number;
  spikeRatio: number;
  recentArticles: EditorialBriefNews[];
}

export interface ToneHighlight {
  news: EditorialBriefNews;
  toneCoefficient: number;
  toneLabel: string;
}

export interface EditorialDashboardMeta {
  windowHours: number;
  windowStart: string;
  generatedAt: string;
  newsInWindow: number;
}

export interface EditorialDashboard {
  developingTopics: DevelopingTopic[];
  duplicateGroups: DuplicateGroup[];
  entitySpikes: EntitySpike[];
  toneHighlights: ToneHighlight[];
  meta: EditorialDashboardMeta;
}
