import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-employee-assignment-list',
  standalone: true,
  imports: [CommonModule, BackNavComponent],
  templateUrl: './employee-assignment-list.component.html',
})
export class EmployeeAssignmentListComponent {}
