// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.Core.SizeCalculators
{
  [Serializable]
  internal class UInt64SizeCalculator : SizeCalculatorBase<ulong>
  {
    public override int GetDefaultSize()
    {
      return sizeof (ulong);
    }

    public override int GetValueSize(ulong value)
    {
      return sizeof (ulong);
    }


    // Constructors

    public UInt64SizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
}