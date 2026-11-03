import { useState, type FormEvent } from 'react';

import { api } from '../api';

// Simulated trips end now and last this long.
const SIMULATED_TRIP_MINUTES = 45;

// Matches the Telematics service: faster trips are flagged for review.
const REVIEW_SPEED_THRESHOLD_MPH = 100;

interface SimulateTripFormProps {
  policyNumber: string;
  onRecorded: () => void;
  onError: (message: string) => void;
}

export function SimulateTripForm({ policyNumber, onRecorded, onError }: SimulateTripFormProps) {
  const [miles, setMiles] = useState(25);
  const [hardBrakes, setHardBrakes] = useState(1);
  const [rapidAccelerations, setRapidAccelerations] = useState(0);
  const [maxSpeed, setMaxSpeed] = useState(72);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();

    const end = new Date();
    const start = new Date(end.getTime() - SIMULATED_TRIP_MINUTES * 60_000);

    setSubmitting(true);
    try {
      await api.recordTrip({
        policyNumber,
        startTimeUtc: start.toISOString(),
        endTimeUtc: end.toISOString(),
        milesDriven: miles,
        hardBrakeCount: hardBrakes,
        rapidAccelerationCount: rapidAccelerations,
        maxSpeedMph: maxSpeed,
      });
      onRecorded();
    } catch (error) {
      onError((error as Error).message);
    } finally {
      setSubmitting(false);
    }
  }

  const hasPolicy = policyNumber !== '';

  return (
    <form className="panel form" onSubmit={handleSubmit}>
      <div>
        <h2>Simulate a trip</h2>
        <p className="hint">{hasPolicy
            ? `Records a ${SIMULATED_TRIP_MINUTES}-minute trip ending now for ${policyNumber}.`
            : 'Load or create a policy first.'}</p>
      </div>

      <fieldset className="form" disabled={!hasPolicy || submitting}>
        <div className="field">
          <div className="field-label-row">
            <label htmlFor="trip-miles">Miles driven</label>
            <span className="field-value">{`${miles} mi`}</span>
          </div>
          <input
            id="trip-miles"
            type="range"
            min="1"
            max="200"
            value={miles}
            onChange={(event) => setMiles(Number(event.target.value))}
          />
        </div>

        <div className="grid-3">
          <NumberField id="trip-hard-brakes" label="Hard brakes" value={hardBrakes} onChange={setHardBrakes} />
          <NumberField
            id="trip-rapid-accelerations"
            label="Rapid accelerations"
            value={rapidAccelerations}
            onChange={setRapidAccelerations}
          />
          <NumberField id="trip-max-speed" label="Max speed (mph)" value={maxSpeed} onChange={setMaxSpeed} />
        </div>

        {maxSpeed > REVIEW_SPEED_THRESHOLD_MPH && <p className="hint tone-bad">Trips over {REVIEW_SPEED_THRESHOLD_MPH} mph are flagged for review.</p>}

        <button type="submit" className="button button-success">
          {submitting ? 'Recording trip…' : 'Record trip'}
        </button>
      </fieldset>
    </form>
  );
}

interface NumberFieldProps {
  id: string;
  label: string;
  value: number;
  onChange: (value: number) => void;
}

function NumberField({ id, label, value, onChange }: NumberFieldProps) {
  return (
    <div className="field">
      <label htmlFor={id}>{label}</label>
      <input id={id} type="number" min="0" value={value} onChange={(event) => onChange(Number(event.target.value))} />
    </div>
  );
}
