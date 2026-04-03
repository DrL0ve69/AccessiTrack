export type ViolationSeverity = 'Critical' | 'Major' | 'Minor';

export interface Violation {
  readonly id: string;
  readonly auditId: string;
  readonly wcagCriterion: string;
  readonly wcagCriterionName: string;
  readonly htmlElement: string;
  readonly description: string;
  readonly severity: ViolationSeverity;
  readonly isResolved: boolean;
  readonly resolutionNote: string | null;
  readonly reportedAt: string;
}

export interface CreateViolationDto {
  readonly auditId: string;
  readonly wcagCriterion: string;
  readonly wcagCriterionName: string;
  readonly htmlElement: string;
  readonly description: string;
  readonly severity: ViolationSeverity;
}