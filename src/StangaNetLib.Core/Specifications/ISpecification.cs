using System.Linq.Expressions;

namespace StangaNetLib.Core.Specifications;

/// <summary>
/// Encapsulates a query criterion that can be applied to a collection of <typeparamref name="T"/>.
/// Supports includes, ordering, and pagination for use with ORM repositories.
/// </summary>
public interface ISpecification<T>
{
    /// <summary>The filter expression to apply to the query.</summary>
    Expression<Func<T, bool>> Criteria { get; }

    /// <summary>Navigation properties to eager-load.</summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>Ascending ordering expression. Null if no ordering.</summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>Descending ordering expression. Null if no ordering.</summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>Number of items to take (used with paging).</summary>
    int Take { get; }

    /// <summary>Number of items to skip (used with paging).</summary>
    int Skip { get; }

    /// <summary>True when paging parameters have been applied.</summary>
    bool IsPagingEnabled { get; }

    /// <summary>Evaluates the specification against an in-memory entity (for unit testing).</summary>
    bool IsSatisfiedBy(T entity);
}
