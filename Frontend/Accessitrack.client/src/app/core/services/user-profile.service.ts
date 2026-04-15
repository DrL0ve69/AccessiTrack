import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  UserProfileDto,
  UserSummaryDto,
  UpdateUserProfileRequest,
  UpdateUserRoleRequest,
  PaginatedUsersResponse,
} from '../models/auth.model';

/**
 * UserProfileService - Manages user profile CRUD operations.
 *
 * Provides methods for:
 * - Fetching current user profile
 * - Fetching other users' profiles (with privacy filtering)
 * - Listing all users (paginated, searchable)
 * - Updating user profiles
 * - Managing user roles (admin only)
 * - Uploading avatars
 * - Deleting user profiles (soft delete)
 */
@Injectable({
  providedIn: 'root',
})
export class UserProfileService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/userprofile`;

  /**
   * Get current authenticated user's full profile
   */
  getCurrentProfile(): Observable<UserProfileDto> {
    return this.http.get<UserProfileDto>(`${this.apiUrl}/profile`);
  }

  /**
   * Get specific user's profile by ID
   * Private fields are hidden unless user is viewing own profile or is admin
   */
  getProfileById(userId: string): Observable<UserProfileDto> {
    return this.http.get<UserProfileDto>(`${this.apiUrl}/${userId}`);
  }

  /**
   * Get list of all active users with search and filtering
   * @param searchTerm - Search by full name
   * @param roleFilter - Filter by role: 'Admin' | 'Member'
   * @param page - Page number (1-based)
   * @param pageSize - Items per page
   */
  getAllUsers(
    searchTerm?: string,
    roleFilter?: 'Admin' | 'Member',
    page: number = 1,
    pageSize: number = 20
  ): Observable<PaginatedUsersResponse> {
    let params = new URLSearchParams();
    if (searchTerm) params.append('search', searchTerm);
    if (roleFilter) params.append('role', roleFilter);
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());

    return this.http.get<PaginatedUsersResponse>(
      `${this.apiUrl}?${params.toString()}`
    );
  }

  /**
   * Update user profile (non-role fields)
   * @param userId - User ID to update
   * @param data - Update request
   */
  updateProfile(
    userId: string,
    data: UpdateUserProfileRequest
  ): Observable<UserProfileDto> {
    return this.http.put<UserProfileDto>(`${this.apiUrl}/${userId}`, data);
  }

  /**
   * Upload avatar for user
   * @param userId - User ID
   * @param file - Image file to upload
   */
  uploadAvatar(userId: string, file: File): Observable<{ avatarUrl: string }> {
    const formData = new FormData();
    formData.append('avatar', file);
    return this.http.post<{ avatarUrl: string }>(
      `${this.apiUrl}/${userId}/avatar`,
      formData
    );
  }

  /**
   * Update user role (admin only)
   * @param userId - User ID to update
   * @param role - New role
   */
  updateUserRole(
    userId: string,
    role: 'Admin' | 'Member'
  ): Observable<UserProfileDto> {
    return this.http.post<UserProfileDto>(`${this.apiUrl}/${userId}/role`, {
      role,
    } as UpdateUserRoleRequest);
  }

  /**
   * Soft delete user profile
   * User will no longer appear in members list
   * @param userId - User ID to delete
   */
  deleteProfile(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${userId}`);
  }
}
