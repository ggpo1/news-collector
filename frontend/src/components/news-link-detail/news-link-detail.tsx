import { formatConfidence, LINK_METHOD_LABELS, LINK_TYPE_LABELS } from '../../api/link-labels';
import type { NewsItemList, NewsLink } from '../../api/types';
import * as S from './news-link-detail.styles';

interface NewsLinkDetailProps {
  link: NewsLink | null;
}

function formatDate(value: string | null): string {
  if (!value) {
    return '—';
  }

  return new Intl.DateTimeFormat('ru-RU', {
    dateStyle: 'long',
    timeStyle: 'short',
  }).format(new Date(value));
}

function ArticleCard({ item, label }: { item: NewsItemList; label: string }) {
  return (
    <S.Article>
      <S.ArticleMeta>
        <span>{label}</span>
        <span>{item.sourceName}</span>
        <time dateTime={item.publishedAt ?? undefined}>{formatDate(item.publishedAt)}</time>
        {item.hasContent && <span>полный текст</span>}
      </S.ArticleMeta>
      <S.ArticleTitle>{item.title}</S.ArticleTitle>
      {item.summary && <S.ArticleSummary>{item.summary}</S.ArticleSummary>}
      <S.ExternalLink href={item.url} target="_blank" rel="noreferrer">
        Открыть оригинал →
      </S.ExternalLink>
    </S.Article>
  );
}

export function NewsLinkDetail({ link }: NewsLinkDetailProps) {
  if (!link) {
    return (
      <S.Panel>
        <S.Placeholder>Выберите связь из списка</S.Placeholder>
      </S.Panel>
    );
  }

  return (
    <S.Panel>
      <S.Header>
        <S.Meta>
          <S.Badge>{LINK_TYPE_LABELS[link.linkType]}</S.Badge>
          <span>{LINK_METHOD_LABELS[link.linkMethod]}</span>
          <time dateTime={link.createdAt}>
            Создана{' '}
            {new Intl.DateTimeFormat('ru-RU', {
              dateStyle: 'long',
              timeStyle: 'short',
            }).format(new Date(link.createdAt))}
          </time>
        </S.Meta>
        <S.Title>Связанные новости</S.Title>
      </S.Header>

      <S.ConfidenceBar>
        <S.ConfidenceLabel>
          <span>Уверенность</span>
          <span>{formatConfidence(link.confidence)}</span>
        </S.ConfidenceLabel>
        <S.ConfidenceTrack>
          <S.ConfidenceFill $value={link.confidence} />
        </S.ConfidenceTrack>
      </S.ConfidenceBar>

      <S.Articles>
        <ArticleCard item={link.newsLow} label="Новость A" />
        <ArticleCard item={link.newsHigh} label="Новость B" />
      </S.Articles>
    </S.Panel>
  );
}
