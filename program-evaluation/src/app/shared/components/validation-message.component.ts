import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl } from '@angular/forms';

@Component({
  selector: 'app-validation-message',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './validation-message.component.html',
  styleUrls: ['./validation-message.component.scss']
})
export class ValidationMessageComponent {
  @Input({ required: true }) public control!: AbstractControl;
  @Input({ required: true }) public message!: string;

  public shouldShow(): boolean {
    return this.control.invalid && (this.control.dirty || this.control.touched);
  }
}
