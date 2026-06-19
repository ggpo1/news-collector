import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { validateInvitation } from '../../api/client';
import { PATHS } from '../../app/paths';
import * as S from './login-page.styles';

export function RegisterPage() {
  const navigate = useNavigate();
  const [invitationCode, setInvitationCode] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleValidateCode = async (event: React.FormEvent) => {
    event.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      const result = await validateInvitation(invitationCode.trim());
      navigate(PATHS.registerDetails, {
        state: { invitationCode: invitationCode.trim(), role: result.role },
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Неверный или уже использованный код');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <S.Page>
      <S.Card>
        <S.Title>Регистрация</S.Title>
        <S.Subtitle>Введите пригласительный код, который вы получили от администратора</S.Subtitle>

        <S.Form onSubmit={(event) => void handleValidateCode(event)}>
          <S.Field>
            Код приглашения
            <S.Input
              value={invitationCode}
              autoComplete="off"
              spellCheck={false}
              disabled={submitting}
              placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
              onChange={(event) => setInvitationCode(event.target.value)}
            />
          </S.Field>

          {error && <S.Error>{error}</S.Error>}

          <S.Button type="submit" disabled={submitting || !invitationCode.trim()}>
            {submitting ? 'Проверка…' : 'Продолжить'}
          </S.Button>
        </S.Form>

        <S.Footer>
          <S.LinkButton as={Link} to={PATHS.login}>
            Уже есть аккаунт? Войти
          </S.LinkButton>
        </S.Footer>
      </S.Card>
    </S.Page>
  );
}
