import { Navigate } from 'react-router-dom';
import { LoadingState } from '../components/ui/loading-state';
import { useAuth } from '../contexts/auth-context';
import { PATHS } from './paths';

export function RootRedirect() {
  const { user, loading } = useAuth();

  if (loading) {
    return <LoadingState label="Проверка сессии…" />;
  }

  return <Navigate to={user ? PATHS.dashboard : PATHS.promo} replace />;
}

export function NotFoundRedirect() {
  const { user, loading } = useAuth();

  if (loading) {
    return <LoadingState label="Проверка сессии…" />;
  }

  return <Navigate to={user ? PATHS.dashboard : PATHS.promo} replace />;
}
