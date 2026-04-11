# CI/CD & Deployment (Azure DevOps + Vercel)

## Vercel Deployment
- **Framework:** Angular 21.
- **Build Command:** `npm run build -- --configuration production`.
- **Output Directory:** `dist/portfolio-clean/browser`.
- **Pre-deployment:** Always run `axe-core` accessibility audits in the pipeline.

## .NET Pipelines (Azure DevOps)
- **EF Migrations:** Use `dotnet ef migrations script --idempotent --output migrate.sql`.
- **Artifacts:** Publish the API as a self-contained linux-x64 binary for high performance.
- **Environment:** Use `DOTNET_ENVIRONMENT=Production` and map secrets from KeyVault.

