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

// Crée une interface pour la commande si elle n'existe pas
export interface LogViolationCommand {
  auditId: string;
  wcagCriterion: string;
  wcagCriterionName: string;
  htmlElement: string;
  description: string;
  severity: number;
  isResolved: boolean;
}