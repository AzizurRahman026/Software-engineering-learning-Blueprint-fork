import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ConfigService } from '../../../Core/Services/config.service';
import {
  AddCommentRequest,
  BlogComment,
  BlogPostDetail,
  BlogPostSummary,
  CreateBlogPostRequest,
  CreatePostResponse,
  ToggleLikeResponse,
  UpdateBlogPostRequest
} from '../Models/blog.model';

@Injectable({ providedIn: 'root' })
export class BlogService {
  private readonly apiUrl: string;

  constructor(private http: HttpClient, private configService: ConfigService) {
    this.apiUrl = this.configService.baseUrl + '/blog';
  }

  getPosts(page = 1, pageSize = 10): Observable<BlogPostSummary[]> {
    return this.http.get<BlogPostSummary[]>(`${this.apiUrl}?page=${page}&pageSize=${pageSize}`);
  }

  getPost(id: string): Observable<BlogPostDetail> {
    return this.http.get<BlogPostDetail>(`${this.apiUrl}/${id}`);
  }

  createPost(payload: CreateBlogPostRequest): Observable<CreatePostResponse> {
    return this.http.post<CreatePostResponse>(this.apiUrl, payload);
  }

  updatePost(id: string, payload: UpdateBlogPostRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, payload);
  }

  deletePost(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  addComment(postId: string, payload: AddCommentRequest): Observable<BlogComment> {
    return this.http.post<BlogComment>(`${this.apiUrl}/${postId}/comments`, payload);
  }

  deleteComment(postId: string, commentId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${postId}/comments/${commentId}`);
  }

  toggleLike(postId: string): Observable<ToggleLikeResponse> {
    return this.http.post<ToggleLikeResponse>(`${this.apiUrl}/${postId}/like`, {});
  }
}
