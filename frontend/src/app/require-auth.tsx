import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { LoadingState } from '../components/ui/loading-state';
import { useAuth } from '../contexts/auth-context';
import { PATHS } from './paths';

export function RequireAuth({ children }: { children: ReactNode }) {
  const { user, loading } = useAuth();
  const location = useLocation();

  if (loading) {
    return <LoadingState label="Проверка сессии…" />;
  }

  if (!user) {
    return <Navigate to={PATHS.login} replace state={{ from: location.pathname }} />;
  }

  return children;
}
