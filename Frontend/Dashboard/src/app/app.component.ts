import { Component, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Store } from '@ngrx/store';
import { init } from './Core/Store/actions/counter.actions';
import { loadSubjects } from './Core/Store/actions/subject.actions';
import { SignalrService } from './Core/Services/signalr.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit{
  protected readonly title = signal('Dashboard');

  constructor(private signalrService: SignalrService, private store: Store){}
  ngOnInit(): void {
    this.store.dispatch(init());
    this.store.dispatch(loadSubjects());
  }
}
