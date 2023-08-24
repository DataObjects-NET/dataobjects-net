// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Linq;
using Xtensive.Reflection;

namespace Xtensive.Orm
{
  /// <summary>
  /// Extends LINQ methods for <see cref="Xtensive.Orm.Linq"/> queries.
  /// </summary>
  public static partial class QueryableExtensions
  {
    /// <summary>
    /// Asynchronously determines whether all the elements of a sequence satisfy a condition.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">An <see cref="IQueryable{T}"/> whose elements to test for a condition.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true
    /// if every element of the source sequence passes the test in the specified predicate;
    /// otherwise, false. </returns>
    public static Task<bool> AllAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteScalarAsync<TSource, bool>(WellKnownMembers.Queryable.All, source, predicate, cancellationToken);
    }

    /// <summary>
    /// Asynchronously determines whether a sequence contains any elements.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{T}"/> to check for being empty.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true
    /// if the source sequence contains any elements; otherwise, false.</returns>
    public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, bool>(WellKnownMembers.Queryable.Any, source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously determines whether any element of a sequence satisfies a condition.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{T}"/> whose elements to test for a condition.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true
    /// if any elements in the source sequence pass the test in the specified predicate;
    /// otherwise, false.</returns>
    public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteScalarAsync<TSource, bool>(
        WellKnownMembers.Queryable.AnyWithPredicate, source, predicate, cancellationToken);
    }

    // Average<int>

    /// <summary>
    /// Asynchronously computes the average of a sequence of values.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// average of the sequence of values.</returns>
    public static Task<double> AverageAsync(this IQueryable<int> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<int, double>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnownTypes.Int32], source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the average of a sequence of values.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// average of the sequence of values.</returns>
    public static Task<double?> AverageAsync(this IQueryable<int?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<int?, double?>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnownTypes.NullableInt32], source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the average of a sequence of values that is obtained
    /// by invoking a projection function on each element of the input sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// average of the projected values.</returns>
    public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, double>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnownTypes.Int32],
        source, selector, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the average of a sequence of values that is obtained
    /// by invoking a projection function on each element of the input sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// average of the projected values.</returns>
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

    /// <summary>
    /// Asynchronously computes the average of a sequence of values.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// average of the sequence of values.</returns>
    public static Task<double> AverageAsync(this IQueryable<long> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<long, double>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnownTypes.Int64], source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the average of a sequence of values.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// average of the sequence of values.</returns>
    public static Task<double?> AverageAsync(this IQueryable<long?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<long?, double?>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnownTypes.NullableInt64], source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the average of a sequence of values that is obtained
    /// by invoking a projection function on each element of the input sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// average of the projected values.</returns>
    public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, double>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnownTypes.Int64],
        source, selector, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the average of a sequence of values that is obtained
    /// by invoking a projection function on each element of the input sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// average of the projected values.</returns>
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

    /// <summary>
    /// Asynchronously computes the average of a sequence of values.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// average of the sequence of values.</returns>
    public static Task<double> AverageAsync(this IQueryable<double> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<double, double>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnownTypes.Double], source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the average of a sequence of values.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// average of the sequence of values.</returns>
    public static Task<double?> AverageAsync(this IQueryable<double?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<double?, double?>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnownTypes.NullableDouble], source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the average of a sequence of values that is obtained
    /// by invoking a projection function on each element of the input sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// average of the projected values.</returns>
    public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, double>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnownTypes.Double],
        source, selector, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the average of a sequence of values that is obtained
    /// by invoking a projection function on each element of the input sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// average of the projected values.</returns>
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

    /// <summary>
    /// Asynchronously computes the average of a sequence of values.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// average of the sequence of values.</returns>
    public static Task<float> AverageAsync(this IQueryable<float> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<float, float>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnownTypes.Single], source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the average of a sequence of values.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// average of the sequence of values.</returns>
    public static Task<float?> AverageAsync(this IQueryable<float?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<float?, float?>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnownTypes.NullableSingle], source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the average of a sequence of values that is obtained
    /// by invoking a projection function on each element of the input sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// average of the projected values.</returns>
    public static Task<float> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, float>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnownTypes.Single],
        source, selector, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the average of a sequence of values that is obtained
    /// by invoking a projection function on each element of the input sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// average of the projected values.</returns>
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

    /// <summary>
    /// Asynchronously computes the average of a sequence of values.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// average of the sequence of values.</returns>
    public static Task<decimal> AverageAsync(this IQueryable<decimal> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<decimal, decimal>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnownTypes.Decimal], source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the average of a sequence of values.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// average of the sequence of values.</returns>
    public static Task<decimal?> AverageAsync(this IQueryable<decimal?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<decimal?, decimal?>(
        WellKnownMembers.Queryable.AverageMethodInfos[WellKnownTypes.NullableDecimal], source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the average of a sequence of values that is obtained
    /// by invoking a projection function on each element of the input sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// average of the projected values.</returns>
    public static Task<decimal> AverageAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, decimal>(
        WellKnownMembers.Queryable.AverageWithSelectorMethodInfos[WellKnownTypes.Decimal],
        source, selector, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the average of a sequence of values that is obtained
    /// by invoking a projection function on each element of the input sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// average of the projected values.</returns>
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

    /// <summary>
    /// Asynchronously determines whether a sequence contains a specified element.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> to return the ssingle element of.</param>
    /// <param name="item">The object to locate in the sequence.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true
    /// if the input sequence contains the specified value; otherwise, false.</returns>
    public static Task<bool> ContainsAsync<TSource>(this IQueryable<TSource> source, TSource item,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, bool>(WellKnownMembers.Queryable.Contains,
        source, Expression.Constant(item, typeof(TSource)), cancellationToken);
    }

    // Count

    /// <summary>
    /// Asynchronously returns the number of elements in a sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> that contains elements to be counted.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// number of elements in the input sequence.</returns>
    public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, int>(WellKnownMembers.Queryable.Count, source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns the number of elements in a sequence that satisfy a condition.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> that contains elements to be counted.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// number of elements in the sequence that satisfy the condition in the predicate function.</returns>
    public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteScalarAsync<TSource, int>(WellKnownMembers.Queryable.CountWithPredicate,
        source, predicate, cancellationToken);
    }

    // First

    /// <summary>
    /// Asynchronously returns the first element of a sequence
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> to return the first element of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    ///  first element in source.</returns>
    public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.First, source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns the first element of a sequence that satisfies a specified condition.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> to return the first element of.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    ///  first element in source that passes the test in predicate.</returns>
    public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.FirstWithPredicate,
        source, predicate, cancellationToken);
    }

    // FirstOrDefault

    /// <summary>
    /// Asynchronously returns the first element of a sequence, or a default value if
    /// the sequence contains no elements.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> to return the first element of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains default
    /// (TSource) if source is empty; otherwise, the first element in source.</returns>
    public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.FirstOrDefault, source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns the first element of a sequence that satisfies a specified
    /// condition or a default value if no such element is found.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> to return the first element of.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains default
    /// (TSource) if source is empty or if no element passes the test specified by predicate;
    /// otherwise, the first element in source that passes the test specified by predicate.</returns>
    public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.FirstOrDefaultWithPredicate,
        source, predicate, cancellationToken);
    }

    // Last

    /// <summary>
    /// Asynchronously returns the last element of a sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> to return the last element of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// last element in source.</returns>
    public static Task<TSource> LastAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.Last, source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns the last element of a sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> to return the last element of.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// last element in source.</returns>
    public static Task<TSource> LastAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.LastWithPredicate,
        source, predicate, cancellationToken);
    }

    // LastOrDefault

    /// <summary>
    /// Asynchronously returns the last element of a sequence, or a default value if
    /// the sequence contains no elements.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> to return the last element of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains default
    /// (TSource) if source is empty; otherwise, the last element in source.</returns>
    public static Task<TSource> LastOrDefaultAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.LastOrDefault, source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns the last element of a sequence that satisfies a specified
    /// condition or a default value if no such element is found.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> to return the last element of.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains default
    /// (TSource) if source is empty or if no element passes the test specified by predicate;
    /// otherwise, the last element in source that passes the test specified by predicate.</returns>
    public static Task<TSource> LastOrDefaultAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.LastOrDefaultWithPredicate,
        source, predicate, cancellationToken);
    }

    // LongCount

    /// <summary>
    /// Asynchronously returns an System.Int64 that represents the number of elements
    /// in a sequence that satisfy a condition.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> that contains the elements to be counted.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// number of elements in the input sequence.</returns>
    public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, long>(WellKnownMembers.Queryable.LongCount, source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns an System.Int64 that represents the number of elements
    /// in a sequence that satisfy a condition.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> that contains the elements to be counted.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// number of elements in the sequence that satisfy the condition in the predicate
    /// function.</returns>
    public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteScalarAsync<TSource, long>(WellKnownMembers.Queryable.LongCountWithPredicate,
        source, predicate, cancellationToken);
    }

    // Max

    /// <summary>
    /// Asynchronously returns the maximum value of a sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> that contains the elements ot determine the maximum of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// maximum value in the sequence.</returns>
    public static Task<TSource> MaxAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.Max, source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously invokes a projection function on each element of a sequence and
    /// returns the maximum resulting value.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <typeparam name="TResult">he type of the value returned by the function represented by selector.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> that contains the elements ot determine the maximum of.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// maximum value in the sequence.</returns>
    public static Task<TResult> MaxAsync<TSource, TResult>(this IQueryable<TSource> source,
      Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, TResult>(WellKnownMembers.Queryable.MaxWithSelector,
        source, selector, cancellationToken);
    }

    // Min

    /// <summary>
    /// Asynchronously returns the minimum value of a sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> that contains the elements ot determine the minimum of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static Task<TSource> MinAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.Min, source, cancellationToken);
    }

    /// <summary>
    /// synchronously invokes a projection function on each element of a sequence and
    /// returns the minimum resulting value.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <typeparam name="TResult">The type of the value returned by the function represented by selector.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> that contains the elements ot determine the minimum of.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static Task<TResult> MinAsync<TSource, TResult>(this IQueryable<TSource> source,
      Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, TResult>(WellKnownMembers.Queryable.MinWithSelector,
        source, selector, cancellationToken);
    }

    // Single

    /// <summary>
    /// Asynchronously returns the only element of a sequence that satisfies a specified
    /// condition, and throws an exception if more than one such element exists.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> to return the single element of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// single element of the input sequence that satisfies the condition in predicate.</returns>
    public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.Single, source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns the only element of a sequence that satisfies a specified
    /// condition, and throws an exception if more than one such element exists.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> to return the single element of.</param>
    /// <param name="predicate">A function to test an element for a condition.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// single element of the input sequence that satisfies the condition in predicate.</returns>
    public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.SingleWithPredicate,
        source, predicate, cancellationToken);
    }

    // SingleOrDefault

    /// <summary>
    /// Asynchronously returns the only element of a sequence, or a default value if
    /// the sequence is empty; this method throws an exception if there is more than
    /// one element in the sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> to return the single element of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// single element of the input sequence, or default (TSource) if the sequence contains
    /// no elements.</returns>
    public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.SingleOrDefault, source,
        cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns the only element of a sequence, or a default value if
    /// the sequence is empty; this method throws an exception if there is more than
    /// one element in the sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}"/> to return the single element of.</param>
    /// <param name="predicate">A function to test an element for a condition.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// single element of the input sequence that satisfies the condition in predicate,
    /// or default (TSource) if no such element is found.</returns>
    public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(predicate, nameof(predicate));

      return ExecuteScalarAsync<TSource, TSource>(WellKnownMembers.Queryable.SingleOrDefaultWithPredicate,
        source, predicate, cancellationToken);
    }

    // Sum<int>

    /// <summary>
    /// Asynchronously computes the sum of a sequence of values.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="source">A sequence of values to calculate the sum of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// sum of the values in the sequence.</returns>
    public static Task<int> SumAsync(this IQueryable<int> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<int, int>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnownTypes.Int32], source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the sum of a sequence of values.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="source">A sequence of values to calculate the sum of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// sum of the values in the sequence.</returns>
    public static Task<int?> SumAsync(this IQueryable<int?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<int?, int?>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnownTypes.NullableInt32], source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the sum of the sequence of values that is obtained by
    /// invoking a projection function on each element of the input sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values of type <typeparamref name="TSource"/>.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// sum of the projected values.</returns>
    public static Task<int> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, int>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnownTypes.Int32],
        source, selector, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the sum of the sequence of values that is obtained by
    /// invoking a projection function on each element of the input sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values of type <typeparamref name="TSource"/>.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// sum of the projected values.</returns>
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

    /// <summary>
    /// Asynchronously computes the sum of a sequence of values.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="source">A sequence of values to calculate the sum of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// sum of the values in the sequence.</returns>
    public static Task<long> SumAsync(this IQueryable<long> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<long, long>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnownTypes.Int64], source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the sum of a sequence of values.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="source">A sequence of values to calculate the sum of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// sum of the values in the sequence.</returns>
    public static Task<long?> SumAsync(this IQueryable<long?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<long?, long?>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnownTypes.NullableInt64], source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the sum of the sequence of values that is obtained by
    /// invoking a projection function on each element of the input sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values of type <typeparamref name="TSource"/>.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// sum of the projected values.</returns>
    public static Task<long> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, long>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnownTypes.Int64],
        source, selector, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the sum of the sequence of values that is obtained by
    /// invoking a projection function on each element of the input sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values of type <typeparamref name="TSource"/>.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// sum of the projected values.</returns>
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

    /// <summary>
    /// Asynchronously computes the sum of a sequence of values.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="source">A sequence of values to calculate the sum of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// sum of the values in the sequence.</returns>
    public static Task<double> SumAsync(this IQueryable<double> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<double, double>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnownTypes.Double], source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the sum of a sequence of values.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="source">A sequence of values to calculate the sum of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// sum of the values in the sequence.</returns>
    public static Task<double?> SumAsync(this IQueryable<double?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<double?, double?>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnownTypes.NullableDouble], source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the sum of the sequence of values that is obtained by
    /// invoking a projection function on each element of the input sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values of type <typeparamref name="TSource"/>.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// sum of the projected values.</returns>
    public static Task<double> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, double>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnownTypes.Double],
        source, selector, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the sum of the sequence of values that is obtained by
    /// invoking a projection function on each element of the input sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values of type <typeparamref name="TSource"/>.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// sum of the projected values.</returns>
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

    /// <summary>
    /// Asynchronously computes the sum of a sequence of values.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="source">A sequence of values to calculate the sum of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// sum of the values in the sequence.</returns>
    public static Task<float> SumAsync(this IQueryable<float> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<float, float>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnownTypes.Single], source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the sum of a sequence of values.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="source">A sequence of values to calculate the sum of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// sum of the values in the sequence.</returns>
    public static Task<float?> SumAsync(this IQueryable<float?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<float?, float?>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnownTypes.NullableSingle], source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the sum of the sequence of values that is obtained by
    /// invoking a projection function on each element of the input sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values of type <typeparamref name="TSource"/>.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// sum of the projected values.</returns>
    public static Task<float> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, float>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnownTypes.Single],
        source, selector, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the sum of the sequence of values that is obtained by
    /// invoking a projection function on each element of the input sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values of type <typeparamref name="TSource"/>.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// sum of the projected values.</returns>
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

    /// <summary>
    /// Asynchronously computes the sum of a sequence of values.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="source">A sequence of values to calculate the sum of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// sum of the values in the sequence.</returns>
    public static Task<decimal> SumAsync(this IQueryable<decimal> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<decimal, decimal>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnownTypes.Decimal], source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the sum of a sequence of values.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="source">A sequence of values to calculate the sum of.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// sum of the values in the sequence.</returns>
    public static Task<decimal?> SumAsync(this IQueryable<decimal?> source,
      CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));

      return ExecuteScalarAsync<decimal?, decimal?>(
        WellKnownMembers.Queryable.SumMethodInfos[WellKnownTypes.NullableDecimal], source, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the sum of the sequence of values that is obtained by
    /// invoking a projection function on each element of the input sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values of type <typeparamref name="TSource"/>.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// sum of the projected values.</returns>
    public static Task<decimal> SumAsync<TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(selector, nameof(selector));

      return ExecuteScalarAsync<TSource, decimal>(
        WellKnownMembers.Queryable.SumWithSelectorMethodInfos[WellKnownTypes.Decimal],
        source, selector, cancellationToken);
    }

    /// <summary>
    /// Asynchronously computes the sum of the sequence of values that is obtained by
    /// invoking a projection function on each element of the input sequence.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values of type <typeparamref name="TSource"/>.</param>
    /// <param name="selector">A projection function to apply to each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the
    /// sum of the projected values.</returns>
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

    private static readonly MethodInfo TupleCreateMethod =
      typeof(Tuple).GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Single(mi => mi.Name == nameof(Tuple.Create) && mi.GetGenericArguments().Length == 2);

    /// <summary>
    /// Asynchronously creates a <see cref="List{TSource}"/> from an <see cref="IQueryable{TSource}"/>
    /// by enumerating it asynchronously.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{T}"/> to create a <see cref="List{TSource}"/> from.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a
    /// <see cref="List{TSource}"/> that contains values from the input sequence.</returns>
    public static async Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      var list = new List<TSource>();
      var asyncSource = source.AsAsyncEnumerable().WithCancellation(cancellationToken).ConfigureAwaitFalse();
      await foreach (var element in asyncSource) {
        list.Add(element);
      }

      return list;
    }

    /// <summary>
    /// Asynchronously creates an array from an <see cref="IQueryable{TSource}"/> System.Linq.IQueryable`1
    /// by enumerating it asynchronously.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{T}"/> to create an array from.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an
    /// array that contains values from the input sequence.</returns>
    public static async Task<TSource[]> ToArrayAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default) =>
      (await source.ToListAsync(cancellationToken).ConfigureAwaitFalse()).ToArray();

    /// <summary>
    /// Creates a <see cref="Dictionary{TKey, TSource}"/> from an <see cref="IQueryable{TSource}"/>
    /// by enumerating it asynchronously according to a specified key selector function.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TKey">>The type of the key returned by <paramref name="keySelector"/>.</typeparam>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{T}"/> to create a <see cref="Dictionary{TKey, TSource}"/> from.</param>
    /// <param name="keySelector">A function to extract a key from each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a
    /// <see cref="Dictionary{TKey, TValue}"/> that contains values of type <typeparamref name="TSource"/>
    /// selected from the input sequence.</returns>
    public static async Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TKey, TSource>(
      this IQueryable<TSource> source,
      Expression<Func<TSource, TKey>> keySelector, CancellationToken cancellationToken = default)
    {
      var tupleFactoryMethod = TupleCreateMethod.CachedMakeGenericMethod(typeof(TKey), typeof(TSource));
      var itemParam = new[] {Expression.Parameter(typeof(TSource), "item")};
      var body = Expression.Call(null, tupleFactoryMethod,
        ExpressionReplacer.ReplaceAll(keySelector.Body, keySelector.Parameters, itemParam),
        itemParam[0]);
      var query = source.Select(FastExpression.Lambda<Func<TSource, Tuple<TKey, TSource>>>(body, itemParam));
      var dictionary = new Dictionary<TKey, TSource>();
      var asyncSource = query.AsAsyncEnumerable().WithCancellation(cancellationToken).ConfigureAwaitFalse();
      await foreach (var tuple in asyncSource) {
        dictionary.Add(tuple.Item1, tuple.Item2);
      }

      return dictionary;
    }

    /// <summary>
    /// Creates a <see cref="Dictionary{TKey, TValue}"/> from an <see cref="IQueryable{TSource}"/>
    /// by enumerating it asynchronously according to a specified key selector and value selector functions.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TKey">>The type of the key returned by <paramref name="keySelector"/>.</typeparam>
    /// <typeparam name="TValue">>The type of the key returned by <paramref name="valueSelector"/>.</typeparam>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{T}"/> to create a <see cref="Dictionary{TKey, TValue}"/> from.</param>
    /// <param name="keySelector">A function to extract a key from each element.</param>
    /// <param name="valueSelector">A function to extract a value from each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a
    /// <see cref="Dictionary{TKey, TValue}"/> that contains values of type <typeparamref name="TValue"/>
    /// selected from the input sequence.</returns>
    public static async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue, TSource>(
      this IQueryable<TSource> source,
      Expression<Func<TSource, TKey>> keySelector,
      Expression<Func<TSource, TValue>> valueSelector,
      CancellationToken cancellationToken = default)
    {
      var tupleFactoryMethod = TupleCreateMethod.CachedMakeGenericMethod(typeof(TKey), typeof(TValue));
      var itemParam = new[] {Expression.Parameter(typeof(TSource), "item")};
      var body = Expression.Call(null, tupleFactoryMethod,
        ExpressionReplacer.ReplaceAll(keySelector.Body, keySelector.Parameters, itemParam),
        ExpressionReplacer.ReplaceAll(valueSelector.Body, valueSelector.Parameters, itemParam));
      var query = source.Select(FastExpression.Lambda<Func<TSource, Tuple<TKey, TValue>>>(body, itemParam));
      var dictionary = new Dictionary<TKey, TValue>();
      var asyncSource = query.AsAsyncEnumerable().WithCancellation(cancellationToken).ConfigureAwaitFalse();
      await foreach (var tuple in asyncSource) {
        dictionary.Add(tuple.Item1, tuple.Item2);
      }

      return dictionary;
    }

    /// <summary>
    /// Asynchronously creates a <see cref="HashSet{TSource}"/> from an <see cref="IQueryable{TSource}"/>
    /// by enumerating it asynchronously.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{T}"/> to create a <see cref="HashSet{TSource}"/> from.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a
    /// <see cref="HashSet{TSource}"/> that contains values of type <typeparamref name="TSource"/> from the input sequence.</returns>
    public static async Task<HashSet<TSource>> ToHashSetAsync<TSource>(this IQueryable<TSource> source,
      CancellationToken cancellationToken = default)
    {
      var hashSet = new HashSet<TSource>();
      var asyncSource = source.AsAsyncEnumerable().WithCancellation(cancellationToken).ConfigureAwaitFalse();
      await foreach (var element in asyncSource) {
        hashSet.Add(element);
      }

      return hashSet;
    }

    /// <summary>
    /// Asynchronously creates a <see cref="ILookup{TKey, TSource}"/> from an <see cref="IQueryable{T}"/>
    /// by enumerating it asynchronously according to a specified key selector function.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{T}"/> to create a <see cref="ILookup{TKey, TSource}"/> from.</param>
    /// <param name="keySelector">A function to extract a key from each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a
    /// <see cref="ILookup{TKey, TSource}"/> that contains values of type <typeparamref name="TSource"/>
    /// selected from the input sequence.</returns>
    public static async Task<ILookup<TKey, TSource>> ToLookupAsync<TKey, TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, TKey>> keySelector, CancellationToken cancellationToken = default)
    {
      var tupleFactoryMethod = TupleCreateMethod.CachedMakeGenericMethod(typeof(TKey), typeof(TSource));
      var itemParam = new[] {Expression.Parameter(typeof(TSource), "item")};
      var body = Expression.Call(null, tupleFactoryMethod,
        ExpressionReplacer.ReplaceAll(keySelector.Body, keySelector.Parameters, itemParam),
        itemParam[0]);
      var query = source.Select(FastExpression.Lambda<Func<TSource, Tuple<TKey, TSource>>>(body, itemParam));
      var queryResult = await query.ExecuteAsync(cancellationToken).ConfigureAwaitFalse();
      return queryResult.ToLookup(tuple => tuple.Item1, tuple => tuple.Item2);
    }

    /// <summary>
    /// Asynchronously creates a <see cref="ILookup{TKey, TValue}"/> from an <see cref="IQueryable{T}"/>
    /// by enumerating it asynchronously according to a specified key selector and an
    /// element selector function.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
    /// <typeparam name="TValue">The type of the value returned by <paramref name="valueSelector"/>.</typeparam>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{T}"/> to create a <see cref="ILookup{TKey, TValue}"/> from.</param>
    /// <param name="keySelector">A function to extract a key from each element.</param>
    /// <param name="valueSelector">A function to extract a value from each element.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a
    /// <see cref="ILookup{TKey, TElement}"/> that contains values of type <typeparamref name="TValue"/>
    /// selected from the input sequence.</returns>
    public static async Task<ILookup<TKey, TValue>> ToLookupAsync<TKey, TValue, TSource>(this IQueryable<TSource> source,
      Expression<Func<TSource, TKey>> keySelector,
      Expression<Func<TSource, TValue>> valueSelector,
      CancellationToken cancellationToken = default)
    {
      var tupleFactoryMethod = TupleCreateMethod.CachedMakeGenericMethod(typeof(TKey), typeof(TValue));
      var itemParam = new[] {Expression.Parameter(typeof(TSource), "item")};
      var body = Expression.Call(null, tupleFactoryMethod,
        ExpressionReplacer.ReplaceAll(keySelector.Body, keySelector.Parameters, itemParam),
        ExpressionReplacer.ReplaceAll(valueSelector.Body, valueSelector.Parameters, itemParam));
      var query = source.Select(FastExpression.Lambda<Func<TSource, Tuple<TKey, TValue>>>(body, itemParam));
      var queryResult = await query.ExecuteAsync(cancellationToken).ConfigureAwaitFalse();
      return queryResult.ToLookup(tuple => tuple.Item1, tuple => tuple.Item2);
    }

    /// <summary>
    /// Returns an <see cref="IAsyncEnumerable{TSource}"/> which can be enumerated asynchronously.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">An <see cref="IQueryable{T}"/> to enumerate.</param>
    /// <returns>The query results.</returns>
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
              ? operation.CachedMakeGenericMethod(typeof(TSource), typeof(TResult))
              : operation.CachedMakeGenericMethod(typeof(TSource));
        }

        var arguments = expression == null ? new[] {source.Expression} : new[] {source.Expression, expression};
        return provider.ExecuteScalarAsync<TResult>(Expression.Call(null, operation, arguments), cancellationToken);
      }

      throw new InvalidOperationException("QueryProvider doesn't support async operations.");
    }
  }
}