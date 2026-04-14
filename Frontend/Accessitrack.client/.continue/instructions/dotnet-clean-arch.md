# .NET 10 & C# 14 Clean Architecture Rules

## Domain Layer (Core.Domain)
- **Entities:** Must be in `Domain/Entities`. Use `sealed` classes. Use **Primary Constructors**.
- **Value Objects:** Use `public sealed record` for immutability.
- **Logic:** Business invariants MUST live in the Entity methods, not Handlers.
- **Rules:** No dependencies on EF Core or Application.

## Application Layer (Core.Application)
- **MediatR:** One file per Command/Query containing Request, Handler, and Response.
- **Validation:** Use `FluentValidation`. Validators live next to the Command.
- **DI:** Use `inject()` style or Primary Constructors. Prefer `IReadOnlyList` for collections.

## Infrastructure Layer (Infrastructure)
- **Persistence:** Contains `ApplicationDbContext` and `EntityConfiguration` files.
- **Configurations:** Use `IEntityTypeConfiguration<T>` to keep the DbContext clean.
- **Repositories:** Implement interfaces from the Domain/Application layers.