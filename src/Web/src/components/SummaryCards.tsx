import { PREMIUM_TIERS, formatAdjustment, tierFor, toneFor } from '../premiumTiers';
import type { Invoice } from '../types';

interface SummaryCardsProps {
  hasPolicy: boolean;
  latestInvoice: Invoice | undefined;
}

function adjustmentNote(percent: number): string {
  if (percent < 0) return 'Discount on the base rate';
  if (percent > 0) return 'Surcharge on the base rate';
  return 'Standard base rate';
}

export function SummaryCards({ hasPolicy, latestInvoice }: SummaryCardsProps) {
  if (!latestInvoice) {
    return (
      <section className="summary">
        <div className="panel summary-empty">
          <p>{hasPolicy ? 'No trips billed yet for this policy.' : 'Load or create a policy to see its score.'}</p>
        </div>
        <TierLegend />
      </section>
    );
  }

  const score = latestInvoice.calculatedSafetyScore;
  const adjustment = latestInvoice.premiumAdjustmentPercentage;
  const tone = toneFor(adjustment);

  return (
    <section className="summary">
      <div className="cards">
        <div className="panel card">
          <h3>Safety score</h3>
          <p className={`card-value tone-${tone}`}>
            {score} <span className="card-unit">/ 100</span>
          </p>
          <div className="meter" role="presentation">
            <div className={`meter-fill tone-bg-${tone}`} style={{ width: `${score}%` }} />
          </div>
        </div>

        <div className="panel card">
          <h3>Premium adjustment</h3>
          <p className={`card-value tone-${tone}`}>{formatAdjustment(adjustment)}</p>
          <p className="hint">{adjustmentNote(adjustment)}</p>
        </div>

        <div className="panel card">
          <h3>Miles this period</h3>
          <p className="card-value">
            {latestInvoice.totalMilesDriven} <span className="card-unit">mi</span>
          </p>
          <p className="hint">Billing period {latestInvoice.billingPeriod}</p>
        </div>
      </div>

      <TierLegend currentScore={score} />
    </section>
  );
}

function TierLegend({ currentScore }: { currentScore?: number }) {
  const current = currentScore === undefined ? undefined : tierFor(currentScore);

  return (
    <div className="panel tiers">
      <h3>Premium tiers</h3>
      <ul>
        {PREMIUM_TIERS.map((tier) => {
          const isCurrent = tier === current;
          return (
            <li key={tier.minScore} className={isCurrent ? 'tier tier-current' : 'tier'}>
              <span>{tier.maxScore === null ? `${tier.minScore}+` : `${tier.minScore}–${tier.maxScore}`}</span>
              <span className={`tone-${toneFor(tier.adjustment)}`}>{formatAdjustment(tier.adjustment)}</span>
              {isCurrent && <span className="tier-badge">current</span>}
            </li>
          );
        })}
      </ul>
    </div>
  );
}
