import { Violation } from "./violation.model";

export type AuditStatus = 'InProgress' | 'PendingReview' | 'Completed' | 'Archived';

export interface Audit {
  readonly id: string;
  readonly projectId: string;
  readonly startedAt: string;
  readonly completedAt: string | null;
  readonly status: AuditStatus;
  readonly notes: string | null;

  readonly totalViolations: number;      // Correspond au JSON
  readonly criticalViolations: number;   // Correspond au JSON
  readonly resolvedViolations: number;   // Correspond au JSON

  readonly violations?: Violation[];
}

export interface CreateAuditDto {
  readonly projectId: string;
}

/**
 * Result of an automatic accessibility audit scan.
 * Contains summary of violations found by severity.
 */
export interface AutomaticAuditResult {
  readonly auditId: string;
  readonly violationsFound: number;
  readonly criticalViolations: number;
  readonly majorViolations: number;
  readonly minorViolations: number;
}