import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { EnrollmentService } from '@features/program/services/enrollment.service';
import { Enrollment } from '@features/program/models/enrollment.model';
import { TableColumnConfig } from '@shared/common/entity-types';
import { EntityTableComponent } from '@shared/components/entity-table/entity-table.component';
import { EntityFormDialogComponent } from '@shared/components/entity-form-dialog/entity-form-dialog.component';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-enrollment',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, EntityTableComponent, BackNavComponent],
  templateUrl: './enrollment.component.html',
})
export class EnrollmentComponent implements OnInit {
  private enrollmentService = inject(EnrollmentService);
  private route = inject(ActivatedRoute);
  private dialog = inject(MatDialog);

  loading = signal(true);
  enrollments = signal<Enrollment[]>([]);
  programId!: string;

  columns: TableColumnConfig<Enrollment>[] = [
    { key: 'employeeName', label: 'Employee' },
    { key: 'employeeCode', label: 'Code', mono: true },
    { key: 'enrolledAt', label: 'Enrolled', formatter: (r: Enrollment) => new Date(r.enrolledAt).toLocaleDateString() },
    { key: 'isActive', label: 'Status', formatter: (r: Enrollment) => r.isActive ? 'Active' : 'Completed' },
    { key: 'completedAt', label: 'Completed', formatter: (r: Enrollment) => r.completedAt ? new Date(r.completedAt).toLocaleDateString() : '—' }
  ];

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('programId') || this.route.parent?.snapshot.paramMap.get('id');
    if (!id) return;
    this.programId = id;
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.enrollmentService.listByProgram(this.programId, { pageSize: 100 }).subscribe({
      next: (result) => { this.enrollments.set(result.items); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openEnroll(): void {
    const ref = this.dialog.open(EntityFormDialogComponent, {
      width: '480px',
      data: {
        title: 'Enroll employees',
        fields: [{ key: 'employeeIds', label: 'Employee IDs (comma-separated)', type: 'text', required: true }],
        submitText: 'Enroll'
      }
    });
    ref.afterClosed().subscribe((value) => {
      if (!value) return;
      const ids = String(value['employeeIds']).split(',').map((s: string) => s.trim()).filter(Boolean);
      this.enrollmentService.enroll(this.programId, ids).subscribe(() => {
        this.load();
      });
    });
  }

  remove(enrollment: Enrollment): void {
    this.enrollmentService.unenroll(this.programId, enrollment.employeeId).subscribe(() => {
      this.load();
    });
  }
}