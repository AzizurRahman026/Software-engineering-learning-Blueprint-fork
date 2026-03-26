import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from './config.service';

export interface ChatResponse {
  response: string;
}

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private readonly chatUrl: string;

  constructor(
    private http: HttpClient,
    private configService: ConfigService
  ) {
    this.chatUrl = `${this.configService.baseUrl}/chat`;
  }

  sendMessage(message: string): Observable<ChatResponse> {
    return this.http.post<ChatResponse>(this.chatUrl, { message });
  }
}
