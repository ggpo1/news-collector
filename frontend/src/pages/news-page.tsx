import { useCallback, useEffect, useMemo, useState } from 'react';
import { Navigate, useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { buildNewsPath, parseNewsPageParam, parseNewsRoute } from '../app/news-route';
import { EditorialTagSelect } from '../components/editorial-tag-select/editorial-tag-select';
import {
  CategorySelect,
  UNCATEGORIZED_FILTER,
} from '../components/category-select/category-select';
import { MasterDetailLayout } from '../components/master-detail-layout/master-detail-layout';
import { NewsDetail } from '../components/news-detail/news-detail';
import { NewsList } from '../components/news-list/news-list';
import { Pagination } from '../components/pagination/pagination';
import { SourceSelect } from '../components/source-select/source-select';
import { ToneSelect, type NewsToneFilterValue } from '../components/tone-select/tone-select';
import * as SectionS from '../components/section-page/section-page.styles';
import { useCategories } from '../hooks/use-categories/use-categories';
import { useEditorialTags } from '../hooks/use-editorial-tags/use-editorial-tags';
import { useNews } from '../hooks/use-news/use-news';
import { useSources } from '../hooks/use-sources/use-sources';
import * as S from './news-page.styles';

export function NewsPage() {
  const { pageNum } = useParams();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [editorialTagFilter, setEditorialTagFilter] = useState<string | null>(null);

  const route = useMemo(
    () => parseNewsRoute(pageNum, searchParams),
    [pageNum, searchParams],
  );

  const parsedPage = parseNewsPageParam(pageNum);
  const page = parsedPage ?? 1;

  const { sources, loading: sourcesLoading, error: sourcesError } = useSources();
  const { categories, loading: categoriesLoading, error: categoriesError } = useCategories();
  const { tags: editorialTags, loading: tagsLoading } = useEditorialTags();
  const activeSources = sources.filter((source) => source.isActive);

  const categoryFilter = route.uncategorized
    ? UNCATEGORIZED_FILTER
    : route.categoryId;

  const { items, totalPages, totalCount, loading, error, reload } = useNews({
    page,
    sourceId: route.sourceId,
    categoryId: route.categoryId,
    uncategorized: route.uncategorized,
    toneFilter: route.tone,
    editorialTagId: editorialTagFilter,
  });

  const navigateToRoute = useCallback(
    (next: Parameters<typeof buildNewsPath>[0]) => {
      navigate(buildNewsPath(next));
    },
    [navigate],
  );

  useEffect(() => {
    if (parsedPage !== null && totalPages > 0 && page > totalPages) {
      navigateToRoute({ ...route, page: totalPages });
    }
  }, [navigateToRoute, page, parsedPage, route, totalPages]);

  const updateFilters = useCallback(
    (patch: {
      sourceId?: string | null;
      categoryId?: string | null;
      uncategorized?: boolean;
      tone?: NewsToneFilterValue | null;
      newsId?: string | null;
    }) => {
      navigateToRoute({
        page: 1,
        sourceId: patch.sourceId !== undefined ? patch.sourceId : route.sourceId,
        categoryId:
          patch.uncategorized
            ? null
            : patch.categoryId !== undefined
              ? patch.categoryId
              : route.categoryId,
        uncategorized:
          patch.uncategorized !== undefined ? patch.uncategorized : route.uncategorized,
        tone: patch.tone !== undefined ? patch.tone : route.tone,
        newsId: patch.newsId !== undefined ? patch.newsId : null,
      });
    },
    [navigateToRoute, route],
  );

  const handleSourceChange = (sourceId: string | null) => {
    updateFilters({ sourceId });
  };

  const handleCategoryChange = (value: string | null) => {
    if (value === UNCATEGORIZED_FILTER) {
      updateFilters({ uncategorized: true, categoryId: null });
      return;
    }

    updateFilters({
      uncategorized: false,
      categoryId: value,
    });
  };

  const handleToneChange = (tone: NewsToneFilterValue | null) => {
    updateFilters({ tone });
  };

  const handleEditorialTagChange = (tagId: string | null) => {
    setEditorialTagFilter(tagId);
    navigateToRoute({ ...route, page: 1, newsId: null });
  };

  const handlePageChange = (nextPage: number) => {
    navigateToRoute({ ...route, page: nextPage, newsId: null });
  };

  const handleSelectNews = (newsId: string) => {
    navigateToRoute({ ...route, newsId });
  };

  const handleBackToList = () => {
    navigateToRoute({ ...route, newsId: null });
  };

  if (parsedPage === null) {
    return <Navigate to={buildNewsPath({ ...route, page: 1 })} replace />;
  }

  const filterError = sourcesError ?? categoriesError;

  return (
    <SectionS.SectionPage>
      <MasterDetailLayout
        stickyList
        detailOpen={Boolean(route.newsId)}
        onBack={handleBackToList}
        backLabel="К списку новостей"
        listHeader={
          <>
            <S.FiltersRow>
              <SourceSelect
                sources={activeSources}
                value={route.sourceId}
                loading={sourcesLoading}
                onChange={handleSourceChange}
              />
              <CategorySelect
                categories={categories}
                value={categoryFilter}
                loading={categoriesLoading}
                onChange={handleCategoryChange}
              />
              <ToneSelect value={route.tone} onChange={handleToneChange} />
              <EditorialTagSelect
                tags={editorialTags}
                value={editorialTagFilter}
                loading={tagsLoading}
                onChange={handleEditorialTagChange}
              />
            </S.FiltersRow>
            {filterError && <S.ErrorBanner role="alert">{filterError}</S.ErrorBanner>}
          </>
        }
        listBody={
          <NewsList
            items={items}
            loading={loading}
            error={error}
            selectedId={route.newsId}
            showSource={!route.sourceId}
            onSelect={handleSelectNews}
          />
        }
        listFooter={
          <Pagination
            embedded
            page={page}
            totalPages={totalPages}
            totalCount={totalCount}
            onPageChange={handlePageChange}
          />
        }
        detail={<NewsDetail newsId={route.newsId} onContentLoaded={() => void reload()} />}
      />
    </SectionS.SectionPage>
  );
}
