import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
  computed,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { UserProfileService } from '../../core/services/user-profile.service';
import { UserSummaryDto } from '../../core/models/auth.model';

@Component({
  selector: 'app-members-list',
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterLink],
  templateUrl: './members-list.component.html',
  styleUrl: './members-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MembersListComponent {
  private readonly userProfileService = inject(UserProfileService);
  private readonly fb = inject(FormBuilder);

  readonly filterForm = this.fb.nonNullable.group({
    searchTerm: [''],
    roleFilter: [''],
  });

  readonly isLoading = signal(true);
  readonly error = signal<string | null>(null);
  readonly users = signal<UserSummaryDto[]>([]);
  readonly totalCount = signal(0);
  readonly currentPage = signal(1);
  readonly pageSize = signal(20);
  readonly totalPages = computed(() =>
    Math.ceil(this.totalCount() / this.pageSize())
  );

  constructor() {
    this.loadMembers();
    this.filterForm.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => {
        this.currentPage.set(1);
        this.loadMembers();
      });
  }

  private loadMembers(): void {
    this.isLoading.set(true);
    this.error.set(null);

    const { searchTerm, roleFilter } = this.filterForm.getRawValue();

    this.userProfileService
      .getAllUsers(
        searchTerm || undefined,
        (roleFilter as 'Admin' | 'Member' | undefined) || undefined,
        this.currentPage(),
        this.pageSize()
      )
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: (response) => {
          this.users.set(response.users);
          this.totalCount.set(response.totalCount);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Failed to load members:', err);
          this.error.set('Impossible de charger la liste des membres');
          this.isLoading.set(false);
        },
      });
  }

  nextPage(): void {
    if (this.currentPage() < this.totalPages()) {
      this.currentPage.update((p) => p + 1);
      this.loadMembers();
    }
  }

  previousPage(): void {
    if (this.currentPage() > 1) {
      this.currentPage.update((p) => p - 1);
      this.loadMembers();
    }
  }

  onPageSizeChange(): void {
    this.currentPage.set(1);
    this.loadMembers();
  }

  retry(): void {
    this.loadMembers();
  }

  getInitials(fullName: string): string {
    return fullName
      .split(' ')
      .map((word) => word[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }
}
