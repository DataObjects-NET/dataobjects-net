// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.07.31

using System;
using System.Collections.Generic;
using System.Transactions;

namespace Xtensive.Storage
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
      using (Transaction.Open())
        action.Invoke(argument);
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
      using (Transaction.Open(session))
        action.Invoke(argument);
    }

    /// <summary>
    /// Invokes the action wrapping it into a transaction.
    /// </summary>
    /// <typeparam name="T">The type of the action argument.</typeparam>
    /// <param name="action">The action.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    public static void InvokeTransactionally<T>(this Action<T> action, IsolationLevel isolationLevel, T argument)
    {
      using (Transaction.Open(isolationLevel))
        action.Invoke(argument);
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
      using (Transaction.Open(session, isolationLevel))
        action.Invoke(argument);
    }

    /// <summary>
    /// Invokes the action wrapping it into a transaction.
    /// </summary>
    /// <param name="action">The action.</param>
    public static void InvokeTransactionally(this Action action)
    {
      using (Transaction.Open())
        action.Invoke();
    }

    /// <summary>
    /// Invokes the action wrapping it into a transaction.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="session">The session.</param>
    public static void InvokeTransactionally(this Action action, Session session)
    {
      using (Transaction.Open(session))
        action.Invoke();
    }

    /// <summary>
    /// Invokes the action wrapping it into a transaction.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    public static void InvokeTransactionally(this Action action, IsolationLevel isolationLevel)
    {
      using (Transaction.Open(isolationLevel))
        action.Invoke();
    }

    /// <summary>
    /// Invokes the action wrapping it into a transaction.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="session">The session.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    public static void InvokeTransactionally(this Action action, Session session, IsolationLevel isolationLevel)
    {
      using (Transaction.Open(session, isolationLevel))
        action.Invoke();
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
      using (Transaction.Open())
        return function.Invoke(argument);
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
      using (Transaction.Open(session))
        return function.Invoke(argument);
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
      using (Transaction.Open(isolationLevel))
        return function.Invoke(argument);
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
      using (Transaction.Open(session, isolationLevel))
        return function.Invoke(argument);
    }

    /// <summary>
    /// Invokes the function wrapping it into a transaction.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="function">The function.</param>
    public static TResult InvokeTransactionally<TResult>(this Func<TResult> function)
    {
      using (Transaction.Open())
        return function.Invoke();
    }

    /// <summary>
    /// Invokes the function wrapping it into a transaction.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="function">The function.</param>
    /// <param name="session">The session.</param>
    public static TResult InvokeTransactionally<TResult>(this Func<TResult> function, Session session)
    {
      using (Transaction.Open(session))
        return function.Invoke();
    }

    /// <summary>
    /// Invokes the function wrapping it into a transaction.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="function">The function.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    public static TResult InvokeTransactionally<TResult>(this Func<TResult> function, IsolationLevel isolationLevel)
    {
      using (Transaction.Open(isolationLevel))
        return function.Invoke();
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
      using (Transaction.Open(session, isolationLevel))
        return function.Invoke();
    }

    #endregion

    #region IEnumerable<T> extensions

    /// <summary>
    /// Wraps the sequence into a transaction.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="sequence">The sequence to wrap.</param>
    /// <returns>
    /// An enumerable wrapping the original sequence into a transaction.
    /// </returns>
    public static IEnumerable<TItem> ToTransactional<TItem>(this IEnumerable<TItem> sequence)
    {
      using (Transaction.Open())
        foreach (var item in sequence)
          yield return item;
    }

    /// <summary>
    /// Wraps the sequence into a transaction.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="sequence">The sequence to wrap.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// An enumerable wrapping the original sequence into a transaction.
    /// </returns>
    public static IEnumerable<TItem> ToTransactional<TItem>(this IEnumerable<TItem> sequence, IsolationLevel isolationLevel)
    {
      using (Transaction.Open(isolationLevel))
        foreach (var item in sequence)
          yield return item;
    }

    /// <summary>
    /// Wraps the sequence into a transaction.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="sequence">The sequence to wrap.</param>
    /// <param name="session">The session.</param>
    /// <returns>
    /// An enumerable wrapping the original sequence into a transaction.
    /// </returns>
    public static IEnumerable<TItem> ToTransactional<TItem>(this IEnumerable<TItem> sequence, Session session)
    {
      using (Transaction.Open(session))
        foreach (var item in sequence)
          yield return item;
    }

    /// <summary>
    /// Wraps the sequence into a transaction.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="sequence">The sequence to wrap.</param>
    /// <param name="session">The session.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// An enumerable wrapping the original sequence into a transaction.
    /// </returns>
    public static IEnumerable<TItem> ToTransactional<TItem>(this IEnumerable<TItem> sequence, Session session, IsolationLevel isolationLevel)
    {
      using (Transaction.Open(session, isolationLevel))
        foreach (var item in sequence)
          yield return item;
    }

    #endregion
  }
}