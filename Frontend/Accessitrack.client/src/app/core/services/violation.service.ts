import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateViolationDto, Violation, LogViolationCommand } from '../models/violation.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ViolationService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/violations`;

  create(request: { command: LogViolationCommand }): Observable<Violation> {
    return this.http.post<Violation>(this.apiUrl, request);
  }

  resolve(id: string, note: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/resolve`, { note });
  }
}