import { Component, OnInit, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { PostService } from '../Posts/Services/post.service';
import { PostSummary } from '../Posts/Models/post.model';
import { SubscriberService } from '../Subscribers/Services/subscriber.service';
import { Subscriber } from '../Subscribers/Models/subscriber.model';
import { AuthService } from '../Auth/Services/auth.service';
import { UserRole } from '../Auth/Models/auth.model';
import { ConfirmDialogComponent } from '../../Shared/Components/confirm-dialog/confirm-dialog.component';

type AdminTab = 'moderation' | 'subscribers' | 'roles';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [DatePipe, FormsModule, RouterLink, ConfirmDialogComponent],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.scss'
})
export class AdminComponent implements OnInit {
  private readonly postService = inject(PostService);
  private readonly subscriberService = inject(SubscriberService);
  readonly authService = inject(AuthService);

  tab: AdminTab = 'moderation';

  pending: PostSummary[] = [];
  pendingLoading = false;
  pendingError = '';

  subscribers: Subscriber[] = [];
  subscribersLoaded = false;

  // Confirm-dialog state for a reject action.
  rejectTarget: PostSummary | null = null;

  // Roles form (SuperAdmin only).
  roleUserId = '';
  roleValue: UserRole = 'Admin';
  roleMessage = '';
  roleError = '';

  ngOnInit(): void {
    this.loadPending();
  }

  setTab(tab: AdminTab): void {
    this.tab = tab;
    if (tab === 'subscribers' && !this.subscribersLoaded) {
      this.loadSubscribers();
    }
  }

  // ── Moderation ──────────────────────────────────────────────
  loadPending(): void {
    this.pendingLoading = true;
    this.pendingError = '';
    this.postService.getPending().subscribe({
      next: (posts) => {
        this.pending = posts;
        this.pendingLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.pendingError = err.error?.message ?? 'Failed to load pending posts.';
        this.pendingLoading = false;
      }
    });
  }

  publish(post: PostSummary): void {
    this.postService.publish(post.id).subscribe({
      next: () => (this.pending = this.pending.filter((p) => p.id !== post.id))
    });
  }

  requestReject(post: PostSummary): void {
    this.rejectTarget = post;
  }

  confirmReject(): void {
    const target = this.rejectTarget;
    if (!target) return;
    this.postService.reject(target.id).subscribe({
      next: () => {
        this.pending = this.pending.filter((p) => p.id !== target.id);
        this.rejectTarget = null;
      }
    });
  }

  cancelReject(): void {
    this.rejectTarget = null;
  }

  // ── Subscribers ─────────────────────────────────────────────
  loadSubscribers(): void {
    this.subscriberService.getAll().subscribe({
      next: (subs) => {
        this.subscribers = subs;
        this.subscribersLoaded = true;
      }
    });
  }

  // ── Roles (SuperAdmin) ──────────────────────────────────────
  assignRole(): void {
    this.roleMessage = '';
    this.roleError = '';
    if (!this.roleUserId.trim()) {
      this.roleError = 'User id is required.';
      return;
    }
    this.authService.assignRole(this.roleUserId.trim(), this.roleValue).subscribe({
      next: (user) => (this.roleMessage = `${user.username} is now ${user.role}.`),
      error: (err: HttpErrorResponse) => (this.roleError = err.error?.message ?? 'Failed to assign role.')
    });
  }
}
