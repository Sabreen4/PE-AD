import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-submission-list',
  standalone: true,
  imports: [CommonModule, BackNavComponent],
  templateUrl: './submission-list.component.html',
})
export class SubmissionListComponent {}
