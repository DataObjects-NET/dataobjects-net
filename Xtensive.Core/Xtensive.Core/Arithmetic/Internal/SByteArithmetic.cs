// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.Core.Arithmetic
{
  [Serializable]
  internal class SByteArithmetic
    : ArithmeticBase<sbyte>
  {
    private const sbyte zero = 0;
    private const sbyte one = 1;

    public override sbyte Zero
    {
      get { return zero; }
    }

    public override sbyte One
    {
      get { return one; }
    }

    public override sbyte Add(sbyte value1, sbyte value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (sbyte)(value1 + value2);
        }
      }
      else {
        checked{
          return (sbyte)(value1 + value2);
        }
      }
    }

    public override sbyte Negation(sbyte value)
    {
      if (OverflowAllowed) {
        unchecked{
          return (sbyte)(-1*value);
        }
      }
      else {
        checked{
          return (sbyte)(-1*value);
        }
      }
    }

    public override sbyte Subtract(sbyte value1, sbyte value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (sbyte)(value1 - value2);
        }
      }
      else {
        checked{
          return (sbyte)(value1 - value2);
        }
      }
    }

    public override sbyte Multiply(sbyte value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (sbyte)(value*factor);
        }
      }
      else {
        checked{
          return (sbyte)(value*factor);
        }
      }
    }

    public override sbyte Divide(sbyte value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (sbyte)(value/factor);
        }
      }
      else {
        checked{
          return (sbyte)(value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<sbyte> CreateNew(ArithmeticRules rules)
    {
      return new SByteArithmetic(Provider, rules);
    }


    // Constructors

    public SByteArithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
}