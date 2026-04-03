import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Audit, CreateAuditDto } from '../models/audit.model';
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

  complete(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/complete`, {});
  }
}