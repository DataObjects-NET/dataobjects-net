using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Orm;

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
    public static int Delete<T>(this IQueryable<T> query) where T : class, IEntity
    {
      return new BulkDeleteOperation<T>(query).Execute();
    }

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
      Expression<Func<T, TResult>> update) where T: IEntity
    {
      return new Updatable<T>(query, field, update);
    }

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
      Expression<Func<T, TResult>> update) where T: IEntity
    {
      return new Updatable<T>((Updatable<T>) query, field, update);
    }

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
      return Set(query, field, a => value);
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
      TResult value) where T: IEntity
    {
      return Set(query, field, a => value);
    }

    /// <summary>
    /// Executes the UPDATE operation.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="query">The query.</param>
    /// <param name="evaluator">The expression, that specify new values. Constructor parameters are ignored.</param>
    /// <returns>Number of updated entities.</returns>
    public static int Update<T>(this IQueryable<T> query, Expression<Func<T, T>> evaluator) where T : class, IEntity
    {
      return new BulkUpdateOperation<T>(query, evaluator).Execute();
    }

    /// <summary>
    /// Executes the UPDATE operation.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="query">The query.</param>
    /// <returns>Number of updated entities.</returns>
    public static int Update<T>(this IUpdatable<T> query) where T : class, IEntity
    {
      return new BulkUpdateOperation<T>(query).Execute();
    }

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

    #region Non-public methods


    #endregion
  }
}