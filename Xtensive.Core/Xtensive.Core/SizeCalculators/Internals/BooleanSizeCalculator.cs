// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.Core.SizeCalculators
{
  [Serializable]
  internal class BooleanSizeCalculator : SizeCalculatorBase<bool>
  {
    public override int GetDefaultSize()
    {
      return sizeof (bool);
    }

    public override int GetValueSize(bool value)
    {
      return sizeof (bool);
    }


    // Constructors

    public BooleanSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
}