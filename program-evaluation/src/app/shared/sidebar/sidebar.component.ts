import { Component, Input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';

export interface NavItem {
  label: string;
  route: string;
  icon: string;
  badge?: string | number | null;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, MatIconModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss'
})
export class SidebarComponent {
  @Input() public navItems: NavItem[] = [];
  @Input() public brand: string = 'PE&DA';
  @Input() public collapsed: boolean = false;
  @Input() public mobileOpen: boolean = false;

  readonly collapsedChange = output<boolean>();
  readonly mobileClose = output<void>();
  readonly settingsClick = output<void>();

  public toggleCollapsed(): void {
    this.collapsedChange.emit(!this.collapsed);
  }

  public onMobileClose(): void {
    this.mobileClose.emit();
  }
}
