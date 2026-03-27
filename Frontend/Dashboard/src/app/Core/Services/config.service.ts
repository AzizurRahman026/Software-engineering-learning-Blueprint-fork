import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment.development';

@Injectable({
    providedIn: 'root'
})
export class ConfigService {
    private readonly _baseUrl = environment.baseUrl;

    constructor() {
        console.log('App is running in production:', environment.production);
        console.log('Base URL:', this._baseUrl);
    }

    get baseUrl(): string {
        return this._baseUrl;
    }
}
