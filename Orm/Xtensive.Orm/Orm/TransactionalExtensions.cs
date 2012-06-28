// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.07.31

using System;
using System.Collections.Generic;
using System.Transactions;

using System.Linq;

namespace Xtensive.Orm
{
  /// <summary>
  /// Various extensions related to transactions.
  /// </summary>
  public static class TransactionalExtensions
  {
    #region Action, Action<T> extensions

    /// <summary>
    /// Invokes the action wrapping it into a transaction.
    /// </summary>
    /// <typeparam name="T">The type of the action argument.</typeparam>
    /// <param name="action">The action.</param>
    /// <param name="argument">The argument.</param>
    public static void InvokeTransactionally<T>(this Action<T> action, T argument)
    {
      var session = Session.Demand();
      using (var transactionScope = session.OpenTransaction()) {
        action.Invoke(argument);
        transactionScope.Complete();
      }
    }

    /// <summary>
    /// Invokes the action wrapping it into a transaction.
    /// </summary>
    /// <typeparam name="T">The type of the action argument.</typeparam>
    /// <param name="action">The action.</param>
    /// <param name="session">The session.</param>
    /// <param name="argument">The argument.</param>
    public static void InvokeTransactionally<T>(this Action<T> action, Session session, T argument)
    {
      using (var transactionScope = session.OpenTransaction()) {
        action.Invoke(argument);
        transactionScope.Complete();
      }
    }

    /// <summary>
    /// Invokes the action wrapping it into a transaction.
    /// </summary>
    /// <typeparam name="T">The type of the action argument.</typeparam>
    /// <param name="action">The action.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    public static void InvokeTransactionally<T>(this Action<T> action, IsolationLevel isolationLevel, T argument)
    {
      var session = Session.Demand();
      using (var transactionScope = session.OpenTransaction(isolationLevel)) {
        action.Invoke(argument);
        transactionScope.Complete();
      }
    }

    /// <summary>
    /// Invokes the action wrapping it into a transaction.
    /// </summary>
    /// <typeparam name="T">The type of the action argument.</typeparam>
    /// <param name="action">The action.</param>
    /// <param name="session">The session.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <param name="argument">The argument.</param>
    public static void InvokeTransactionally<T>(this Action<T> action, Session session, IsolationLevel isolationLevel, T argument)
    {
      using (var transactionScope = session.OpenTransaction(isolationLevel)) {
        action.Invoke(argument);
        transactionScope.Complete();
      }
    }

    /// <summary>
    /// Invokes the action wrapping it into a transaction.
    /// </summary>
    /// <param name="action">The action.</param>
    public static void InvokeTransactionally(this Action action)
    {
      var session = Session.Demand();
      using (var transactionScope = session.OpenTransaction()) {
        action.Invoke();
        transactionScope.Complete();
      }
    }

    /// <summary>
    /// Invokes the action wrapping it into a transaction.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="session">The session.</param>
    public static void InvokeTransactionally(this Action action, Session session)
    {
      using (var transactionScope = session.OpenTransaction()){
        action.Invoke();
        transactionScope.Complete();
      }
    }

    /// <summary>
    /// Invokes the action wrapping it into a transaction.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    public static void InvokeTransactionally(this Action action, IsolationLevel isolationLevel)
    {
      var session = Session.Demand();
      using (var transactionScope = session.OpenTransaction(isolationLevel)) {
        action.Invoke();
        transactionScope.Complete();
      }
    }

    /// <summary>
    /// Invokes the action wrapping it into a transaction.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="session">The session.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    public static void InvokeTransactionally(this Action action, Session session, IsolationLevel isolationLevel)
    {
      using (var transactionScope = session.OpenTransaction(isolationLevel)) {
        action.Invoke();
        transactionScope.Complete();
      }
    }

    #endregion

    #region Func<TResult>, Func<T, TResult> extensions

    /// <summary>
    /// Invokes the function wrapping it into a transaction.
    /// </summary>
    /// <typeparam name="T">The type of the function argument.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="function">The function.</param>
    /// <param name="argument">The argument.</param>
    public static TResult InvokeTransactionally<T, TResult>(this Func<T, TResult> function, T argument)
    {
      var session = Session.Demand();
      using (var transactionScope = session.OpenTransaction()) {
        var result = function.Invoke(argument);
        transactionScope.Complete();
        return result;
      }
    }

    /// <summary>
    /// Invokes the function wrapping it into a transaction.
    /// </summary>
    /// <typeparam name="T">The type of the function argument.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="function">The function.</param>
    /// <param name="session">The session.</param>
    /// <param name="argument">The argument.</param>
    public static TResult InvokeTransactionally<T, TResult>(this Func<T, TResult> function, Session session, T argument)
    {
      using (var transactionScope = session.OpenTransaction()) {
        var result = function.Invoke(argument);
        transactionScope.Complete();
        return result;
      }
    }

    /// <summary>
    /// Invokes the function wrapping it into a transaction.
    /// </summary>
    /// <typeparam name="T">The type of the function argument.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="function">The function.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    public static TResult InvokeTransactionally<T, TResult>(this Func<T, TResult> function, IsolationLevel isolationLevel, T argument)
    {
      var session = Session.Demand();
      using (var transactionScope = session.OpenTransaction(isolationLevel)) {
        var result = function.Invoke(argument);
        transactionScope.Complete();
        return result;
      }
    }

    /// <summary>
    /// Invokes the function wrapping it into a transaction.
    /// </summary>
    /// <typeparam name="T">The type of the function argument.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="function">The function.</param>
    /// <param name="session">The session.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <param name="argument">The argument.</param>
    public static TResult InvokeTransactionally<T, TResult>(this Func<T, TResult> function, Session session, IsolationLevel isolationLevel, T argument)
    {
      using (var transactionScope = session.OpenTransaction(isolationLevel)) {
        var result = function.Invoke(argument);
        transactionScope.Complete();
        return result;
      }
    }

    /// <summary>
    /// Invokes the function wrapping it into a transaction.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="function">The function.</param>
    public static TResult InvokeTransactionally<TResult>(this Func<TResult> function)
    {
      var session = Session.Demand();
      using (var transactionScope = session.OpenTransaction()) {
        var result = function.Invoke();
        transactionScope.Complete();
        return result;
      }
    }

    /// <summary>
    /// Invokes the function wrapping it into a transaction.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="function">The function.</param>
    /// <param name="session">The session.</param>
    public static TResult InvokeTransactionally<TResult>(this Func<TResult> function, Session session)
    {
      using (var transactionScope = session.OpenTransaction()) {
        var result = function.Invoke();
        transactionScope.Complete();
        return result;
      }
    }

    /// <summary>
    /// Invokes the function wrapping it into a transaction.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="function">The function.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    public static TResult InvokeTransactionally<TResult>(this Func<TResult> function, IsolationLevel isolationLevel)
    {
      var session = Session.Demand();
      using (var transactionScope = session.OpenTransaction(isolationLevel)) {
        var result = function.Invoke();
        transactionScope.Complete();
        return result;
      }
    }

    /// <summary>
    /// Invokes the function wrapping it into a transaction.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="function">The function.</param>
    /// <param name="session">The session.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    public static TResult InvokeTransactionally<TResult>(this Func<TResult> function, Session session, IsolationLevel isolationLevel)
    {
      using (var transactionScope = session.OpenTransaction(isolationLevel)) {
        var result = function.Invoke();
        transactionScope.Complete();
        return result;
      }
    }

    #endregion

    #region IEnumerable<T> extensions

    /// <summary>
    /// Converts the sequence to transactional.
    /// In fact, it does nothing if current transaction is available;
    /// otherwise it opens a new transaction, caches the sequence enumeration result,
    /// closes the transaction and returns cached sequence enumerator.
    /// </summary>
    /// <typeparam name="T">The type of item in sequence.</typeparam>
    /// <param name="source">The sequence to convert.</param>
    /// <returns>"Transactional" version of sequence.</returns>
    [Obsolete("Use ToTransactional<T>(IEnumerable<T>, Session) method instead.")]
    public static IEnumerable<T> ToTransactional<T>(this IEnumerable<T> source)
    {
      return source.ToTransactional(Session.Demand(), IsolationLevel.Unspecified);
    }

    /// <summary>
    /// Converts the sequence to transactional.
    /// In fact, it does nothing if current transaction is available;
    /// otherwise it opens a new transaction, caches the sequence enumeration result,
    /// closes the transaction and returns cached sequence enumerator.
    /// </summary>
    /// <typeparam name="T">The type of item in sequence.</typeparam>
    /// <param name="source">The sequence to convert.</param>
    /// <param name="session">The session.</param>
    /// <returns>"Transactional" version of sequence.</returns>
    public static IEnumerable<T> ToTransactional<T>(this IEnumerable<T> source, Session session)
    {
      return source.ToTransactional(session, IsolationLevel.Unspecified);
    }

    /// <summary>
    /// Converts the sequence to transactional.
    /// In fact, it does nothing if current transaction is available;
    /// otherwise it opens a new transaction, caches the sequence enumeration result,
    /// closes the transaction and returns cached sequence enumerator.
    /// </summary>
    /// <typeparam name="T">The type of item in sequence.</typeparam>
    /// <param name="source">The sequence to convert.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>"Transactional" version of sequence.</returns>
    [Obsolete("Use ToTransactional<T>(IEnumerable<T>, Session, IsolationLevel) method instead.")]
    public static IEnumerable<T> ToTransactional<T>(this IEnumerable<T> source, IsolationLevel isolationLevel)
    {
      return source.ToTransactional(Session.Demand(), isolationLevel);
    }

    /// <summary>
    /// Converts the sequence to transactional.
    /// In fact, it does nothing if current transaction is available;
    /// otherwise it opens a new transaction, caches the sequence enumeration result,
    /// closes the transaction and returns cached sequence enumerator.
    /// </summary>
    /// <typeparam name="T">The type of item in sequence.</typeparam>
    /// <param name="source">The sequence to convert.</param>
    /// <param name="session">The session.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>"Transactional" version of sequence.</returns>
    public static IEnumerable<T> ToTransactional<T>(this IEnumerable<T> source, Session session, IsolationLevel isolationLevel)
    {
      using (session.Activate(true))
      using (var tx = session.OpenAutoTransaction(isolationLevel)) {
        foreach (var item in source)
          yield return item;
        tx.Complete();
      }
    }

    #endregion
  }
}