import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';
import { publicGuard } from './core/guards/public.guard';

/**
 * App Routes Configuration - Angular 21+ with functional guards.
 * 
 * Route Guard Strategy:
 * - publicGuard: Allows only unauthenticated users (login, register)
 * - authGuard: Requires authentication
 * - adminGuard: Requires authentication + Admin role
 * 
 * Auth Flow:
 * 1. User navigates to /login (publicGuard ensures not authenticated)
 * 2. Login succeeds → AuthService stores token + user in signals
 * 3. AuthService emits signals update → UI updates
 * 4. User navigates to protected route
 * 5. authGuard checks isAuthenticated() → allows if true
 * 6. AuthInterceptor adds Bearer token to all requests
 * 7. Backend validates JWT token
 */
export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/home/home').then(m => m.HomeComponent),
  },

  // ====== Public Routes (publicGuard ensures unauthenticated users only) ======
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then(
        (m) => m.LoginComponent
      ),
    canActivate: [publicGuard],
    title: 'Connexion — AccessiTrack',
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register/register.component').then(
        (m) => m.RegisterComponent
      ),
    canActivate: [publicGuard],
    title: 'Inscription — AccessiTrack',
  },

  // ====== Protected Routes (authGuard ensures authenticated) ======
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/dashboard/dashboard.component').then(
        (m) => m.DashboardComponent
      ),
    canActivate: [authGuard],
    title: 'Tableau de bord — AccessiTrack',
  },
  {
    path: 'profile',
    loadComponent: () =>
      import('./features/profile/user-profile.component').then(
        (m) => m.UserProfileComponent
      ),
    canActivate: [authGuard],
    title: 'Profil utilisateur — AccessiTrack',
  },

  // ====== Admin Routes (authGuard + adminGuard) ======
  {
    path: 'admin',
    loadComponent: () =>
      import('./features/admin/admin-dashboard/admin-dashboard.component').then(
        (m) => m.AdminDashboardComponent
      ),
    canActivate: [authGuard, adminGuard],
    title: 'Administration — AccessiTrack',
  },

  // ====== Projects Routes ======
  {
    path: 'projects',
    loadComponent: () =>
      import('./features/projects/project-list/project-list').then(
        (m) => m.ProjectListComponent
      ),
    canActivate: [authGuard],
    title: 'Projets — AccessiTrack',
  },
  {
    path: 'projects/new',
    loadComponent: () =>
      import('./features/projects/project-form/project-form').then(
        (m) => m.ProjectFormComponent
      ),
    canActivate: [authGuard],
    title: 'Nouveau projet — AccessiTrack',
  },

  // ====== Audits Routes ======
  {
    path: 'projects/:id/audits',
    loadComponent: () =>
      import('./features/audits/audit-list/audit-list').then(
        (m) => m.AuditListComponent
      ),
    canActivate: [authGuard],
    title: 'Audits — AccessiTrack',
  },
  {
    path: 'projects/:projectId/audits/:auditId',
    loadComponent: () =>
      import('./features/audits/audit-details/audit-details').then(
        (m) => m.AuditDetailsComponent
      ),
    canActivate: [authGuard],
    title: 'Détails de l\'audit — AccessiTrack',
  },

  // ====== Violations Routes ======
  {
    path: 'projects/:projectId/audits/:id/violations/new',
    loadComponent: () =>
      import('./features/violations/violations-form').then(
        (m) => m.ViolationFormComponent
      ),
    canActivate: [authGuard],
    title: 'Nouvelle violation — AccessiTrack',
  },

  // ====== Members Routes ======
  {
    path: 'members',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/members/members-list.component').then(
            (m) => m.MembersListComponent
          ),
        title: 'Équipe — AccessiTrack',
      },
      {
        path: ':userId',
        loadComponent: () =>
          import('./features/members/member-detail.component').then(
            (m) => m.MemberDetailComponent
          ),
        title: 'Profil du membre — AccessiTrack',
      },
    ],
  },

  // ====== Wildcard - Catch-all ======
  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
