// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.Core.Arithmetic
{
  [Serializable]
  internal class Int16Arithmetic
    : ArithmeticBase<short>
  {
    private const short zero = 0;
    private const short one = 1;

    public override short Zero
    {
      get { return zero; }
    }

    public override short One
    {
      get { return one; }
    }

    public override short Add(short value1, short value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (short)(value1 + value2);
        }
      }
      else {
        checked{
          return (short)(value1 + value2);
        }
      }
    }

    public override short Negation(short value)
    {
      if (OverflowAllowed) {
        unchecked{
          return (short)(-1*value);
        }
      }
      else {
        checked{
          return (short)(-1*value);
        }
      }
    }

    public override short Subtract(short value1, short value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (short)(value1 - value2);
        }
      }
      else {
        checked{
          return (short)(value1 - value2);
        }
      }
    }

    public override short Multiply(short value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (short)(value*factor);
        }
      }
      else {
        checked{
          return (short)(value*factor);
        }
      }
    }

    public override short Divide(short value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (short)(value/factor);
        }
      }
      else {
        checked{
          return (short)(value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<short> CreateNew(ArithmeticRules rules)
    {
      return new Int16Arithmetic(Provider, rules);
    }


    // Constructors

    public Int16Arithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
}