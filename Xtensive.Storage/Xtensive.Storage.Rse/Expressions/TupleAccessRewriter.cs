// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.17

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Linq;

namespace Xtensive.Storage.Rse.Expressions
{
  /// <summary>
  /// An expression visitor specialized for rewriting tuple access expressions.
  /// </summary>
  public class TupleAccessRewriter : ExpressionVisitor
  {
    protected readonly Func<ApplyParameter, int, int> resolveOuterColumn;
    protected readonly List<int> mappings;

    /// <inheritdoc/>
    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }

    /// <inheritdoc/>
    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.AsTupleAccess() != null) {
        var columnIndex = mc.GetTupleAccessArgument();
        var outerParameter = mc.ExtractApplyParameterFromTupleAccess();
        int newIndex = outerParameter != null
          ? resolveOuterColumn(outerParameter, columnIndex)
          : mappings.IndexOf(columnIndex);
        return Expression.Call(mc.Object, mc.Method, Expression.Constant(newIndex));
      }
      return base.VisitMethodCall(mc);
    }

    /// <summary>
    /// Replaces column usages according to a specified map.
    /// </summary>
    /// <param name="expression">The predicate.</param>
    /// <returns></returns>
    public virtual Expression Rewrite(Expression expression)
    {
      return Visit(expression);
    }
    
    private static int DefaultResolveOuterColumn(ApplyParameter parameter, int columnIndex)
    {
      throw new NotSupportedException();
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="mappings">The mappings.</param>
    public TupleAccessRewriter(List<int> mappings)
      : this(mappings, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="resolveOuterColumn">A <see langword="delegate"/> invoked when outer column usage is to be rewritten.</param>
    /// <param name="mappings">The mappings.</param>
    public TupleAccessRewriter(List<int> mappings, Func<ApplyParameter, int, int> resolveOuterColumn)
    {
      this.mappings = mappings;
      this.resolveOuterColumn = resolveOuterColumn ?? DefaultResolveOuterColumn;
    }
  }
}