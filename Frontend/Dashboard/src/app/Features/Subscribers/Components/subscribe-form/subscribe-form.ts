import { Component, inject, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { SubscriberService } from '../../Services/subscriber.service';

@Component({
  selector: 'app-subscribe-form',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './subscribe-form.html',
  styleUrl: './subscribe-form.scss'
})
export class SubscribeFormComponent {
  private readonly subscriberService = inject(SubscriberService);

  // Optional heading/description so the same form works in the hero and the footer.
  heading = input<string>('Subscribe to our newsletter');
  description = input<string>('Get new posts delivered straight to your inbox.');

  email = '';
  submitting = false;
  message = '';
  error = '';

  submit(): void {
    if (this.submitting || !this.email.trim()) return;

    this.submitting = true;
    this.message = '';
    this.error = '';

    this.subscriberService.subscribe({ email: this.email.trim() }).subscribe({
      next: (res) => {
        this.submitting = false;
        this.message = res?.message ?? 'Please check your inbox to confirm.';
        this.email = '';
      },
      error: (err: HttpErrorResponse) => {
        this.submitting = false;
        this.error = err.error?.message ?? err.error?.detail ?? 'Something went wrong. Please try again.';
      }
    });
  }
}
