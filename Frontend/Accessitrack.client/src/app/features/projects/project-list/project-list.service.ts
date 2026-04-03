// project.service.ts
import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Project {
  id: string;
  name: string;
  targetUrl: string;
  clientName: string;
  totalAudits: number;
  isArchived: boolean;
}

export interface CreateProjectDto {
  name: string;
  targetUrl: string;
  clientName: string;
}

@Injectable({
  providedIn: 'root'  // ✅ Singleton global, pas besoin de NgModule
})
export class ProjectService {
  // ✅ inject() au lieu du constructeur
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'https://localhost:7001/api/projects';

  getAll(): Observable<Project[]> {
    return this.http.get<Project[]>(this.apiUrl);
  }

  create(dto: CreateProjectDto): Observable<string> {
    return this.http.post<string>(this.apiUrl, dto);
  }
}