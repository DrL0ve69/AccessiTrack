import { bootstrapApplication } from '@angular/platform-browser';
import { inject } from '@vercel/analytics';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app';
import { environment } from './environments/environment';

// Initialize Vercel Web Analytics
inject({
  mode: environment.production ? 'production' : 'development',
  debug: !environment.production,
});

bootstrapApplication(AppComponent, appConfig).catch((err) => console.error(err));
