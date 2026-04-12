import {
  ChangeDetectionStrategy,
  Component,
  signal,
  computed,
  ElementRef,
  AfterViewInit,
  inject,
  viewChild,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

interface Service {
  icon: string;
  title: string;
  description: string;
}

interface Testimonial {
  quote: string;
  author: string;
  role: string;
}

@Component({
  selector: 'app-home',
  templateUrl: './home.html',
  styleUrl: './home.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  host: { class: 'home-host' },
})
export class HomeComponent implements AfterViewInit {
  readonly auth = inject(AuthService);
  readonly ctaRoute = computed(() =>
    this.auth.isAuthenticated()
      ? this.auth.isAdmin() ? '/admin' : '/projects'
      : '/register'
  );

  readonly menuOpen = signal(false);
  toggleMenu() { this.menuOpen.update(v => !v); }

  readonly services: Service[] = [
    {
      icon: '🔍',
      title: 'Automated WCAG Auditing',
      description:
        'Submit any URL and get a detailed WCAG 2.2 compliance report powered by axe-core — in seconds.',
    },
    {
      icon: '📊',
      title: 'Violation Dashboards',
      description:
        'Visualize severity breakdowns, progress over time, and exportable reports for your entire portfolio.',
    },
    {
      icon: '🛠',
      title: 'Remediation Guidance',
      description:
        'Every violation comes with code examples and ARIA fix suggestions, not just a pass/fail score.',
    },
    {
      icon: '🔒',
      title: 'Role-Based Access',
      description:
        'Members manage their own projects. Admins oversee the full audit pipeline across all clients.',
    },
    {
      icon: '📋',
      title: 'Audit History',
      description:
        'Track accessibility improvements sprint-by-sprint with timestamped audit sessions per project.',
    },
    {
      icon: '⚡',
      title: 'Built for Teams',
      description:
        'Invite clients, share reports, and collaborate on fixes — without leaving the platform.',
    },
  ];

  readonly testimonials: Testimonial[] = [
    { quote: 'AccessiTrack helped us pass AODA compliance 3 months ahead of schedule.', author: 'Camille R.', role: 'Frontend Lead @ Finova' },
    { quote: 'The violation breakdown is miles ahead of any free tool we had tried.', author: 'Marcus T.', role: 'CTO @ Launchpad Digital' },
    { quote: 'Our WCAG score went from 43 to 91 in 6 weeks using this dashboard alone.', author: 'Priya S.', role: 'Accessibility Specialist @ GovTech' },
    { quote: 'Clean, fast, and actually explains how to fix each error.', author: 'Théo M.', role: 'Dev @ Créalab' },
    { quote: 'Finally an audit tool that doesn\'t feel like a spreadsheet from 2008.', author: 'Sandra K.', role: 'UX Director @ Amplitude' },
    { quote: 'We use AccessiTrack as our accessibility source-of-truth across 12 clients.', author: 'Jordan L.', role: 'Agency Owner @ ClearPath Studio' },
  ];

  // Duplicate for seamless infinite scroll
  readonly tickerItems = computed(() => [...this.testimonials, ...this.testimonials]);

  private heroRef = viewChild<ElementRef<HTMLElement>>('heroRef');

  ngAfterViewInit() {
    // Intersection-based reveal handled via CSS + class toggle for perf
  }
}