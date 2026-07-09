import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ConfigService } from '../../../Core/Services/config.service';
import { Subscriber, SubscribeRequest, SubscriptionResponse } from '../Models/subscriber.model';

@Injectable({ providedIn: 'root' })
export class SubscriberService {
  private readonly apiUrl: string;

  constructor(private http: HttpClient, private configService: ConfigService) {
    this.apiUrl = this.configService.baseUrl + '/subscribers';
  }

  subscribe(payload: SubscribeRequest): Observable<SubscriptionResponse> {
    return this.http.post<SubscriptionResponse>(this.apiUrl, payload);
  }

  confirm(token: string): Observable<SubscriptionResponse> {
    return this.http.get<SubscriptionResponse>(`${this.apiUrl}/confirm?token=${encodeURIComponent(token)}`);
  }

  unsubscribe(token: string): Observable<SubscriptionResponse> {
    return this.http.post<SubscriptionResponse>(`${this.apiUrl}/unsubscribe?token=${encodeURIComponent(token)}`, {});
  }

  // Admin only (X-User-Id attached by the interceptor).
  getAll(): Observable<Subscriber[]> {
    return this.http.get<Subscriber[]>(this.apiUrl);
  }
}
