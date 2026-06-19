import { useEffect, useState } from 'react';
import type { AppSection } from '../components/app-nav/app-nav';
import { AppShell } from '../components/app-shell/app-shell';
import { LoginPage } from '../components/login-page/login-page';
import { MasterDetailLayout } from '../components/master-detail-layout/master-detail-layout';
import { NewsDetail } from '../components/news-detail/news-detail';
import { NewsLinksView } from '../components/news-links-view/news-links-view';
import { NewsList } from '../components/news-list/news-list';
import { NewsRewritesView } from '../components/news-rewrites-view/news-rewrites-view';
import { Pagination } from '../components/pagination/pagination';
import { SourceSelect } from '../components/source-select/source-select';
import { SourcesView } from '../components/sources-view/sources-view';
import { UsersView } from '../components/users-view/users-view';
import { LoadingState } from '../components/ui/loading-state';
import { useAuth } from '../contexts/auth-context';
import { useNews } from '../hooks/use-news/use-news';
import { useSources } from '../hooks/use-sources/use-sources';

const SECTION_META: Record<AppSection, { title: string; subtitle: string }> = {
  news: {
    title: 'Новости',
    subtitle: 'Лента по выбранному источнику',
  },
  links: {
    title: 'Связи',
    subtitle: 'Новости на одну тему из разных источников',
  },
  rewrites: {
    title: 'Переписи',
    subtitle: 'Сохранённые AI-версии новостей',
  },
  sources: {
    title: 'Источники',
    subtitle: 'RSS-фиды и настройки сбора',
  },
  users: {
    title: 'Пользователи',
    subtitle: 'Учётные записи редакторов',
  },
};

export default function App() {
  const { user, loading: authLoading, isChiefEditor } = useAuth();
  const [section, setSection] = useState<AppSection>('news');
  const { sources, loading: sourcesLoading, error: sourcesError, reload: reloadSources } =
    useSources();
  const activeSources = sources.filter((source) => source.isActive);
  const [selectedSourceId, setSelectedSourceId] = useState<string | null>(null);
  const [selectedNewsId, setSelectedNewsId] = useState<string | null>(null);

  const { items, page, totalPages, totalCount, loading, error, setPage, reload } =
    useNews(selectedSourceId);

  useEffect(() => {
    if (activeSources.length > 0 && !selectedSourceId) {
      setSelectedSourceId(activeSources[0].id);
    }
  }, [activeSources, selectedSourceId]);

  useEffect(() => {
    setSelectedNewsId(null);
  }, [selectedSourceId, page]);

  useEffect(() => {
    setSelectedNewsId(null);
  }, [section]);

  useEffect(() => {
    if (!isChiefEditor && (section === 'sources' || section === 'users')) {
      setSection('news');
    }
  }, [isChiefEditor, section]);

  const handleOpenSourceNews = (sourceNewsId: string, sourceId: string) => {
    setSelectedSourceId(sourceId);
    setSelectedNewsId(sourceNewsId);
    setSection('news');
  };

  if (authLoading) {
    return <LoadingState label="Проверка сессии…" />;
  }

  if (!user) {
    return <LoginPage />;
  }

  const meta = SECTION_META[section];

  return (
    <AppShell
      section={section}
      sectionTitle={meta.title}
      sectionSubtitle={meta.subtitle}
      onSectionChange={setSection}
      error={section === 'news' ? sourcesError : section === 'sources' ? sourcesError : null}
      toolbar={
        section === 'news' ? (
          <SourceSelect
            sources={activeSources}
            value={selectedSourceId}
            loading={sourcesLoading}
            onChange={setSelectedSourceId}
          />
        ) : undefined
      }
    >
      {section === 'news' && (
        <MasterDetailLayout
          detailOpen={Boolean(selectedNewsId)}
          onBack={() => setSelectedNewsId(null)}
          backLabel="К списку новостей"
          list={
            <>
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
            </>
          }
          detail={
            <NewsDetail newsId={selectedNewsId} onContentLoaded={() => void reload()} />
          }
        />
      )}

      {section === 'links' && <NewsLinksView />}

      {section === 'rewrites' && (
        <NewsRewritesView onOpenSourceNews={handleOpenSourceNews} />
      )}

      {section === 'sources' && isChiefEditor && (
        <SourcesView
          sources={sources}
          loading={sourcesLoading}
          onChanged={() => void reloadSources()}
        />
      )}

      {section === 'users' && isChiefEditor && <UsersView />}
    </AppShell>
  );
}
