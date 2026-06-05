import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { routes } from './app.routes';
import { subjectReducer } from './Core/Store/reducers/subject.reducer';
import { SubjectEffects } from './Core/Store/effects/subject.effects';
import { userIdInterceptor } from './Core/Interceptors/user-id.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([userIdInterceptor])),
    provideStore({
      subjects: subjectReducer
    }),
    provideEffects([SubjectEffects])
  ]
};
