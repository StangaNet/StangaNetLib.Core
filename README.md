# StangaNetLib.Core

[![.NET CI](https://github.com/StangaNet/StangaNetLib.Core/actions/workflows/main.yml/badge.svg)](https://github.com/StangaNet/StangaNetLib.Core/actions/workflows/main.yml)
![NuGet](https://img.shields.io/badge/nuget-1.0.1-blue)
![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-512BD4)

StangaNetLib.Core is a lightweight, zero-dependency foundation for implementing Clean Architecture and Domain-Driven Design (DDD) in .NET. It provides the essential primitives required to build robust, testable, and maintainable domain models.

## Design Philosophy

- **Domain-Centric**: Native support for Aggregate Roots, Domain Events, and the Specification pattern.
- **Zero Dependencies**: Built exclusively on the .NET Base Class Library (BCL) to ensure maximum stability and minimal integration overhead.
- **Architectural Integrity**: Enforces separation of concerns through the Result pattern, Guard clauses, and Unit of Work abstractions.
- **Modern Standards**: Optimized for .NET 8.0 and 9.0 with a focus on performance and type safety.

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
<PackageReference Include="StangaNetLib.Core" Version="1.0.1" />
```

---

## Core Components

*The library is organized into functional namespaces. For implementation details, refer directly to the source code.*

| Namespace | Core Components | Purpose |
| :--- | :--- | :--- |
| `StangaNetLib.Core.Common` | `Entity<TId>`, `AggregateRoot<TId>`, `Result<T>`, `IUnitOfWork` | DDD primitives and functional outcome patterns. |
| `StangaNetLib.Core.Events` | `DomainEvent`, `IDomainEventDispatcher`, `IDomainEventHandler<T>` | Domain event lifecycle and dispatch infrastructure. |
| `StangaNetLib.Core.Specifications` | `ISpecification<T>`, `BaseSpecification<T>` | Encapsulated query logic and domain specifications. |
| `StangaNetLib.Core.Repositories` | `IRepository<T, TId>` | Abstractions for domain-specific persistence. |
| `StangaNetLib.Core.ValueObjects` | `ValueObject` | Structural equality primitives for domain values. |
| `StangaNetLib.Core.Guards` | `Guard.Against` | Defensive programming and assertion utilities. |
| `StangaNetLib.Core.Pagination` | `PaginationParams`, `PagedResult<T>` | Standardized pagination and paging results. |
| `StangaNetLib.Core.Validators` | `ValidationResult` | Domain-level validation contracts. |
| `StangaNetLib.Core.Exceptions` | `DomainException` | Base exceptions for domain-specific errors. |
| `StangaNetLib.Core.Auditing` | `IAuditService`, `AuditEntry` | Audit logging and tracking primitives. |

---

## Project Structure

```
StangaNetLib.Core/
├── build/
│   └── StangaNetLib.Core.props  # MSBuild auto-import — injects AssemblyMetadata into consumers
├── src/
│   └── StangaNetLib.Core/
│       ├── Auditing/          # IAuditService, AuditEntry
│       ├── Common/          # Entity, AggregateRoot, Result, Error, ErrorType, IUnitOfWork
│       ├── Events/          # DomainEvent, IDomainEventDispatcher, IDomainEventHandler
│       ├── Exceptions/      # DomainException
│       ├── Guards/          # Guard.Against
│       ├── Pagination/      # PaginationParams, PagedResult
│       ├── Repositories/    # IRepository
│       ├── Specifications/  # ISpecification, BaseSpecification
│       ├── Validators/      # ValidationResult
│       └── ValueObjects/    # ValueObject
└── tests/
    └── StangaNetLib.Core.Tests/
```
