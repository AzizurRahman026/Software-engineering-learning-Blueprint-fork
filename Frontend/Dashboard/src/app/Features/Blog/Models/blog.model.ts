export interface BlogPostSummary {
  id: string;
  title: string;
  summary: string;
  authorId: string;
  authorUsername: string;
  createdAt: string;
  updatedAt?: string | null;
  likeCount: number;
  commentCount: number;
}

export interface BlogComment {
  id: string;
  blogPostId: string;
  authorId: string;
  authorUsername: string;
  content: string;
  createdAt: string;
}

export interface BlogPostDetail {
  id: string;
  title: string;
  content: string;
  summary: string;
  authorId: string;
  authorUsername: string;
  createdAt: string;
  updatedAt?: string | null;
  likeCount: number;
  commentCount: number;
  likedByCurrentUser: boolean;
  comments: BlogComment[];
}

export interface CreateBlogPostRequest {
  title: string;
  content: string;
  summary?: string;
}

export interface UpdateBlogPostRequest {
  title: string;
  content: string;
  summary?: string;
}

export interface AddCommentRequest {
  content: string;
}

export interface ToggleLikeResponse {
  liked: boolean;
  likeCount: number;
}

export interface CreatePostResponse {
  id: string;
}
