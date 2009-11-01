// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.Core.SizeCalculators
{
  [Serializable]
  internal class ByteSizeCalculator : SizeCalculatorBase<byte>
  {
    public override int GetDefaultSize()
    {
      return sizeof (byte);
    }

    public override int GetValueSize(byte value)
    {
      return sizeof (byte);
    }


    // Constructors

    public ByteSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
}