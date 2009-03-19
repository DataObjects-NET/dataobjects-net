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
  internal class ByteArithmetic
    : ArithmeticBase<byte>
  {
    private const byte zero = 0;
    private const byte one = 1;

    /// <inheritdoc/>
    public override byte Zero
    {
      get { return zero; }
    }

    /// <inheritdoc/>
    public override byte One
    {
      get { return one; }
    }

    /// <inheritdoc/>
    public override byte Add(byte value1, byte value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (byte)(value1 + value2);
        }
      }
      else {
        checked{
          return (byte)(value1 + value2);
        }
      }
    }

    /// <inheritdoc/>
    public override byte Negation(byte value)
    {
      if (OverflowAllowed) {
        unchecked{
          return (byte)(-1*value);
        }
      }
      else {
        checked{
          return (byte)(-1*value);
        }
      }
    }

    /// <inheritdoc/>
    public override byte Subtract(byte value1, byte value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (byte)(value1 - value2);
        }
      }
      else {
        checked{
          return (byte)(value1 - value2);
        }
      }
    }

    /// <inheritdoc/>
    public override byte Multiply(byte value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (byte)(value*factor);
        }
      }
      else {
        checked{
          return (byte)(value*factor);
        }
      }
    }

    /// <inheritdoc/>
    public override byte Divide(byte value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (byte)(value/factor);
        }
      }
      else {
        checked{
          return (byte)(value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<byte> CreateNew(ArithmeticRules rules)
    {
      return new ByteArithmetic(Provider, rules);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public ByteArithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
}