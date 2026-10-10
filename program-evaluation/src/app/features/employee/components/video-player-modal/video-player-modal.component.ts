import { Component, inject, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

export interface VideoPlayerData {
  programId: string;
  weekId: string;
  title: string;
  duration: string | null;
  weekNumber: number;
}

@Component({
  selector: 'app-video-player-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './video-player-modal.component.html',
  styleUrls: ['./video-player-modal.component.scss']
})
export class VideoPlayerModalComponent {
  private readonly sanitizer: DomSanitizer = inject(DomSanitizer);

  public readonly visible = signal(false);
  public readonly data = signal<VideoPlayerData | null>(null);
  public readonly videoSrc = signal<SafeResourceUrl | string | null>(null);
  public readonly error = signal<string | null>(null);

  private closeCallback: (() => void) | null = null;

  public open(data: VideoPlayerData, videoStreamUrl: SafeResourceUrl | string): void {
    this.data.set(data);
    this.error.set(null);
    this.videoSrc.set(videoStreamUrl);
    this.visible.set(true);
  }

  public close(): void {
    this.visible.set(false);
    this.videoSrc.set(null);
    this.data.set(null);
    this.error.set(null);
    if (this.closeCallback) this.closeCallback();
    this.closeCallback = null;
  }
}
