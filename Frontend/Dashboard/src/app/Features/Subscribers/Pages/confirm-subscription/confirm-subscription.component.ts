import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { SubscriberService } from '../../Services/subscriber.service';

@Component({
  selector: 'app-confirm-subscription',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './confirm-subscription.component.html',
  styleUrl: './confirm-subscription.component.scss'
})
export class ConfirmSubscriptionComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly subscriberService = inject(SubscriberService);

  loading = true;
  message = '';
  error = '';

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token') ?? '';
    if (!token) {
      this.loading = false;
      this.error = 'This confirmation link is missing its token.';
      return;
    }

    this.subscriberService.confirm(token).subscribe({
      next: (res) => {
        this.loading = false;
        this.message = res?.message ?? 'Your subscription is confirmed.';
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        this.error = err.error?.message ?? 'This confirmation link is invalid or has expired.';
      }
    });
  }

  goHome(): void {
    this.router.navigate(['/']);
  }
}
