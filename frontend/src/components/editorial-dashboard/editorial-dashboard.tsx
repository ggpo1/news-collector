import { type ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import type { EditorialBriefNews, EditorialDashboard } from '../../api/types';
import { formatToneCoefficient } from '../../api/tone';
import { PATHS } from '../../app/paths';
import { LoadingState } from '../ui/loading-state';
import * as S from './editorial-dashboard.styles';

interface EditorialDashboardViewProps {
  dashboard: EditorialDashboard | null;
  loading: boolean;
  error: string | null;
  windowHours: 24 | 48 | 72;
  onWindowHoursChange: (hours: 24 | 48 | 72) => void;
  onReload: () => void;
}

function formatSources(names: string[]) {
  if (names.length <= 4) {
    return names.join(', ');
  }

  return `${names.slice(0, 4).join(', ')} +${names.length - 4}`;
}

function formatWhen(value: string | null, fallback: string) {
  if (!value) {
    return fallback;
  }

  return new Date(value).toLocaleString('ru-RU', {
    day: '2-digit',
    month: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  });
}

function NewsMeta({ item }: { item: EditorialBriefNews }) {
  const tone = formatToneCoefficient(item.toneCoefficient);
  return (
    <>
      {item.sourceName}
      {item.categoryName ? ` · ${item.categoryName}` : ''}
      {item.hasContent ? ' · полный текст' : ''}
      {tone ? ` · тон ${tone}` : ''}
      {' · '}
      {formatWhen(item.publishedAt, formatWhen(item.fetchedAt, 'без даты'))}
    </>
  );
}

export function EditorialDashboardView({
  dashboard,
  loading,
  error,
  windowHours,
  onWindowHoursChange,
  onReload,
}: EditorialDashboardViewProps) {
  const navigate = useNavigate();

  const openNews = (newsId: string) => {
    navigate(PATHS.news, { state: { newsId } });
  };

  return (
    <S.Page>
      <S.Toolbar>
        <S.ToolbarGroup>
          <S.Select
            value={windowHours}
            disabled={loading}
            onChange={(event) => onWindowHoursChange(Number(event.target.value) as 24 | 48 | 72)}
            aria-label="Окно анализа"
          >
            <option value={24}>24 часа</option>
            <option value={48}>48 часов</option>
            <option value={72}>72 часа</option>
          </S.Select>
          <S.RefreshButton type="button" disabled={loading} onClick={onReload}>
            Обновить
          </S.RefreshButton>
        </S.ToolbarGroup>
        {dashboard && (
          <S.MetaLine>
            {dashboard.meta.newsInWindow} материалов за {dashboard.meta.windowHours} ч · обновлено{' '}
            {formatWhen(dashboard.meta.generatedAt, '—')}
          </S.MetaLine>
        )}
      </S.Toolbar>

      {error && <S.ErrorText>{error}</S.ErrorText>}
      {loading && !dashboard ? <LoadingState label="Собираем редакционный обзор…" /> : null}

      {dashboard && (
        <S.Grid>
          <TopicSection
            title="Развивается тема"
            hint="3+ источника за выбранное окно, связаны по заголовку/смыслу"
            empty="Пока нет тем с тремя и более источниками. Topic-linker связывает материалы автоматически."
            items={dashboard.developingTopics}
            renderItem={(topic) => (
              <S.Card key={topic.clusterKey} type="button" onClick={() => openNews(topic.primaryArticle.id)}>
                <S.Badges>
                  <S.Badge $variant="hot">{topic.sourceCount} источника</S.Badge>
                  <S.Badge>{topic.articleCount} материала</S.Badge>
                </S.Badges>
                <S.CardTitle>{topic.headline}</S.CardTitle>
                <S.CardMeta>
                  Источники: {formatSources(topic.sourceNames)}
                  <br />
                  Лучший материал: <NewsMeta item={topic.primaryArticle} />
                </S.CardMeta>
                {topic.relatedArticles.length > 0 && (
                  <S.RelatedList>
                    {topic.relatedArticles.map((item) => (
                      <li key={item.id}>
                        {item.sourceName}: {item.title}
                      </li>
                    ))}
                  </S.RelatedList>
                )}
              </S.Card>
            )}
          />

          <TopicSection
            title="Дубликаты и перекрёстные публикации"
            hint="Связанные материалы (2+), лучший — с полным текстом и раньше по времени"
            empty="Нет сгруппированных дубликатов вне «развивающихся тем»."
            items={dashboard.duplicateGroups}
            renderItem={(group) => (
              <S.Card key={group.clusterKey} type="button" onClick={() => openNews(group.primaryArticle.id)}>
                <S.Badges>
                  {group.hasDuplicateLink && <S.Badge $variant="dup">дубликат</S.Badge>}
                  <S.Badge>{group.sourceCount} источника</S.Badge>
                  <S.Badge>{group.articleCount} материала</S.Badge>
                </S.Badges>
                <S.CardTitle>{group.headline}</S.CardTitle>
                <S.CardMeta>
                  Источники: {formatSources(group.sourceNames)}
                  <br />
                  Рекомендуем: <NewsMeta item={group.primaryArticle} />
                </S.CardMeta>
              </S.Card>
            )}
          />

          <TopicSection
            title="Всплески сущностей"
            hint="Частые упоминания персон/компаний vs предыдущий период"
            empty="Нет заметных всплесков. Нужны извлечённые сущности (entity-extractor)."
            items={dashboard.entitySpikes}
            renderItem={(spike) => {
              const firstArticle = spike.recentArticles[0];
              return (
              <S.Card
                key={spike.entityId}
                type="button"
                disabled={!firstArticle}
                onClick={() => firstArticle && openNews(firstArticle.id)}
              >
                <S.Badges>
                  <S.Badge $variant="entity">{spike.entityType}</S.Badge>
                  <S.Badge $variant="hot">×{spike.spikeRatio.toFixed(1)}</S.Badge>
                  <S.Badge>{spike.mentionsInWindow} упоминаний</S.Badge>
                </S.Badges>
                <S.CardTitle>{spike.entityName}</S.CardTitle>
                <S.CardMeta>
                  Было {spike.mentionsInPreviousWindow} → стало {spike.mentionsInWindow} за окно
                </S.CardMeta>
                {spike.recentArticles.length > 0 && (
                  <S.RelatedList>
                    {spike.recentArticles.map((item) => (
                      <li key={item.id}>
                        {item.sourceName}: {item.title}
                      </li>
                    ))}
                  </S.RelatedList>
                )}
              </S.Card>
              );
            }}
          />

          <TopicSection
            title="Эмоциональные поводы"
            hint="Материалы с сильной тональностью (|тон| ≥ 0.55)"
            empty="Нет ярко окрашенных материалов. Запустите news-tone-analyzer."
            items={dashboard.toneHighlights}
            renderItem={(highlight) => (
              <S.Card key={highlight.news.id} type="button" onClick={() => openNews(highlight.news.id)}>
                <S.Badges>
                  <S.Badge $variant="tone">{highlight.toneLabel}</S.Badge>
                  <S.Badge>{formatToneCoefficient(highlight.toneCoefficient)}</S.Badge>
                </S.Badges>
                <S.CardTitle>{highlight.news.title}</S.CardTitle>
                <S.CardMeta>
                  <NewsMeta item={highlight.news} />
                </S.CardMeta>
              </S.Card>
            )}
          />
        </S.Grid>
      )}
    </S.Page>
  );
}

function TopicSection<T>({
  title,
  hint,
  empty,
  items,
  renderItem,
}: {
  title: string;
  hint: string;
  empty: string;
  items: T[];
  renderItem: (item: T) => ReactNode;
}) {
  return (
    <S.Section>
      <S.SectionTitle>{title}</S.SectionTitle>
      <S.SectionHint>{hint}</S.SectionHint>
      {items.length === 0 ? (
        <S.EmptySection>{empty}</S.EmptySection>
      ) : (
        <S.CardList>{items.map((item) => renderItem(item))}</S.CardList>
      )}
    </S.Section>
  );
}
