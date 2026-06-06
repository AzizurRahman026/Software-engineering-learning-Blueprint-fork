import { Component, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { BlogService } from '../../Services/blog.service';
import { BlogPostSummary } from '../../Models/blog.model';
import { AuthService } from '../../../Auth/Services/auth.service';
import { AuthModalService } from '../../../Auth/Services/auth-modal.service';

@Component({
  selector: 'app-blog-list',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './blog-list.html',
  styleUrl: './blog-list.scss'
})
export class BlogListComponent implements OnInit {
  posts: BlogPostSummary[] = [];
  loading = false;
  error = '';

  constructor(
    private blogService: BlogService,
    private router: Router,
    public authService: AuthService,
    private authModal: AuthModalService
  ) {}

  ngOnInit(): void {
    this.loadPosts();
  }

  loadPosts(): void {
    this.loading = true;
    this.error = '';
    this.blogService.getPosts().subscribe({
      next: (posts) => {
        this.posts = posts;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message ?? 'Failed to load posts.';
        this.loading = false;
      }
    });
  }

  openPost(id: string): void {
    this.router.navigate(['/blog', id]);
  }

  startCreate(): void {
    // Creating requires a logged-in author; prompt sign-in otherwise.
    if (!this.authService.currentUser()) {
      this.authModal.open('login');
      return;
    }
    this.router.navigate(['/blog/create']);
  }
}
