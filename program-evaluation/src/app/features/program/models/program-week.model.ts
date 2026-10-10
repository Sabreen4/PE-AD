export interface ProgramWeek {
  id: string;
  programId: string;
  weekNumber: number;
  startDate: string;
  endDate: string;
  topic: string | null;
  description: string | null;
  videoName: string | null;
  displayTitle: string | null;
  videoDuration: string | null;
  thumbnailPath: string | null;
  createdAt: string;
  updatedAt: string;
}