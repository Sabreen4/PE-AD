export interface Attendance {
  id: string;
  employeeId: string;
  employeeName: string;
  date: string;
  status: string;
  checkIn: string | null;
  checkOut: string | null;
  hoursWorked: number | null;
  notes: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface AttendanceFilter {
  employeeId?: string;
  dateFrom?: string;
  dateTo?: string;
  status?: string;
}
