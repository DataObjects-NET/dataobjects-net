// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.Core.SizeCalculators
{
  [Serializable]
  internal class Int32SizeCalculator : SizeCalculatorBase<int>
  {
    public override int GetDefaultSize()
    {
      return sizeof (int);
    }

    public override int GetValueSize(int value)
    {
      return sizeof (int);
    }


    // Constructors

    public Int32SizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
}