# StangaNetLib.Core

Foundation library for **Clean Architecture** and **Domain-Driven Design** in .NET.
Provides the building blocks — Entity, Result, ValueObject, Specifications, Guards, Pagination, Domain Events — with **zero external dependencies**.

[![.NET CI](https://github.com/StangaNet/StangaNetLib.Core/actions/workflows/main.yml/badge.svg)](https://github.com/StangaNet/StangaNetLib.Core/actions/workflows/main.yml)
![NuGet](https://img.shields.io/badge/nuget-1.0.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-512BD4)

---

## Installation

The package is published to **GitHub Packages**.

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

---

## Contents

| Namespace | Types | Purpose |
|---|---|---|
| `StangaNetLib.Core.Common` | `Entity<TId>`, `Entity`, `Result<T>`, `Result`, `Error`, `IUnitOfWork` | Core DDD building blocks |
| `StangaNetLib.Core.ValueObjects` | `ValueObject` | Structural-equality value objects |
| `StangaNetLib.Core.Events` | `DomainEvent`, `IDomainEventDispatcher`, `IDomainEventHandler<T>` | Domain event infrastructure |
| `StangaNetLib.Core.Exceptions` | `DomainException` | Base domain exception |
| `StangaNetLib.Core.Guards` | `Guard.Against` | Defensive guard clauses |
| `StangaNetLib.Core.Pagination` | `PaginationParams`, `PagedResult<T>` | Paginated query support |
| `StangaNetLib.Core.Repositories` | `IRepository<T, TId>`, `IRepository<T>` | Generic repository contract |
| `StangaNetLib.Core.Specifications` | `ISpecification<T>`, `BaseSpecification<T>` | Specification pattern |
| `StangaNetLib.Core.Validators` | `ValidationResult` | Domain-level validation result |

---

## Entity

`Entity<TId>` is the base class for all domain entities. It provides:

- A strongly-typed `Id` property (any `notnull` type).
- `CreatedAt` / `UpdatedAt` UTC timestamps.
- Domain event collection (`AddDomainEvent`, `ClearDomainEvents`).
- Structural equality by `Id` (overrides `Equals`, `GetHashCode`, `==`, `!=`).

`Entity` is the convenience base class for entities keyed by `Guid` (the common case).

```csharp
public class Order : Entity          // Guid key, auto-generated
{
    public string CustomerName { get; private set; }

    public Order(string customerName) : base()
    {
        CustomerName = Guard.Against.NullOrWhiteSpace(customerName, nameof(customerName));
        AddDomainEvent(new OrderCreatedEvent(Id));
    }

    public void Rename(string newName)
    {
        CustomerName = Guard.Against.NullOrWhiteSpace(newName, nameof(newName));
        MarkUpdated();
    }
}

// Custom key type
public class Article : Entity<int>
{
    public Article(int id) : base(id) { }
}
```

---

## Result / Error

`Result<T>` and `Result` represent the outcome of an operation without throwing exceptions.
`Error` is a structured error with a machine-readable `Code` and a human-readable `Description`.

### Creating results

```csharp
// Success
Result<Order> ok = Result<Order>.Success(order);
Result<Order> ok = order;              // implicit conversion

// Failure — single error
Result<Order> fail = Result<Order>.Failure(Error.NotFound("Order", id));
Result<Order> fail = Error.NotFound("Order", id);  // implicit conversion

// Failure — multiple errors (e.g. validation)
Result<Order> fail = Result<Order>.Failure(new[]
{
    Error.Validation("Name", "Too short"),
    Error.Validation("Email", "Invalid format"),
});

// Void result
Result voidOk   = Result.Success();
Result voidFail = Result.Failure(Error.Internal("Something went wrong."));
```

### Built-in Error factories

```csharp
Error.Validation("Email", "Invalid format")   // Code: "Validation.Email"
Error.NotFound("Order", 42)                   // Code: "Order.NotFound"
Error.Conflict("Order.Duplicate", "…")
Error.Unauthorized()                          // Code: "Auth.Unauthorized"
Error.Forbidden()                             // Code: "Auth.Forbidden"
Error.Internal("Unexpected error.")           // Code: "Internal.Error"
```

### Combinators

```csharp
// Match — branch on success/failure
IActionResult response = result.Match(
    onSuccess: order  => Ok(order),
    onFailure: error  => BadRequest(error.Description));

// Map — transform the value on success
Result<string> name = result.Map(o => o.CustomerName);

// MapAsync — async transform
Result<OrderDto> dto = await result.MapAsync(o => MapToDto(o));

// Bind — chain another Result-returning operation
Result<Invoice> invoice = result.Bind(o => CreateInvoice(o));

// BindAsync — async chain
Result<Invoice> invoice = await result.BindAsync(o => CreateInvoiceAsync(o));
```

---

## ValueObject

`ValueObject` provides structural equality based on the components you expose.

```csharp
public class Money : ValueObject
{
    public decimal Amount   { get; }
    public string  Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount   = amount;
        Currency = currency;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}

var a = new Money(10m, "EUR");
var b = new Money(10m, "EUR");
Console.WriteLine(a == b);  // True
```

---

## Domain Events

Raise domain events inside entities; dispatch them after persistence.

```csharp
// 1. Define an event
public class OrderCreatedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public OrderCreatedEvent(Guid orderId) => OrderId = orderId;
}

// 2. Raise it inside an entity
public class Order : Entity
{
    public Order()
    {
        AddDomainEvent(new OrderCreatedEvent(Id));
    }
}

// 3. Handle it (implement in Infrastructure / Application layer)
public class SendOrderConfirmationHandler : IDomainEventHandler<OrderCreatedEvent>
{
    public async Task HandleAsync(OrderCreatedEvent e, CancellationToken ct)
    {
        // send email, update read models, etc.
    }
}

// 4. Dispatch after SaveChanges (implement IDomainEventDispatcher in Infrastructure)
await dispatcher.DispatchManyAsync(order.DomainEvents, cancellationToken);
order.ClearDomainEvents();
```

---

## Guard Clauses

Defensive assertions at the beginning of constructors and methods.
All clauses return the validated value on success so they can be used inline.

```csharp
// Null checks
var name  = Guard.Against.Null(value, nameof(value));
var name  = Guard.Against.NullOrEmpty(str, nameof(str));
var name  = Guard.Against.NullOrWhiteSpace(str, nameof(str));

// Numeric checks
var count = Guard.Against.Negative(count, nameof(count));
var count = Guard.Against.NegativeOrZero(count, nameof(count));
var age   = Guard.Against.OutOfRange(age, nameof(age), min: 0, max: 150);

// Guid
var id    = Guard.Against.EmptyGuid(id, nameof(id));
var val   = Guard.Against.Default(val, nameof(val));   // any struct

// String length
var code  = Guard.Against.TooShort(code, nameof(code), minLength: 3);
var code  = Guard.Against.TooLong(code, nameof(code), maxLength: 50);

// Collection
var items = Guard.Against.NullOrEmpty(items, nameof(items));
```

All clauses throw standard BCL exceptions (`ArgumentNullException`, `ArgumentException`, `ArgumentOutOfRangeException`).

---

## Pagination

```csharp
// Input — bind from query string
var p = new PaginationParams { Page = 2, PageSize = 50 };
// p.Skip == 50  (derived, ready for EF Core .Skip(p.Skip).Take(p.PageSize))

// Output
var paged = new PagedResult<Order>(orders, p.Page, p.PageSize, totalCount);

Console.WriteLine(paged.TotalPages);       // ceil(totalCount / PageSize)
Console.WriteLine(paged.HasNextPage);      // Page < TotalPages
Console.WriteLine(paged.HasPreviousPage);  // Page > 1

// Map to DTO without losing metadata
PagedResult<OrderDto> dto = paged.Map(o => new OrderDto(o));

// Empty result
var empty = PagedResult<Order>.Empty();
```

`PageSize` is automatically clamped between 1 and `PaginationParams.MaxPageSize` (100).

---

## Repository

Define domain-specific repositories by extending the generic interface.

```csharp
// In the Domain layer — extend with domain-specific queries
public interface IOrderRepository : IRepository<Order>   // Guid key
{
    Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default);
}

// Custom key type
public interface IArticleRepository : IRepository<Article, int> { }
```

Implement in the Infrastructure layer (e.g. wrapping EF Core).
Persistence is deferred: `AddAsync` / `UpdateAsync` / `DeleteAsync` stage changes; call `IUnitOfWork.SaveChangesAsync` to commit.

### IUnitOfWork

```csharp
await unitOfWork.BeginTransactionAsync();
try
{
    await orderRepo.AddAsync(order);
    await unitOfWork.SaveChangesAsync();
    await unitOfWork.CommitTransactionAsync();
}
catch
{
    await unitOfWork.RollbackTransactionAsync();
    throw;
}
```

---

## Specification Pattern

Encapsulate query logic in self-contained, testable specification classes.

```csharp
// Define a specification in the Domain layer
public class ActiveOrdersSpec : BaseSpecification<Order>
{
    public ActiveOrdersSpec(Guid customerId)
        : base(o => o.CustomerId == customerId && o.Status == OrderStatus.Active)
    {
        AddInclude(o => o.Items);
        ApplyOrderByDescending(o => o.CreatedAt);
        ApplyPaging(skip: 0, take: 20);
    }
}

// Unit-test the specification logic in-memory (no database required)
var spec = new ActiveOrdersSpec(customerId);
bool matches = spec.IsSatisfiedBy(order);   // compiles and runs the expression

// Apply in the Infrastructure layer (EF Core example)
IQueryable<Order> query = dbSet
    .Where(spec.Criteria);

foreach (var include in spec.Includes)
    query = query.Include(include);

if (spec.IsPagingEnabled)
    query = query.Skip(spec.Skip).Take(spec.Take);
```

---

## ValidationResult

Domain-level validation, independent of FluentValidation (which belongs in the Application layer).

```csharp
public static class OrderValidator
{
    public static ValidationResult Validate(Order order)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(order.CustomerName))
            errors.Add("Customer name is required.");

        if (order.TotalAmount <= 0)
            errors.Add("Total amount must be positive.");

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }
}

// Usage
var validation = OrderValidator.Validate(order);

if (validation.IsFailure)
{
    // Convert to Result for use-case return values
    return validation.ToResult<Order>();

    // Or get the combined message
    Console.WriteLine(validation.GetErrorMessage());
}
```

---

## DomainException

Base class for typed domain exceptions. Derive project-specific exceptions from it.

```csharp
public class OrderNotFoundException : DomainException
{
    public OrderNotFoundException(Guid id)
        : base($"Order '{id}' was not found.") { }
}

public class InvalidOrderStateException : DomainException
{
    public InvalidOrderStateException(string message) : base(message) { }
}
```

---

## Project Structure

```
StangaNetLib.Core/
├── src/
│   └── StangaNetLib.Core/
│       ├── Common/          # Entity, Result, Error, IUnitOfWork
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

---

## CI / CD

The GitHub Actions workflow (`.github/workflows/main.yml`) runs on `workflow_dispatch` and:

1. Restores, builds, and tests against both **net8.0** and **net9.0**.
2. Packs the NuGet package.
3. Publishes to **GitHub Packages**.
