import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from '../../Models/subject.model';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { selectAllSubjects, selectSubjectsError, selectSubjectsLoading } from '../../../../Core/Store/selectors/subject.selectors';
import { AsyncPipe } from '@angular/common';
import { createSubject, deleteSubject, loadSubjects, updateSubject } from '../../../../Core/Store/actions/subject.actions';
import { SignalrService } from '../../../../Core/Services/signalr.service';

@Component({
  selector: 'app-subjects-component',
  standalone: true,
  imports: [FormsModule, AsyncPipe],
  templateUrl: './subjects-component.html',
  styleUrl: './subjects-component.scss',
})
export class SubjectsComponent implements OnInit {
  subjects$: Observable<Subject[]>;
  loading$: Observable<boolean>;
  error$: Observable<string | null>;

  selectedSubject: Subject | null = null;
  isEditing = false;
  isCreating = false;
  editingSubjectId: string | null = null;

  newSubject: Subject = {
    id: '',
    name: '',
    description: ''
  };

  constructor(private store: Store, private router: Router) {
    console.log('SubjectsComponent instantiated');
    this.subjects$ = this.store.select(selectAllSubjects);
    this.loading$ = this.store.select(selectSubjectsLoading);
    this.error$ = this.store.select(selectSubjectsError);
  }

  ngOnInit(): void {
    console.log('SubjectsComponent initialized');
    this.store.dispatch(loadSubjects());
  }

  startCreate(): void {
    console.log('Starting subject creation...');
    this.isCreating = true;
    this.isEditing = false;
    this.editingSubjectId = null;
    this.newSubject = { id: '', name: '', description: '' };
    this.selectedSubject = null;
  }

  cancelCreate() {
    this.isCreating = false;
    this.newSubject = { id: '', name: '', description: '' };
  }

  createSubject(): void {
    this.store.dispatch(createSubject({ subject: this.newSubject }));
    this.isCreating = false;
    this.newSubject = { id: '', name: '', description: '' };
  }

  cancelEdit(): void {
    this.isEditing = false;
    this.editingSubjectId = null;
    this.newSubject = { id: '', name: '', description: '' };
  }

  editSubject(subject: Subject): void {
    this.isEditing = true;
    this.isCreating = false;
    this.editingSubjectId = subject.id;
    this.newSubject = { ...subject };
  }

  updateSubject(): void {
    if (!this.editingSubjectId) return;
    this.store.dispatch(updateSubject({ id: this.editingSubjectId, subject: this.newSubject }));
    this.isEditing = false;
    this.editingSubjectId = null;
    this.newSubject = { id: '', name: '', description: '' };
  }

  deleteSubject(subjectId: string): void {
    if (confirm('Are you sure you want to delete this subject?')) {
      this.store.dispatch(deleteSubject({ id: subjectId }));
      this.selectedSubject = null;
    }
  }
}
