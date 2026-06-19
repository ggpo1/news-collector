import { useNavigate } from 'react-router-dom';
import { NewsRewritesView } from '../components/news-rewrites-view/news-rewrites-view';
import { PATHS } from '../app/paths';

export function RewritesPage() {
  const navigate = useNavigate();

  const handleOpenSourceNews = (sourceNewsId: string, sourceId: string) => {
    navigate(PATHS.news, { state: { sourceId, newsId: sourceNewsId } });
  };

  return <NewsRewritesView onOpenSourceNews={handleOpenSourceNews} />;
}
