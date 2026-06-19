import { useState } from "react";
import { USER_ROLE_LABELS } from "../../api/role-labels";
import { validateInvitation } from "../../api/client";
import type { UserRole } from "../../api/types";
import { useAuth } from "../../contexts/auth-context";
import * as S from "./login-page.styles";

type AuthMode = "login" | "register-code" | "register-details";

export function LoginPage() {
  const { login, register } = useAuth();
  const [mode, setMode] = useState<AuthMode>("login");
  const [loginName, setLoginName] = useState("");
  const [password, setPassword] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [invitationCode, setInvitationCode] = useState("");
  const [invitationRole, setInvitationRole] = useState<UserRole | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const resetRegisterState = () => {
    setInvitationCode("");
    setInvitationRole(null);
    setLoginName("");
    setPassword("");
    setDisplayName("");
    setError(null);
  };

  const switchToLogin = () => {
    setMode("login");
    resetRegisterState();
  };

  const switchToRegister = () => {
    setMode("register-code");
    resetRegisterState();
  };

  const handleLogin = async (event: React.FormEvent) => {
    event.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      await login(loginName.trim(), password);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Не удалось войти");
    } finally {
      setSubmitting(false);
    }
  };

  const handleValidateCode = async (event: React.FormEvent) => {
    event.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      const result = await validateInvitation(invitationCode.trim());
      setInvitationRole(result.role);
      setMode("register-details");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Неверный или уже использованный код");
    } finally {
      setSubmitting(false);
    }
  };

  const handleRegister = async (event: React.FormEvent) => {
    event.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      await register(invitationCode.trim(), loginName.trim(), password, displayName.trim());
    } catch (err) {
      setError(err instanceof Error ? err.message : "Не удалось зарегистрироваться");
    } finally {
      setSubmitting(false);
    }
  };

  if (mode === "register-code") {
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
              {submitting ? "Проверка…" : "Продолжить"}
            </S.Button>
          </S.Form>

          <S.Footer>
            <S.LinkButton type="button" disabled={submitting} onClick={switchToLogin}>
              Уже есть аккаунт? Войти
            </S.LinkButton>
          </S.Footer>
        </S.Card>
      </S.Page>
    );
  }

  if (mode === "register-details") {
    return (
      <S.Page>
        <S.Card>
          <S.Title>Создание аккаунта</S.Title>
          <S.Subtitle>
            {invitationRole
              ? `Роль по приглашению: ${USER_ROLE_LABELS[invitationRole]}`
              : "Заполните данные для входа в систему"}
          </S.Subtitle>

          <S.Form onSubmit={(event) => void handleRegister(event)}>
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
              Имя
              <S.Input
                value={displayName}
                autoComplete="name"
                disabled={submitting}
                onChange={(event) => setDisplayName(event.target.value)}
              />
            </S.Field>

            <S.Field>
              Пароль
              <S.Input
                type="password"
                value={password}
                autoComplete="new-password"
                disabled={submitting}
                onChange={(event) => setPassword(event.target.value)}
              />
            </S.Field>

            {error && <S.Error>{error}</S.Error>}

            <S.Button type="submit" disabled={submitting}>
              {submitting ? "Регистрация…" : "Зарегистрироваться"}
            </S.Button>
          </S.Form>

          <S.Footer>
            <S.LinkButton
              type="button"
              disabled={submitting}
              onClick={() => {
                setMode("register-code");
                setError(null);
              }}
            >
              Назад к коду
            </S.LinkButton>
          </S.Footer>
        </S.Card>
      </S.Page>
    );
  }

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
            {submitting ? "Вход…" : "Войти"}
          </S.Button>
        </S.Form>
        <S.Footer>
          <S.Hint>Или если есть код приглашения:</S.Hint>
          <S.LinkButton type="button" disabled={submitting} onClick={switchToRegister}>
            Зарегистрироваться по коду
          </S.LinkButton>
        </S.Footer>
      </S.Card>
    </S.Page>
  );
}
