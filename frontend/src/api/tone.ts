export function formatToneCoefficient(value: number | null | undefined): string | null {
  if (value === null || value === undefined) {
    return null;
  }

  const sign = value > 0 ? '+' : '';
  return `${sign}${value.toFixed(2)}`;
}
