import type { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../contexts/auth-context';
import { PATHS } from './paths';

export function ChiefEditorRoute({ children }: { children: ReactNode }) {
  const { isChiefEditor } = useAuth();

  if (!isChiefEditor) {
    return <Navigate to={PATHS.news} replace />;
  }

  return children;
}
