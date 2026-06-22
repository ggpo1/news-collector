import { useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';
import {
  CategorySelect,
  UNCATEGORIZED_FILTER,
} from '../components/category-select/category-select';
import { MasterDetailLayout } from '../components/master-detail-layout/master-detail-layout';
import { NewsDetail } from '../components/news-detail/news-detail';
import { NewsList } from '../components/news-list/news-list';
import { Pagination } from '../components/pagination/pagination';
import { SourceSelect } from '../components/source-select/source-select';
import * as SectionS from '../components/section-page/section-page.styles';
import { useCategories } from '../hooks/use-categories/use-categories';
import { useNews } from '../hooks/use-news/use-news';
import { useSources } from '../hooks/use-sources/use-sources';
import * as S from './news-page.styles';

interface NewsLocationState {
  sourceId?: string;
  newsId?: string;
}

export function NewsPage() {
  const location = useLocation();
  const locationState = (location.state as NewsLocationState | null) ?? null;
  const { sources, loading: sourcesLoading, error: sourcesError } = useSources();
  const { categories, loading: categoriesLoading, error: categoriesError } = useCategories();
  const activeSources = sources.filter((source) => source.isActive);
  const [selectedSourceId, setSelectedSourceId] = useState<string | null>(
    locationState?.sourceId ?? null,
  );
  const [categoryFilter, setCategoryFilter] = useState<string | null>(null);
  const [selectedNewsId, setSelectedNewsId] = useState<string | null>(
    locationState?.newsId ?? null,
  );

  const uncategorized = categoryFilter === UNCATEGORIZED_FILTER;
  const selectedCategoryId =
    categoryFilter && !uncategorized ? categoryFilter : null;

  const { items, page, totalPages, totalCount, loading, error, setPage, reload } = useNews({
    sourceId: selectedSourceId,
    categoryId: selectedCategoryId,
    uncategorized,
  });

  useEffect(() => {
    setSelectedNewsId(null);
  }, [selectedSourceId, categoryFilter, page]);

  const filterError = sourcesError ?? categoriesError;

  return (
    <SectionS.SectionPage>
      <MasterDetailLayout
        stickyList
        detailOpen={Boolean(selectedNewsId)}
        onBack={() => setSelectedNewsId(null)}
        backLabel="К списку новостей"
        listHeader={
          <>
            <S.FiltersRow>
              <SourceSelect
                sources={activeSources}
                value={selectedSourceId}
                loading={sourcesLoading}
                onChange={setSelectedSourceId}
              />
              <CategorySelect
                categories={categories}
                value={categoryFilter}
                loading={categoriesLoading}
                onChange={setCategoryFilter}
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
            selectedId={selectedNewsId}
            onSelect={setSelectedNewsId}
          />
        }
        listFooter={
          <Pagination
            embedded
            page={page}
            totalPages={totalPages}
            totalCount={totalCount}
            onPageChange={setPage}
          />
        }
        detail={<NewsDetail newsId={selectedNewsId} onContentLoaded={() => void reload()} />}
      />
    </SectionS.SectionPage>
  );
}
