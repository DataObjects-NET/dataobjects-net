using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Linq;

namespace Xtensive.Orm
{
  /// <summary>
  /// Extends LINQ methods for <see cref="Xtensive.Orm.Linq"/> queries.
  /// </summary>
  public static partial class QueryableExtensions
  {
    public static Task<bool> AllAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteAsync<TSource, bool>(WellKnownMembers.Queryable.All, source, predicate, cancellationToken);
    }

    public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<TSource, bool>(WellKnownMembers.Queryable.Any, source, cancellationToken);
    }

    public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteAsync<TSource, bool>(
        WellKnownMembers.Queryable.AnyWithPredicate, source, predicate, cancellationToken);
    }

    // Average<int>

    public static Task<double> AverageAsync(this IQueryable<int> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<int, double>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnown.Types.Int32Type], source, cancellationToken);
    }

    public static Task<double?> AverageAsync(this IQueryable<int?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<int?, double?>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnown.Types.NullableInt32Type], source, cancellationToken);
    }

    public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, double>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnown.Types.Int32Type],
        source, selector, cancellationToken);
    }

    public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, double?>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnown.Types.NullableInt32Type],
        source, selector, cancellationToken);
    }

    // Average<long>

    public static Task<double> AverageAsync(this IQueryable<long> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<long, double>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnown.Types.Int64Type], source, cancellationToken);
    }

    public static Task<double?> AverageAsync(this IQueryable<long?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<long?, double?>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnown.Types.NullableInt64Type], source, cancellationToken);
    }

    public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, double>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnown.Types.Int64Type],
        source, selector, cancellationToken);
    }

    public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, double?>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnown.Types.NullableInt64Type],
        source, selector, cancellationToken);
    }

    // Average<double>

    public static Task<double> AverageAsync(this IQueryable<double> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<double, double>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnown.Types.DoubleType], source, cancellationToken);
    }

    public static Task<double?> AverageAsync(this IQueryable<double?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<double?, double?>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnown.Types.NullableDoubleType], source, cancellationToken);
    }

    public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, double>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnown.Types.DoubleType],
        source, selector, cancellationToken);
    }

    public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, double?>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnown.Types.NullableDoubleType],
        source, selector, cancellationToken);
    }

    // Average<float>

    public static Task<float> AverageAsync(this IQueryable<float> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<float, float>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnown.Types.FloatType], source, cancellationToken);
    }

    public static Task<float?> AverageAsync(this IQueryable<float?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<float?, float?>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnown.Types.NullableFloatType], source, cancellationToken);
    }

    public static Task<float> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, float>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnown.Types.FloatType],
        source, selector, cancellationToken);
    }

    public static Task<float?> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, float?>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnown.Types.NullableFloatType],
        source, selector, cancellationToken);
    }

    // Average<decimal>

    public static Task<decimal> AverageAsync(this IQueryable<decimal> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<decimal, decimal>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnown.Types.DecimalType], source, cancellationToken);
    }

    public static Task<decimal?> AverageAsync(this IQueryable<decimal?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<decimal?, decimal?>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnown.Types.NullableDecimalType], source, cancellationToken);
    }

    public static Task<decimal> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, decimal>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnown.Types.DecimalType],
        source, selector, cancellationToken);
    }

    public static Task<decimal?> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, decimal?>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnown.Types.NullableDecimalType],
        source, selector, cancellationToken);
    }

    // Contains

    public static Task<bool> ContainsAsync<TSource>(this IQueryable<TSource> source, TSource item,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<TSource, bool>(WellKnownMembers.Queryable.Contains,
        source, Expression.Constant(item, typeof(TSource)), cancellationToken);
    }

    // Count

    public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<TSource, int>(WellKnownMembers.Queryable.Count, source, cancellationToken);
    }

    public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteAsync<TSource, int>(WellKnownMembers.Queryable.CountWithPredicate,
        source, predicate, cancellationToken);
    }

    // First / FirstOrDefault

    public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<TSource, TSource>(WellKnownMembers.Queryable.First, source, cancellationToken);
    }

    public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteAsync<TSource, TSource>(WellKnownMembers.Queryable.FirstWithPredicate,
        source, predicate, cancellationToken);
    }

    public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<TSource, TSource>(WellKnownMembers.Queryable.FirstOrDefault, source, cancellationToken);
    }

    public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteAsync<TSource, TSource>(WellKnownMembers.Queryable.FirstOrDefaultWithPredicate,
        source, predicate, cancellationToken);
    }

    // Last / LastOrDefault

    public static Task<TSource> LastAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<TSource, TSource>(WellKnownMembers.Queryable.Last, source, cancellationToken);
    }

    public static Task<TSource> LastAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteAsync<TSource, TSource>(WellKnownMembers.Queryable.LastWithPredicate,
        source, predicate, cancellationToken);
    }

    public static Task<TSource> LastOrDefaultAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<TSource, TSource>(WellKnownMembers.Queryable.LastOrDefault, source, cancellationToken);
    }

    public static Task<TSource> LastOrDefaultAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteAsync<TSource, TSource>(WellKnownMembers.Queryable.LastOrDefaultWithPredicate,
        source, predicate, cancellationToken);
    }

    // LongCount

    public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<TSource, long>(WellKnownMembers.Queryable.LongCount, source, cancellationToken);
    }

    public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteAsync<TSource, long>(WellKnownMembers.Queryable.LongCountWithPredicate,
        source, predicate, cancellationToken);
    }

    // Max / Min

    public static Task<TSource> MaxAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<TSource, TSource>(WellKnownMembers.Queryable.Max, source, cancellationToken);
    }

    public static Task<TSource> MaxAsync<TSource, TResult>(this IQueryable<TSource> source,
      Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, TSource>(WellKnownMembers.Queryable.MinWithSelector,
        source, selector, cancellationToken);
    }

    public static Task<TSource> MinAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<TSource, TSource>(WellKnownMembers.Queryable.Max, source, cancellationToken);
    }

    public static Task<TSource> MinAsync<TSource, TResult>(this IQueryable<TSource> source,
      Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, TSource>(WellKnownMembers.Queryable.MinWithSelector,
        source, selector, cancellationToken);
    }

    // Single / SingleOrDefault

    public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<TSource, TSource>(WellKnownMembers.Queryable.Single, source, cancellationToken);
    }

    public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteAsync<TSource, TSource>(WellKnownMembers.Queryable.SingleWithPredicate,
        source, predicate, cancellationToken);
    }

    public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<TSource, TSource>(WellKnownMembers.Queryable.SingleOrDefault, source, cancellationToken);
    }

    public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteAsync<TSource, TSource>(WellKnownMembers.Queryable.SingleOrDefaultWithPredicate,
        source, predicate, cancellationToken);
    }

    // Average<int>

    public static Task<int> SumAsync(this IQueryable<int> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<int, int>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnown.Types.Int32Type], source, cancellationToken);
    }

    public static Task<int?> SumAsync(this IQueryable<int?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<int?, int?>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnown.Types.NullableInt32Type], source, cancellationToken);
    }

    public static Task<int> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, int>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnown.Types.Int32Type],
        source, selector, cancellationToken);
    }

    public static Task<int?> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, int?>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnown.Types.NullableInt32Type],
        source, selector, cancellationToken);
    }

    // Average<long>

    public static Task<long> SumAsync(this IQueryable<long> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<long, long>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnown.Types.Int64Type], source, cancellationToken);
    }

    public static Task<long?> SumAsync(this IQueryable<long?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<long?, long?>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnown.Types.NullableInt64Type], source, cancellationToken);
    }

    public static Task<long> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, long>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnown.Types.Int64Type],
        source, selector, cancellationToken);
    }

    public static Task<long?> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, long?>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnown.Types.NullableInt64Type],
        source, selector, cancellationToken);
    }

    // Average<double>

    public static Task<double> SumAsync(this IQueryable<double> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<double, double>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnown.Types.DoubleType], source, cancellationToken);
    }

    public static Task<double?> SumAsync(this IQueryable<double?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<double?, double?>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnown.Types.NullableDoubleType], source, cancellationToken);
    }

    public static Task<double> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, double>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnown.Types.DoubleType],
        source, selector, cancellationToken);
    }

    public static Task<double?> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, double?>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnown.Types.NullableDoubleType],
        source, selector, cancellationToken);
    }

    // Average<float>

    public static Task<float> SumAsync(this IQueryable<float> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<float, float>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnown.Types.FloatType], source, cancellationToken);
    }

    public static Task<float?> SumAsync(this IQueryable<float?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<float?, float?>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnown.Types.NullableFloatType], source, cancellationToken);
    }

    public static Task<float> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, float>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnown.Types.FloatType],
        source, selector, cancellationToken);
    }

    public static Task<float?> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, float?>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnown.Types.NullableFloatType],
        source, selector, cancellationToken);
    }

    // Average<decimal>

    public static Task<decimal> SumAsync(this IQueryable<decimal> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<decimal, decimal>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnown.Types.DecimalType], source, cancellationToken);
    }

    public static Task<decimal?> SumAsync(this IQueryable<decimal?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteAsync<decimal?, decimal?>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnown.Types.NullableDecimalType], source, cancellationToken);
    }

    public static Task<decimal> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, decimal>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnown.Types.DecimalType],
        source, selector, cancellationToken);
    }

    public static Task<decimal?> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteAsync<TSource, decimal?>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnown.Types.NullableDecimalType],
        source, selector, cancellationToken);
    }

    // Collection methods

    public static async Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      var list = new List<TSource>();
      await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken)) {
        list.Add(element);
      }

      return list;
    }

    public static async Task<TSource[]> ToArrayAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default) => (await source.ToListAsync(cancellationToken)).ToArray();

    public static async Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TKey, TSource>(
      this IQueryable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default)
    {
      var dictionary = new Dictionary<TKey, TSource>();
      await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken)) {
        dictionary.Add(keySelector(element), element);
      }

      return dictionary;
    }

    public static async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue, TSource>(
      this IQueryable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector,
      CancellationToken cancellationToken = default)
    {
      var dictionary = new Dictionary<TKey, TValue>();
      await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken)) {
        dictionary.Add(keySelector(element), valueSelector(element));
      }

      return dictionary;
    }

    public static async Task<HashSet<TSource>> ToHashSetAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      var hashSet = new HashSet<TSource>();
      await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken)) {
        hashSet.Add(element);
      }

      return hashSet;
    }

    public static async Task<HashSet<TElement>> ToHashSetAsync<TElement, TSource>(this IQueryable<TSource> source,
      Func<TSource, TElement> elementSelector, CancellationToken cancellationToken = default)
    {
      var hashSet = new HashSet<TElement>();
      await foreach (var item in source.AsAsyncEnumerable().WithCancellation(cancellationToken)) {
        hashSet.Add(elementSelector(item));
      }

      return hashSet;
    }

    public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(this IQueryable<TSource> source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      if (source is IAsyncEnumerable<TSource> asyncEnumerable) {
        return asyncEnumerable;
      }

      throw new InvalidOperationException("Query can't be executed asynchronously.");
    }

    // Private methods

    private static Task<TResult> ExecuteAsync<TSource, TResult>(MethodInfo operation,
      IQueryable<TSource> source, CancellationToken cancellationToken) =>
      ExecuteAsync<TSource, TResult>(operation, source, null, cancellationToken);

    private static Task<TResult> ExecuteAsync<TSource, TResult>(MethodInfo operation,
      IQueryable<TSource> source,
      Expression expression,
      CancellationToken cancellationToken = default)
    {
      if (source.Provider is QueryProvider provider) {
        if (operation.IsGenericMethod) {
          operation
            = operation.GetGenericArguments().Length == 2
              ? operation.MakeGenericMethod(typeof(TSource), typeof(TResult))
              : operation.MakeGenericMethod(typeof(TSource));
        }

        var arguments = expression == null ? new[] {source.Expression} : new[] {source.Expression, expression};
        return provider.ExecuteAsync<TResult>(Expression.Call(null, operation, arguments), cancellationToken);
      }

      throw new InvalidOperationException("QueryProvider doesn't support async operations.");
    }
  }
}