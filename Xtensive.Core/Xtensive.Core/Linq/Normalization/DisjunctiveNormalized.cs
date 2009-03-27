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
  /// A disjunctive normalized expression 
  /// ("([not] Arg1 [and [not] Arg2 [and...]]) [or (...)]").
  /// </summary>
  [Serializable]
  public class DisjunctiveNormalized : Disjunction<Conjunction<Expression>>
  {
    /// <summary>
    /// Gets the total conjunction operand count.
    /// </summary>
    public int ConjunctionOperandCount {
      get { return Operands.Sum(c => c.Operands.Count); }
    }


    // Constructors

    /// <inheritdoc/>
    public DisjunctiveNormalized()
    {
    }

    /// <inheritdoc/>
    public DisjunctiveNormalized(Conjunction<Expression> single)
      : base(single)
    {
    }

    /// <inheritdoc/>
    public DisjunctiveNormalized(IEnumerable<Conjunction<Expression>> operands)
      : base(operands)
    {
    }
  }
}