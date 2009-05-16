// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.07

using System;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Linq.Internals;

namespace Xtensive.Core.Linq
{
  /// <summary>
  /// Extension methods for compiling strictly typed lambda expressions.
  /// </summary>
  public static class ExpressionCompileExtensions
  {
    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<TResult> CompileCached<TResult>(this Expression<Func<TResult>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Func<object[], TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<T1, TResult> CompileCached<T1, TResult>(this Expression<Func<T1, TResult>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Func<object[], T1, TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<T1, T2, TResult> CompileCached<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Func<object[], T1, T2, TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<T1, T2, T3, TResult> CompileCached<T1, T2, T3, TResult>(this Expression<Func<T1, T2, T3, TResult>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Func<object[], T1, T2, T3, TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<T1, T2, T3, T4, TResult> CompileCached<T1, T2, T3, T4, TResult>(this Expression<Func<T1, T2, T3, T4, TResult>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Func<object[], T1, T2, T3, T4, TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<T1, T2, T3, T4, T5, TResult> CompileCached<T1, T2, T3, T4, T5, TResult>(this Expression<Func<T1, T2, T3, T4, T5, TResult>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Func<object[], T1, T2, T3, T4, T5, TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<T1, T2, T3, T4, T5, T6, TResult> CompileCached<T1, T2, T3, T4, T5, T6, TResult>(this Expression<Func<T1, T2, T3, T4, T5, T6, TResult>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Func<object[], T1, T2, T3, T4, T5, T6, TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<T1, T2, T3, T4, T5, T6, T7, TResult> CompileCached<T1, T2, T3, T4, T5, T6, T7, TResult>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, TResult>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Func<object[], T1, T2, T3, T4, T5, T6, T7, TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> CompileCached<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Func<object[], T1, T2, T3, T4, T5, T6, T7, T8, TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action CompileCached(this Expression<Action> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Action<object[]>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action<T1> CompileCached<T1>(this Expression<Action<T1>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Action<object[], T1>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action<T1, T2> CompileCached<T1, T2>(this Expression<Action<T1, T2>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Action<object[], T1, T2>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action<T1, T2, T3> CompileCached<T1, T2, T3>(this Expression<Action<T1, T2, T3>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Action<object[], T1, T2, T3>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action<T1, T2, T3, T4> CompileCached<T1, T2, T3, T4>(this Expression<Action<T1, T2, T3, T4>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Action<object[], T1, T2, T3, T4>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action<T1, T2, T3, T4, T5> CompileCached<T1, T2, T3, T4, T5>(this Expression<Action<T1, T2, T3, T4, T5>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Action<object[], T1, T2, T3, T4, T5>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action<T1, T2, T3, T4, T5, T6> CompileCached<T1, T2, T3, T4, T5, T6>(this Expression<Action<T1, T2, T3, T4, T5, T6>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Action<object[], T1, T2, T3, T4, T5, T6>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action<T1, T2, T3, T4, T5, T6, T7> CompileCached<T1, T2, T3, T4, T5, T6, T7>(this Expression<Action<T1, T2, T3, T4, T5, T6, T7>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Action<object[], T1, T2, T3, T4, T5, T6, T7>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action<T1, T2, T3, T4, T5, T6, T7, T8> CompileCached<T1, T2, T3, T4, T5, T6, T7, T8>(this Expression<Action<T1, T2, T3, T4, T5, T6, T7, T8>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Action<object[], T1, T2, T3, T4, T5, T6, T7, T8>) result.First).Bind(result.Second);
    }

  }
}