// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.04

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Helpers
{
  /// <summary>
  /// Provides exception aggregation support.
  /// </summary>
  [Serializable]
  public class ExceptionAggregator : 
    IDisposable, 
    ICountable<Exception>
  {
    private Action<Exception> exceptionHandler;
    private List<Exception> exceptions;

    /// <summary>
    /// Gets or sets the exception handler.
    /// </summary>
    public Action<Exception> ExceptionHandler
    {
      get { return exceptionHandler; }
      set { exceptionHandler = value; }
    }

    /// <summary>
    /// Gets the number of caught exceptions.
    /// </summary>
    public long Count
    {
      get { return exceptions!=null ? exceptions.Count : 0; }
    }

    #region Execute(...) methods

    /// <summary>
    /// Executes the specified action catching all the exceptions from it,
    /// adding it to internal list of caught exceptions and
    /// and passing it to <see cref="ExceptionHandler"/> handler.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public void Execute(Action action)
    {
      try {
        action();
      }
      catch (Exception e) {
        HandleException(e);
      }
    }

    /// <summary>
    /// Executes the specified action catching all the exceptions from it,
    /// adding it to internal list of caught exceptions and
    /// and passing it to <see cref="ExceptionHandler"/> handler.
    /// </summary>
    /// <typeparam name="T">The type of action argument.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <param name="argument">The action argument value.</param>
    public void Execute<T>(Action<T> action, T argument)
    {
      try {
        action(argument);
      }
      catch (Exception e) {
        HandleException(e);
      }
    }

    /// <summary>
    /// Executes the specified action catching all the exceptions from it,
    /// adding it to internal list of caught exceptions and
    /// and passing it to <see cref="ExceptionHandler"/> handler.
    /// </summary>
    /// <typeparam name="T1">The type of the 1st action argument.</typeparam>
    /// <typeparam name="T2">The type of the 2nd action argument.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <param name="argument1">The 1st action argument value.</param>
    /// <param name="argument2">The 2nd action argument value.</param>
    public void Execute<T1, T2>(Action<T1, T2> action, T1 argument1, T2 argument2)
    {
      try {
        action(argument1, argument2);
      }
      catch (Exception e) {
        HandleException(e);
      }
    }

    /// <summary>
    /// Executes the specified action catching all the exceptions from it,
    /// adding it to internal list of caught exceptions and
    /// and passing it to <see cref="ExceptionHandler"/> handler.
    /// </summary>
    /// <typeparam name="T1">The type of the 1st action argument.</typeparam>
    /// <typeparam name="T2">The type of the 2nd action argument.</typeparam>
    /// <typeparam name="T3">The type of the 3rd action argument.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <param name="argument1">The 1st action argument value.</param>
    /// <param name="argument2">The 2nd action argument value.</param>
    /// <param name="argument3">The 3rd action argument value.</param>
    public void Execute<T1, T2, T3>(Action<T1, T2, T3> action, T1 argument1, T2 argument2, T3 argument3)
    {
      try {
        action(argument1, argument2, argument3);
      }
      catch (Exception e) {
        HandleException(e);
      }
    }

    /// <summary>
    /// Executes the specified function catching all the exceptions from it,
    /// adding it to internal list of caught exceptions and
    /// and passing it to <see cref="ExceptionHandler"/> handler.
    /// </summary>
    /// <typeparam name="TResult">The type of function result.</typeparam>
    /// <param name="function">The function to execute.</param>
    /// <returns>Function execution result, if no exception was caught;
    /// otherwise, <see langword="default(TResult)"/>.</returns>
    public TResult Execute<TResult>(Func<TResult> function)
    {
      try {
        return function();
      }
      catch (Exception e) {
        HandleException(e);
      }
      return default(TResult);
    }

    /// <summary>
    /// Executes the specified function catching all the exceptions from it,
    /// adding it to internal list of caught exceptions and
    /// and passing it to <see cref="ExceptionHandler"/> handler.
    /// </summary>
    /// <typeparam name="T">The type of the function argument.</typeparam>
    /// <typeparam name="TResult">The type of function result.</typeparam>
    /// <param name="function">The function to execute.</param>
    /// <param name="argument">The function argument value.</param>
    /// <returns>Function execution result, if no exception was caught;
    /// otherwise, <see langword="default(TResult)"/>.</returns>
    public TResult Execute<T, TResult>(Func<T, TResult> function, T argument)
    {
      try {
        return function(argument);
      }
      catch (Exception e) {
        HandleException(e);
      }
      return default(TResult);
    }

    /// <summary>
    /// Executes the specified function catching all the exceptions from it,
    /// adding it to internal list of caught exceptions and
    /// and passing it to <see cref="ExceptionHandler"/> handler.
    /// </summary>
    /// <typeparam name="T1">The type of the 1st function argument.</typeparam>
    /// <typeparam name="T2">The type of the 2nd function argument.</typeparam>
    /// <typeparam name="TResult">The type of function result.</typeparam>
    /// <param name="function">The function to execute.</param>
    /// <param name="argument1">The 1st function argument value.</param>
    /// <param name="argument2">The 2nd function argument value.</param>
    /// <returns>Function execution result, if no exception was caught;
    /// otherwise, <see langword="default(TResult)"/>.</returns>
    public TResult Execute<T1, T2, TResult>(Func<T1, T2, TResult> function, T1 argument1, T2 argument2)
    {
      try {
        return function(argument1, argument2);
      }
      catch (Exception e) {
        HandleException(e);
      }
      return default(TResult);
    }

    /// <summary>
    /// Executes the specified function catching all the exceptions from it,
    /// adding it to internal list of caught exceptions and
    /// and passing it to <see cref="ExceptionHandler"/> handler.
    /// </summary>
    /// <typeparam name="T1">The type of the 1st function argument.</typeparam>
    /// <typeparam name="T2">The type of the 2nd function argument.</typeparam>
    /// <typeparam name="T3">The type of the 3rd function argument.</typeparam>
    /// <typeparam name="TResult">The type of function result.</typeparam>
    /// <param name="function">The function to execute.</param>
    /// <param name="argument1">The 1st function argument value.</param>
    /// <param name="argument2">The 2nd function argument value.</param>
    /// <param name="argument3">The 3rd function argument value.</param>
    /// <returns>Function execution result, if no exception was caught;
    /// otherwise, <see langword="default(TResult)"/>.</returns>
    public TResult Execute<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> function, T1 argument1, T2 argument2, T3 argument3)
    {
      try {
        return function(argument1, argument2, argument3);
      }
      catch (Exception e) {
        HandleException(e);
      }
      return default(TResult);
    }

    #endregion

    #region IEnumerable<...> methods

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<Exception> GetEnumerator()
    {
      if (exceptions==null)
        return EnumerableUtils.GetEmptyEnumerator<Exception>();
      else 
        return exceptions.GetEnumerator();
    }

    #endregion

    /// <summary>
    /// Invoked on any exception caught by <see cref="Execute"/> methods.
    /// </summary>
    /// <param name="exception">The caught exception.</param>
    /// <remarks>
    /// If this method throws an exception, it won't be caught.
    /// I.e. it will throw "through" any of <see cref="Execute"/> methods.
    /// </remarks>
    protected virtual void HandleException(Exception exception)
    {
      exceptionHandler(exception);
      if (exceptions==null)
        exceptions = new List<Exception>();
      exceptions.Add(exception);
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ExceptionAggregator()
      : this(null)
    {
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="exceptionHandler">The exception handler.</param>
    public ExceptionAggregator(Action<Exception> exceptionHandler)
    {
      this.exceptionHandler = exceptionHandler;
    }

    // Descructor

    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// <exception cref="AggregateException">Thrown if at least one exception was caught 
    /// by <see cref="Execute"/> methods.</exception>
    public void Dispose()
    {
      if (exceptions!=null && exceptions.Count>0)
        throw new AggregateException(exceptions);
    }
  }
}