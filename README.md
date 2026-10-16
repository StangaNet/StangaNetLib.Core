# StangaNetLib.Core

[![.NET CI](https://github.com/StangaNet/StangaNetLib.Core/actions/workflows/main.yml/badge.svg)](https://github.com/StangaNet/StangaNetLib.Core/actions/workflows/main.yml)
![NuGet](https://img.shields.io/badge/nuget-1.0.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-512BD4)

StangaNetLib.Core is a lightweight foundation for Clean Architecture and Domain-Driven Design (DDD) in .NET. It provides essential primitives for robust, testable domain models: `Result<T>`, entities, aggregate roots, specifications, guards, and more.

## Design Philosophy

- **Domain-Centric**: Aggregate roots, domain events, and the Specification pattern.
- **Minimal dependencies**: Only `Microsoft.Extensions.Options` (for `SettingsValidatorBase`); everything else is BCL.
- **Architectural Integrity**: Result pattern, guard clauses, and Unit of Work / repository ports.
- **Modern Standards**: .NET 8.0 and 9.0, nullable reference types, documented public API.

## Installation

The package is hosted on **GitHub Packages**.

```xml
<!-- NuGet.config — add the GitHub Packages source -->
<configuration>
  <packageSources>
    <add key="github" value="https://nuget.pkg.github.com/StangaNet/index.json" />
  </packageSources>
</configuration>
```

```xml
<!-- .csproj -->
<PackageReference Include="StangaNetLib.Core" Version="1.0.0" />
```

See [CHANGELOG.md](./CHANGELOG.md) for release history.

---

## Core Components

*The library is organized into functional namespaces. For implementation details, refer directly to the source code.*

| Namespace | Core Components | Purpose |
| :--- | :--- | :--- |
| `StangaNetLib.Core.Common` | `Entity<TId>`, `AggregateRoot<TId>`, `Result<T>`, `Error`, `IUnitOfWork` | DDD primitives and functional outcome patterns. |
| `StangaNetLib.Core.Events` | `DomainEvent`, `IDomainEventDispatcher`, `IDomainEventHandler<T>` | Domain event lifecycle and dispatch ports. |
| `StangaNetLib.Core.Specifications` | `ISpecification<T>`, `BaseSpecification<T>` | Encapsulated query logic and domain specifications. |
| `StangaNetLib.Core.Repositories` | `IRepository<T, TId>` | Persistence ports for aggregates/entities. |
| `StangaNetLib.Core.ValueObjects` | `ValueObject` | Structural equality primitives for domain values. |
| `StangaNetLib.Core.Guards` | `Guard.Against` | Defensive programming and assertion utilities. |
| `StangaNetLib.Core.Pagination` | `PaginationParams`, `PagedResult<T>` | Standardized pagination and paging results. |
| `StangaNetLib.Core.Validators` | `ValidationResult`, `SettingsValidatorBase<TOptions>` | Domain validation and options validation base. |
| `StangaNetLib.Core.Exceptions` | `DomainException` | Base exceptions for domain-specific errors. |
| `StangaNetLib.Core.Auditing` | `IAuditService`, `AuditEntry` | Audit logging ports and entry model. |

---

## Project Structure

```
StangaNetLib.Core/
├── assets/
│   └── package-icon.png
├── build/
│   └── StangaNetLib.Core.props  # MSBuild auto-import — injects AssemblyMetadata into consumers
├── src/
│   └── StangaNetLib.Core/
│       ├── Auditing/          # IAuditService, AuditEntry
│       ├── Common/            # Entity, AggregateRoot, Result, Error, ErrorType, IUnitOfWork
│       ├── Events/            # DomainEvent, IDomainEventDispatcher, IDomainEventHandler
│       ├── Exceptions/        # DomainException
│       ├── Guards/            # Guard.Against
│       ├── Pagination/        # PaginationParams, PagedResult
│       ├── Repositories/      # IRepository
│       ├── Specifications/    # ISpecification, BaseSpecification
│       ├── Validators/        # ValidationResult, SettingsValidatorBase
│       └── ValueObjects/      # ValueObject
└── tests/
    └── StangaNetLib.Core.Tests/
```
