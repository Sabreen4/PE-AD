import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { User } from '@features/identity/models/user.model';

@Component({
  selector: 'app-user-detail-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatIconModule],
  templateUrl: './user-detail-dialog.component.html',
  styleUrls: ['./user-detail-dialog.component.scss']
})
export class UserDetailDialogComponent {
  private ref = inject<MatDialogRef<UserDetailDialogComponent>>(MatDialogRef);
  protected data = inject<User>(MAT_DIALOG_DATA);

  protected get initials(): string {
    return (this.data.firstName?.charAt(0) ?? '') + (this.data.lastName?.charAt(0) ?? '');
  }

  close(): void { this.ref.close(); }
}
