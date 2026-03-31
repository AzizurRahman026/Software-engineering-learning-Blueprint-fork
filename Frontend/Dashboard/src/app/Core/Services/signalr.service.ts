import { Injectable, OnDestroy } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { NotificationDto } from '../../Shared/Models/notification.model';
import { ConfigService } from './config.service';

@Injectable({
  providedIn: 'root'
})
export class SignalrService implements OnDestroy {
  private hubConnection!: signalR.HubConnection;
  private readonly notificationSubject = new Subject<NotificationDto>();

  /** Subscribe to this to receive real-time notifications */
  public notifications$: Observable<NotificationDto> = this.notificationSubject.asObservable();

  constructor(private configService: ConfigService) {
    this.buildConnection();
    this.startConnection();
  }

  private buildConnection(): void {
    // baseUrl is "http://localhost:5000/api" → hub is at "http://localhost:5000/notifications"
    const hubUrl = this.configService.baseUrl.replace('/api', '/notifications');

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // retry intervals in ms
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Register the listener for "ReceiveNotification" — matches backend SendAsync method name
    this.hubConnection.on('ReceiveNotification', (notification: NotificationDto) => {
      console.log('[SignalR] Notification received:', notification);
      alert(notification.message);
      this.notificationSubject.next(notification);
    });

    this.hubConnection.onreconnecting((error) => {
      console.warn('[SignalR] Reconnecting...', error);
    });

    this.hubConnection.onreconnected((connectionId) => {
      console.log('[SignalR] Reconnected. ConnectionId:', connectionId);
    });

    this.hubConnection.onclose((error) => {
      console.error('[SignalR] Connection closed.', error);
    });
  }

  private startConnection(): void {
    this.hubConnection
      .start()
      .then(() => console.log('[SignalR] Connection established.'))
      .catch((err) => {
        console.error('[SignalR] Connection failed:', err);
        // Retry after 5 seconds if initial connection fails
        setTimeout(() => this.startConnection(), 5000);
      });
  }

  ngOnDestroy(): void {
    this.hubConnection?.stop();
  }
}
