// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.07

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Reflection;

using Factory = System.Func<
    System.Linq.Expressions.Expression,
    System.Linq.Expressions.ParameterExpression[],
    System.Linq.Expressions.LambdaExpression
  >;

using SlowFactory = System.Func<
    System.Linq.Expressions.Expression,
    System.Collections.Generic.IEnumerable<System.Linq.Expressions.ParameterExpression>,
    System.Linq.Expressions.LambdaExpression
  >;

using FastFactory = System.Func<
    System.Linq.Expressions.Expression,
    string,
    bool,
    System.Collections.Generic.IReadOnlyList<System.Linq.Expressions.ParameterExpression>,
    System.Linq.Expressions.LambdaExpression
  >;

namespace Xtensive.Linq
{
  internal sealed class LambdaExpressionFactory
  {
    private static readonly Type[] internalFactorySignature = new[] {
      typeof(Expression), typeof(string), typeof(bool), typeof(IReadOnlyList<ParameterExpression>)
    };

    private static readonly object _lock = new object();
    private static volatile LambdaExpressionFactory instance;

    public static LambdaExpressionFactory Instance {
      get {
        if (instance == null) lock (_lock) if (instance == null)
          instance = new LambdaExpressionFactory();
        return instance;
      }
    }

    private readonly ThreadSafeDictionary<Type, Factory> cache;
    private readonly Func<Type, Factory> createHandler;
    private readonly MethodInfo slowFactoryMethod;

    public LambdaExpression CreateLambda(Type delegateType, Expression body, ParameterExpression[] parameters)
    {
      var factory = cache.GetValue(delegateType, createHandler);
      return factory.Invoke(body, parameters);
    }

    public LambdaExpression CreateLambda(Expression body, ParameterExpression[] parameters)
    {
      var delegateType = DelegateHelper.MakeDelegateType(body.Type, parameters.Select(p => p.Type));
      return CreateLambda(delegateType, body, parameters);
    }

    #region Private / internal methods

    internal Factory CreateFactorySlow(Type delegateType)
    {
      var factory = (SlowFactory) Delegate.CreateDelegate(
        typeof(SlowFactory), slowFactoryMethod.MakeGenericMethod(delegateType));

      return (body, parameters) => factory.Invoke(body, parameters);
    }

    internal static Factory CreateFactoryFast(Type delegateType)
    {
      var method = typeof(Expression<>).MakeGenericType(delegateType).GetMethod(
        "Create", BindingFlags.Static | BindingFlags.NonPublic, null, internalFactorySignature, null);

      if (method == null) {
        return null;
      }

      var factory = (FastFactory) Delegate.CreateDelegate(typeof(FastFactory), null, method);
      return (body, parameters) => factory.Invoke(body, null, false, parameters);
    }

    internal static bool CanUseFastFactory()
    {
      try {
        return CreateFactoryFast(typeof(Func<int>)) != null;
      }
      catch {
        return false;
      }
    }

    #endregion

    // Constructors

    private LambdaExpressionFactory()
    {
      cache = ThreadSafeDictionary<Type, Factory>.Create(new object());

      slowFactoryMethod = typeof(Expression).GetMethods().Single(m =>
        m.IsGenericMethod &&
        m.Name == "Lambda" &&
        m.GetParameters()[1].ParameterType == typeof(IEnumerable<ParameterExpression>));

      createHandler = CanUseFastFactory() ? (Func<Type, Factory>) CreateFactoryFast : CreateFactorySlow;
    }
  }
}