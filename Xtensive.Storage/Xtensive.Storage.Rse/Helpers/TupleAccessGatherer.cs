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
  /// An expression visitor specialized for finding tuple access expressions.
  /// </summary>
  public class TupleAccessGatherer : ExpressionVisitor
  {
    private ParameterExpression tupleParameter;
    protected readonly Action<ApplyParameter, int> registerOuterColumn;
    protected List<int> mappings;

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
        if (outerParameter != null)
          registerOuterColumn(outerParameter, columnIndex);
        else
          mappings.Add(columnIndex);
        return mc;
      }
      return base.VisitMethodCall(mc);
    }

    /// <summary>
    /// Gathers used columns from specified <see cref="Expression"/>.
    /// </summary>
    /// <param name="expression">The predicate.</param>
    /// <returns>List containing all used columns (order and uniqueness are not guaranteed).</returns>
    public virtual List<int> Gather(Expression expression)
    {
      try {
        mappings = new List<int>();
        Visit(expression);
        return mappings;
      }
      finally {
        mappings = null;
      }
    }

    /// <summary>
    /// Gathers used columns from specified <see cref="Expression"/>.
    /// </summary>
    /// <param name="expression">The predicate.</param>
    /// <param name="parameter">The tuple parameter to be considered.</param>
    /// <returns>List containing all used columns (order and uniqueness are not guaranteed).</returns>
    public List<int> Gather(Expression expression, ParameterExpression parameter)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentNotNull(parameter, "parameter");
      tupleParameter = parameter;
      try {
        return Gather(expression);
      }
      finally {
        tupleParameter = null;
      }
    }

    private static void DefaultRegisterOuterColumn(ApplyParameter parameter, int columnIndex)
    {}

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
    public TupleAccessGatherer()
      : this(null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="registerOuterColumn">A <see langword="delegate"/> invoked on each outer column usage.</param>
    public TupleAccessGatherer(Action<ApplyParameter, int> registerOuterColumn)
    {
      this.registerOuterColumn = registerOuterColumn ?? DefaultRegisterOuterColumn;
    }
  }
}