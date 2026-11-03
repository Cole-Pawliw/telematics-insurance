// Mirrors the tiers in the Billing service (MonthlyInvoice.PremiumAdjustmentFor). Display only.

export interface PremiumTier {
  minScore: number;
  maxScore: number | null;
  adjustment: number;
}

export const PREMIUM_TIERS: PremiumTier[] = [
  { minScore: 90, maxScore: null, adjustment: -15 },
  { minScore: 75, maxScore: 89, adjustment: -5 },
  { minScore: 60, maxScore: 74, adjustment: 0 },
  { minScore: 45, maxScore: 59, adjustment: 10 },
  { minScore: 0, maxScore: 44, adjustment: 25 },
];

export function tierFor(score: number): PremiumTier {
  return PREMIUM_TIERS.find((tier) => score >= tier.minScore) ?? PREMIUM_TIERS[PREMIUM_TIERS.length - 1];
}

/** Formats a premium adjustment, e.g. +10% or -5%. */
export function formatAdjustment(percent: number): string {
  return percent > 0 ? `+${percent}%` : `${percent}%`;
}

/** Colour class for a premium adjustment: discounts are good, surcharges are bad. */
export function toneFor(adjustment: number): 'good' | 'neutral' | 'bad' {
  return adjustment < 0 ? 'good' : adjustment > 0 ? 'bad' : 'neutral';
}
