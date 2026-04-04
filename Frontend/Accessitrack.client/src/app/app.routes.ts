import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full',
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/dashboard/dashboard.component')
        .then(m => m.DashboardComponent),
    title: 'Tableau de bord — AccessiTrack',
  },
  {
    path: 'projects',
    loadComponent: () =>
      import('./features/projects/project-list/project-list')
        .then(m => m.ProjectListComponent),
    title: 'Projets — AccessiTrack',
  },
  {
    path: 'projects/new',
    loadComponent: () =>
      import('./features/projects/project-form/project-form')
        .then(m => m.ProjectFormComponent),
    title: 'Nouveau projet — AccessiTrack',
  },
  {
    path: 'projects/:id/audits',
    loadComponent: () =>
      import('./features/audits/audit-list/audit-list')
        .then(m => m.AuditListComponent),
    title: 'Audits — AccessiTrack',
  },
  {
    path: 'projects/:projectId/audits/:id/violations/new',
    loadComponent: () =>
      import('./features/violations/violations-form')
        .then(m => m.ViolationFormComponent),
    title: 'Nouvelle violation — AccessiTrack',
  },
  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
