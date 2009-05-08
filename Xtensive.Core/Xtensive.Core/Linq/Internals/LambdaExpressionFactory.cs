// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.07

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Threading;
using Factory = System.Func<
  System.Linq.Expressions.Expression,
  System.Collections.Generic.IEnumerable<System.Linq.Expressions.ParameterExpression>,
  System.Linq.Expressions.LambdaExpression
  >;

namespace Xtensive.Core.Linq.Internals
{
  internal sealed class LambdaExpressionFactory
  {
    private static readonly object _lock = new object();
    private static volatile LambdaExpressionFactory instance;

    public static LambdaExpressionFactory Instance {
      get {
        if (instance == null)
          lock (_lock)
            if (instance == null)
              instance = new LambdaExpressionFactory();
        return instance;
      }
    }
    
    #region Nested type : CacheKey

    private struct CacheKey
      : IEquatable<CacheKey>
    {
      public Type returnType; // null => Action
      public Type[] argumentTypes;
      public int hashCode;

      public override int GetHashCode()
      {
        return hashCode;
      }

      public bool Equals(CacheKey other)
      {
        if (returnType != other.returnType)
          return false;
        if (argumentTypes.Length != other.argumentTypes.Length)
          return false;
        for (int i = 0; i < argumentTypes.Length; i++)
          if (argumentTypes[i] != other.argumentTypes[i])
            return false;
        return true;
      }

      public override bool Equals(object obj)
      {
        return Equals((CacheKey)obj);
      }
    }

    #endregion

    private readonly MethodInfo factoryMethod;
    private readonly Type[] funcTypes;
    private readonly Type[] actionTypes;
    private readonly ThreadSafeDictionary<CacheKey, Factory> cache;

    public LambdaExpression CreateLambda(Expression body, ParameterExpression[] parameters)
    {
      var key = CreateCacheKey(body.Type, parameters.Select(p => p.Type));
      var factory = cache.GetValue(key, (_key, _this) => _this.CompileCacheKey(_key), this);
      return factory.Invoke(body, parameters);
    }

    #region Private / internal methods

    private CacheKey CreateCacheKey(Type returnType, IEnumerable<Type> argumentTypes)
    {
      var types = argumentTypes.ToArray();
      int hashCode = returnType.GetHashCode();
      for (int i = 0; i < types.Length; i++)
        hashCode ^= types[i].GetHashCode();
      returnType = returnType==typeof (void) ? null : returnType;
      return new CacheKey {hashCode = hashCode, argumentTypes = types, returnType = returnType};
    }

    private Factory CompileCacheKey(CacheKey key)
    {
      var length = key.argumentTypes.Length;
      if (key.returnType == null) {
        if (length == 0)
          return (Factory) Delegate.CreateDelegate(
            typeof (Factory),
            factoryMethod.MakeGenericMethod(actionTypes[0]));
        return (Factory) Delegate.CreateDelegate(
          typeof (Factory),
          factoryMethod.MakeGenericMethod(actionTypes[length].MakeGenericType(key.argumentTypes)));
      }
      var argumentTypes = new Type[length + 1];
      Array.Copy(key.argumentTypes, argumentTypes, length);
      argumentTypes[length] = key.returnType;
      return (Factory) Delegate.CreateDelegate(
        typeof (Factory),
        factoryMethod.MakeGenericMethod(funcTypes[length].MakeGenericType(argumentTypes)));
    }
    
    #endregion

    // Constructor

    private LambdaExpressionFactory()
    {
      cache = ThreadSafeDictionary<CacheKey, Factory>.Create(new object());
      factoryMethod = typeof (Expression).GetMethods()
        .Where(m => m.IsGenericMethod
          && m.Name == "Lambda"
          && m.GetParameters()[1].ParameterType == typeof(IEnumerable<ParameterExpression>))
        .Single();
      
      funcTypes = new []
        {
          typeof (Func<>),
          typeof (Func<,>),
          typeof (Func<,,>),
          typeof (Func<,,,>),
          typeof (Func<,,,,>),
          typeof (Func<,,,,,>),
          typeof (Func<,,,,,,>),
          typeof (Func<,,,,,,,>),
          typeof (Func<,,,,,,,,>),
          typeof (Func<,,,,,,,,,>),
        };

      actionTypes = new []
        {
          typeof (Action),
          typeof (Action<,>),
          typeof (Action<,,>),
          typeof (Action<,,,>),
          typeof (Action<,,,,>),
          typeof (Action<,,,,,>),
          typeof (Action<,,,,,,>),
          typeof (Action<,,,,,,,>),
          typeof (Action<,,,,,,,,>),
        };
    }
  }
}