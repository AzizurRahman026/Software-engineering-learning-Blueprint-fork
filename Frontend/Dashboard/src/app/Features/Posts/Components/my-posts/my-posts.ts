import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { PostService } from '../../Services/post.service';
import { PostSummary } from '../../Models/post.model';
import { AuthService } from '../../../Auth/Services/auth.service';
import { PostCardComponent } from '../post-card/post-card';

@Component({
  selector: 'app-my-posts',
  standalone: true,
  imports: [PostCardComponent],
  templateUrl: './my-posts.html',
  styleUrl: './my-posts.scss'
})
export class MyPostsComponent implements OnInit {
  posts: PostSummary[] = [];
  loading = false;
  error = '';

  constructor(
    private postService: PostService,
    private router: Router,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    // The endpoint is author-scoped and requires a token; bounce anonymous visitors.
    if (!this.authService.currentUser()) {
      this.router.navigate(['/posts']);
      return;
    }
    this.loadPosts();
  }

  loadPosts(): void {
    this.loading = true;
    this.error = '';
    this.postService.getMyPosts().subscribe({
      next: (posts) => {
        this.posts = posts;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message ?? 'Failed to load your posts.';
        this.loading = false;
      }
    });
  }

  openPost(id: string): void {
    this.router.navigate(['/posts', id]);
  }

  startCreate(): void {
    this.router.navigate(['/posts/create']);
  }
}
