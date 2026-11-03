import type { FormEvent } from 'react';

interface PolicyLookupProps {
  value: string;
  loading: boolean;
  onChange: (value: string) => void;
  onLoad: () => void;
}

export function PolicyLookup({ value, loading, onChange, onLoad }: PolicyLookupProps) {
  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    onLoad();
  }

  return (
    <form className="panel lookup" onSubmit={handleSubmit}>
      <div className="field lookup-field">
        <label htmlFor="policy-number">Policy number</label>
        <div className="lookup-row">
          <input
            id="policy-number"
            value={value}
            placeholder="e.g. POL-1A2B3C4D"
            autoComplete="off"
            onChange={(event) => onChange(event.target.value)}
          />
          <button type="submit" className="button button-secondary" disabled={loading || !value.trim()}>
            {loading ? 'Loading…' : 'Load'}
          </button>
        </div>
        <p className="hint">Creating a policy below loads it automatically.</p>
      </div>
    </form>
  );
}
