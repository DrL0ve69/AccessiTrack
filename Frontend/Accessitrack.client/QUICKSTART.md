# Quick Start - Getting AccessiTrack Running

## Prerequisites
- .NET 10 SDK installed
- Angular 21+ CLI (`npm install -g @angular/cli`)
- SQL Server LocalDB or SQL Server
- Node.js 18+

---

## 🚀 Backend Setup (Required First)

```bash
# Navigate to backend
cd AccessiTrack/Backend

# Build solution
dotnet build

# Create and apply EF migration (MUST DO THIS)
dotnet ef migrations add AddUserProfileExtensions \
  --project AccessiTrack.Infrastructure \
  --startup-project AccessiTrack.API

dotnet ef database update \
  --project AccessiTrack.Infrastructure \
  --startup-project AccessiTrack.API

# Create uploads folder for avatars
mkdir -p AccessiTrack.API/wwwroot/uploads/avatars

# Run backend
dotnet run --project AccessiTrack.API
# Backend runs on https://localhost:5001 or http://localhost:5000
```

---

## 🎨 Frontend Setup

```bash
# Navigate to frontend
cd AccessiTrack/Frontend/Accessitrack.client

# Install dependencies
npm install

# Start development server
ng serve
# Frontend runs on http://localhost:4200
```

---

## ✅ Test the Implementation

### 1. **Login**
- Go to http://localhost:4200/login
- Default credentials: `admin@accessitrack.com` / `Admin@123!`
- Should redirect to `/profile`

### 2. **Edit Profile**
- Upload avatar (jpg, png, webp, max 5MB)
- Edit name, phone, bio
- Toggle privacy for email/phone/bio
- Click Save

### 3. **View Members**
- Click "Équipe" in navbar
- Search by name (e.g., search "admin")
- Filter by role
- Paginate through results
- Click member card to view profile

### 4. **Admin Features** (if logged in as admin)
- View "Administration" in navbar
- Edit any member's profile
- Change member roles
- Delete member profiles

### 5. **Register New Member**
- Go to http://localhost:4200/register
- Create account → Redirects to /profile
- Complete profile with avatar

---

## 📊 API Endpoints (Backend Only)

```bash
# Get current user's profile
curl -X GET http://localhost:5000/api/userprofile/me \
  -H "Authorization: Bearer {token}"

# List all users (paginated, searchable)
curl -X GET "http://localhost:5000/api/userprofile?search=john&role=Admin&page=1&pageSize=20" \
  -H "Authorization: Bearer {token}"

# Update own profile
curl -X PUT http://localhost:5000/api/userprofile/{userId} \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "John Doe",
    "phoneNumber": "+1 (555) 123-4567",
    "bio": "I love WCAG!",
    "emailPrivate": true
  }'

# Upload avatar (form-data)
curl -X POST http://localhost:5000/api/userprofile/{userId}/avatar \
  -H "Authorization: Bearer {token}" \
  -F "file=@profile.jpg"

# Change user role (admin only)
curl -X POST http://localhost:5000/api/userprofile/{userId}/role \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"role": "Admin"}'

# Delete profile
curl -X DELETE http://localhost:5000/api/userprofile/{userId} \
  -H "Authorization: Bearer {token}"
```

---

## 🔑 Key Usernames/Credentials

Default seeded accounts:
- **Admin**: `admin@accessitrack.com` / `Admin@123!`

After registration, new accounts default to "Member" role.

---

## 📂 File Structure - What Changed

### Frontend
```
src/
├── app/
│   ├── core/
│   │   ├── services/
│   │   │   └── user-profile.service.ts (NEW)
│   │   └── models/
│   │       └── auth.model.ts (extended with UserProfileDto, etc.)
│   ├── shared/
│   │   └── layout/
│   │       └── main-nav/ (NEW - navbar component)
│   │           ├── main-nav.ts
│   │           ├── main-nav.html
│   │           └── main-nav.scss
│   ├── features/
│   │   ├── members/ (NEW - members feature)
│   │   │   ├── members-list.component.ts
│   │   │   └── member-detail.component.ts
│   │   ├── profile/
│   │   │   └── user-profile.ts (updated with full CRUD)
│   │   └── auth/
│   │       ├── login/ (redirect fixed)
│   │       └── register/ (redirect fixed)
│   ├── app.ts (updated - navbar, no sidebar)
│   ├── app.html (navbar layout)
│   ├── app.scss (layout adjusted)
│   └── app.routes.ts (members routes added)
```

### Backend
```
AccessiTrack/Backend/
├── AccessiTrack.Domain/
│   └── Entities/
│       └── UserProfile.cs (extended with phone, avatar, bio, privacy, soft delete)
├── AccessiTrack.Application/
│   └── Features/
│       └── UserProfile/ (NEW)
│           ├── DTOs/
│           │   └── UserProfileDto.cs (all DTOs)
│           ├── Queries/
│           │   └── UserProfileQueries.cs (4 queries)
│           └── Commands/
│               └── UserProfileCommands.cs (4 commands)
├── AccessiTrack.Infrastructure/
│   ├── Services/
│   │   └── FileStorageService.cs (NEW - avatar upload)
│   ├── DependencyInjection.cs (FileStorageService registered)
│   └── Migrations/
│       └── (new migration will be created after running add-migration)
└── AccessiTrack.API/
    ├── Controllers/
    │   └── UserProfileController.cs (NEW - 6 endpoints)
    └── wwwroot/
        └── uploads/ (create manually: mkdir -p wwwroot/uploads/avatars)
```

---

## ⚠️ Important Notes

### Before Running
1. **Ensure database migrations run successfully** (this creates the new profile columns)
2. **Create the uploads folder** for avatar storage
3. **Verify API URL in frontend environment** points to backend

### During Development
- Backend should run on `https://localhost:5001` or `http://localhost:5000`
- Frontend should run on `http://localhost:4200`
- Both should be available simultaneously (different terminals)

### After First Login
- A user profile is created automatically  
- User is redirected to `/profile` to complete their profile
- Member can only edit own profile (not role)
- Admin can edit any profile including roles

### Privacy & Authorization
- All profile updates are validated server-side
- Privacy flags are respected when viewing other users' profiles
- Admin always sees all fields regardless of privacy
- Soft-deleted users (IsDeleted = true) don't appear in lists

---

## 🆘 If Something Doesn't Work

### Migration Failed
```bash
# Check database connection in appsettings.json
# Delete any failed migrations:
dotnet ef migrations remove --project AccessiTrack.Infrastructure

# Try again
dotnet ef migrations add AddUserProfileExtensions \
  --project AccessiTrack.Infrastructure \
  --startup-project AccessiTrack.API

dotnet ef database update
```

### CORS Error in Frontend
- Ensure backend CORS includes `http://localhost:4200`
- Check `appsettings.json` for CORS policy
- Restart backend after any changes

### Avatar Upload Failed
- Verify `wwwroot/uploads/avatars/` folder exists
- Check file is < 5MB
- Verify file type (jpg, png, webp only)
- Check NTFS permissions on uploads folder

### Users Not Showing in Members List
- Verify users have profiles created (check database)
- Ensure `IsDeleted = false` for visible users
- Check pagination/search parameters

---

## 📝 Testing Workflow

```bash
# Terminal 1: Backend
cd AccessiTrack/Backend
dotnet run --project AccessiTrack.API

# Terminal 2: Frontend
cd AccessiTrack/Frontend/Accessitrack.client
ng serve

# Terminal 3: Browser & Manual Testing
# 1. Open http://localhost:4200
# 2. Login with admin@accessitrack.com / Admin@123!
# 3. Should redirect to /profile
# 4. Fill profile details, upload avatar, save
# 5. Navigate to /members
# 6. Search, filter, paginate
# 7. Click member → view profile
# 8. As admin, change roles and delete profiles
```

---

## 🎉 You're All Set!

All code is ready to run. Just:
1. Run migrations
2. Create uploads folder
3. Start backend & frontend
4. Test the implementation

See `IMPLEMENTATION_SUMMARY.md` for complete documentation.
