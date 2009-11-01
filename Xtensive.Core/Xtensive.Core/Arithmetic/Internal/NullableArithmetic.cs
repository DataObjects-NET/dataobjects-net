// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.06

using System;

namespace Xtensive.Core.Arithmetic
{
  [Serializable]
  internal class NullableArithmetic<T>
    : WrappingArithmetic<T?, T>
    where T : struct
  {
    public override T? Zero
    {
      get { return null; }
    }

    public override T? One
    {
      get { return BaseArithmetic.One; }
    }

    public override T? Add(T? value1, T? value2)
    {
      if (!NullIsZero && (!value1.HasValue || !value2.HasValue))
        return null;
      return BaseArithmetic.Add(value1.GetValueOrDefault(), value2.GetValueOrDefault());
    }

    public override T? Negation(T? value)
    {
      if (!NullIsZero && !value.HasValue)
        return null;
      return BaseArithmetic.Negation(value.GetValueOrDefault());
    }

    public override T? Subtract(T? value1, T? value2)
    {
      if (!NullIsZero && (!value1.HasValue || !value2.HasValue))
        return null;
      return BaseArithmetic.Subtract(value1.GetValueOrDefault(), value2.GetValueOrDefault());
    }

    public override T? Multiply(T? value, double factor)
    {
      if (!NullIsZero && !value.HasValue)
        return null;
      return BaseArithmetic.Multiply(value.GetValueOrDefault(), factor);
    }

    public override T? Divide(T? value, double factor)
    {
      if (!NullIsZero && !value.HasValue)
        return null;
      return BaseArithmetic.Divide(value.GetValueOrDefault(), factor);
    }

    /// <inheritdoc/>
    protected override IArithmetic<T?> CreateNew(ArithmeticRules rules)
    {
      return new NullableArithmetic<T>(Provider, rules);
    }


    // Constructors

    public NullableArithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
}