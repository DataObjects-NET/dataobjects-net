// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.25

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Linq;

namespace Xtensive.Linq
{
  /// <summary>
  /// Abstract base visitor that handles methods of <see cref="IQueryable"/> and <see cref="IEnumerable{T}"/> by calling <see cref="VisitQueryableMethod"/>.
  /// </summary>
  [Serializable]
  public abstract class QueryableVisitor : ExpressionVisitor
  {
    /// <inheritdoc/>
    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.Arguments.Count > 0 && mc.Arguments[0].Type==typeof (string))
        return base.VisitMethodCall(mc);

      var method = GetQueryableMethod(mc);
      if (method==null)
        return base.VisitMethodCall(mc);

      return VisitQueryableMethod(mc, method.Value);
    }

    /// <summary>
    /// Visits method of <see cref="IQueryable"/> or <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="mc">The method call expression.</param>
    /// <param name="methodKind">Kind of the method.</param>
    protected abstract Expression VisitQueryableMethod(MethodCallExpression mc, QueryableMethodKind methodKind);

    /// <summary>
    /// Parses <see cref="QueryableMethodKind"/> for the specified expression.
    /// </summary>
    /// <param name="call">A call to process.</param>
    /// <returns><see cref="QueryableMethodKind"/> for the specified expression,
    /// or null if method is not a LINQ method.</returns>
    public static QueryableMethodKind? GetQueryableMethod(MethodCallExpression call)
    {
      if (call==null)
        return null;
      var declaringType = call.Method.DeclaringType;
      if (declaringType==typeof (Queryable) || declaringType==typeof (Enumerable))
        return ParseQueryableMethodKind(call.Method.Name);
      return null;
    }

    private static QueryableMethodKind? ParseQueryableMethodKind(string methodName)
    {
#if NET40
      QueryableMethodKind result;
      if (Enum.TryParse(methodName, out result))
        return result;
      return null;
#else
      try {
        return (QueryableMethodKind) Enum.Parse(typeof (QueryableMethodKind), methodName);
      }
      catch {
        return null;
      }
#endif
    }
  }
}