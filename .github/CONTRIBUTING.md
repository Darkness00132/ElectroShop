# Contributing to ElectroShop

Thank you for helping improve ElectroShop. This repository is a .NET 10 e-commerce backend organized around Clean Architecture, domain-driven design, CQRS with MediatR, and Vertical Slice Architecture.

This guide describes the workflow and the project-specific standards expected for changes to any business capability.

## Before You Start

1. Read the [README](../README.md) for the architecture, setup, and current capabilities.
2. Read the [PRD](../PRD.md) for the product scope and the behavioral decisions behind each feature.
3. Check existing [issues](../../issues) and pull requests before starting work.
4. For significant changes, open or discuss an issue first so the domain and API behavior is understood before implementation.
5. Confirm that your contribution complies with the [PolyForm Noncommercial License 1.0.0](../LICENSE.md).

## Local Development

### Requirements

- .NET 10 SDK
- SQL Server (connection string `DefaultConnection`)
- Redis (connection string `Redis`)
- Git

### Commands

| Task | Command |
|---|---|
| Restore | `dotnet restore` |
| Build (CI gate) | `dotnet build --configuration Release -warnaserror` |
| Run all tests | `dotnet test` |
| Run the API | `dotnet run --project Ecommerce.Api` |
| Apply migrations | `dotnet ef database update --project Infrastructure --startup-project Ecommerce.Api` |
| Add a migration | `dotnet ef migrations add <Name> --project Infrastructure --startup-project Ecommerce.Api` |

> [!IMPORTANT]
> CI builds with **warnings treated as errors**. `dotnet test` alone does **not** compile the API project — always run the full `dotnet build` above before committing, or a broken `Ecommerce.Api` can slip into a commit unnoticed.

The API documentation is served by Scalar at the root URL (`https://localhost:<port>/`). Use its **Authorize** control with a JWT access token for protected endpoints.

## Architecture Rules

### Clean Architecture Boundaries

- `Domain` contains entities, value objects, enums, and business invariants.
- `Application` contains use cases (commands/queries + handlers + validators), pipeline behaviors, DTOs, and abstractions.
- `Infrastructure` contains EF Core persistence, repositories, Identity, storage, email, and background jobs.
- `Ecommerce.Api` contains controllers, HTTP contracts, middleware, and authorization setup.
- Dependencies point inward. Never reference `Ecommerce.Api` or `Infrastructure` from `Application` or `Domain`.
- The Domain project's only allowed framework dependency is the Identity package for `AppUser`/`AppRole`.

### Vertical Slice Organization

Place each use case in its feature folder, keeping everything it needs together:

```text
Application/Features/<Feature>/
├── Commands/<UseCase>/
│   ├── <UseCase>Command.cs        # the request record
│   ├── <UseCase>Handler.cs        # one handler per use case
│   └── <UseCase>Validator.cs      # FluentValidation rules (when input is non-trivial)
├── Queries/<UseCase>/…
├── Dtos/                          # feature-facing DTOs
└── <Feature>Mapping.cs            # AutoMapper profile (when projections are used)
```

Rules:

- One command or query represents one focused use case.
- Handlers depend only on abstractions (`IRepository<T>`, `IUnitOfWork`, `ICurrentUserService`, …), never on concrete infrastructure.
- Keep controllers thin: map the request to a command, send it through `ISender`, translate the outcome to HTTP. Business invariants live in Domain entities, not in controllers.
- Domain state changes happen through explicit entity methods (for example `order.Cancel()`, `inventory.IncreaseStock(...)`), never by setting properties from outside.
- Introduce a shared abstraction only when at least two slices genuinely need it (see `Application/Common/Checkout` for the pattern).

### Caching Conventions

- Read-side queries that should be cached implement `ICacheableQuery<T>` and declare a cache key, tags, and lifetime.
- Commands that change cached data implement `ICacheInvalidatingCommand` and declare the keys and tags to evict. Cache names live in `Application/Constants/CacheNames.cs`.
- When a command changes data that other aggregates embed in their cached responses (for example a discount shown inside cached product listings), invalidate by tag so every affected entry refreshes.

### API Conventions

- Admin management controllers are named `*ManagementController`, live under `Controllers/Admin/`, carry `[Authorize(Roles = AppRoles.<Area>Administrators)]`, and use the route `api/admin/<resource>`.
- Public storefront endpoints live under `api/<resource>`.
- Every endpoint documents its summary, remarks, parameters, and all response codes in XML comments — these drive the Scalar/OpenAPI page.
- **Do not use the `{id:guid}` route constraint.** A malformed identifier should fail model binding and return **400**, not a 404 "endpoint not found".

  ```csharp
  // Preferred
  [HttpPut("{id}")]
  public async Task<IActionResult> Update(Guid id, …)

  // Avoid — a non-GUID id would return 404 instead of 400
  [HttpPut("{id:guid}")]
  ```

- Return values follow the exception model; do not return ad-hoc error shapes:

| Exception thrown | HTTP result |
|---|---|
| `ValidationException` | 400 with field errors |
| `DomainException` | 400 (invariant violated) |
| `NotFoundException` | 404 |
| `UnauthorizedException` | 401 |
| `ForbiddenException` | 403 |
| `ConflictException` | 409 (uniqueness, stock, state conflicts) |

- `DbUpdateConcurrencyException` (optimistic concurrency) is handled globally and surfaces as 409. Guard multi-write flows that must not oversell with concurrency tokens rather than locks.

## Testing Requirements

This project follows **behavior-driven unit testing**: tests describe and verify *what the system does*, not *how it is implemented*. That keeps them resistant to refactoring and maintenance.

- Name tests as business rules, using behavior language:

  ```csharp
  public async Task An_Order_Cannot_Be_Placed_When_There_Is_Not_Enough_Stock()
  public async Task The_Previous_Image_Is_Deleted_When_It_Is_Replaced_By_A_New_One()
  ```

- Drive only the public entry point of the use case (`handler.Handle(command, …)`). Never assert on private state, call order, or internal helpers.
- Assert observable outcomes through the ports the handler depends on: captured entities passed to `AddAsync`, verified repository/storage interactions, returned DTOs, and thrown application exceptions.
- Keep the Arrange–Act–Assert structure with `// Arrange`, `// Act`, `// Assert` comments and shared private factory/setup helpers per test class.

Every behavior change should include or update tests. At minimum cover:

- [ ] The valid path and the observable result (saved state, returned value).
- [ ] Each failure path and its exception (not found → `NotFoundException`, conflicts → `ConflictException`, …).
- [ ] Compensation logic: uploaded files are cleaned up when saving fails; replaced images are removed after a successful save.
- [ ] Domain state machines: invalid transitions throw.
- [ ] Boundary values (quantities, stock limits, date ranges, rating bounds).

Where tests live:

- `Domain.Test` — entities, invariants, value objects, state transitions.
- `Application.Test` — handlers, validators, and use-case flows (one file per handler, or one per cohesive handler family).
- `Infrastructure.Test` — repositories, token generation, email rendering, image manipulation.

Run the complete suite before opening a pull request:

```bash
dotnet build --configuration Release -warnaserror
dotnet test
```

## Database and Migration Changes

When changing entities, relationships, or persistence behavior:

1. Add the migration with the command from [Local Development](#local-development).
2. **Review the generated migration.** If it contains operations unrelated to your change, the model has drifted from the last migration — do not bundle unrelated schema changes into your feature. Remove them from the migration and raise the drift separately.
3. Keep each migration tied to its feature commit.
4. Verify locally with `dotnet ef database update`.
5. Never commit connection strings, credentials, tokens, or production configuration.

## Commit and Branch Guidance

- **One commit per feature.** A commit must not mix two features. Small fixes that belong to the same feature belong to that feature's commit.
- Title the commit after the outcome, then summarize what changed in a few bullets:

  ```text
  finish orders feature

  - customers place an order from their cart (cash on delivery): prices, discounts, and stock are checked live
  - order lines snapshot product names, SKUs, and prices at purchase time
  - behaviour tests cover placement, promo application, and stock restore
  ```

- Branch names mirror the work: `feature/product-search`, `fix/order-stock-restore`, `refactor/account-token-service`, `docs/contributing-guide`.
- Keep unrelated formatting or refactoring out of feature commits.

## Pull Requests

Use the repository pull request template and include:

- A concise summary of the problem and the solution.
- Related issue links (for example `Closes #123`).
- The affected layer, feature slice, and domain capability.
- API contract, authorization, database, or configuration impact.
- Tests added or updated, and the exact validation performed (`build` + `test` output).

Before requesting review, confirm the checklist:

- [ ] `dotnet build --configuration Release -warnaserror` succeeds.
- [ ] `dotnet test` passes.
- [ ] New use cases follow the vertical slice structure.
- [ ] Clean Architecture dependency direction is preserved.
- [ ] Domain invariants and authorization rules are covered by behavior tests.
- [ ] New/changed endpoints document response codes and follow the naming and status-code conventions.
- [ ] Migrations are included and reviewed, or explicitly unnecessary.
- [ ] No secrets or local configuration values are committed.
- [ ] README/PRD updated when behavior or architecture changes.

## Reporting Bugs and Requesting Features

Use the repository issue templates:

- **Bug reports:** include the endpoint, HTTP method, request context, response code, runtime, database/tools, and reproducible steps.
- **Feature requests:** describe the business problem, the proposed solution, the affected component, and whether it affects contracts, migrations, or tests.

Security vulnerabilities should not be disclosed in a public issue. Contact the project maintainer privately using the contact information in the README.

## License

By contributing, you acknowledge that the project is licensed under the [PolyForm Noncommercial License 1.0.0](../LICENSE.md) and that contributions must remain consistent with its noncommercial scope.
