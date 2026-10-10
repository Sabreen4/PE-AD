import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { FormFieldConfig } from '@shared/common/entity-types';
export interface EntityFormDialogData {
  title: string;
  fields: FormFieldConfig[];
  model?: Record<string, string | number | boolean | null | undefined>;
  submitText?: string;
}

@Component({
  selector: 'app-entity-form-dialog',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatCheckboxModule, MatButtonModule, MatDatepickerModule
  ],
  templateUrl: './entity-form-dialog.component.html',
  styleUrl: './entity-form-dialog.component.scss'
})
export class EntityFormDialogComponent {
  private fb = inject(FormBuilder);
  private ref = inject<MatDialogRef<EntityFormDialogComponent, Record<string, string | number | boolean | null | undefined> | undefined>>(MatDialogRef);
  data = inject<EntityFormDialogData>(MAT_DIALOG_DATA);

  form = this.fb.group(
    Object.fromEntries(
      this.data.fields.map((field) => [
        field.key,
        this.fb.control(
          { value: this.data.model?.[field.key] ?? (field.type === 'checkbox' ? false : ''), disabled: !!field.disabled },
          field.required ? [Validators.required] : []
        )
      ])
    )
  );

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.ref.close(this.form.getRawValue());
  }

  cancel(): void {
    this.ref.close(undefined);
  }
}