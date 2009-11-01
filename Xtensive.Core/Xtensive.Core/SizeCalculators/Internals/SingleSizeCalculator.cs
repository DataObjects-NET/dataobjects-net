// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.Core.SizeCalculators
{
  [Serializable]
  internal class SingleSizeCalculator : SizeCalculatorBase<float>
  {
    public override int GetDefaultSize()
    {
      return sizeof (float);
    }

    public override int GetValueSize(float value)
    {
      return sizeof (float);
    }


    // Constructors

    public SingleSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
}