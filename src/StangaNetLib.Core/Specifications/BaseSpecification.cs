using System.Linq.Expressions;

namespace StangaNetLib.Core.Specifications;

/// <summary>
/// Base implementation of <see cref="ISpecification{T}"/>.
/// Derive concrete specifications in the Domain layer.
/// </summary>
/// <typeparam name="T">The entity type this specification filters.</typeparam>
public abstract class BaseSpecification<T> : ISpecification<T>
{
    /// <inheritdoc/>
    public Expression<Func<T, bool>> Criteria { get; private set; }
    private readonly List<Expression<Func<T, object>>> _includes = [];
    /// <inheritdoc/>
    public IReadOnlyList<Expression<Func<T, object>>> Includes => _includes;
    /// <inheritdoc/>
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    /// <inheritdoc/>
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    /// <inheritdoc/>
    public int Take { get; private set; }
    /// <inheritdoc/>
    public int Skip { get; private set; }
    /// <inheritdoc/>
    public bool IsPagingEnabled { get; private set; }

    /// <summary>Initialises the specification with the given filter expression.</summary>
    /// <param name="criteria">The LINQ predicate that entities must satisfy.</param>
    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    /// <summary>Adds an eager-load navigation expression to <see cref="Includes"/>.</summary>
    /// <param name="includeExpression">Expression selecting the navigation property to include.</param>
    protected void AddInclude(Expression<Func<T, object>> includeExpression)
        => _includes.Add(includeExpression);

    /// <summary>Sets ascending ordering and clears any descending order previously applied.</summary>
    /// <param name="orderByExpression">Expression selecting the property to order by ascending.</param>
    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
        OrderByDescending = null;
    }

    /// <summary>Sets descending ordering and clears any ascending order previously applied.</summary>
    /// <param name="orderByDescExpression">Expression selecting the property to order by descending.</param>
    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
        OrderBy = null;
    }

    /// <summary>Enables paging and sets the skip/take window.</summary>
    /// <param name="skip">Number of items to skip.</param>
    /// <param name="take">Number of items to return.</param>
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    private Func<T, bool>? _compiledCriteria;

    /// <summary>
    /// Evaluates the specification against a single in-memory entity.
    /// The expression is compiled once and cached for subsequent calls.
    /// </summary>
    /// <param name="entity">The entity to evaluate.</param>
    /// <returns><c>true</c> when the entity satisfies the specification's criteria.</returns>
    public bool IsSatisfiedBy(T entity)
        => (_compiledCriteria ??= Criteria.Compile())(entity);
}
