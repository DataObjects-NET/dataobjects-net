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
      if (mc.Method.DeclaringType==typeof (Queryable) || mc.Method.DeclaringType==typeof (Enumerable)) {
        QueryableMethodKind kind;
        try {
          kind = (QueryableMethodKind) Enum.Parse(typeof (QueryableMethodKind), mc.Method.Name);
        }
        catch(ArgumentException) {
          return base.VisitMethodCall(mc);
        }
        if (mc.Arguments.Count > 0 && mc.Arguments[0].Type == typeof(string))
          return base.VisitMethodCall(mc);
        return VisitQueryableMethod(mc, kind);
      }
      return base.VisitMethodCall(mc);
    }

    /// <summary>
    /// Visits method of <see cref="IQueryable"/> or <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="mc">The method call expression.</param>
    /// <param name="methodKind">Kind of the method.</param>
    protected abstract Expression VisitQueryableMethod(MethodCallExpression mc, QueryableMethodKind methodKind);
  }
}