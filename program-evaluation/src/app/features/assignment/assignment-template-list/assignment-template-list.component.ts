import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-assignment-template-list',
  standalone: true,
  imports: [CommonModule, BackNavComponent],
  templateUrl: './assignment-template-list.component.html',
})
export class AssignmentTemplateListComponent {}
