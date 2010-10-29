// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.23

using System;

namespace Xtensive.SizeCalculators
{
  [Serializable]
  internal class NullableSizeCalculator<T> : WrappingSizeCalculator<T?, T> 
    where T : struct
  {
    private int defaultSize;

    public override int GetDefaultSize()
    {
      return defaultSize;
    }

    public override int GetValueSize(T? value)
    {
      return sizeof (bool) + 
        (value.HasValue ? 
          BaseSizeCalculator.GetValueSize(value.GetValueOrDefault()) : 
          BaseSizeCalculator.GetDefaultSize());
    }


    // Constructors

    public NullableSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
      defaultSize = sizeof (bool) + BaseSizeCalculator.GetDefaultSize();
    }

    public override void OnDeserialization(object sender)
    {
      base.OnDeserialization(sender);
      defaultSize = sizeof (bool) + BaseSizeCalculator.GetDefaultSize();
    }
  }
}