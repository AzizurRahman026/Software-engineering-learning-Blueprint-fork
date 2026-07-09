export type SubscriptionStatus = 'Pending' | 'Confirmed' | 'Unsubscribed';

export interface Subscriber {
  id: string;
  email: string;
  status: SubscriptionStatus;
  subscribedAt: string;
  confirmedAt?: string | null;
}

export interface SubscribeRequest {
  email: string;
}

export interface SubscriptionResponse {
  message: string;
}
