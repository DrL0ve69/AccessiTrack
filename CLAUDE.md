\# CLAUDE.md



This file provides guidance to Claude Code when working with code in this repository.



\---



\## Build \& Run Commands



\### Backend



```bash

\# Build the solution

dotnet build



\# Run the API

dotnet run --project src/Api



\# Run tests (no rebuild)

dotnet test --no-build



\# Run tests with coverage

dotnet test --collect:"XPlat Code Coverage"



\# EF Core migrations (always run from solution root)

dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/Api

dotnet ef database update --project src/Infrastructure --startup-project src/Api

```



\### Frontend



```bash

\# Install dependencies

npm install



\# Start dev server

ng serve



\# Build for production

ng build --configuration production



\# Run unit tests

npx vitest



\# Lint

ng lint

```



\---



\## Project Structure



\### Backend



```

src/

├── Api/                  # Entry point, endpoints, middleware, DI composition root

├── Application/          # Commands, queries, handlers, DTOs, validators, pipeline behaviors

├── Domain/               # Entities, value objects, domain events, domain exceptions

├── Infrastructure/       # EF Core, Identity, external integrations, repository implementations

tests/

├── Unit/

└── Integration/

```



\### Frontend



```

src/

├── app/

│   ├── core/             # Singleton services, guards, interceptors, app-level providers

│   ├── shared/           # Reusable standalone components, pipes, directives

│   ├── features/         # Lazy-loaded feature folders (one per domain feature)

│   └── app.routes.ts     # Root route configuration

├── environments/

└── assets/

```



\---



\## Architecture



\### Philosophy — Clean Architecture with CQRS



\- \*\*Domain\*\* has zero external dependencies — pure business logic and invariants only.

\- \*\*Application\*\* orchestrates use cases via Mediator handlers. Depends only on Domain.

\- \*\*Infrastructure\*\* implements interfaces defined in Application. Never referenced by Application directly.

\- \*\*Api\*\* is thin — maps HTTP to Mediator requests, no business logic whatsoever.



\*\*Why CQRS?\*\* Read and write models have different performance and shape requirements. Keeping them separate makes both simpler.  

\*\*Why Mediator?\*\* Decouples handlers from the HTTP layer, enables pipeline behaviors (validation, logging, performance), and is source-generated for better runtime performance.



\### Dependency Direction



```

Domain ← Application ← Infrastructure

&#x20;                    ← Api

```



Never reference an outer layer from an inner one. Never reference Infrastructure from Api directly.



\---



\## Backend Standards (C# 14 / .NET 10)



\### Tech Stack



\- .NET 10, ASP.NET Core (controllers or Minimal APIs depending on project)

\- Entity Framework Core 10 with SQL Server

\- Mediator (source-generated) for CQRS

\- FluentValidation for request validation

\- Scalar for API documentation (OpenAPI)

\- ASP.NET Core Identity with JWT Bearer tokens



\### C# Style



\- Use \*\*primary constructors\*\* for dependency injection everywhere — no `private readonly` field + constructor boilerplate.

\- Use \*\*file-scoped namespaces\*\* in every `.cs` file.

\- Prefer `is null` / `is not null` over `== null` / `!= null`.

\- Prefer `IReadOnlyList<T>` or `IEnumerable<T>` over `List<T>` in return types.

\- Apply `sealed` to records and DTOs where inheritance is not intended.

\- No magic strings — use `const` or `static readonly` for role names, claim types, policy names, and route segments.

\- XML doc comments on all public Domain types and Application interfaces.



\### CQRS \& Mediator



\- \*\*Commands\*\* mutate state and return minimal data (new `Guid` or `Unit`).

\- \*\*Queries\*\* return data and never mutate state.

\- Name requests explicitly: `CreateProjectCommand`, `GetProjectBySlugQuery`.

\- Pipeline behaviors own all cross-cutting concerns (validation, logging, performance) — never duplicate this inside handlers.

\- Validators live in the same folder as their command or query.



\### Validation



\- All validation lives in \*\*FluentValidation\*\* validators in the Application layer — no exceptions.

\- `ValidationBehavior` (Mediator pipeline) throws `ValidationException` before the handler is ever reached.

\- \*\*Never\*\* check `ModelState` manually in controllers — it is redundant by design.

\- Validators are registered automatically via assembly scanning at startup.



\### Security \& Authentication



\- All API controllers or endpoint groups use \*\*`\[Authorize]`\*\* by default — secure by default.

\- Public endpoints explicitly opt out with \*\*`\[AllowAnonymous]`\*\* — this is always a conscious decision.

\- Admin-only endpoints use `\[Authorize(Roles = Roles.Admin)]`.

\- Role name constants live in a single `Roles` static class.

\- Sensitive config (`Jwt:Key`, connection strings) must never be committed. Use `dotnet user-secrets` locally and environment variables in production.



\### Error Handling



\- Throw \*\*domain-specific exceptions\*\* from handlers and domain services (`NotFoundException`, `ConflictException`, `ForbiddenException`).

\- Global exception middleware maps exceptions to RFC 9457 problem-details responses with correct HTTP status codes.

\- \*\*No try/catch in controllers or endpoint handlers\*\* — they dispatch to Mediator and return the result.

\- Never expose internal exception details in API responses.



\### Domain Rules



\- \*\*Never modify `Domain/`\*\* without explicit approval — changes there have cascading effects everywhere.

\- Use `IDateTimeProvider` (or equivalent abstraction) instead of `DateTime.UtcNow` directly — keeps domain logic testable.

\- Domain entities enforce their own invariants — invalid state must be impossible to construct.



\---



\## Frontend Standards (Angular / TypeScript)



\### Tech Stack



\- Angular 20+ (standalone components by default)

\- TypeScript with strict mode enabled

\- Signals for state management

\- Vitest for unit testing



\### TypeScript



\- Enable \*\*strict mode\*\* in `tsconfig.json` — no exceptions.

\- Prefer type inference when the type is obvious.

\- Never use `any` — use `unknown` when the type is genuinely uncertain, then narrow it.

\- Use `readonly` on properties that should not be mutated after construction.



\### Angular



\- \*\*Never set `standalone: true`\*\* inside decorators — it is the default in Angular v20+ and is redundant noise.

\- Always use \*\*`inject()`\*\* instead of constructor injection.

\- Implement \*\*lazy loading\*\* for all feature routes.

\- Use \*\*`NgOptimizedImage`\*\* for all static images (`NgOptimizedImage` does not support inline base64).

\- Never use `@HostBinding` or `@HostListener` — put host bindings in the `host` object of `@Component` or `@Directive`.



\### Components



\- Set \*\*`changeDetection: ChangeDetectionStrategy.OnPush`\*\* on every component — no exceptions.

\- Use \*\*`input()`\*\* and \*\*`output()`\*\* functions instead of `@Input()` / `@Output()` decorators.

\- Use \*\*`computed()`\*\* for all derived state.

\- Keep components small and focused on a single responsibility.

\- Prefer inline templates for small components; use file-relative paths for external templates and styles.

\- Prefer \*\*Reactive Forms\*\* over Template-driven forms.

\- Never use `ngClass` — use `class` bindings instead.

\- Never use `ngStyle` — use `style` bindings instead.



\### State Management



\- Use \*\*signals\*\* for local component state.

\- Use \*\*`computed()`\*\* for derived state — never duplicate derived values.

\- Keep state transformations pure and predictable — no side effects inside computed.

\- Never use `mutate()` on signals — use `update()` or `set()`.



\### Templates



\- Keep templates simple — no complex logic or expressions.

\- Use \*\*native control flow\*\* (`@if`, `@for`, `@switch`) — never `\*ngIf`, `\*ngFor`, `\*ngSwitch`.

\- Use the \*\*async pipe\*\* to handle observables in templates.

\- Never assume globals like `new Date()` are available in templates.



\### Services



\- Design services around a \*\*single responsibility\*\*.

\- Always use \*\*`providedIn: 'root'`\*\* for singleton services.

\- Use \*\*`inject()`\*\* instead of constructor injection.



\### Accessibility



\- All components \*\*must pass AXE checks\*\* — zero violations.

\- Follow \*\*WCAG AA\*\* minimums: focus management, color contrast ratios, and correct ARIA attributes.

\- Interactive elements must be keyboard navigable.

\- Images must have meaningful `alt` text (or `alt=""` if decorative).



\---



\## Workflow Rules



\- \*\*Always create a feature branch\*\* before making any changes — never commit directly to `main`.

\- \*\*Run tests after every implementation\*\* — `dotnet test` for backend, `npx vitest` for frontend.

\- \*\*Never modify `Domain/`\*\* without explicit discussion and approval.

\- \*\*Keep commits atomic\*\* — one logical change per commit, written in imperative mood (`Add`, `Fix`, `Refactor`).

\- \*\*No commented-out code\*\* — delete it, version control has the history.

\- \*\*No TODOs without a tracking issue\*\* — either fix it now or open a ticket.

\- Pull requests must have a passing test suite before merge.

