using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Reflection;

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

      return ExecuteScalarAsync<TSource, bool>(WellKnownMembers.Queryable.All, source, predicate, cancellationToken);
    }

    public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, bool>(WellKnownMembers.Queryable.Any, source, cancellationToken);
    }

    public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteScalarAsync<TSource, bool>(
        WellKnownMembers.Queryable.AnyWithPredicate, source, predicate, cancellationToken);
    }

    // Average<int>

    public static Task<double> AverageAsync(this IQueryable<int> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<int, double>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnownTypes.Int32], source, cancellationToken);
    }

    public static Task<double?> AverageAsync(this IQueryable<int?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<int?, double?>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnownTypes.NullableInt32], source, cancellationToken);
    }

    public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, double>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnownTypes.Int32],
        source, selector, cancellationToken);
    }

    public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, double?>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnownTypes.NullableInt32],
        source, selector, cancellationToken);
    }

    // Average<long>

    public static Task<double> AverageAsync(this IQueryable<long> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<long, double>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnownTypes.Int64], source, cancellationToken);
    }

    public static Task<double?> AverageAsync(this IQueryable<long?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<long?, double?>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnownTypes.NullableInt64], source, cancellationToken);
    }

    public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, double>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnownTypes.Int64],
        source, selector, cancellationToken);
    }

    public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, double?>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnownTypes.NullableInt64],
        source, selector, cancellationToken);
    }

    // Average<double>

    public static Task<double> AverageAsync(this IQueryable<double> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<double, double>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnownTypes.Double], source, cancellationToken);
    }

    public static Task<double?> AverageAsync(this IQueryable<double?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<double?, double?>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnownTypes.NullableDouble], source, cancellationToken);
    }

    public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, double>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnownTypes.Double],
        source, selector, cancellationToken);
    }

    public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, double?>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnownTypes.NullableDouble],
        source, selector, cancellationToken);
    }

    // Average<float>

    public static Task<float> AverageAsync(this IQueryable<float> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<float, float>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnownTypes.Single], source, cancellationToken);
    }

    public static Task<float?> AverageAsync(this IQueryable<float?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<float?, float?>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnownTypes.NullableSingle], source, cancellationToken);
    }

    public static Task<float> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, float>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnownTypes.Single],
        source, selector, cancellationToken);
    }

    public static Task<float?> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, float?>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnownTypes.NullableSingle],
        source, selector, cancellationToken);
    }

    // Average<decimal>

    public static Task<decimal> AverageAsync(this IQueryable<decimal> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<decimal, decimal>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnownTypes.Decimal], source, cancellationToken);
    }

    public static Task<decimal?> AverageAsync(this IQueryable<decimal?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<decimal?, decimal?>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnownTypes.NullableDecimal], source, cancellationToken);
    }

    public static Task<decimal> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, decimal>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnownTypes.Decimal],
        source, selector, cancellationToken);
    }

    public static Task<decimal?> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, decimal?>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnownTypes.NullableDecimal],
        source, selector, cancellationToken);
    }

    // Contains

    public static Task<bool> ContainsAsync<TSource>(this IQueryable<TSource> source, TSource item,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, bool>(WellKnownMembers.Queryable.Contains,
        source, Expression.Constant(item, typeof(TSource)), cancellationToken);
    }

    // Count

    public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, int>(WellKnownMembers.Queryable.Count, source, cancellationToken);
    }

    public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteScalarAsync<TSource, int>(WellKnownMembers.Queryable.CountWithPredicate,
        source, predicate, cancellationToken);
    }

    // First

    public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.First, source, cancellationToken);
    }

    public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.FirstWithPredicate,
        source, predicate, cancellationToken);
    }

    // FirstOrDefault

    public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.FirstOrDefault, source, cancellationToken);
    }

    public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.FirstOrDefaultWithPredicate,
        source, predicate, cancellationToken);
    }

    // Last

    public static Task<TSource> LastAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.Last, source, cancellationToken);
    }

    public static Task<TSource> LastAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.LastWithPredicate,
        source, predicate, cancellationToken);
    }

    // LastOrDefault

    public static Task<TSource> LastOrDefaultAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.LastOrDefault, source, cancellationToken);
    }

    public static Task<TSource> LastOrDefaultAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.LastOrDefaultWithPredicate,
        source, predicate, cancellationToken);
    }

    // LongCount

    public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, long>(WellKnownMembers.Queryable.LongCount, source, cancellationToken);
    }

    public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteScalarAsync<TSource, long>(WellKnownMembers.Queryable.LongCountWithPredicate,
        source, predicate, cancellationToken);
    }

    // Max

    public static Task<TSource> MaxAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.Max, source, cancellationToken);
    }

    public static Task<TResult> MaxAsync<TSource, TResult>(this IQueryable<TSource> source,
      Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, TResult>(WellKnownMembers.Queryable.MaxWithSelector,
        source, selector, cancellationToken);
    }

    // Min

    public static Task<TSource> MinAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.Min, source, cancellationToken);
    }

    public static Task<TResult> MinAsync<TSource, TResult>(this IQueryable<TSource> source,
      Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, TResult>(WellKnownMembers.Queryable.MinWithSelector,
        source, selector, cancellationToken);
    }

    // Single

    public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.Single, source, cancellationToken);
    }

    public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.SingleWithPredicate,
        source, predicate, cancellationToken);
    }

    // SingleOrDefault

    public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.SingleOrDefault, source, cancellationToken);
    }

    public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.SingleOrDefaultWithPredicate,
        source, predicate, cancellationToken);
    }

    // Sum<int>

    public static Task<int> SumAsync(this IQueryable<int> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<int, int>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnownTypes.Int32], source, cancellationToken);
    }

    public static Task<int?> SumAsync(this IQueryable<int?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<int?, int?>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnownTypes.NullableInt32], source, cancellationToken);
    }

    public static Task<int> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, int>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnownTypes.Int32],
        source, selector, cancellationToken);
    }

    public static Task<int?> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, int?>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnownTypes.NullableInt32],
        source, selector, cancellationToken);
    }

    // Sum<long>

    public static Task<long> SumAsync(this IQueryable<long> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<long, long>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnownTypes.Int64], source, cancellationToken);
    }

    public static Task<long?> SumAsync(this IQueryable<long?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<long?, long?>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnownTypes.NullableInt64], source, cancellationToken);
    }

    public static Task<long> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, long>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnownTypes.Int64],
        source, selector, cancellationToken);
    }

    public static Task<long?> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, long?>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnownTypes.NullableInt64],
        source, selector, cancellationToken);
    }

    // Sum<double>

    public static Task<double> SumAsync(this IQueryable<double> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<double, double>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnownTypes.Double], source, cancellationToken);
    }

    public static Task<double?> SumAsync(this IQueryable<double?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<double?, double?>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnownTypes.NullableDouble], source, cancellationToken);
    }

    public static Task<double> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, double>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnownTypes.Double],
        source, selector, cancellationToken);
    }

    public static Task<double?> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, double?>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnownTypes.NullableDouble],
        source, selector, cancellationToken);
    }

    // Sum<float>

    public static Task<float> SumAsync(this IQueryable<float> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<float, float>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnownTypes.Single], source, cancellationToken);
    }

    public static Task<float?> SumAsync(this IQueryable<float?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<float?, float?>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnownTypes.NullableSingle], source, cancellationToken);
    }

    public static Task<float> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, float>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnownTypes.Single],
        source, selector, cancellationToken);
    }

    public static Task<float?> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, float?>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnownTypes.NullableSingle],
        source, selector, cancellationToken);
    }

    // Sum<decimal>

    public static Task<decimal> SumAsync(this IQueryable<decimal> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<decimal, decimal>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnownTypes.Decimal], source, cancellationToken);
    }

    public static Task<decimal?> SumAsync(this IQueryable<decimal?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<decimal?, decimal?>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnownTypes.NullableDecimal], source, cancellationToken);
    }

    public static Task<decimal> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, decimal>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnownTypes.Decimal],
        source, selector, cancellationToken);
    }

    public static Task<decimal?> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, decimal?>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnownTypes.NullableDecimal],
        source, selector, cancellationToken);
    }

    // Collection methods

    public static async Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      var list = new List<TSource>();
      var asyncSource = source.AsAsyncEnumerable().WithCancellation(cancellationToken).ConfigureAwait(false);
      await foreach (var element in asyncSource) {
        list.Add(element);
      }

      return list;
    }

    public static async Task<TSource[]>
      ToArrayAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default) =>
      (await source.ToListAsync(cancellationToken).ConfigureAwait(false)).ToArray();

    public static async Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TKey, TSource>(
      this IQueryable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default)
    {
      var dictionary = new Dictionary<TKey, TSource>();
      var asyncSource = source.AsAsyncEnumerable().WithCancellation(cancellationToken).ConfigureAwait(false);
      await foreach (var element in asyncSource) {
        dictionary.Add(keySelector(element), element);
      }

      return dictionary;
    }

    public static async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue, TSource>(
      this IQueryable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector,
      CancellationToken cancellationToken = default)
    {
      var dictionary = new Dictionary<TKey, TValue>();
      var asyncSource = source.AsAsyncEnumerable().WithCancellation(cancellationToken).ConfigureAwait(false);
      await foreach (var element in asyncSource) {
        dictionary.Add(keySelector(element), valueSelector(element));
      }

      return dictionary;
    }

    public static async Task<HashSet<TSource>> ToHashSetAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      var hashSet = new HashSet<TSource>();
      var asyncSource = source.AsAsyncEnumerable().WithCancellation(cancellationToken).ConfigureAwait(false);
      await foreach (var element in asyncSource) {
        hashSet.Add(element);
      }

      return hashSet;
    }

    public static async Task<ILookup<TKey, TSource>> ToLookupAsync<TKey, TSource>(this IQueryable<TSource> source,
      Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default)
    {
      var queryResult = await source.ExecuteAsync(cancellationToken).ConfigureAwait(false);
      return queryResult.ToLookup(keySelector);
    }

    public static async Task<ILookup<TKey, TValue>> ToLookupAsync<TKey, TValue, TSource>(
      this IQueryable<TSource> source,
      Func<TSource, TKey> keySelector,
      Func<TSource, TValue> valueSelector,
      CancellationToken cancellationToken = default)
    {
      var queryResult = await source.ExecuteAsync(cancellationToken).ConfigureAwait(false);
      return queryResult.ToLookup(keySelector, valueSelector);
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

    private static Task<TResult> ExecuteScalarAsync<TSource, TResult>(MethodInfo operation,
      IQueryable<TSource> source, CancellationToken cancellationToken) =>
      ExecuteScalarAsync<TSource, TResult>(operation, source, null, cancellationToken);

    private static Task<TResult> ExecuteScalarAsync<TSource, TResult>(MethodInfo operation,
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
        return provider.ExecuteScalarAsync<TResult>(Expression.Call(null, operation, arguments), cancellationToken);
      }

      throw new InvalidOperationException("QueryProvider doesn't support async operations.");
    }
  }
}