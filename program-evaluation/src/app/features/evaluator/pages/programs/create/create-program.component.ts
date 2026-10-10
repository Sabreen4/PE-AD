import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { ProgramService } from '@features/program/services/program.service';
import { EnrollmentService } from '@features/program/services/enrollment.service';
import { UserService } from '@features/identity/services/user.service';
import { AuthenticationStateService } from '@core/authentication/authentication-state.service';
import { User } from '@features/identity/models/user.model';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastService } from '@shared/toast/toast.service';
import { DebouncedSearchDirective } from '@shared/directives/debounced-search.directive';
import { BackNavComponent } from '@shared/components/back-nav/back-nav.component';

@Component({
  selector: 'app-create-program',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink, DebouncedSearchDirective, BackNavComponent],
  templateUrl: './create-program.component.html',
  styleUrl: './create-program.component.scss'
})
export class CreateProgramComponent implements OnInit {
  private readonly fb: FormBuilder = inject(FormBuilder);
  private readonly programService: ProgramService = inject(ProgramService);
  private readonly enrollmentService: EnrollmentService = inject(EnrollmentService);
  private readonly userService: UserService = inject(UserService);
  private readonly authState: AuthenticationStateService = inject(AuthenticationStateService);
  private readonly router: Router = inject(Router);
  private readonly toast: ToastService = inject(ToastService);

  public readonly isSubmitting = signal<boolean>(false);
  public readonly isLoadingEmployees = signal<boolean>(false);

  public readonly employees = signal<User[]>([]);
  public readonly selectedEmployeeIds = signal<Set<string>>(new Set());
  public readonly employeeSearchQuery = signal<string>('');

  public programForm: FormGroup = this.fb.group({
    name: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    description: new FormControl<string>('', { nonNullable: true }),
    durationWeeks: new FormControl<number>(4, { nonNullable: true, validators: [Validators.required, Validators.min(1)] }),
  });

  public ngOnInit(): void {
    this.loadEmployees();
  }

  public get f(): Record<string, FormControl> {
    return this.programForm.controls as Record<string, FormControl>;
  }

  public get filteredEmployees(): User[] {
    const query = this.employeeSearchQuery().toLowerCase().trim();
    if (!query) return this.employees();
    return this.employees().filter(
      (e) =>
        e.firstName.toLowerCase().includes(query) ||
        e.lastName.toLowerCase().includes(query) ||
        e.email.toLowerCase().includes(query)
    );
  }

  public toggleEmployee(id: string): void {
    this.selectedEmployeeIds.update((set) => {
      const next = new Set(set);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }

  public selectAll(): void {
    this.selectedEmployeeIds.update(() => new Set(this.employees().map((e) => e.id)));
  }

  public deselectAll(): void {
    this.selectedEmployeeIds.update(() => new Set());
  }

  private loadEmployees(): void {
    const userOrgId = this.authState.user()?.organizationId;
    const orgId = userOrgId ? Number(userOrgId) : null;

    this.isLoadingEmployees.set(true);
    this.userService.list().subscribe({
      next: (res) => {
        const filtered = res.items.filter((u) => {
          const r = (u.role || '').toLowerCase();
          const roles = (u.roles || []).map((x) => x.toLowerCase());
          const isEmp = r === 'employee' || roles.includes('employee');
          const isUserActive = u.isActive !== false;
          const uOrgId = u.organizationId ? Number(u.organizationId) : null;
          const matchesOrg = !orgId || (uOrgId !== null && uOrgId === orgId);
          return isEmp && isUserActive && matchesOrg;
        });
        this.employees.set(filtered);
        this.isLoadingEmployees.set(false);
      },
      error: () => {
        this.isLoadingEmployees.set(false);
      },
    });
  }

  public onSubmit(): void {
    if (this.programForm.invalid || this.isSubmitting()) return;

    this.isSubmitting.set(true);

    const formValue = this.programForm.value as { name: string; description: string; durationWeeks: number };

    this.programService.create({
      name: formValue.name,
      description: formValue.description || null,
      durationWeeks: formValue.durationWeeks,
    }).subscribe({
      next: (program) => {
        const selectedIds: string[] = Array.from(this.selectedEmployeeIds());
        if (selectedIds.length > 0) {
          this.enrollmentService.enroll(program.id, selectedIds).subscribe({
            next: () => {
              this.isSubmitting.set(false);
              this.toast.success('Program created', 'Program created with enrolled employees.');
              this.router.navigate(['/evaluator/programs', program.id]);
            },
            error: () => {
              this.isSubmitting.set(false);
              this.toast.warning('Program created', 'Program created, but some enrollments failed.');
              this.router.navigate(['/evaluator/programs', program.id]);
            },
          });
        } else {
          this.isSubmitting.set(false);
          this.toast.success('Program created', 'Program created successfully.');
          this.router.navigate(['/evaluator/programs', program.id]);
        }
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        this.toast.error('Failed to create program', err.error?.message || 'Failed to create program.');
      },
    });
  }
}
