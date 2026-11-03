import type { Trip, TripStatus } from '../types';

interface TripHistoryProps {
  trips: Trip[];
}

const STATUS_LABELS: Record<TripStatus, string> = {
  Completed: 'Completed',
  FlaggedForReview: 'Flagged for review',
};

export function TripHistory({ trips }: TripHistoryProps) {
  return (
    <section className="panel">
      <div className="section-header">
        <h2>Trip history</h2>
        <span className="hint">{`${trips.length} ${trips.length === 1 ? 'trip' : 'trips'}`}</span>
      </div>

      {trips.length === 0 ? (
        <p className="hint">No trips recorded for this policy yet.</p>
      ) : (
        <div className="table-scroll">
          <table className="table">
            <thead>
              <tr>
                <th>Ended</th>
                <th className="num">Miles</th>
                <th className="num">Hard brakes</th>
                <th className="num">Rapid accels</th>
                <th className="num">Max speed</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {trips.map((trip) => (
                <tr key={trip.id}>
                  <td>{new Date(trip.endTimeUtc).toLocaleString()}</td>
                  <td className="num">{trip.milesDriven}</td>
                  <td className="num">{trip.hardBrakeCount}</td>
                  <td className="num">{trip.rapidAccelerationCount}</td>
                  <td className="num">{`${trip.maxSpeedMph} mph`}</td>
                  <td>
                    <span className={`badge badge-${trip.status === 'Completed' ? 'good' : 'bad'}`}>
                      {STATUS_LABELS[trip.status]}
                    </span>
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
