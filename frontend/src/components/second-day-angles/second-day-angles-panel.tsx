import type { SecondDayAngles } from '../../api/types';
import * as S from './second-day-angles-panel.styles';

const ROLE_LABELS: Record<string, string> = {
  affected: 'пострадавший',
  beneficiary: 'выгодоприобретатель',
  neutral: 'нейтральный',
};

const ANGLE_TYPE_LABELS: Record<string, string> = {
  historical: 'история',
  stakeholder: 'игроки',
  factcheck: 'фактчек',
  consequence: 'последствия',
  profile: 'портрет',
  investigation: 'расследование',
  analysis: 'анализ',
};

function formatDate(value: string | null): string {
  if (!value) {
    return '—';
  }

  return new Intl.DateTimeFormat('ru-RU', { dateStyle: 'medium' }).format(new Date(value));
}

function roleLabel(role: string): string {
  return ROLE_LABELS[role.toLowerCase()] ?? role;
}

function angleTypeLabel(type: string): string {
  return ANGLE_TYPE_LABELS[type.toLowerCase()] ?? type;
}

interface SecondDayAnglesPanelProps {
  data: SecondDayAngles;
}

export function SecondDayAnglesPanel({ data }: SecondDayAnglesPanelProps) {
  return (
    <S.Panel>
      <S.Section>
        <S.SectionTitle>Готовые темы (5–7 углов)</S.SectionTitle>
        {data.suggestedAngles.length === 0 ? (
          <S.Empty>Модель не предложила тем — попробуйте позже или обогатите архив сущностями.</S.Empty>
        ) : (
          <S.AngleList>
            {data.suggestedAngles.map((angle) => (
              <S.AngleCard key={angle.title}>
                <S.AngleHeader>
                  <S.AngleTitle>{angle.title}</S.AngleTitle>
                  <S.AngleBadge>{angleTypeLabel(angle.angleType)}</S.AngleBadge>
                </S.AngleHeader>
                <S.AngleRationale>{angle.rationale}</S.AngleRationale>
              </S.AngleCard>
            ))}
          </S.AngleList>
        )}
      </S.Section>

      {data.historicalParallels.length > 0 && (
        <S.Section>
          <S.SectionTitle>Исторические параллели</S.SectionTitle>
          <S.ItemList>
            {data.historicalParallels.map((item) => (
              <S.ItemCard key={item.newsId}>
                <S.ItemMeta>
                  <span>{item.sourceName}</span>
                  <span>{formatDate(item.publishedAt)}</span>
                </S.ItemMeta>
                <S.ItemTitle>{item.title}</S.ItemTitle>
                <S.ItemText>{item.structuralSimilarity}</S.ItemText>
                <S.ItemHint>{item.parallelSummary}</S.ItemHint>
              </S.ItemCard>
            ))}
          </S.ItemList>
        </S.Section>
      )}

      {data.stakeholders.length > 0 && (
        <S.Section>
          <S.SectionTitle>Пострадавшие и выгодоприобретатели</S.SectionTitle>
          <S.ItemList>
            {data.stakeholders.map((item) => (
              <S.ItemCard key={`${item.name}-${item.role}`}>
                <S.ItemHeader>
                  <S.ItemTitle>{item.name}</S.ItemTitle>
                  <S.RoleBadge $role={item.role}>{roleLabel(item.role)}</S.RoleBadge>
                </S.ItemHeader>
                <S.ItemMeta>
                  <span>{item.entityType}</span>
                </S.ItemMeta>
                <S.ItemText>{item.reason}</S.ItemText>
              </S.ItemCard>
            ))}
          </S.ItemList>
        </S.Section>
      )}

      {data.numberContradictions.length > 0 && (
        <S.Section>
          <S.SectionTitle>Противоречия в цифрах</S.SectionTitle>
          <S.ItemList>
            {data.numberContradictions.map((item) => (
              <S.ItemCard key={item.metric}>
                <S.ItemTitle>{item.metric}</S.ItemTitle>
                <S.ValuesList>
                  {item.values.map((value) => (
                    <S.ValueRow key={`${value.sourceName}-${value.value}`}>
                      <span>{value.sourceName}</span>
                      <strong>{value.value}</strong>
                    </S.ValueRow>
                  ))}
                </S.ValuesList>
                {item.factCheckAngle ? <S.ItemHint>{item.factCheckAngle}</S.ItemHint> : null}
              </S.ItemCard>
            ))}
          </S.ItemList>
        </S.Section>
      )}
    </S.Panel>
  );
}
