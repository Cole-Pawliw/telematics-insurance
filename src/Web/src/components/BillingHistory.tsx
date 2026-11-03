import { formatAdjustment, toneFor } from '../premiumTiers';
import type { Invoice } from '../types';

interface BillingHistoryProps {
  invoices: Invoice[];
}

export function BillingHistory({ invoices }: BillingHistoryProps) {
  return (
    <section className="panel">
      <div className="section-header">
        <h2>Billing history</h2>
      </div>

      {invoices.length === 0 ? (
        <p className="hint">No billing periods yet.</p>
      ) : (
        <div className="table-scroll">
          <table className="table">
            <thead>
              <tr>
                <th>Period</th>
                <th className="num">Miles</th>
                <th className="num">Safety score</th>
                <th className="num">Adjustment</th>
              </tr>
            </thead>
            <tbody>
              {invoices.map((invoice) => (
                <tr key={invoice.id}>
                  <td>{invoice.billingPeriod}</td>
                  <td className="num">{invoice.totalMilesDriven}</td>
                  <td className="num">{invoice.calculatedSafetyScore}</td>
                  <td className={`num tone-${toneFor(invoice.premiumAdjustmentPercentage)}`}>
                    {formatAdjustment(invoice.premiumAdjustmentPercentage)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
