// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.Core.SizeCalculators
{
  [Serializable]
  internal class UInt32SizeCalculator : SizeCalculatorBase<uint>
  {
    public override int GetDefaultSize()
    {
      return sizeof (uint);
    }

    public override int GetValueSize(uint value)
    {
      return sizeof (uint);
    }


    // Constructors

    public UInt32SizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
}