import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { SubscriberService } from '../../Services/subscriber.service';

@Component({
  selector: 'app-unsubscribe',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './unsubscribe.component.html',
  styleUrl: './unsubscribe.component.scss'
})
export class UnsubscribeComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly subscriberService = inject(SubscriberService);

  loading = true;
  message = '';
  error = '';

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token') ?? '';
    if (!token) {
      this.loading = false;
      this.error = 'This unsubscribe link is missing its token.';
      return;
    }

    this.subscriberService.unsubscribe(token).subscribe({
      next: (res) => {
        this.loading = false;
        this.message = res?.message ?? 'You have been unsubscribed.';
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        this.error = err.error?.message ?? 'This unsubscribe link is invalid.';
      }
    });
  }
}
