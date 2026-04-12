# AccessiTrack Profile Management System - Implementation Guide

## ✅ Implementation Complete

All changes have been successfully implemented across frontend and backend. This guide covers what was built and the next steps to verify everything works.

---

## 🎯 What Was Built

### Frontend Changes (Angular 21+)

#### 1. **Fixed Issues**
- `src/app/features/auth/login/login.component.ts` - Fixed `styleUrls` → `styleUrl`
- `src/app/features/auth/register/register.component.ts` - Fixed `styleUrls` → `styleUrl`
- Completed navbar component files (main-nav.ts, main-nav.html, main-nav.scss)

#### 2. **Navigation**
- **MainNavComponent** (`src/app/shared/layout/main-nav/main-nav.ts`)
  - Responsive navbar with mobile hamburger menu
  - Links: Dashboard, Projects, Members, Admin (admin only)
  - User dropdown with profile and logout
  - Accessibility features (ARIA labels, keyboard navigation)

#### 3. **Profile Management**
- **UserProfileService** (`src/app/core/services/user-profile.service.ts`)
  - `getCurrentProfile()` - Fetch current user's full profile
  - `getProfileById(userId)` - Fetch any user's profile (privacy-filtered)
  - `getAllUsers(searchTerm, roleFilter, page, pageSize)` - List all users with pagination
  - `updateProfile(userId, data)` - Update profile fields
  - `uploadAvatar(userId, file)` - Upload avatar (multipart)
  - `updateUserRole(userId, role)` - Admin only, change user roles
  - `deleteProfile(userId)` - Soft delete profile

#### 4. **Components**
- **UserProfileComponent** (`src/app/features/profile/user-profile.ts`)
  - Edit own profile: name, phone, bio, preferences
  - Avatar upload with preview
  - Privacy toggles for: email, phone, bio
  - Delete account with confirmation
  - Form validation and error handling

- **MembersListComponent** (`src/app/features/members/members-list.component.ts`)
  - List all users (paginated)
  - Search by name (real-time)
  - Filter by role (Admin/Member)
  - Pagination: 10, 20, or 50 per page
  - User cards with avatar previews

- **MemberDetailComponent** (`src/app/features/members/member-detail.component.ts`)
  - View any user's public profile
  - Privacy-filtered fields (hidden if marked private)
  - Edit button visible to: own profile owner, admins
  - Avatar display with fallback initials

#### 5. **Routing Updates**
- `src/app/app.routes.ts` - Added `/members` routes:
  - `GET /members` → MembersListComponent
  - `GET /members/:userId` → MemberDetailComponent
- Login/Register now redirect to `/profile` (not `/dashboard`)

#### 6. **Models**
- `src/app/core/models/auth.model.ts` - Extended with:
  - `UserProfileDto` - Full profile with privacy flags
  - `UserSummaryDto` - Public summary (respects privacy)
  - `UpdateUserProfileRequest` - Update DTO (no role field)
  - `UpdateUserRoleRequest` - Role change DTO
  - `PaginatedUsersResponse` - Paginated users list

---

### Backend Changes (.NET 10)

#### 1. **Domain Entity**
- `Domain/Entities/UserProfile.cs` - Extended with:
  - `PhoneNumber` (string?, nullable)
  - `Avatar` (string?, nullable)
  - `Bio` (string?, nullable)
  - `UpdatedAt` (DateTime?, nullable)
  - `EmailPrivate`, `PhonePrivate`, `BioPrivate` (bool flags)
  - `IsDeleted`, `DeletedAt` (soft delete support)

#### 2. **DTOs**
- `Application/Features/UserProfile/DTOs/UserProfileDto.cs`:
  - `UserProfileDto` - Full profile response
  - `UserSummaryDto` - Public summary
  - `UpdateUserProfileRequest` - Update request
  - `UpdateUserRoleRequest` - Role update request
  - `PaginatedUsersResponse` - Paginated response

#### 3. **CQRS Handlers**

**Queries** (`Application/Features/UserProfile/Queries/UserProfileQueries.cs`):
- `GetCurrentUserProfileQuery` - Current user's full profile
- `GetUserProfileByIdQuery` - Any user's profile (privacy-filtered)
- `GetAllUsersQuery` - Paginated, searchable user list
- `CanUpdateUserQuery` - Authorization check

**Commands** (`Application/Features/UserProfile/Commands/UserProfileCommands.cs`):
- `UpdateUserProfileCommand` - Update profile fields (no role)
- `UploadAvatarCommand` - Upload and store avatar
- `UpdateUserRoleCommand` - Admin only, change roles
- `DeleteUserProfileCommand` - Soft delete profile

#### 4. **Controller**
- `API/Controllers/UserProfileController.cs` - 6 endpoints:
  - `GET /api/userprofile/me` - Current user's profile
  - `GET /api/userprofile/{userId}` - Specific user's profile
  - `GET /api/userprofile` - List all users (paginated, searchable)
  - `PUT /api/userprofile/{userId}` - Update profile
  - `POST /api/userprofile/{userId}/avatar` - Upload avatar
  - `POST /api/userprofile/{userId}/role` - Change role (admin only)
  - `DELETE /api/userprofile/{userId}` - Soft delete profile

#### 5. **Services**
- `Infrastructure/Services/FileStorageService.cs`:
  - Validates file (size, type)
  - Stores avatars in `wwwroot/uploads/avatars/`
  - Returns accessible URLs
  - Handles file deletion on profile update/delete

#### 6. **Dependency Injection**
- `Infrastructure/DependencyInjection.cs` - Registered:
  - `IFileStorageService` → `FileStorageService` (Scoped)

---

## 🚀 Next Steps to Make It Work

### 1. **Create EF Core Migration** (Required)

Run from backend root (`AccessiTrack/Backend/`):

```bash
# Add migration for UserProfile schema changes
dotnet ef migrations add AddUserProfileExtensions --project AccessiTrack.Infrastructure --startup-project AccessiTrack.API

# Apply migration to database
dotnet ef database update --project AccessiTrack.Infrastructure --startup-project AccessiTrack.API
```

**What it adds to DB:**
- `PhoneNumber` (varchar(20), nullable)
- `Avatar` (varchar(500), nullable)
- `Bio` (varchar(500), nullable)
- `UpdatedAt` (datetime2, nullable)
- `EmailPrivate`, `PhonePrivate`, `BioPrivate` (bit, default 0)
- `IsDeleted` (bit, default 0)
- `DeletedAt` (datetime2, nullable)

### 2. **Create wwwroot/uploads Folder**

In the API project root:
```bash
mkdir -p wwwroot/uploads/avatars
```

**Why:** Avatar uploads need somewhere to store files locally.

### 3. **Verify Configuration**

Ensure `appsettings.json` has:
```json
{
  "Jwt": {
    "Key": "your-secret-key-min-32-chars",
    "Issuer": "AccessiTrack.API",
    "Audience": "AccessiTrack.Client"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AccessiTrackDb;Trusted_Connection=true;"
  }
}
```

### 4. **Test the Implementation**

#### Backend Testing:
```bash
# Start backend
cd AccessiTrack/Backend
dotnet run --project AccessiTrack.API

# Test endpoints (using curl or Postman)
# 1. Login to get token
POST /api/auth/login
Content-Type: application/json
{
  "email": "admin@accessitrack.com",
  "password": "Admin@123!"
}

# 2. Get your profile (use token from login)
GET /api/userprofile/me
Authorization: Bearer {token}

# 3. List all users
GET /api/userprofile?page=1&pageSize=20
Authorization: Bearer {token}

# 4. Update profile
PUT /api/userprofile/{userId}
Content-Type: application/json
{
  "fullName": "John Doe",
  "phoneNumber": "+1 (555) 123-4567",
  "bio": "I love WCAG compliance!",
  "emailPrivate": true,
  "highContrastEnabled": true
}

# 5. Upload avatar
POST /api/userprofile/{userId}/avatar
Content-Type: multipart/form-data
{
  "file": [binary image data]
}
```

#### Frontend Testing:
```bash
# Start frontend
cd AccessiTrack/Frontend/Accessitrack.client
npm start

# Test flow:
# 1. Login at http://localhost:4200/login
#    → Redirects to /profile
# 2. Edit profile, upload avatar, toggle privacy
# 3. Save changes
# 4. Navigate to /members
# 5. Search for users, filter by role, paginate
# 6. Click member name → View their profile
# 7. As admin, update member role
# 8. Soft delete profile → Removed from members list
```

---

## 📋 Authorization Rules

| Action | Endpoint | Self | Admin | Member |
|--------|----------|------|-------|--------|
| View own profile | GET /me | ✅ | ✅ | ✅ |
| View other's profile | GET /{id} | N/A | ✅ | ✅ |
| Edit own profile | PUT /{id} | ✅ | ✅ | ✅ |
| Edit other's profile | PUT /{id} | ❌ | ✅ | ❌ |
| Change role | POST /{id}/role | ❌ | ✅ | ❌ |
| Soft delete own profile | DELETE /{id} | ✅ | ✅ | ✅ |
| Soft delete other's profile | DELETE /{id} | ❌ | ✅ | ❌ |
| List all users | GET / | ✅ | ✅ | ✅ |

---

## 🔒 Privacy Features

Users can mark these fields as private:
- **Email** - Hidden from non-owner/non-admin users
- **Phone Number** - Hidden from non-owner/non-admin users
- **Bio** - Hidden from non-owner/non-admin users

Admins can always see all fields regardless of privacy settings.

---

## 📁 Key Files Created/Modified

### Frontend
- ✅ `src/app/shared/layout/main-nav/main-nav.ts` (new)
- ✅ `src/app/shared/layout/main-nav/main-nav.html` (new)
- ✅ `src/app/shared/layout/main-nav/main-nav.scss` (new)
- ✅ `src/app/core/services/user-profile.service.ts` (new)
- ✅ `src/app/features/members/members-list.component.ts` (new)
- ✅ `src/app/features/members/member-detail.component.ts` (new)
- ✅ `src/app/features/profile/user-profile.ts` (updated)
- ✅ `src/app/core/models/auth.model.ts` (extended)
- ✅ `src/app/app.ts` (navbar + removed sidebar)
- ✅ `src/app/app.html` (navbar layout)
- ✅ `src/app/app.scss` (layout adjustments)
- ✅ `src/app/app.routes.ts` (members routes)
- ✅ `src/app/features/auth/login/login.component.ts` (fixed + redirect)
- ✅ `src/app/features/auth/register/register.component.ts` (fixed + redirect)

### Backend
- ✅ `Domain/Entities/UserProfile.cs` (extended)
- ✅ `Application/Features/UserProfile/DTOs/UserProfileDto.cs` (new)
- ✅ `Application/Features/UserProfile/Queries/UserProfileQueries.cs` (new)
- ✅ `Application/Features/UserProfile/Commands/UserProfileCommands.cs` (new)
- ✅ `API/Controllers/UserProfileController.cs` (new)
- ✅ `Infrastructure/Services/FileStorageService.cs` (new)
- ✅ `Infrastructure/DependencyInjection.cs` (updated)

---

## 🎨 Architecture Patterns Used

### Frontend
- **Angular 21+**: Standalone components, signals, reactive forms
- **ChangeDetectionStrategy.OnPush**: All components for better performance
- **Reactive Forms**: For profile editing with full validation
- **Computed Signals**: Derived state (isOwnProfile, canEdit, etc.)
- **Functional Patterns**: No class-based components

### Backend
- **Clean Architecture**: Domain → Application → Infrastructure → API
- **CQRS with MediatR**: Queries (read), Commands (write)
- **Repository Pattern**: Data access abstraction
- **Soft Delete**: Profiles marked as deleted, not removed
- **Privacy Filtering**: DTO respects privacy flags at the service layer

---

## 💡 Features Implemented

✅ **Profile CRUD**
- View own profile (full details)
- View others' profiles (respect privacy)
- Edit own profile (all fields except role)
- Admin can edit any profile (all fields except role)
- Admin can change user roles (separate endpoint)
- Soft delete profile (not visible in members list)

✅ **Member Management**
- Search users by name (real-time)
- Filter by role (Admin/Member)
- Paginate results (10, 20, 50 per page)
- View any member's public profile
- Privacy controls respected

✅ **Avatar Management**
- Upload image files (jpg, png, webp)
- File validation (size, type)
- Store locally in wwwroot/uploads/avatars/
- Display with fallback initials

✅ **Navigation**
- Responsive navbar with mobile menu
- Users see Members link, Admins see Admin link
- User dropdown with profile and logout
- Active link highlighting

✅ **After Auth**
- Login/Register → Redirect to /profile (not dashboard)
- Profile shows full edit form with privacy controls
- User can navigate to members list

---

## ⚙️ Configuration

### Frontend Environment
- `src/environments/environment.ts` - Ensure `apiUrl` points to backend:
  ```typescript
  export const environment = {
    production: false,
    apiUrl: 'http://localhost:5000' // or your backend URL
  };
  ```

### Backend Program.cs
- CORS already configured for `http://localhost:4200` and production URL
- Identity configured with strong password requirements
- JWT configured with 8-hour expiration

---

## 🐛 Troubleshooting

### Avatar uploads not working
- ✓ Check `wwwroot/uploads/avatars/` folder exists
- ✓ Verify file size < 5MB
- ✓ Verify file type is jpg/png/webp
- ✓ Check file permissions on uploads folder

### Members list shows no users
- ✓ Ensure users exist in database
- ✓ Check that users have profiles created
- ✓ Verify soft-deleted users are excluded (IsDeleted = false)
- ✓ Try searching with blank term to see all

### Edit profile returns 400 error
- ✓ Check form validation (fullName required, max 500 bio)
- ✓ Verify authorization (member can't edit others)
- ✓ Check privacy flags are valid booleans

### Profile not updating after login
- ✓ Ensure UserProfileService.getCurrentProfile() is called
- ✓ Check LocalStorage is enabled
- ✓ Verify token is stored in localStorage

---

## 📝 Testing Checklist

- [ ] Backend migrations applied
- [ ] wwwroot/uploads/avatars/ folder created
- [ ] Login → Redirects to /profile ✓
- [ ] Edit profile, save changes, reload page (persists) ✓
- [ ] Upload avatar, displays on profile ✓
- [ ] Privacy toggles hide fields in members list ✓
- [ ] Navigate to members list, search by name ✓
- [ ] Filter members by role ✓
- [ ] Pagination works (page 1 → page 2) ✓
- [ ] As member, try editing other profile (403 error) ✓
- [ ] As admin, edit member profile and role ✓
- [ ] Delete profile → Removed from members list ✓
- [ ] Navbar shows/hides admin link based on role ✓
- [ ] Mobile responsive navbar works ✓

---

## 🎯 Summary

✅ **All code generated and implemented**
✅ **Full CRUD profile management**
✅ **Soft delete with privacy controls**
✅ **Avatar upload support**
✅ **Admin role change endpoint**
✅ **Responsive navbar without sidebar**
✅ **Members list with search/filter/pagination**
✅ **Post-auth redirect to profile**
✅ **Authorization rules enforced**
✅ **Accessibility features included**

**Next immediate action:** Run EF migrations, create upload folder, test endpoints.
