# AccessiTrack Authentication Flow - End-to-End Architecture

## Overview
This document describes the complete authentication system following **Clean Architecture** principles and **Angular 21+ modern patterns**.

---

## Authentication Architecture Layers

### 1. **Frontend Architecture (Angular 21+)**

```
UI Components (Login, Register)
         ↓
    AuthService (Facade)
         ↓
    HttpClient + Interceptors
         ↓
    Backend API
```

#### Frontend Layers:
- **Components**: Login, Register (handle user input)
- **Services**: AuthService (state management with Signals)
- **Interceptors**: 
  - `authInterceptor`: Adds JWT token to requests
  - `httpErrorInterceptor`: Handles API errors consistently
- **Guards**: Functional route guards for access control
- **Models**: TypeScript interfaces for type safety


---

## Complete Auth Flow (Step-by-Step)

### **Login Flow**

```
1. User enters email or username/password in LoginComponent
   ↓
2. LoginComponent calls authService.login(credentials)
   ↓
3. AuthService posts to POST /api/auth/login with JSON body
   ↓
4. AuthInterceptor detects NO token → skips Authorization header
   ↓
5. Backend receives request → AuthController → LoginCommand via MediatR
   ↓
6. LoginHandler calls identityService.LoginAsync()
   ↓
7. IdentityService:
   - Finds user by email or username in database
   - Validates password hash
   - Creates JWT token with user claims
   - Returns AuthResponseDto { token, userId, email, role, expiresAt }
   ↓
8. Backend responds with AuthResponseDto (changed from string!)
   ↓
9. HttpErrorInterceptor checks status → 200 OK → passes through
   ↓
10. AuthService tap operator executes setSession():
    - Stores token in localStorage
    - Stores user in localStorage
    - Updates _token signal
    - Updates _user signal
    ↓
11. Signals update = Components reactively re-render
    - authGuard now allows navigation
    - User info displays in UI
    ↓
12. LoginComponent.subscribe receives success
    - Navigates to /profile
    ↓
13. Profil route has authGuard and profileGuard
    - authGuard checks isAuthenticated() → TRUE
    - profileGuard checks identity of logged user (for modifications of the personal profile), if Role Admin (Full Crud control)
    - Route loads user-profileComponent
```

### **Protected Request Flow**

```
1. User navigates to /projects (protected route)
   ↓
2. Route guard (authGuard) checks authService.isAuthenticated()
   ↓
3. If false → redirect to /login
   If true → load ProjectListComponent
   ↓
4. ProjectListComponent calls projectService.getProjects()
   ↓
5. HttpClient prepares request
   ↓
6. AuthInterceptor intercepts:
   - Gets token from authService.getToken()
   - Clones request with Authorization: Bearer {token} header
   ↓
7. httpErrorInterceptor catches response
   ↓
8. Backend receives request with Authorization header
   ↓
9. JWT Middleware (UseAuthentication) validates:
   - Verifies token signature with Jwt:Key from appsettings.json
   - Checks token issuer/audience match
   - Checks token expiration
   - Extracts claims (userId, email, role)
   ↓
10. Request succeeds → returns data to frontend
```

### **Token Expiration Flow**

```
1. User has token that's expired in localStorage
   ↓
2. User makes API request
   ↓
3. AuthInterceptor adds expired token to request
   ↓
4. Backend JWT validation fails → returns 401 Unauthorized
   ↓
5. httpErrorInterceptor catches 401:
   - Calls authService.logout()
   - Clears localStorage
   - Updates signals to null
   - Router navigates to /login
   ↓
6. User sees login page again
```

---

## Key Components & Their Roles

### **Frontend Components**

#### `AuthService` (core/services/auth.service.ts)
**Role**: Facade for authentication operations
- **State**: Signals for reactive updates
  - `_token`: JWT token store
  - `_user`: Current user info
- **Computed Properties**: 
  - `isAuthenticated`: !!token
  - `isAdmin`: user?.role === 'Admin'
  - `isMember`: user?.role === 'Member'
- **Methods**:
  - `login()`: POST to backend
  - `register()`: Create account
  - `logout()`: Clear session
  - `getToken()`: Used by interceptors
  - `setSession()`: Private - stores auth data
  - `clearSession()`: Private - removes auth data
  - `loadToken()`: Hydrate from localStorage on init
  - `loadUser()`: Hydrate user from localStorage on init

#### `authInterceptor` (core/interceptors/auth.interceptor.ts)
**Role**: Add JWT token to all requests
- Checks if token exists via `authService.getToken()`
- Clones request and adds `Authorization: Bearer {token}` header
- Catches 401/403 errors and handles logout
- **Angular 21+ pattern**: Functional interceptor `HttpInterceptorFn`

#### `httpErrorInterceptor` (core/interceptors/http-error.interceptor.ts)
**Role**: Centralized error handling
- Maps backend error responses to standardized frontend format
- Handles network errors vs API errors
- Provides user-friendly French messages or English (will add ressources eventually)
- Returns typed `HttpApiError` object

#### Route Guards (core/guards/*.guard.ts)
**Functional Guards** (Angular 21+ pattern):
- `authGuard`: `canActivate: [authGuard]` - requires authentication
- `adminGuard`: `canActivate: [authGuard, adminGuard]` - requires Admin role
- `publicGuard`: `canActivate: [publicGuard]` - blocks authenticated users

### **Backend Components**

#### `AuthController` (API/Controllers/AuthController.cs)
**Fixed**: ProducesResponseType now correctly shows `AuthResponseDto` instead of `string`
- POST /api/auth/login
- POST /api/auth/register
- GET /api/auth/profile (verified endpoint)

#### `IdentityService` (Infrastructure/Services/IdentityService.cs)
**Role**: User authentication business logic
- Validates credentials
- Generates JWT tokens with claims
- Handles user registration
- Assigns default "Member" role

#### `TokenService` (Infrastructure/Identity/TokenService.cs)
**Role**: JWT token generation
- Creates tokens with claims (userId, email, role)
- Sets token expiration (8 hours default)
- Signs with Jwt:Key from configuration

#### `GlobalExceptionHandlerMiddleware` (NEW)
**Role**: Catch all unhandled exceptions, return consistent error format
- Maps exception types to HTTP status codes:
  - `NotFoundException` → 404
  - `ValidationException` → 400
  - `UnauthorizedAccessException` → 401
  - `InvalidOperationException` → 400
  - Default → 500
- Returns `ErrorResponse` with code, message, timestamp

---

## Configuration & Security

### **CORS Configuration** (Program.cs)
```csharp
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins(
            "http://localhost:4200",      // Dev
            "https://accessi-track.vercel.app")  // Production
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
    )
);
```
⚠️ **IMPORTANT FOR DEPLOYMENT**: 
- Verify your production domain matches exactly (case-sensitive)
- Must include protocol (http/https)
- Both frontend and backend must be on whitelist

### **JWT Configuration** (appsettings.json)
```json
{
  "Jwt": {
    "Key": "CHANGE_ME_super_secret_key_min_32_chars_!",
    "Issuer": "AccessiTrack.API",
    "Audience": "AccessiTrack.Client"
  }
}
```
⚠️ **DEPLOYMENT CHECKLIST**:
- [ ] Change JWT:Key to secure random string (min 32 chars)
- [ ] Set JWT:Key in environment variables (NOT in appsettings.json)
- [ ] Keep Jwt:Key consistent between environments
- [ ] Environment-specific appsettings.*.json should override

### **Authentication Pipeline** (Program.cs)
```csharp
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();  // First
app.UseCors("AllowAngular");
app.UseHttpsRedirection();
app.UseAuthentication();   // Validates JWT
app.UseAuthorization();    // Checks permissions
```

---

## Modern Angular 21+ Patterns Used

### 1. **Signals for State Management** (Angular 16+)
```typescript
// No ngOnInit needed - signals work during component initialization
private readonly _user = signal<User | null>(this.loadUser());
readonly user = computed(() => this._user());
```
✅ **Benefits**: 
- Fine-grained reactivity
- No memory leaks
- Auto-cleanup with `takeUntilDestroyed()`

### 2. **Functional Guards** (Angular 15+)
```typescript
export const authGuard: CanActivateFn = () => {
  // Dependency injection works in functional guards
  const authService = inject(AuthService);
};
```
✅ **Benefits**: Simpler, more testable, better tree-shaking

### 3. **Functional Interceptors** (Angular 15+)
```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  // Direct access to services, no class needed
};
```
✅ **Benefits**: Lightweight, better performance

### 4. **Standalone Components** (Angular 14+)
```typescript
@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],  // No NgModule!
  // ...
})
```
✅ **Benefits**: Simpler bundling, tree-shakeable

### 5. **Reactive Forms** (Best Practice)
```typescript
readonly loginForm = this.fb.nonNullable.group({
  email: ['', [Validators.required, Validators.email]],
  password: ['', [Validators.required, Validators.minLength(6)]],
});
```
✅ **Benefits**: Better control validation, type-safe

### 6. **Control Flow Syntax** (Angular 17+)
```html
@if (isLoading()) {
  <p>Loading...</p>
} @else {
  <div>Content</div>
}
```
✅ **Benefits**: Cleaner syntax, better performance

---

## Clean Architecture Benefits

### **Separation of Concerns**
- Components only handle UI
- Services handle business logic
- Guards handle route access
- Interceptors handle cross-cutting concerns

### **Testability**
- Services can be unit tested independently
- Guards can be tested without components
- Mock HttpClient easily

### **Maintainability**
- Auth logic centralized in AuthService
- Error handling centralized in middleware
- Easy to add new guards or interceptors

### **Scalability**
- Can add refresh token strategy
- Can add role-based access control
- Can implement SSO easily

---

## Deployment Verification Checklist

- [ ] **CORS**: Update `appsettings.json` or environment variables with correct frontend URL
- [ ] **JWT**: Set `Jwt:Key` in environment (not in code)
- [ ] **Database**: Run migrations on production database
- [ ] **HTTPS**: Ensure both APIs use HTTPS
- [ ] **Token**: Test token generation and expiration
- [ ] **Error Handling**: GlobalExceptionHandlerMiddleware catches all errors
- [ ] **Interceptors** Both auth and error interceptors are registered
- [ ] **Guards**: All protected routes have guards
- [ ] **localStorage**: Verify clear on logout

---

## Common Issues & Solutions

### **Issue**: Auth fails on deployment
**Solution**: Check CORS whitelist, JWT:Key consistency

### **Issue**: Token not being added to requests
**Solution**: Verify authInterceptor is registered in `appConfig`

### **Issue**: 401 redirects to infinite loop
**Solution**: Check publicGuard on login route - prevents redirect loop

### **Issue**: User data not persisting on page refresh
**Solution**: AuthService.loadUser() loads from localStorage - check localStorage clear

### **Issue**: Role-based access fails
**Solution**: Verify adminGuard is applied to admin routes, check JWT contains role claim
