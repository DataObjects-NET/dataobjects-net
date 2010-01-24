// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.11

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Resources;

namespace Xtensive.Core.ObjectMapping
{
  internal static class MappingHelper
  {
    private static readonly SetSlim<Type> primitiveTypes;

    public static readonly ReadOnlySet<Type> PrimitiveTypes;

    public static PropertyInfo ExtractProperty(LambdaExpression expression, string paramName)
    {
      PropertyInfo result;
      TryExtractProperty(expression, paramName, true, out result);
      return result;
    }

    public static bool TryExtractProperty(LambdaExpression expression, string paramName,
      out PropertyInfo propertyInfo)
    {
      return TryExtractProperty(expression, paramName, false, out propertyInfo);
    }

    public static bool IsCollectionCandidate(Type type)
    {
      return typeof (IEnumerable).IsAssignableFrom(type);
    }

    public static bool TryGetCollectionInterface(Type type, out Type interfaceType, out Type itemType)
    {
      var interfaces = type.GetInterfaces();
      for (var i = 0; i < interfaces.Length; i++) {
        interfaceType = interfaces[i];
        if (interfaceType.IsGenericType) {
          var interfaceGenericDefinition = interfaceType.GetGenericTypeDefinition();
          if (interfaceGenericDefinition==typeof (ICollection<>)) {
            itemType = type.IsArray ? type.GetElementType() : interfaceType.GetGenericArguments()[0];
            return true;
          }
        }
      }
      interfaceType = null;
      itemType = null;
      return false;
    }

    public static bool IsCollection(Type type)
    {
      Type interfaceType;
      Type itemType;
      return IsCollectionCandidate(type) && TryGetCollectionInterface(type, out interfaceType, out itemType);
    }

    public static bool IsTypePrimitive(Type type)
    {
      return type.IsEnum || primitiveTypes.Contains(type);
    }

    private static bool TryExtractProperty(LambdaExpression expression, string paramName, bool throwIfFailed,
      out PropertyInfo propertyInfo)
    {
      propertyInfo = null;
      var asMemberExpression = expression.Body as MemberExpression;
      if (asMemberExpression == null)
        if (throwIfFailed)
          throw new ArgumentException(Strings.ExSpecifiedExpressionIsNotMemberExpression, paramName);
        else
          return false;
      if (!asMemberExpression.Expression.Equals(expression.Parameters[0]))
        if (throwIfFailed)
          throw new ArgumentException(Strings.ExSpecifiedExpressionCanNotBeParsed, paramName);
        else
          return false;
      propertyInfo = asMemberExpression.Member as PropertyInfo;
      if (propertyInfo == null)
        if (throwIfFailed)
          throw new ArgumentException(Strings.ExAccessedMemberIsNotProperty, paramName);
        else
          return false;
      var parameterType = expression.Parameters[0].Type;
      if (propertyInfo.ReflectedType != parameterType)
        propertyInfo = parameterType.GetProperty(propertyInfo.Name);
      return true;
    }
    
    
    // Constructors

    static MappingHelper()
    {
      primitiveTypes = new SetSlim<Type> {
        typeof (Boolean), typeof (Int16), typeof (Int32), typeof (Int64), typeof (Byte), typeof (UInt16),
        typeof (UInt32), typeof (UInt64), typeof(Guid), typeof (Byte), typeof (Char), typeof (String),
        typeof (Decimal), typeof (Single), typeof (Double), typeof (DateTime), typeof (TimeSpan),
        typeof (DateTimeOffset)
      };
      PrimitiveTypes = new ReadOnlySet<Type>(primitiveTypes, false);
    }
  }
}