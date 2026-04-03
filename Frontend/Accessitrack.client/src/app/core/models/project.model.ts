export interface Project {
  readonly id: string;
  readonly name: string;
  readonly targetUrl: string;
  readonly clientName: string;
  readonly createdAt: string;
  readonly totalAudits: number;
  readonly isArchived: boolean;
}

export interface CreateProjectDto {
  readonly name: string;
  readonly targetUrl: string;
  readonly clientName: string;
}