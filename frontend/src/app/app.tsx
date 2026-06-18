import { useEffect, useState } from 'react';
import * as S from './app.styles';
import { AppNav, type AppSection } from '../components/app-nav/app-nav';
import { NewsDetail } from '../components/news-detail/news-detail';
import { NewsLinksView } from '../components/news-links-view/news-links-view';
import { NewsList } from '../components/news-list/news-list';
import { Pagination } from '../components/pagination/pagination';
import { SourceSelect } from '../components/source-select/source-select';
import { useNews } from '../hooks/use-news/use-news';
import { useSources } from '../hooks/use-sources/use-sources';

export default function App() {
  const [section, setSection] = useState<AppSection>('news');
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
          <S.Subtitle>
            {section === 'news' ? 'Новости по выбранному источнику' : 'Связи между новостями на одну тему'}
          </S.Subtitle>
        </div>
        <S.HeaderActions>
          <AppNav value={section} onChange={setSection} />
          {section === 'news' && (
            <SourceSelect
              sources={sources}
              value={selectedSourceId}
              loading={sourcesLoading}
              onChange={setSelectedSourceId}
            />
          )}
        </S.HeaderActions>
      </S.Header>

      {sourcesError && section === 'news' && <S.ErrorBanner>{sourcesError}</S.ErrorBanner>}

      {section === 'news' ? (
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
      ) : (
        <NewsLinksView />
      )}
    </S.Layout>
  );
}
