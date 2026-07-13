import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ConfigService } from '../../../Core/Services/config.service';
import {
  AddCommentRequest,
  PostComment,
  PostDetail,
  PostSummary,
  CreatePostRequest,
  CreatePostResponse,
  ToggleLikeResponse,
  UpdatePostRequest
} from '../Models/post.model';

@Injectable({ providedIn: 'root' })
export class PostService {
  private readonly apiUrl: string;

  constructor(private http: HttpClient, private configService: ConfigService) {
    this.apiUrl = this.configService.baseUrl + '/posts';
  }

  getPosts(page = 1, pageSize = 10): Observable<PostSummary[]> {
    return this.http.get<PostSummary[]>(`${this.apiUrl}?page=${page}&pageSize=${pageSize}`);
  }

  // The signed-in author's own posts (any status: Pending/Published/Rejected).
  getMyPosts(page = 1, pageSize = 10): Observable<PostSummary[]> {
    return this.http.get<PostSummary[]>(`${this.apiUrl}/mine?page=${page}&pageSize=${pageSize}`);
  }

  // Admin: pending posts awaiting moderation (X-User-Id attached by the interceptor).
  getPending(): Observable<PostSummary[]> {
    return this.http.get<PostSummary[]>(`${this.apiUrl}/pending`);
  }

  publish(id: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/${id}/publish`, {});
  }

  reject(id: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/${id}/reject`, {});
  }

  getPost(id: string): Observable<PostDetail> {
    return this.http.get<PostDetail>(`${this.apiUrl}/${id}`);
  }

  createPost(payload: CreatePostRequest): Observable<CreatePostResponse> {
    return this.http.post<CreatePostResponse>(this.apiUrl, payload);
  }

  updatePost(id: string, payload: UpdatePostRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, payload);
  }

  deletePost(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  addComment(postId: string, payload: AddCommentRequest): Observable<PostComment> {
    return this.http.post<PostComment>(`${this.apiUrl}/${postId}/comments`, payload);
  }

  deleteComment(postId: string, commentId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${postId}/comments/${commentId}`);
  }

  toggleLike(postId: string): Observable<ToggleLikeResponse> {
    return this.http.post<ToggleLikeResponse>(`${this.apiUrl}/${postId}/like`, {});
  }
}
