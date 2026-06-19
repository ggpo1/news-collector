import { useState } from 'react';
import { useAuth } from '../../contexts/auth-context';
import * as S from './login-page.styles';

export function LoginPage() {
  const { login } = useAuth();
  const [loginName, setLoginName] = useState('');
  const [password, setPassword] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      await login(loginName.trim(), password);
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

        <S.Form onSubmit={(event) => void handleSubmit(event)}>
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

        <S.Hint>
          Первый запуск: логин <strong>chief</strong>, пароль из переменной AUTH_SEED_CHIEF_PASSWORD
          (по умолчанию changeme).
        </S.Hint>
      </S.Card>
    </S.Page>
  );
}
