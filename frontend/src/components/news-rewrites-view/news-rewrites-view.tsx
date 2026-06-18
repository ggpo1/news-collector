import { useMemo, useState } from 'react';
import { deleteNewsRewrite, getNewsById } from '../../api/client';
import type { NewsItemDetail, NewsRewrite } from '../../api/types';
import { useNewsRewrites } from '../../hooks/use-news-rewrites/use-news-rewrites';
import { NewsRewriteEditor } from '../news-rewrite-editor/news-rewrite-editor';
import { NewsRewritesList } from '../news-rewrites-list/news-rewrites-list';
import { Pagination } from '../pagination/pagination';
import { RewrittenNewsDetail } from '../rewritten-news-detail/rewritten-news-detail';
import * as S from './news-rewrites-view.styles';

export function NewsRewritesView() {
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [editorOpen, setEditorOpen] = useState(false);
  const [editorSourceNews, setEditorSourceNews] = useState<NewsItemDetail | null>(null);
  const [editorRewrite, setEditorRewrite] = useState<NewsRewrite | null>(null);

  const { items, page, totalPages, totalCount, loading, error, setPage, reload } = useNewsRewrites();

  const selectedRewrite = useMemo(
    () => items.find((item) => item.id === selectedId) ?? null,
    [items, selectedId],
  );

  const openEditor = async (rewrite: NewsRewrite) => {
    try {
      const sourceNews = await getNewsById(rewrite.sourceNewsId);
      setEditorSourceNews(sourceNews);
      setEditorRewrite(rewrite);
      setEditorOpen(true);
    } catch (err) {
      window.alert(err instanceof Error ? err.message : 'Не удалось открыть редактор');
    }
  };

  const handleDelete = async (rewrite: NewsRewrite) => {
    await deleteNewsRewrite(rewrite.id);

    if (selectedId === rewrite.id) {
      setSelectedId(null);
    }

    await reload();
  };

  const handleEditorClose = () => {
    setEditorOpen(false);
    setEditorSourceNews(null);
    setEditorRewrite(null);
  };

  const handleEditorSaved = async () => {
    await reload();
  };

  return (
    <>
      <S.Grid>
        <S.ListColumn>
          <NewsRewritesList
            items={items}
            loading={loading}
            error={error}
            selectedId={selectedId}
            onSelect={setSelectedId}
          />
          <Pagination
            page={page}
            totalPages={totalPages}
            totalCount={totalCount}
            onPageChange={setPage}
          />
        </S.ListColumn>

        <RewrittenNewsDetail
          rewrite={selectedRewrite}
          onEdit={(rewrite) => void openEditor(rewrite)}
          onDelete={handleDelete}
        />
      </S.Grid>

      {editorOpen && editorSourceNews && (
        <NewsRewriteEditor
          sourceNews={editorSourceNews}
          initialRewrite={editorRewrite}
          onClose={handleEditorClose}
          onSaved={() => void handleEditorSaved()}
        />
      )}
    </>
  );
}
