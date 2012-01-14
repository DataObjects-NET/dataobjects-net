// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.26

using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Xtensive.Linq.Normalization
{
  /// <summary>
  /// A disjunction ("or") of multiple operands.
  /// </summary>
  /// <typeparam name="T">The type of operand.</typeparam>
  [Serializable]
  public class Disjunction<T> : MultiOperandOperation<T>
  {
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">All operands must be Expressions with type Boolean.</exception>
    public override Expression ToExpression()
    {
      Expression result = null;
      foreach (var operand in Operands) {
        var expression = operand as Expression;
        if (expression==null)
          expression = ((IExpressionSource) operand).ToExpression();
        if (result==null)
          result = expression;
        else {
          result = Expression.Or(expression, result);
        }
      }
      return result;
    }


    // Constructors

    /// <inheritdoc/>
    public Disjunction()
    {
    }

    /// <inheritdoc/>
    public Disjunction(T single)
      : base(single)
    {
    }

    /// <inheritdoc/>
    public Disjunction(IEnumerable<T> operands)
      : base(operands)
    {
    }
  }
}