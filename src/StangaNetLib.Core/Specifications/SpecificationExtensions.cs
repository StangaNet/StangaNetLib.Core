using System.Linq.Expressions;

namespace StangaNetLib.Core.Specifications;

/// <summary>
/// Extension methods for composing specifications using Boolean operators.
/// Resulting specifications combine only the filter criteria; includes, ordering, and paging
/// are not inherited (define a dedicated spec or repository method for those concerns).
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Returns a specification satisfied when both <paramref name="left"/> and <paramref name="right"/> are satisfied.
    /// The combined expression is EF Core-translatable.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="left">The first specification.</param>
    /// <param name="right">The second specification.</param>
    /// <returns>A composite specification whose criteria is the logical AND of both inputs.</returns>
    public static ISpecification<T> And<T>(this ISpecification<T> left, ISpecification<T> right)
        => new CompositeSpecification<T>(ExpressionHelper.And(left.Criteria, right.Criteria));

    /// <summary>
    /// Returns a specification satisfied when either <paramref name="left"/> or <paramref name="right"/> is satisfied.
    /// The combined expression is EF Core-translatable.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="left">The first specification.</param>
    /// <param name="right">The second specification.</param>
    /// <returns>A composite specification whose criteria is the logical OR of both inputs.</returns>
    public static ISpecification<T> Or<T>(this ISpecification<T> left, ISpecification<T> right)
        => new CompositeSpecification<T>(ExpressionHelper.Or(left.Criteria, right.Criteria));

    /// <summary>
    /// Returns a specification satisfied when <paramref name="spec"/> is not satisfied.
    /// The negated expression is EF Core-translatable.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to negate.</param>
    /// <returns>A composite specification whose criteria is the logical NOT of <paramref name="spec"/>.</returns>
    public static ISpecification<T> Not<T>(this ISpecification<T> spec)
        => new CompositeSpecification<T>(ExpressionHelper.Not(spec.Criteria));
}

/// <summary>
/// Internal composite specification produced by <see cref="SpecificationExtensions"/> And/Or/Not.
/// Carries filter criteria only; includes, ordering, and paging are intentionally empty.
/// </summary>
internal sealed class CompositeSpecification<T> : ISpecification<T>
{
    private Func<T, bool>? _compiled;

    internal CompositeSpecification(Expression<Func<T, bool>> criteria)
        => Criteria = criteria;

    /// <inheritdoc/>
    public Expression<Func<T, bool>> Criteria { get; }

    /// <inheritdoc/>
    public IReadOnlyList<Expression<Func<T, object>>> Includes => [];

    /// <inheritdoc/>
    public Expression<Func<T, object>>? OrderBy => null;

    /// <inheritdoc/>
    public Expression<Func<T, object>>? OrderByDescending => null;

    /// <inheritdoc/>
    public int Take => 0;

    /// <inheritdoc/>
    public int Skip => 0;

    /// <inheritdoc/>
    public bool IsPagingEnabled => false;

    /// <inheritdoc/>
    public bool IsSatisfiedBy(T entity) => (_compiled ??= Criteria.Compile())(entity);
}
