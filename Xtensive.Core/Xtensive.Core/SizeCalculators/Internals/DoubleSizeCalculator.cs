// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.Core.SizeCalculators
{
  [Serializable]
  internal class DoubleSizeCalculator : SizeCalculatorBase<double>
  {
    public override int GetDefaultSize()
    {
      return sizeof (double);
    }

    public override int GetValueSize(double value)
    {
      return sizeof (double);
    }


    // Constructors

    public DoubleSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
}