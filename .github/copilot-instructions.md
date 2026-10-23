# StangaNetLib.Core — Copilot instructions

- This is a Clean Architecture / DDD foundation library (`Result<T>`, entities, specs, guards).
- Prefer `Result` / `Result<T>` for expected failures; reserve exceptions for exceptional cases (`DomainException` hierarchy).
- Keep the public API surface small, documented (`///`), and free of infrastructure concerns (no EF, no HTTP).
- Ports such as `IRepository`, `IUnitOfWork`, and `IDomainEventDispatcher` stay as abstractions — do not add in-library implementations.
- Match existing naming, file-scoped namespaces, and EditorConfig / Roslyn conventions.
- Tests use xUnit v3 + FluentAssertions; cover `net8.0`, `net9.0`, and `net10.0` when changing shared behavior.
