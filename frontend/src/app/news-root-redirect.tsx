import { Navigate, useLocation } from 'react-router-dom';
import { PATHS } from './paths';

export function NewsRootRedirect() {
  const { search } = useLocation();

  return <Navigate to={`${PATHS.news}/1${search}`} replace />;
}
