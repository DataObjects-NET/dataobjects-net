// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.07

using System;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Core.Linq
{
  /// <summary>
  /// Extension methods for compiling strictly typed lambda expressions.
  /// </summary>
  public static class CachingExpressionCompilerExtensions
  {
    /// <summary>Compiles the specified lambda.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<TResult> Compile<TResult>(this CachingExpressionCompiler compiler, Expression<Func<TResult>> lambda)
    {
      var result = compiler.RawCompile(lambda);
      return ((Func<object[], TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<T1, TResult> Compile<T1, TResult>(this CachingExpressionCompiler compiler, Expression<Func<T1, TResult>> lambda)
    {
      var result = compiler.RawCompile(lambda);
      return ((Func<object[], T1, TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<T1, T2, TResult> Compile<T1, T2, TResult>(this CachingExpressionCompiler compiler, Expression<Func<T1, T2, TResult>> lambda)
    {
      var result = compiler.RawCompile(lambda);
      return ((Func<object[], T1, T2, TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<T1, T2, T3, TResult> Compile<T1, T2, T3, TResult>(this CachingExpressionCompiler compiler, Expression<Func<T1, T2, T3, TResult>> lambda)
    {
      var result = compiler.RawCompile(lambda);
      return ((Func<object[], T1, T2, T3, TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<T1, T2, T3, T4, TResult> Compile<T1, T2, T3, T4, TResult>(this CachingExpressionCompiler compiler, Expression<Func<T1, T2, T3, T4, TResult>> lambda)
    {
      var result = compiler.RawCompile(lambda);
      return ((Func<object[], T1, T2, T3, T4, TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<T1, T2, T3, T4, T5, TResult> Compile<T1, T2, T3, T4, T5, TResult>(this CachingExpressionCompiler compiler, Expression<Func<T1, T2, T3, T4, T5, TResult>> lambda)
    {
      var result = compiler.RawCompile(lambda);
      return ((Func<object[], T1, T2, T3, T4, T5, TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<T1, T2, T3, T4, T5, T6, TResult> Compile<T1, T2, T3, T4, T5, T6, TResult>(this CachingExpressionCompiler compiler, Expression<Func<T1, T2, T3, T4, T5, T6, TResult>> lambda)
    {
      var result = compiler.RawCompile(lambda);
      return ((Func<object[], T1, T2, T3, T4, T5, T6, TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<T1, T2, T3, T4, T5, T6, T7, TResult> Compile<T1, T2, T3, T4, T5, T6, T7, TResult>(this CachingExpressionCompiler compiler, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TResult>> lambda)
    {
      var result = compiler.RawCompile(lambda);
      return ((Func<object[], T1, T2, T3, T4, T5, T6, T7, TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> Compile<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this CachingExpressionCompiler compiler, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>> lambda)
    {
      var result = compiler.RawCompile(lambda);
      return ((Func<object[], T1, T2, T3, T4, T5, T6, T7, T8, TResult>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action Compile(this CachingExpressionCompiler compiler, Expression<Action> lambda)
    {
      var result = compiler.RawCompile(lambda);
      return ((Action<object[]>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action<T1> Compile<T1>(this CachingExpressionCompiler compiler, Expression<Action<T1>> lambda)
    {
      var result = compiler.RawCompile(lambda);
      return ((Action<object[], T1>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action<T1, T2> Compile<T1, T2>(this CachingExpressionCompiler compiler, Expression<Action<T1, T2>> lambda)
    {
      var result = compiler.RawCompile(lambda);
      return ((Action<object[], T1, T2>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action<T1, T2, T3> Compile<T1, T2, T3>(this CachingExpressionCompiler compiler, Expression<Action<T1, T2, T3>> lambda)
    {
      var result = compiler.RawCompile(lambda);
      return ((Action<object[], T1, T2, T3>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action<T1, T2, T3, T4> Compile<T1, T2, T3, T4>(this CachingExpressionCompiler compiler, Expression<Action<T1, T2, T3, T4>> lambda)
    {
      var result = compiler.RawCompile(lambda);
      return ((Action<object[], T1, T2, T3, T4>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action<T1, T2, T3, T4, T5> Compile<T1, T2, T3, T4, T5>(this CachingExpressionCompiler compiler, Expression<Action<T1, T2, T3, T4, T5>> lambda)
    {
      var result = compiler.RawCompile(lambda);
      return ((Action<object[], T1, T2, T3, T4, T5>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action<T1, T2, T3, T4, T5, T6> Compile<T1, T2, T3, T4, T5, T6>(this CachingExpressionCompiler compiler, Expression<Action<T1, T2, T3, T4, T5, T6>> lambda)
    {
      var result = compiler.RawCompile(lambda);
      return ((Action<object[], T1, T2, T3, T4, T5, T6>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action<T1, T2, T3, T4, T5, T6, T7> Compile<T1, T2, T3, T4, T5, T6, T7>(this CachingExpressionCompiler compiler, Expression<Action<T1, T2, T3, T4, T5, T6, T7>> lambda)
    {
      var result = compiler.RawCompile(lambda);
      return ((Action<object[], T1, T2, T3, T4, T5, T6, T7>) result.First).Bind(result.Second);
    }

    /// <summary>Compiles the specified lambda.</summary>
    /// <returns>Compiled lambda.</returns>
    public static Action<T1, T2, T3, T4, T5, T6, T7, T8> Compile<T1, T2, T3, T4, T5, T6, T7, T8>(this CachingExpressionCompiler compiler, Expression<Action<T1, T2, T3, T4, T5, T6, T7, T8>> lambda)
    {
      var result = compiler.RawCompile(lambda);
      return ((Action<object[], T1, T2, T3, T4, T5, T6, T7, T8>) result.First).Bind(result.Second);
    }

  }
}