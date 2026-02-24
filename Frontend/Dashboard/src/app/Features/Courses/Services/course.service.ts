import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from "../../../Core/Services/config.service";
import { Subject } from '../Models/subject.model';

@Injectable({
  providedIn: 'root'
})
export class CourseService {
  private apiUrl: string;

  constructor(private http: HttpClient, private configService: ConfigService) {
    this.apiUrl = this.configService.baseUrl + '/course';
  }

  getAllCourses(): Observable<Subject[]> {
      return this.http.get<Subject[]>(this.apiUrl);
  }

  getCourseById(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }
}