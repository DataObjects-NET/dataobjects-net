// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.26

using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Xtensive.Core.Linq.Normalization
{
  /// <summary>
  /// A disjunction of many operands.
  /// </summary>
  /// <typeparam name="T">The type of operands.</typeparam>
  [Serializable]
  public class Disjunction<T> : MultioperandOperation<T>
  {
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">All operands must be Expressions with type Boolean.</exception>
    public override Expression ToExpression()
    {
      var operands = new Stack<Expression>();
      foreach (var operand in Operands) {
        var expression = operand as Expression;
        if (expression==null || expression.Type!=typeof (bool))
          throw new InvalidOperationException("All operands must be Expressions with type Boolean.");
        operands.Push(expression);
      }

      if (operands.Count==0) {
        return null;
      }

      if (operands.Count==1) {
        return operands.Pop();
      }

      var result = Expression.Or(operands.Pop(), operands.Pop());

      while (operands.Count > 0) {
        result = Expression.Or(operands.Pop(), result);
      }

      return result;
    }


    // Constructors

    /// <inheritdoc/>
    public Disjunction()
    {
    }

    /// <inheritdoc/>
    public Disjunction(IEnumerable<T> operands, params IEnumerable<T>[] operandSets)
      : base(operands, operandSets)
    {
    }

    /// <inheritdoc/>
    public Disjunction(T operand, params T[] operands)
      :base(operand, operands)
    {
    }
  }
}