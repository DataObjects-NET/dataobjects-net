// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.Core.SizeCalculators
{
  [Serializable]
  internal class SByteSizeCalculator : SizeCalculatorBase<sbyte>
  {
    public override int GetDefaultSize()
    {
      return sizeof (sbyte);
    }

    public override int GetValueSize(sbyte value)
    {
      return sizeof (sbyte);
    }


    // Constructors

    public SByteSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
}