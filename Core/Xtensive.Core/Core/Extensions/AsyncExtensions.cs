// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.31

using System;

namespace Xtensive.Core
{
  /// <summary>
  /// Extension methods related to asynchronous processing.
  /// </summary>
  public static class AsyncExtensions
  {
    /// <summary>
    /// Invokes the function asynchronously.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="function">The function to invoke.</param>
    /// <returns>A function, that, being invoked, 
    /// waits and returns a value of asynchronous invocation.</returns>
    public static Func<TResult> InvokeAsync<TResult>(this Func<TResult> function)
    {
      var asyncResult = function.BeginInvoke(null, null);
      return () => { return function.EndInvoke(asyncResult); };
    }

    /// <summary>
    /// Invokes the function asynchronously.
    /// </summary>
    /// <typeparam name="T1">The type of the argument.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="function">The function to invoke.</param>
    /// <param name="arg1">The argument.</param>
    /// <returns>
    /// A function, that, being invoked,
    /// waits and returns a value of asynchronous invocation.
    /// </returns>
    public static Func<TResult> InvokeAsync<T1, TResult>(this Func<T1, TResult> function, T1 arg1)
    {
      var asyncResult = function.BeginInvoke(arg1, null, null);
      return () => { return function.EndInvoke(asyncResult); };
    }

    /// <summary>
    /// Invokes the function asynchronously.
    /// </summary>
    /// <typeparam name="T1">The type of the 1st argument.</typeparam>
    /// <typeparam name="T2">The type of the 2nd argument.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="function">The function to invoke.</param>
    /// <param name="arg1">The 1st argument.</param>
    /// <param name="arg2">The 2nd argument.</param>
    /// <returns>
    /// A function, that, being invoked,
    /// waits and returns a value of asynchronous invocation.
    /// </returns>
    public static Func<TResult> InvokeAsync<T1, T2, TResult>(this Func<T1, T2, TResult> function, T1 arg1, T2 arg2)
    {
      var asyncResult = function.BeginInvoke(arg1, arg2, null, null);
      return () => { return function.EndInvoke(asyncResult); };
    }

    /// <summary>
    /// Invokes the function asynchronously.
    /// </summary>
    /// <typeparam name="T1">The type of the 1st argument.</typeparam>
    /// <typeparam name="T2">The type of the 2nd argument.</typeparam>
    /// <typeparam name="T3">The type of the 3rd argument.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="function">The function to invoke.</param>
    /// <param name="arg1">The 1st argument.</param>
    /// <param name="arg2">The 2nd argument.</param>
    /// <param name="arg3">The 3rd argument.</param>
    /// <returns>
    /// A function, that, being invoked,
    /// waits and returns a value of asynchronous invocation.
    /// </returns>
    public static Func<TResult> InvokeAsync<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> function, T1 arg1, T2 arg2, T3 arg3)
    {
      var asyncResult = function.BeginInvoke(arg1, arg2, arg3, null, null);
      return () => { return function.EndInvoke(asyncResult); };
    }
  }
}