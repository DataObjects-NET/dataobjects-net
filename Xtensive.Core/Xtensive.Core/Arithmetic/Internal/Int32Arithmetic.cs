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
  internal class Int32Arithmetic
    : ArithmeticBase<int>
  {
    private const int zero = 0;
    private const int one = 1;

    /// <inheritdoc/>
    public override int Zero
    {
      get { return zero; }
    }

    /// <inheritdoc/>
    public override int One
    {
      get { return one; }
    }

    /// <inheritdoc/>
    public override int Add(int value1, int value2)
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
    public override int Negation(int value)
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
    public override int Subtract(int value1, int value2)
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
    public override int Multiply(int value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (int)(value*factor);
        }
      }
      else {
        checked{
          return (int)(value*factor);
        }
      }
    }

    /// <inheritdoc/>
    public override int Divide(int value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (int)(value/factor);
        }
      }
      else {
        checked{
          return (int)(value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<int> CreateNew(ArithmeticRules rules)
    {
      return new Int32Arithmetic(Provider, rules);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public Int32Arithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
}