// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.Core.Arithmetic
{
  [Serializable]
  internal class SingleArithmetic
    : ArithmeticBase<float>
  {
    private const float zero = 0f;
    private const float one = 1f;

    public override float Add(float value1, float value2)
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

    public override float Subtract(float value1, float value2)
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

    public override float Negation(float value)
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

    public override float Zero
    {
      get { return zero; }
    }

    public override float One
    {
      get { return one; }
    }

    public override float Multiply(float value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (float)(value*factor);
        }
      }
      else {
        checked{
          return (float)(value*factor);
        }
      }
    }

    public override float Divide(float value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (float)(value/factor);
        }
      }
      else {
        checked{
          return (float)(value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<float> CreateNew(ArithmeticRules rules)
    {
      return new SingleArithmetic(Provider, rules);
    }


    // Constructors

    public SingleArithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
}