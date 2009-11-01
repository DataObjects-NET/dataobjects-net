// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.Core.Arithmetic
{
  [Serializable]
  internal class UInt64Arithmetic
    : ArithmeticBase<ulong>
  {
    private const ulong zero = 0;
    private const ulong one = 1;

    public override ulong Zero
    {
      get { return zero; }
    }

    public override ulong One
    {
      get { return one; }
    }

    public override ulong Add(ulong value1, ulong value2)
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

    public override ulong Negation(ulong value)
    {
      throw new NotSupportedException();
    }

    public override ulong Subtract(ulong value1, ulong value2)
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

    public override ulong Multiply(ulong value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (ulong)(value*factor);
        }
      }
      else {
        checked{
          return (ulong)(value*factor);
        }
      }
    }

    public override ulong Divide(ulong value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (ulong)(value/factor);
        }
      }
      else {
        checked{
          return (ulong)(value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<ulong> CreateNew(ArithmeticRules rules)
    {
      return new UInt64Arithmetic(Provider, rules);
    }


    // Constructors

    public UInt64Arithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
}