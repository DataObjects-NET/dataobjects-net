// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.26

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Xtensive.Core.Linq.Normalization
{
  /// <summary>
  /// A disjunctive normalized expression.
  /// </summary>
  [Serializable]
  public class DisjunctiveNormalized : Disjunction<Conjunction<Expression>>
  {
    /// <inheritdoc/>
    public override Expression ToExpression()
    {
      var operands = new Stack<Expression>();
      foreach (var conjuction in Operands) {
        operands.Push(conjuction.ToExpression());
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


    // Construcors

    /// <inheritdoc/>
    public DisjunctiveNormalized()
    {
    }

    /// <inheritdoc/>
    public DisjunctiveNormalized(IEnumerable<Conjunction<Expression>> operands, params IEnumerable<Conjunction<Expression>>[] operandSets)
      : base(operands, operandSets)
    {
    }

    /// <inheritdoc/>
    public DisjunctiveNormalized(Conjunction<Expression> operand, params Conjunction<Expression>[] operands)
      : base(operand, operands)
    {
    }
  }
}