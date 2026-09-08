using System.Linq.Expressions;

namespace StangaNetLib.Core.Specifications;

internal static class ExpressionHelper
{
    internal static Expression<Func<T, bool>> And<T>(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var param = left.Parameters[0];
        var body = Expression.AndAlso(
            left.Body,
            new ParameterReplacer(right.Parameters[0], param).Visit(right.Body));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    internal static Expression<Func<T, bool>> Or<T>(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var param = left.Parameters[0];
        var body = Expression.OrElse(
            left.Body,
            new ParameterReplacer(right.Parameters[0], param).Visit(right.Body));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    internal static Expression<Func<T, bool>> Not<T>(Expression<Func<T, bool>> spec)
        => Expression.Lambda<Func<T, bool>>(Expression.Not(spec.Body), spec.Parameters[0]);

    private sealed class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParam ? newParam : base.VisitParameter(node);
    }
}
