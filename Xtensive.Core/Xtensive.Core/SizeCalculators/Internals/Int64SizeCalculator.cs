// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.Core.SizeCalculators
{
  [Serializable]
  internal class Int64SizeCalculator : SizeCalculatorBase<long>
  {
    public override int GetDefaultSize()
    {
      return sizeof (long);
    }

    public override int GetValueSize(long value)
    {
      return sizeof (long);
    }


    // Constructors

    public Int64SizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
}