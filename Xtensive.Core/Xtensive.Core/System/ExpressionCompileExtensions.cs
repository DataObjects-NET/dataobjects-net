// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.07

using System.Linq.Expressions;
using Xtensive.Core.Linq.Internals;

namespace System
{
  /// <summary>
  /// Extension methods for compiling strictly typed lambda expressions.
  /// </summary>
  public static class ExpressionCompileExtensions
  {
    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<TResult> CachingCompile<TResult>(this Expression<Func<TResult>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Func<object[], TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<T1, TResult> CachingCompile<T1, TResult>(this Expression<Func<T1, TResult>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Func<object[], T1, TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<T1, T2, TResult> CachingCompile<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Func<object[], T1, T2, TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<T1, T2, T3, TResult> CachingCompile<T1, T2, T3, TResult>(this Expression<Func<T1, T2, T3, TResult>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Func<object[], T1, T2, T3, TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action CachingCompile(this Expression<Action> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Action<object[]>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action<T1> CachingCompile<T1>(this Expression<Action<T1>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Action<object[], T1>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action<T1, T2> CachingCompile<T1, T2>(this Expression<Action<T1, T2>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Action<object[], T1, T2>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda and caches the result of compilation.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action<T1, T2, T3> CachingCompile<T1, T2, T3>(this Expression<Action<T1, T2, T3>> lambda)
    {
      var result = CachingExpressionCompiler.Instance.Compile(lambda);
      return ((Action<object[], T1, T2, T3>) result.First).Bind(result.Second);
    }

  }
}