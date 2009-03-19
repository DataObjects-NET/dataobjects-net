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
  internal class Int64Arithmetic
    : ArithmeticBase<long>
  {
    private const long zero = 0;
    private const long one = 1;

    /// <inheritdoc/>
    public override long Zero
    {
      get { return zero; }
    }

    /// <inheritdoc/>
    public override long One
    {
      get { return one; }
    }

    /// <inheritdoc/>
    public override long Add(long value1, long value2)
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
    public override long Negation(long value)
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
    public override long Subtract(long value1, long value2)
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
    public override long Multiply(long value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (long)(value*factor);
        }
      }
      else {
        checked{
          return (long)(value*factor);
        }
      }
    }

    /// <inheritdoc/>
    public override long Divide(long value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (long)(value/factor);
        }
      }
      else {
        checked{
          return (long)(value/factor);
        }
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public Int64Arithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }

    /// <inheritdoc/>
    protected override IArithmetic<long> CreateNew(ArithmeticRules rules)
    {
      throw new NotImplementedException();
    }
  }
}