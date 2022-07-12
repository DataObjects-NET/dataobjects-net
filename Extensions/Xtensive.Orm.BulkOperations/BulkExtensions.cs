// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Xtensive.Orm.BulkOperations
{
  /// <summary>
  /// Extension methods for bulk operation.
  /// </summary>
  public static class BulkExtensions
  {
    /// <summary>
    /// Executes bulk delete of entities specified by the query.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="query">The query.</param>
    /// <returns>Number of the deleted entities.</returns>
    public static int Delete<T>(this IQueryable<T> query) where T : class, IEntity =>
      new BulkDeleteOperation<T>(query).Execute();

    /// <summary>
    /// Asynchronously executes bulk delete of entities specified by the query.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="query">The query.</param>
    /// <param name="token">The cancellation token to terminate execution if needed.</param>
    /// <returns>Number of the deleted entities.</returns>
    public static Task<int> DeleteAsync<T>(this IQueryable<T> query, CancellationToken token = default)
      where T : class, IEntity =>
      new BulkDeleteOperation<T>(query).ExecuteAsync(token);

    /// <summary>
    /// Executes bulk update of entities specified by the query.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <typeparam name="TResult">The type of the field.</typeparam>
    /// <param name="query">The query.</param>
    /// <param name="field">The expression, that specify the field.</param>
    /// <param name="update">The expression, that specify new value of the field.</param>
    /// <returns>Instance of <see cref=" IUpdatable&lt;T&gt;"/>.</returns>
    [Pure]
    public static IUpdatable<T> Set<T, TResult>(this IQueryable<T> query, Expression<Func<T, TResult>> field,
      Expression<Func<T, TResult>> update) where T: IEntity =>
      new Updatable<T>(query, field, update);

    /// <summary>
    /// Executes bulk update of entities specified by the query.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <typeparam name="TResult">The type of the field.</typeparam>
    /// <param name="query"><see cref="IUpdatable{T}"/>, that describes UPDATE operation.</param>
    /// <param name="field">The expression, that specify the field.</param>
    /// <param name="update">The expression, that specify new value of the field.</param>
    /// <returns>Instance of <see cref=" IUpdatable&lt;T&gt;"/>.</returns>
    [Pure]
    public static IUpdatable<T> Set<T, TResult>(this IUpdatable<T> query, Expression<Func<T, TResult>> field,
      Expression<Func<T, TResult>> update) where T: IEntity =>
      new Updatable<T>((Updatable<T>) query, field, update);

    /// <summary>
    /// Executes bulk update of entities specified by the query.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <typeparam name="TResult">The type of the field.</typeparam>
    /// <param name="query">The query.</param>
    /// <param name="field">The expression, that specify the field.</param>
    /// <param name="value">New value of the field.</param>
    /// <returns>Instance of <see cref=" IUpdatable&lt;T&gt;"/>.</returns>
    [Pure]
    public static IUpdatable<T> Set<T, TResult>(this IQueryable<T> query, Expression<Func<T, TResult>> field,
      TResult value) where T: IEntity
    {
      // Manually constructed expression is simpler than `a => value`
      var valueFunc = Expression.Lambda<Func<T, TResult>>(Expression.Constant(value, typeof(TResult)),
        Expression.Parameter(typeof(T), "a"));

      return Set(query, field, valueFunc);
    }

    /// <summary>
    /// Executes bulk update of entities specified by the query.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <typeparam name="TResult">The type of the field.</typeparam>
    /// <param name="query"><see cref="IUpdatable{T}"/>, that describes UPDATE operation.</param>
    /// <param name="field">The expression, that specify the field.</param>
    /// <param name="value">New value of the field.</param>
    /// <returns>Instance of <see cref=" IUpdatable&lt;T&gt;"/>.</returns>
    [Pure]
    public static IUpdatable<T> Set<T, TResult>(this IUpdatable<T> query, Expression<Func<T, TResult>> field,
      TResult value) where T: IEntity =>
      Set(query, field, a => value);

    /// <summary>
    /// Executes the UPDATE operation.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="query">The query.</param>
    /// <param name="evaluator">The expression, that specify new values. Constructor parameters are ignored.</param>
    /// <returns>Number of updated entities.</returns>
    public static int Update<T>(this IQueryable<T> query, Expression<Func<T, T>> evaluator) where T : class, IEntity =>
      new BulkUpdateOperation<T>(query, evaluator).Execute();

    /// <summary>
    /// Asynchronously executes the UPDATE operation.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="query">The query.</param>
    /// <param name="evaluator">The expression, that specify new values. Constructor parameters are ignored.</param>
    /// <param name="token">The cancellation token to terminate execution if necessary.</param>
    /// <returns>Number of updated entities.</returns>
    public static Task<int> UpdateAsync<T>(this IQueryable<T> query, Expression<Func<T, T>> evaluator,
      CancellationToken token = default) where T : class, IEntity =>
      new BulkUpdateOperation<T>(query, evaluator).ExecuteAsync(token);

    /// <summary>
    /// Executes the UPDATE operation.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="query">The query.</param>
    /// <returns>Number of updated entities.</returns>
    public static int Update<T>(this IUpdatable<T> query) where T : class, IEntity =>
      new BulkUpdateOperation<T>(query).Execute();

    /// <summary>
    /// Asynchronously executes the UPDATE operation.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="query">The query.</param>
    /// <param name="token">The cancellation token to terminate execution if necessary.</param>
    /// <returns>Number of updated entities.</returns>
    public static Task<int> UpdateAsync<T>(this IUpdatable<T> query, CancellationToken token = default)
      where T : class, IEntity =>
      new BulkUpdateOperation<T>(query).ExecuteAsync(token);

    /// <summary>
    /// Executes INSERT operation.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="queryEndpoint">The query endpoint.</param>
    /// <param name="evaluator">The expression, tha specify new values.</param>
    /// <returns>Key of the created entity.</returns>
    public static Key Insert<T>(this QueryEndpoint queryEndpoint, Expression<Func<T>> evaluator) where T : Entity
    {
      var operation = new InsertOperation<T>(queryEndpoint.Provider, evaluator);
      operation.Execute();
      return operation.Key;
    }

    /// <summary>
    /// Asynchronously executes INSERT operation.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="queryEndpoint">The query endpoint.</param>
    /// <param name="evaluator">The expression, tha specify new values.</param>
    /// <param name="token">The cancellation token to terminate execution if necessary.</param>
    /// <returns>Key of the created entity.</returns>
    public static async Task<Key> InsertAsync<T>(this QueryEndpoint queryEndpoint, Expression<Func<T>> evaluator,
      CancellationToken token = default) where T : Entity
    {
      var operation = new InsertOperation<T>(queryEndpoint.Provider, evaluator);
      await operation.ExecuteAsync(token).ConfigureAwait(false);
      return operation.Key;
    }
  }
}
