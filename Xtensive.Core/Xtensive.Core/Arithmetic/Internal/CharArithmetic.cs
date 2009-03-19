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
  internal class CharArithmetic
    : ArithmeticBase<char>
  {
    private const char zero = '\x0000';
    private const char one = '\x0001';

    /// <inheritdoc/>
    public override char Zero
    {
      get { return zero; }
    }

    /// <inheritdoc/>
    public override char One
    {
      get { return one; }
    }

    /// <inheritdoc/>
    public override char Add(char value1, char value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (char)(value1 + value2);
        }
      }
      else {
        checked{
          return (char)(value1 + value2);
        }
      }
    }

    /// <inheritdoc/>
    public override char Negation(char value)
    {
      if (OverflowAllowed) {
        unchecked{
          return (char)(-1*value);
        }
      }
      else {
        checked{
          return (char)(-1*value);
        }
      }
    }

    /// <inheritdoc/>
    public override char Subtract(char value1, char value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (char)(value1 - value2);
        }
      }
      else {
        checked{
          return (char)(value1 - value2);
        }
      }
    }

    /// <inheritdoc/>
    public override char Multiply(char value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (char)(value*factor);
        }
      }
      else {
        checked{
          return (char)(value*factor);
        }
      }
    }

    /// <inheritdoc/>
    public override char Divide(char value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (char)(value/factor);
        }
      }
      else {
        checked{
          return (char)(value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<char> CreateNew(ArithmeticRules rules)
    {
      return new CharArithmetic(Provider, rules);
    }


    // Constructors
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public CharArithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
}