import { useState, type FormEvent } from 'react';

import { api } from '../api';

const VIN_LENGTH = 17;

interface CreatePolicyFormProps {
  onCreated: (policyNumber: string) => void;
  onError: (message: string) => void;
}

export function CreatePolicyForm({ onCreated, onError }: CreatePolicyFormProps) {
  const [customerName, setCustomerName] = useState('');
  const [vin, setVin] = useState('');
  const [baseRate, setBaseRate] = useState(150);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setSubmitting(true);
    try {
      const result = await api.createPolicy({
        customerName,
        vin,
        baseMonthlyRate: baseRate,
        effectiveDateUtc: new Date().toISOString(),
      });
      setCustomerName('');
      setVin('');
      onCreated(result.policyNumber);
    } catch (error) {
      onError((error as Error).message);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form className="panel form" onSubmit={handleSubmit}>
      <div>
        <h2>Create a policy</h2>
        <p className="hint">Issue a new policy and start tracking it.</p>
      </div>

      <div className="field">
        <label htmlFor="customer-name">Customer name</label>
        <input
          id="customer-name"
          placeholder="Jane Doe"
          value={customerName}
          onChange={(event) => setCustomerName(event.target.value)}
          required
        />
      </div>

      <div className="field">
        <label htmlFor="vin">Vehicle VIN</label>
        <input
          id="vin"
          className="mono"
          placeholder="1HGCR2F83HA000000"
          value={vin}
          onChange={(event) => setVin(event.target.value.toUpperCase())}
          minLength={VIN_LENGTH}
          maxLength={VIN_LENGTH}
          autoComplete="off"
          spellCheck={false}
          required
        />
        <p className="hint">{`${vin.length}/${VIN_LENGTH} characters · letters I, O and Q are not used in VINs`}</p>
      </div>

      <div className="field">
        <label htmlFor="base-rate">Base monthly rate (CAD)</label>
        <input
          id="base-rate"
          type="number"
          min="0"
          step="0.01"
          value={baseRate}
          onChange={(event) => setBaseRate(Number(event.target.value))}
          required
        />
      </div>

      <button type="submit" className="button button-primary" disabled={submitting}>
        {submitting ? 'Issuing policy…' : 'Issue policy'}
      </button>
    </form>
  );
}
