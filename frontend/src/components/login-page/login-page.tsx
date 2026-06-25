import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/auth-context';
import { PATHS } from '../../app/paths';
import * as S from './login-page.styles';

interface LoginLocationState {
  from?: string;
}

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as LoginLocationState | null)?.from ?? PATHS.news;
  const [loginName, setLoginName] = useState('');
  const [password, setPassword] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleLogin = async (event: React.FormEvent) => {
    event.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      await login(loginName.trim(), password);
      navigate(from, { replace: true });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не удалось войти');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <S.Page>
      <S.Card>
        <S.Title>News Collector</S.Title>
        <S.Subtitle>Войдите, чтобы работать с новостями и переписываниями</S.Subtitle>

        <S.Form onSubmit={(event) => void handleLogin(event)}>
          <S.Field>
            Логин
            <S.Input
              value={loginName}
              autoComplete="username"
              disabled={submitting}
              onChange={(event) => setLoginName(event.target.value)}
            />
          </S.Field>

          <S.Field>
            Пароль
            <S.Input
              type="password"
              value={password}
              autoComplete="current-password"
              disabled={submitting}
              onChange={(event) => setPassword(event.target.value)}
            />
          </S.Field>

          {error && <S.Error>{error}</S.Error>}

          <S.Button type="submit" disabled={submitting}>
            {submitting ? 'Вход…' : 'Войти'}
          </S.Button>
        </S.Form>

        <S.Footer>
          <S.Hint>Или если есть код приглашения:</S.Hint>
          <S.LinkButton as={Link} to={PATHS.register}>
            Зарегистрироваться по коду
          </S.LinkButton>
          <S.PromoLink as={Link} to={PATHS.promo}>
            О возможностях платформы →
          </S.PromoLink>
        </S.Footer>
      </S.Card>
    </S.Page>
  );
}
