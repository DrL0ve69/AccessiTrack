export interface AuthResponse {
  token: string;
  userId: string;
  email: string;
  role: 'Admin' | 'Member';
  expiresAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  userName: string;
  email: string;
  password: string;
}

export interface User {
  userId: string;
  email: string;
  role: 'Admin' | 'Member';
}

export interface UserProfile {
  id: string;
  identityId: string;
  fullName: string;
  email: string;
  preferredLanguage: string;
  highContrastEnabled: boolean;
  createdAt: string;
}

/**
 * Extended User Profile with full details
 */
export interface UserProfileDto {
  id: string;
  userId: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  avatar?: string;
  bio?: string;
  preferredLanguage: string;
  highContrastEnabled: boolean;
  role: 'Admin' | 'Member';

  // Privacy flags
  emailPrivate: boolean;
  phonePrivate: boolean;
  bioPrivate: boolean;

  createdAt: string;
  updatedAt?: string;
}

/**
 * Public user summary for members list (respects privacy)
 */
export interface UserSummaryDto {
  userId: string;
  email?: string; // Null/undefined if marked private
  fullName: string;
  avatar?: string;
  bio?: string;
  role: 'Admin' | 'Member';
}

/**
 * Request to update user profile
 * Note: role changes use separate endpoint
 */
export interface UpdateUserProfileRequest {
  fullName?: string;
  phoneNumber?: string;
  bio?: string;
  preferredLanguage?: string;
  highContrastEnabled?: boolean;
  emailPrivate?: boolean;
  phonePrivate?: boolean;
  bioPrivate?: boolean;
}

/**
 * Request to update user role (admin only)
 */
export interface UpdateUserRoleRequest {
  role: 'Admin' | 'Member';
}

/**
 * Paginated users response
 */
export interface PaginatedUsersResponse {
  users: UserSummaryDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

