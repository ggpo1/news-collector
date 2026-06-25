import { Link } from 'react-router-dom';
import { PATHS } from '../../app/paths';
import * as S from './promo-page.styles';

const FEATURES = [
  {
    icon: '◈',
    color: '#5b9aff',
    title: 'Редакционный обзор',
    text: '10–15 поводов вместо 200 RSS: развивающиеся темы, дубликаты, всплески сущностей и эмоциональные материалы — на одном экране.',
  },
  {
    icon: '▣',
    color: '#4361ee',
    title: 'Карточки тем (Story)',
    text: 'Сюжет как объект: таймлайн материалов RU и ино, ключевые сущности, переписи, отправки в Telegram и статус «в работе».',
  },
  {
    icon: '⧉',
    color: '#7209b7',
    title: 'Умные связи',
    text: 'Topic linking по заголовку, embeddings и общим сущностям. Duplicate, SameTopic и Related — в том числе между языками.',
  },
  {
    icon: '☀',
    color: '#e85d04',
    title: 'Утренний и вечерний бриф',
    text: 'LLM-сводка для chief editor: главные темы, новое с прошлого брифа, уже отправленное в каналы и дыры в покрытии.',
  },
  {
    icon: '◎',
    color: '#2a9d8f',
    title: 'Семантическая карта',
    text: 'Граф персон, компаний и стран по совместным упоминаниям — для контекста и расследований.',
  },
  {
    icon: '✎',
    color: '#f4a261',
    title: 'AI-переписи с авторством',
    text: 'Черновики под ваш канал с сохранением редактора. Second-day angles — углы «на второй день» после инфоповода.',
  },
  {
    icon: '◉',
    color: '#e63946',
    title: 'Тональность и фильтры',
    text: 'Авто-оценка тона материалов, фильтр по настроению в ленте, редакционные теги (#срочно, #needs-factcheck).',
  },
  {
    icon: '➤',
    color: '#48cae4',
    title: 'Telegram из редакции',
    text: 'Боты, каналы, маршрутизация по категориям и источникам. Отправка из карточки новости или переписи.',
  },
  {
    icon: '⚙',
    color: '#8b9cb3',
    title: 'Сбор и обогащение',
    text: 'RSS/HTML-источники, полный текст статей, автокатегоризация, NER и фоновые воркеры под нагрузку.',
  },
] as const;

const STATS = [
  { value: '15', label: 'поводов в обзоре вместо сотен RSS' },
  { value: '3', label: 'типа связей между материалами' },
  { value: '2×', label: 'брифа в день для chief editor' },
  { value: '1', label: 'путь от новости до Telegram' },
] as const;

export function PromoPage() {
  return (
    <S.Page>
      <S.Header>
        <S.Logo>
          <S.LogoMark>NC</S.LogoMark>
          News Collector
        </S.Logo>
        <S.HeaderActions>
          <S.GhostButton as={Link} to={PATHS.login}>
            Войти
          </S.GhostButton>
          <S.PrimaryButton as={Link} to={PATHS.register}>
            Начать
          </S.PrimaryButton>
        </S.HeaderActions>
      </S.Header>

      <S.HeroGlow aria-hidden="true" />

      <S.Hero>
        <S.HeroContent>
          <S.Eyebrow>Редакционная ОС для новостных команд</S.Eyebrow>
          <S.HeroTitle>
            <span>Не листайте RSS.</span>
            <br />
            Управляйте поводами.
          </S.HeroTitle>
          <S.HeroLead>
            News Collector собирает ленты, связывает материалы в темы, подсказывает что важно сейчас и
            проводит контент от черновика до Telegram — с контролем редактора на каждом шаге.
          </S.HeroLead>
          <S.HeroActions>
            <S.HeroPrimary as={Link} to={PATHS.login}>
              Войти в систему
            </S.HeroPrimary>
            <S.HeroSecondary as={Link} to={PATHS.register}>
              Регистрация по коду
            </S.HeroSecondary>
          </S.HeroActions>
        </S.HeroContent>

        <S.HeroVisual>
          <S.PreviewCard>
            <S.PreviewHeader>
              <span>Обзор · 48 ч</span>
              <S.PreviewBadge>3 источника</S.PreviewBadge>
            </S.PreviewHeader>
            <S.PreviewItem>
              <S.PreviewTitle>ЦБ сохранил ключевую ставку на фоне инфляционных ожиданий</S.PreviewTitle>
              <S.PreviewMeta>Развивается тема · РБК, Коммерсантъ, Reuters</S.PreviewMeta>
            </S.PreviewItem>
            <S.PreviewItem>
              <S.PreviewTitle>×2.4 всплеск: NVIDIA после отчёта</S.PreviewTitle>
              <S.PreviewMeta>Всплеск сущности · 12 упоминаний за окно</S.PreviewMeta>
            </S.PreviewItem>
            <S.PreviewItem>
              <S.PreviewTitle>Тема только в западных источниках — нет RU-покрытия</S.PreviewTitle>
              <S.PreviewMeta>Дыра в брифе · Guardian, AP</S.PreviewMeta>
            </S.PreviewItem>
          </S.PreviewCard>
        </S.HeroVisual>
      </S.Hero>

      <S.Stats>
        {STATS.map((stat) => (
          <S.StatCard key={stat.label}>
            <S.StatValue>{stat.value}</S.StatValue>
            <S.StatLabel>{stat.label}</S.StatLabel>
          </S.StatCard>
        ))}
      </S.Stats>

      <S.Section>
        <S.SectionHeader>
          <S.SectionTitle>Возможности</S.SectionTitle>
          <S.SectionLead>
            Не просто агрегатор — инструмент для редакции, медиааналитики и выпуска в каналы.
          </S.SectionLead>
        </S.SectionHeader>
        <S.FeatureGrid>
          {FEATURES.map((feature) => (
            <S.FeatureCard key={feature.title} $accent={feature.color}>
              <S.FeatureIcon $color={feature.color}>{feature.icon}</S.FeatureIcon>
              <S.FeatureTitle>{feature.title}</S.FeatureTitle>
              <S.FeatureText>{feature.text}</S.FeatureText>
            </S.FeatureCard>
          ))}
        </S.FeatureGrid>
      </S.Section>

      <S.Section>
        <S.SectionHeader>
          <S.SectionTitle>Как это работает</S.SectionTitle>
          <S.SectionLead>Три слоя: сбор intelligence → решения редактора → публикация.</S.SectionLead>
        </S.SectionHeader>
        <S.Workflow>
          <S.WorkflowStep>
            <S.StepNumber>1</S.StepNumber>
            <S.FeatureTitle>Сбор и обогащение</S.FeatureTitle>
            <S.FeatureText>
              Воркеры подтягивают RSS, вытягивают текст, категоризируют, извлекают сущности, считают тон
              и строят связи между материалами — автоматически, в фоне.
            </S.FeatureText>
          </S.WorkflowStep>
          <S.WorkflowStep>
            <S.StepNumber>2</S.StepNumber>
            <S.FeatureTitle>Редакционный фокус</S.FeatureTitle>
            <S.FeatureText>
              Дашборд, темы, бриф и карта сущностей показывают что важно. Редактор размечает теги,
              правит категории и берёт тему в работу.
            </S.FeatureText>
          </S.WorkflowStep>
          <S.WorkflowStep>
            <S.StepNumber>3</S.StepNumber>
            <S.FeatureTitle>Производство и канал</S.FeatureTitle>
            <S.FeatureText>
              AI-перепись, проверка, отправка в Telegram с историей доставок. Chief editor контролирует
              источники, ботов и команду.
            </S.FeatureText>
          </S.WorkflowStep>
        </S.Workflow>
      </S.Section>

      <S.Section>
        <S.SectionHeader>
          <S.SectionTitle>Для кого</S.SectionTitle>
        </S.SectionHeader>
        <S.AudienceGrid>
          <S.AudienceCard>
            <S.AudienceTitle>Редактор</S.AudienceTitle>
            <S.AudienceList>
              <S.AudienceItem>Лента с фильтрами по тону, категории и тегам</S.AudienceItem>
              <S.AudienceItem>Карточка темы с таймлайном и связанными материалами</S.AudienceItem>
              <S.AudienceItem>Second-day angles и переписи под формат</S.AudienceItem>
              <S.AudienceItem>Отправка в Telegram из интерфейса</S.AudienceItem>
            </S.AudienceList>
          </S.AudienceCard>
          <S.AudienceCard>
            <S.AudienceTitle>Главный редактор</S.AudienceTitle>
            <S.AudienceList>
              <S.AudienceItem>Утренний и вечерний бриф с LLM-сводкой</S.AudienceItem>
              <S.AudienceItem>Обзор «что важно сейчас» и дыры в покрытии</S.AudienceItem>
              <S.AudienceItem>Управление источниками, ботами и каналами</S.AudienceItem>
              <S.AudienceItem>Пользователи и коды приглашения</S.AudienceItem>
            </S.AudienceList>
          </S.AudienceCard>
        </S.AudienceGrid>
      </S.Section>

      <S.Cta>
        <S.CtaCard>
          <S.CtaTitle>Готовы собрать редакцию на одной платформе?</S.CtaTitle>
          <S.CtaLead>
            Войдите по логину или зарегистрируйтесь по коду приглашения от главного редактора.
          </S.CtaLead>
          <S.CtaActions>
            <S.HeroPrimary as={Link} to={PATHS.login}>
              Войти
            </S.HeroPrimary>
            <S.HeroSecondary as={Link} to={PATHS.register}>
              Получить доступ
            </S.HeroSecondary>
          </S.CtaActions>
        </S.CtaCard>
      </S.Cta>

      <S.Footer>News Collector · редакционная платформа для новостных команд</S.Footer>
    </S.Page>
  );
}
