// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.13

using System;
using Xtensive.Core;

namespace Xtensive.SizeCalculators
{
  [Serializable]
  internal class PairSizeCalculator<TFirst, TSecond> : 
    WrappingSizeCalculator<Pair<TFirst, TSecond>, TFirst, TSecond> 
  {
    private int defaultSize;

    public override int GetDefaultSize()
    {
      return defaultSize;
    }

    public override int GetValueSize(Pair<TFirst, TSecond> value)
    {
      int result = 0;
      if (BaseSizeCalculator1!=null)
        result += BaseSizeCalculator1.GetValueSize(value.First);
      else 
        result += Provider.GetSizeCalculatorByInstance(value.First).GetInstanceSize(value.First);
      if (BaseSizeCalculator2!=null)
        result += BaseSizeCalculator2.GetValueSize(value.Second);
      else 
        result += Provider.GetSizeCalculatorByInstance(value.Second).GetInstanceSize(value.Second);
      return result;
    }


    // Constructors

    public PairSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
      defaultSize = GetPackedStructSize(
        (BaseSizeCalculator1!=null ? BaseSizeCalculator1.GetDefaultSize() : SizeCalculatorProvider.PointerFieldSize) +
        (BaseSizeCalculator2!=null ? BaseSizeCalculator2.GetDefaultSize() : SizeCalculatorProvider.PointerFieldSize));
    }

    public override void OnDeserialization(object sender)
    {
      base.OnDeserialization(sender);
      defaultSize = GetPackedStructSize(
        (BaseSizeCalculator1!=null ? BaseSizeCalculator1.GetDefaultSize() : SizeCalculatorProvider.PointerFieldSize) +
        (BaseSizeCalculator2!=null ? BaseSizeCalculator2.GetDefaultSize() : SizeCalculatorProvider.PointerFieldSize));
    }
  }
}