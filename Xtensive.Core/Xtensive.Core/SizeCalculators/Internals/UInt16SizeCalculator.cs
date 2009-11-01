// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.Core.SizeCalculators
{
  [Serializable]
  internal class UInt16SizeCalculator : SizeCalculatorBase<ushort>
  {
    public override int GetDefaultSize()
    {
      return sizeof (ushort);
    }

    public override int GetValueSize(ushort value)
    {
      return sizeof (ushort);
    }


    // Constructors

    public UInt16SizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
}