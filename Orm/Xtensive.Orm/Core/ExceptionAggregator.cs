// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.04

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Collections;
using Xtensive.Core;


namespace Xtensive.Core
{
  /// <summary>
  /// Provides exception aggregation support.
  /// </summary>
  [Serializable]
  public class ExceptionAggregator : 
    IDisposable, 
    IEnumerable<Exception>
  {
    private Action<Exception> exceptionHandler;
    private List<Exception> exceptions;
    private string exceptionMessage;

    private bool isCompleted = false;
    private bool isDisposed = false;

    /// <summary>
    /// Gets or sets the exception handler.
    /// </summary>
    public Action<Exception> ExceptionHandler
    {
      [DebuggerStepThrough]
      get { return exceptionHandler; }
      [DebuggerStepThrough]
      set { exceptionHandler = value; }
    }

    /// <summary>
    /// Gets the number of caught exceptions.
    /// </summary>
    public int Count {
      [DebuggerStepThrough]
      get { return exceptions!=null ? exceptions.Count : 0; }
    }

    /// <summary>
    /// Gets a value indicating exception aggregation was completed successfully,
    /// i.e. aggregated exceptions, if any, can be thrown on disposal.
    /// Set to <see langword="true" /> by <see cref="Complete"/> method call.
    /// </summary>
    public bool IsCompleted {
      [DebuggerStepThrough]
      get { return isCompleted; }
    }

    /// <summary>
    /// Indicates exception aggregation was completed successfully,
    /// i.e. aggregated exceptions, if any, can be thrown on disposal.
    /// Sets <see cref="IsCompleted"/> to <see langword="true" />.
    /// </summary>
    public void Complete()
    {
      isCompleted = true;
    }

    /// <summary>
    /// Adds the specified exception to the list of caught exceptions.
    /// </summary>
    /// <param name="exception">The exception to add.</param>
    /// <param name="handle">Indicates whether <see cref="ExceptionHandler"/>
    /// must be invoked for this exception or not.</param>
    public void Add(Exception exception, bool handle)
    {
      if (handle)
        HandleException(exception);
      else {
        InnerAdd(exception);
      }
    }

    private void InnerAdd(Exception exception)
    {
      if (exceptions==null)
        exceptions = new List<Exception>();
      exceptions.Add(exception);
    }

    #region Execute(...) methods

    /// <summary>
    /// Executes the specified action catching all the exceptions from it,
    /// adding it to internal list of caught exceptions and
    /// and passing it to <see cref="ExceptionHandler"/> handler.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <exception cref="ObjectDisposedException">Aggregator is already disposed.</exception>
    public void Execute(Action action)
    {
      if (isDisposed)
        throw Exceptions.AlreadyDisposed(null);
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
    /// <exception cref="ObjectDisposedException">Aggregator is already disposed.</exception>
    public void Execute<T>(Action<T> action, T argument)
    {
      if (isDisposed)
        throw Exceptions.AlreadyDisposed(null);
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
    /// <exception cref="ObjectDisposedException">Aggregator is already disposed.</exception>
    public void Execute<T1, T2>(Action<T1, T2> action, T1 argument1, T2 argument2)
    {
      if (isDisposed)
        throw Exceptions.AlreadyDisposed(null);
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
    /// <exception cref="ObjectDisposedException">Aggregator is already disposed.</exception>
    public void Execute<T1, T2, T3>(Action<T1, T2, T3> action, T1 argument1, T2 argument2, T3 argument3)
    {
      if (isDisposed)
        throw Exceptions.AlreadyDisposed(null);
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
    /// <exception cref="ObjectDisposedException">Aggregator is already disposed.</exception>
    public TResult Execute<TResult>(Func<TResult> function)
    {
      if (isDisposed)
        throw Exceptions.AlreadyDisposed(null);
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
    /// <exception cref="ObjectDisposedException">Aggregator is already disposed.</exception>
    public TResult Execute<T, TResult>(Func<T, TResult> function, T argument)
    {
      if (isDisposed)
        throw Exceptions.AlreadyDisposed(null);
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
    /// <exception cref="ObjectDisposedException">Aggregator is already disposed.</exception>
    public TResult Execute<T1, T2, TResult>(Func<T1, T2, TResult> function, T1 argument1, T2 argument2)
    {
      if (isDisposed)
        throw Exceptions.AlreadyDisposed(null);
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
    /// <exception cref="ObjectDisposedException">Aggregator is already disposed.</exception>
    public TResult Execute<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> function, T1 argument1, T2 argument2, T3 argument3)
    {
      if (isDisposed)
        throw Exceptions.AlreadyDisposed(null);
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

    /// <inheritdoc/>
    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<Exception> GetEnumerator()
    {
      if (exceptions==null)
        return EnumerableUtils<Exception>.EmptyEnumerator;
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
      if (exceptionHandler!=null)
        exceptionHandler(exception);
      InnerAdd(exception);
    }


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    public ExceptionAggregator()
      : this(null, null)
    {
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="exceptionMessage">The message of <see cref="AggregateException"/>.</param>
    public ExceptionAggregator(string exceptionMessage)
      : this (null, exceptionMessage)
    {
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="exceptionHandler">The exception handler.</param>
    /// <param name="exceptionMessage">The message of <see cref="AggregateException"/>.</param>
    public ExceptionAggregator(Action<Exception> exceptionHandler, string exceptionMessage)
    {
      this.exceptionHandler = exceptionHandler;
      this.exceptionMessage = exceptionMessage;
    }

    // Descructor

    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// <exception cref="AggregateException">Thrown if at least one exception was caught 
    /// by <see cref="Execute"/> methods.</exception>
    public void Dispose()
    {
      if (isDisposed)
        return;
      isDisposed = true;
      if (isCompleted && exceptions!=null && exceptions.Count>0) {
        var exception = string.IsNullOrEmpty(exceptionMessage) 
          ? new AggregateException(exceptions) 
          : new AggregateException(exceptionMessage, exceptions);
        throw exception;
      }
    }
  }
}