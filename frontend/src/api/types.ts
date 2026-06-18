export type SourceType = 'Rss' | 'Html' | 'Api';

export interface Source {
  id: string;
  name: string;
  type: SourceType | number;
  url: string;
  isActive: boolean;
  fetchIntervalMinutes: number;
  lastFetchedAt: string | null;
  createdAt: string;
  updatedAt: string;
}

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

export interface NewsRewrite {
  id: string;
  sourceNewsId: string;
  sourceNewsSourceId: string;
  sourceNewsSourceName: string;
  sourceNewsTitle: string;
  sourceNewsUrl: string;
  sourceNewsPublishedAt: string | null;
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
