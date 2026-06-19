import * as S from './pagination.styles';

interface PaginationProps {
  page: number;
  totalPages: number;
  totalCount: number;
  onPageChange: (page: number) => void;
  embedded?: boolean;
}

export function Pagination({
  page,
  totalPages,
  totalCount,
  onPageChange,
  embedded = false,
}: PaginationProps) {
  if (totalPages <= 1) {
    return null;
  }

  return (
    <S.Root $embedded={embedded} aria-label="Пагинация">
      <S.Info>
        Страница {page} из {totalPages} · всего {totalCount}
      </S.Info>
      <S.Buttons>
        <S.Button type="button" disabled={page <= 1} onClick={() => onPageChange(page - 1)}>
          ← Назад
        </S.Button>
        <S.Button
          type="button"
          disabled={page >= totalPages}
          onClick={() => onPageChange(page + 1)}
        >
          Вперёд →
        </S.Button>
      </S.Buttons>
    </S.Root>
  );
}
