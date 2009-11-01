// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.Core.Arithmetic
{
  [Serializable]
  internal class UInt16Arithmetic
    : ArithmeticBase<ushort>
  {
    private const ushort zero = 0;
    private const ushort one = 1;

    public override ushort Zero
    {
      get { return zero; }
    }

    public override ushort One
    {
      get { return one; }
    }

    public override ushort Add(ushort value1, ushort value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (ushort)(value1 + value2);
        }
      }
      else {
        checked{
          return (ushort)(value1 + value2);
        }
      }
    }

    public override ushort Negation(ushort value)
    {
      if (OverflowAllowed) {
        unchecked{
          return (ushort)(-1*value);
        }
      }
      else {
        checked{
          return (ushort)(-1*value);
        }
      }
    }

    public override ushort Subtract(ushort value1, ushort value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (ushort)(value1 - value2);
        }
      }
      else {
        checked{
          return (ushort)(value1 - value2);
        }
      }
    }

    public override ushort Multiply(ushort value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (ushort)(value*factor);
        }
      }
      else {
        checked{
          return (ushort)(value*factor);
        }
      }
    }

    public override ushort Divide(ushort value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (ushort)(value/factor);
        }
      }
      else {
        checked{
          return (ushort)(value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<ushort> CreateNew(ArithmeticRules rules)
    {
      return new UInt16Arithmetic(Provider, rules);
    }


    // Constructors

    public UInt16Arithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
}