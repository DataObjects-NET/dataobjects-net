// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.26

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Xtensive.Linq.Normalization
{
  /// <summary>
  /// A conjunction ("and") of multiple operands.
  /// </summary>
  /// <typeparam name="T">The type of operand.</typeparam>
  [Serializable]
  public class Conjunction<T> : MultiOperandOperation<T>
  {
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">All operands must be <see cref="Expression"/>s 
    /// with <see cref="bool"/> type.</exception>
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
          result = Expression.And(expression, result);
        }
      }
      return result;
    }


    // Constructors

    /// <inheritdoc/>
    public Conjunction()
    {
    }

    /// <inheritdoc/>
    public Conjunction(T single)
      : base(single)
    {
    }

    /// <inheritdoc/>
    public Conjunction(IEnumerable<T> operands)
      : base(operands)
    {
    }
  }
}