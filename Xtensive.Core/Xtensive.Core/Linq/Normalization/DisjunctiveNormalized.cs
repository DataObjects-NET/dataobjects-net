// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.26

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Linq.Normalization
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

    /// <summary>
    /// Validates this instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">Some conjunction operands are not
    /// <see cref="Expression"/>s of type <see cref="bool"/>.</exception>
    public void Validate()
    {
      if (Operands.SelectMany(c => c.Operands)
        .FirstOrDefault(e => e.Type!=typeof (bool))!=null)
        throw Exceptions.InternalError(
          Resources.Strings.ExSomeOperandsAreNotExpressionsOfTypeBoolean,
          Log.Instance);
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