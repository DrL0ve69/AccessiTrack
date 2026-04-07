import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Audit, CreateAuditDto, AutomaticAuditResult } from '../models/audit.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuditService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/audits`;

  getByProject(projectId: string): Observable<Audit[]> {
    return this.http.get<Audit[]>(`${this.apiUrl}/project/${projectId}`);
  }

  start(dto: CreateAuditDto): Observable<string> {
    return this.http.post<string>(this.apiUrl, dto);
  }

  /**
   * Starts an automatic accessibility audit that scans the project's TargetUrl
   * and detects WCAG violations automatically.
   */
  startAutomatic(projectId: string): Observable<AutomaticAuditResult> {
    return this.http.post<AutomaticAuditResult>(`${this.apiUrl}/automatic`, {
      projectId,
    });
  }

  complete(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/complete`, {});
  }
}