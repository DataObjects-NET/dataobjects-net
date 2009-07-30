// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.17

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Linq;

namespace Xtensive.Storage.Rse.Helpers
{
  /// <summary>
  /// An expression visitor specialized for rewriting tuple access expressions.
  /// </summary>
  public class TupleAccessRewriter : ExpressionVisitor
  {
    private ParameterExpression tupleParameter;
    protected readonly Func<ApplyParameter, int, int> resolveOuterColumn;
    protected readonly IList<int> mappings;

    public IList<int> Mappings
    {
      get { return mappings; }
    }

    /// <inheritdoc/>
    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }

    /// <inheritdoc/>
    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (IsTupleAccess(mc)) {
        var columnIndex = mc.GetTupleAccessArgument();
        var outerParameter = mc.GetApplyParameter();
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

    /// <summary>
    /// Replaces column usages according to a specified map.
    /// </summary>
    /// <param name="expression">The predicate.</param>
    /// <param name="parameter">The tuple parameter to be considered.</param>
    /// <returns></returns>
    public virtual Expression Rewrite(Expression expression, ParameterExpression parameter)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentNotNull(parameter, "parameter");
      tupleParameter = parameter;
      try {
        return Visit(expression);
      }
      finally {
        tupleParameter = null;
      }
    }
    
    private static int DefaultResolveOuterColumn(ApplyParameter parameter, int columnIndex)
    {
      throw new NotSupportedException();
    }

    private bool IsTupleAccess(Expression mc)
    {
      if (tupleParameter == null)
        return mc.AsTupleAccess() != null;
      return mc.AsTupleAccess(tupleParameter)!=null;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="mappings">The mappings.</param>
    public TupleAccessRewriter(IList<int> mappings)
      : this(mappings, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="resolveOuterColumn">A <see langword="delegate"/> invoked when outer column usage is to be rewritten.</param>
    /// <param name="mappings">The mappings.</param>
    public TupleAccessRewriter(IList<int> mappings, Func<ApplyParameter, int, int> resolveOuterColumn)
    {
      this.mappings = mappings;
      this.resolveOuterColumn = resolveOuterColumn ?? DefaultResolveOuterColumn;
    }
  }
}