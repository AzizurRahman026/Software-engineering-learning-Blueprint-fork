import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SubjectService } from '../../../Features/Courses/Services/subject.service';
import { Subject } from '../../../Features/Courses/Models/subject.model';

@Component({
  selector: 'app-header-component',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './header-component.html',
  styleUrl: './header-component.scss',
})
export class HeaderComponent implements OnInit{
  subjects: Subject[] = [];
  
  constructor(private subjectService: SubjectService,
              private cdr: ChangeDetectorRef) {
  }
  ngOnInit(): void {
    this.loadSubjects();
  }

  loadSubjects(): void {
    this.subjectService.getAllSubjects().subscribe({
      next: (data) => {
        this.subjects = data;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading subjects:', err);
      }
    })
  }
}
