import { useState } from 'react';

import { api } from './api';
import { BillingHistory } from './components/BillingHistory';
import { CreatePolicyForm } from './components/CreatePolicyForm';
import { PolicyLookup } from './components/PolicyLookup';
import { SimulateTripForm } from './components/SimulateTripForm';
import { SummaryCards } from './components/SummaryCards';
import { TripHistory } from './components/TripHistory';
import type { Invoice, Trip } from './types';

// Billing is updated asynchronously through RabbitMQ, so wait briefly before reloading.
const BILLING_REFRESH_DELAY_MS = 1000;

interface Message {
  kind: 'success' | 'error';
  text: string;
}

export function App() {
  const [policyInput, setPolicyInput] = useState('');
  const [loadedPolicy, setLoadedPolicy] = useState('');
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [trips, setTrips] = useState<Trip[]>([]);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState<Message | null>(null);

  async function loadDashboard(policyNumber: string) {
    const normalized = policyNumber.trim().toUpperCase();
    if (!normalized) return;
    setLoading(true);
    try {
      const [loadedInvoices, loadedTrips] = await Promise.all([
        api.getInvoices(normalized),
        api.getTrips(normalized),
      ]);
      setInvoices(loadedInvoices);
      setTrips(loadedTrips);
      setLoadedPolicy(normalized);
      setPolicyInput(normalized);
    } catch (error) {
      setMessage({ kind: 'error', text: (error as Error).message });
    } finally {
      setLoading(false);
    }
  }

  function handlePolicyCreated(policyNumber: string) {
    setMessage({ kind: 'success', text: `Created policy ${policyNumber}. It is now loaded below.` });
    void loadDashboard(policyNumber);
  }

  function handleTripRecorded() {
    setMessage({ kind: 'success', text: 'Trip recorded. The score updates in a moment.' });
    setTimeout(() => void loadDashboard(loadedPolicy), BILLING_REFRESH_DELAY_MS);
  }

  function handleError(text: string) {
    setMessage({ kind: 'error', text });
  }

  return (
    <>
      <header className="header">
        <div className="container">
          <h1>Telematics Insurance</h1>
          <p>Create a policy, simulate trips, and watch the safety score and premium adjust.</p>
        </div>
      </header>

      <main className="container">
        <div aria-live="polite">
          {message && (
            <div className={`message message-${message.kind}`}>
              <span>{message.text}</span>
              <button
                type="button"
                className="message-dismiss"
                aria-label="Dismiss"
                onClick={() => setMessage(null)}
              >
                ×
              </button>
            </div>
          )}
        </div>

        <PolicyLookup
          value={policyInput}
          loading={loading}
          onChange={setPolicyInput}
          onLoad={() => void loadDashboard(policyInput)}
        />

        <SummaryCards hasPolicy={loadedPolicy !== ''} latestInvoice={invoices[0]} />

        <div className="grid-2">
          <CreatePolicyForm onCreated={handlePolicyCreated} onError={handleError} />
          <SimulateTripForm policyNumber={loadedPolicy} onRecorded={handleTripRecorded} onError={handleError} />
        </div>

        {loadedPolicy && (
          <>
            <TripHistory trips={trips} />
            <BillingHistory invoices={invoices} />
          </>
        )}
      </main>
    </>
  );
}
