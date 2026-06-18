import { useEffect, useState } from 'react';
import * as S from './app.styles';
import { NewsDetail } from '../components/news-detail/news-detail';
import { NewsList } from '../components/news-list/news-list';
import { Pagination } from '../components/pagination/pagination';
import { SourceSelect } from '../components/source-select/source-select';
import { useNews } from '../hooks/use-news/use-news';
import { useSources } from '../hooks/use-sources/use-sources';

export default function App() {
  const { sources, loading: sourcesLoading, error: sourcesError } = useSources();
  const [selectedSourceId, setSelectedSourceId] = useState<string | null>(null);
  const [selectedNewsId, setSelectedNewsId] = useState<string | null>(null);

  const { items, page, totalPages, totalCount, loading, error, setPage, reload } =
    useNews(selectedSourceId);

  useEffect(() => {
    if (sources.length > 0 && !selectedSourceId) {
      setSelectedSourceId(sources[0].id);
    }
  }, [sources, selectedSourceId]);

  useEffect(() => {
    setSelectedNewsId(null);
  }, [selectedSourceId, page]);

  return (
    <S.Layout>
      <S.Header>
        <div>
          <S.Title>News Collector</S.Title>
          <S.Subtitle>Новости по выбранному источнику</S.Subtitle>
        </div>
        <SourceSelect
          sources={sources}
          value={selectedSourceId}
          loading={sourcesLoading}
          onChange={setSelectedSourceId}
        />
      </S.Header>

      {sourcesError && <S.ErrorBanner>{sourcesError}</S.ErrorBanner>}

      <S.Grid>
        <S.ListColumn>
          <NewsList
            items={items}
            loading={loading}
            error={error}
            selectedId={selectedNewsId}
            onSelect={setSelectedNewsId}
          />
          <Pagination
            page={page}
            totalPages={totalPages}
            totalCount={totalCount}
            onPageChange={setPage}
          />
        </S.ListColumn>

        <NewsDetail newsId={selectedNewsId} onContentLoaded={() => void reload()} />
      </S.Grid>
    </S.Layout>
  );
}
