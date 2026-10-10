export const IDENTITY_ENDPOINTS = {
  login: '/auth/login',
  verifyOtp: '/auth/verify-otp',
  refreshToken: '/auth/refresh-token',
  logout: '/auth/logout',
  users: '/user',
  userById: (id: string): string => `/user/${id}`,
  organizations: '/organization',
  organizationById: (id: number): string => `/organization/${id}`,
};
