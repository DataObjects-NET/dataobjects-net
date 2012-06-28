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
  System.Collections.Generic.IEnumerable<System.Linq.Expressions.ParameterExpression>,
  System.Linq.Expressions.LambdaExpression
  >;

namespace Xtensive.Linq
{
  internal sealed class LambdaExpressionFactory
  {
    private static readonly object _lock = new object();
    private static volatile LambdaExpressionFactory instance;

    public static LambdaExpressionFactory Instance {
      get {
        if (instance == null) lock (_lock) if (instance == null)
          instance = new LambdaExpressionFactory();
        return instance;
      }
    }
    
    private readonly MethodInfo factoryMethod;
    private readonly ThreadSafeDictionary<Type, Factory> cache;

    public LambdaExpression CreateLambda(Type delegateType, Expression body, ParameterExpression[] parameters)
    {
      var factory = cache.GetValue(delegateType, (_delegateType, _this) => _this.CreateFactory(_delegateType), this);
      return factory.Invoke(body, parameters);
    }
  
    public LambdaExpression CreateLambda(Expression body, ParameterExpression[] parameters)
    {
      var delegateType = DelegateHelper.MakeDelegateType(body.Type, parameters.Select(p => p.Type));
      return CreateLambda(delegateType, body, parameters);
    }

    #region Private / internal methods
    
    private Factory CreateFactory(Type delegateType)
    {
      return (Factory) Delegate.CreateDelegate(typeof (Factory), factoryMethod.MakeGenericMethod(delegateType));
    }

    #endregion

    // Constructors

    private LambdaExpressionFactory()
    {
      cache = ThreadSafeDictionary<Type, Factory>.Create(new object());
      factoryMethod = typeof (Expression).GetMethods()
        .Where(m => m.IsGenericMethod
          && m.Name == "Lambda"
            && m.GetParameters()[1].ParameterType == typeof(IEnumerable<ParameterExpression>))
        .Single();
    }
  }
}