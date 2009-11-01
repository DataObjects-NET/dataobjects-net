// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.Core.SizeCalculators
{
  [Serializable]
  internal class SizeCalculatorAwareInterfaceSizeCalculator<T> : SizeCalculatorBase<T> 
    where T : ISizeCalculatorAware
  {
    public override int GetValueSize(T value)
    {
      if (ReferenceEquals(value, null))
        return SizeCalculatorProvider.PointerFieldSize;
      else
        return value.GetSize(Provider);
    }


    // Constructors

    public SizeCalculatorAwareInterfaceSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
}