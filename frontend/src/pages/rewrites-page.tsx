import { useNavigate } from 'react-router-dom';
import { NewsRewritesView } from '../components/news-rewrites-view/news-rewrites-view';
import { buildNewsPath } from '../app/news-route';

export function RewritesPage() {
  const navigate = useNavigate();

  const handleOpenSourceNews = (sourceNewsId: string, sourceId: string) => {
    navigate(
      buildNewsPath({
        page: 1,
        sourceId,
        categoryId: null,
        uncategorized: false,
        tone: null,
        newsId: sourceNewsId,
      }),
    );
  };

  return <NewsRewritesView onOpenSourceNews={handleOpenSourceNews} />;
}
