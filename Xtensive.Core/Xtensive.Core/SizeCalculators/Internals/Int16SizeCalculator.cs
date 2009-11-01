// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.Core.SizeCalculators
{
  [Serializable]
  internal class Int16SizeCalculator : SizeCalculatorBase<short>
  {
    public override int GetDefaultSize()
    {
      return sizeof (short);
    }

    public override int GetValueSize(short value)
    {
      return sizeof (short);
    }


    // Constructors

    public Int16SizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
}