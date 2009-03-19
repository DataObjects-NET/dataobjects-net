// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Arithmetic
{
  [Serializable]
  internal class DecimalArithmetic
    : ArithmeticBase<decimal>
  {
    private const decimal zero = 0m;
    private const decimal one = 1m;

    /// <inheritdoc/>
    public override decimal Zero
    {
      get { return zero; }
    }

    /// <inheritdoc/>
    public override decimal One
    {
      get { return one; }
    }

    /// <inheritdoc/>
    public override decimal Add(decimal value1, decimal value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return value1 + value2;
        }
      }
      else {
        checked{
          return value1 + value2;
        }
      }
    }

    /// <inheritdoc/>
    public override decimal Negation(decimal value)
    {
      if (OverflowAllowed) {
        unchecked{
          return -1*value;
        }
      }
      else {
        checked{
          return -1*value;
        }
      }
    }

    /// <inheritdoc/>
    public override decimal Subtract(decimal value1, decimal value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return value1 - value2;
        }
      }
      else {
        checked{
          return value1 - value2;
        }
      }
    }

    /// <inheritdoc/>
    public override decimal Multiply(decimal value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (decimal)((double)value*factor);
        }
      }
      else {
        checked{
          return (decimal)((double)value*factor);
        }
      }
    }

    /// <inheritdoc/>
    public override decimal Divide(decimal value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (decimal)((double)value/factor);
        }
      }
      else {
        checked{
          return (decimal)((double)value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<decimal> CreateNew(ArithmeticRules rules)
    {
      return new DecimalArithmetic(Provider, rules);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public DecimalArithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
}