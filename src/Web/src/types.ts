// Shapes of the JSON returned by the services.

export interface CreatePolicyRequest {
  customerName: string;
  vin: string;
  baseMonthlyRate: number;
  effectiveDateUtc: string;
}

export interface CreatePolicyResult {
  policyId: string;
  policyNumber: string;
  status: string;
}

export interface RecordTripRequest {
  policyNumber: string;
  startTimeUtc: string;
  endTimeUtc: string;
  milesDriven: number;
  hardBrakeCount: number;
  rapidAccelerationCount: number;
  maxSpeedMph: number;
}

export type TripStatus = 'Completed' | 'FlaggedForReview';

export interface Trip {
  id: string;
  policyNumber: string;
  startTimeUtc: string;
  endTimeUtc: string;
  milesDriven: number;
  hardBrakeCount: number;
  rapidAccelerationCount: number;
  maxSpeedMph: number;
  status: TripStatus;
  recordedAtUtc: string;
}

export interface Invoice {
  id: string;
  policyNumber: string;
  billingPeriod: string;
  totalMilesDriven: number;
  calculatedSafetyScore: number;
  premiumAdjustmentPercentage: number;
  lastUpdatedUtc: string;
}

/** Error body returned by the services (RFC 9457 problem details). */
export interface ProblemDetails {
  title?: string;
  status?: number;
  code?: string;
}
