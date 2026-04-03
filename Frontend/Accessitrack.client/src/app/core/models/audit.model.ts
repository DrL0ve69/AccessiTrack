import { Violation } from "./violation.model";

export type AuditStatus = 'InProgress' | 'PendingReview' | 'Completed' | 'Archived';

export interface Audit {
  readonly id: string;
  readonly projectId: string;
  readonly startedAt: string;
  readonly completedAt: string | null;
  readonly status: AuditStatus;
  readonly notes: string | null;
  readonly violations: Violation[];
}

export interface CreateAuditDto {
  readonly projectId: string;
}