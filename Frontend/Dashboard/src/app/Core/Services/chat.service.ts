import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from './config.service';

export interface ChatThreadInfo {
  threadId: string;
  title: string;
  createdAt: string;
  lastMessageAt: string;
}

export interface ToolCallRecord {
  id: number;
  toolName: string;
  arguments: string;
  result: string;
}

export interface ChatResponseDto {
  threadId: string;
  answer: string;
  provider: number;
  toolCalls: ToolCallRecord[];
}

export interface ChatMessageDto {
  text: string;
  sender: 'user' | 'ai';
}

/** Mirror of the backend's ThreadTitleDto (structured LLM output, Day 24). */
export interface ThreadTitleDto {
  title: string;
  topics: string[];
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

  sendMessage(message: string, provider: number = 0, threadId?: string): Observable<ChatResponseDto> {
    return this.http.post<ChatResponseDto>(this.chatUrl, { 
      query: message, 
      provider: provider,
      threadId: threadId ?? null
    });
  }

  getThreads(): Observable<ChatThreadInfo[]> {
    return this.http.get<ChatThreadInfo[]>(`${this.chatUrl}/threads`);
  }

  getMessages(threadId: string): Observable<ChatMessageDto[]> {
    return this.http.get<ChatMessageDto[]>(`${this.chatUrl}/threads/${threadId}/messages`);
  }
  
  deleteThread(threadId: string): Observable<void> {
    return this.http.delete<void>(`${this.chatUrl}/threads/${threadId}`);
  }

  /** Ask the LLM for a short { title, topics } suggestion for a thread. */
  suggestThreadTitle(threadId: string, provider: number = 0): Observable<ThreadTitleDto> {
    return this.http.post<ThreadTitleDto>(
      `${this.chatUrl}/threads/${threadId}/suggest-title`,
      null,
      { params: { provider } }
    );
  }
}
