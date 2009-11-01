// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.Core.Arithmetic
{
  [Serializable]
  internal class UInt32Arithmetic
    : ArithmeticBase<uint>
  {
    private const uint zero = 0;
    private const uint one = 1;

    public override uint Zero
    {
      get { return zero; }
    }

    public override uint One
    {
      get { return one; }
    }

    public override uint Add(uint value1, uint value2)
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

    public override uint Negation(uint value)
    {
      if (OverflowAllowed) {
        unchecked{
          return (uint)(-1*value);
        }
      }
      else {
        checked{
          return (uint)(-1*value);
        }
      }
    }

    public override uint Subtract(uint value1, uint value2)
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

    public override uint Multiply(uint value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (uint)(value*factor);
        }
      }
      else {
        checked{
          return (uint)(value*factor);
        }
      }
    }

    public override uint Divide(uint value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (uint)(value/factor);
        }
      }
      else {
        checked{
          return (uint)(value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<uint> CreateNew(ArithmeticRules rules)
    {
      return new UInt32Arithmetic(Provider, rules);
    }


    // Constructors

    public UInt32Arithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
}