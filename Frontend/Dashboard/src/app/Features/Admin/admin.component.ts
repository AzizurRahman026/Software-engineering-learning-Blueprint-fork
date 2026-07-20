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
import { UserRole, UserSummary } from '../Auth/Models/auth.model';
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

  // Roles (SuperAdmin only): searchable user list + inline role assignment.
  users: UserSummary[] = [];
  usersLoaded = false;
  usersLoading = false;
  userSearch = '';
  // Per-user target role chosen in the dropdown, keyed by user id.
  roleSelection: Record<string, UserRole> = {};
  // Id of the row currently being saved (disables its button).
  assigningId: string | null = null;
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
    if (tab === 'roles' && !this.usersLoaded) {
      this.loadUsers();
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
  loadUsers(): void {
    this.usersLoading = true;
    this.roleError = '';
    this.authService.getUsers().subscribe({
      next: (users) => {
        this.users = users;
        // Seed each row's dropdown: promote a plain User to Admin, otherwise offer to demote to User.
        this.roleSelection = {};
        for (const u of users) {
          this.roleSelection[u.id] = u.role === 'User' ? 'Admin' : 'User';
        }
        this.usersLoaded = true;
        this.usersLoading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.roleError = err.error?.detail ?? err.error?.message ?? 'Failed to load users.';
        this.usersLoading = false;
      }
    });
  }

  get filteredUsers(): UserSummary[] {
    const q = this.userSearch.trim().toLowerCase();
    if (!q) return this.users;
    return this.users.filter(
      (u) => u.username.toLowerCase().includes(q) || u.email.toLowerCase().includes(q)
    );
  }

  assignRole(user: UserSummary): void {
    this.roleMessage = '';
    this.roleError = '';
    const role = this.roleSelection[user.id];
    if (!role || role === user.role) return;

    this.assigningId = user.id;
    this.authService.assignRole(user.id, role).subscribe({
      next: (updated) => {
        user.role = updated.role;
        this.roleSelection[user.id] = updated.role === 'User' ? 'Admin' : 'User';
        this.roleMessage = `${updated.username} is now ${updated.role}.`;
        this.assigningId = null;
      },
      error: (err: HttpErrorResponse) => {
        this.roleError = err.error?.detail ?? err.error?.message ?? 'Failed to assign role.';
        this.assigningId = null;
      }
    });
  }
}
