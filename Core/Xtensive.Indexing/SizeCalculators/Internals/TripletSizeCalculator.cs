// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.13

using System;
using Xtensive.Core;
using Xtensive.Reflection;

namespace Xtensive.Indexing.SizeCalculators
{
  [Serializable]
  internal class TripletSizeCalculator<TFirst, TSecond, TThird> : 
    WrappingSizeCalculator<Triplet<TFirst, TSecond, TThird>, TFirst, TSecond, TThird> 
  {
    private int defaultSize;

    public override int GetDefaultSize()
    {
      return defaultSize;
    }

    public override int GetValueSize(Triplet<TFirst, TSecond, TThird> value)
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
      if (BaseSizeCalculator3!=null)
        result += BaseSizeCalculator3.GetValueSize(value.Third);
      else 
        result += Provider.GetSizeCalculatorByInstance(value.Third).GetInstanceSize(value.Third);
      return result;
    }


    // Constructors

    public TripletSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
      defaultSize = GetPackedStructSize(
        (BaseSizeCalculator1!=null ? BaseSizeCalculator1.GetDefaultSize() : SizeCalculatorProvider.PointerFieldSize) +
        (BaseSizeCalculator2!=null ? BaseSizeCalculator2.GetDefaultSize() : SizeCalculatorProvider.PointerFieldSize) +
        (BaseSizeCalculator3!=null ? BaseSizeCalculator3.GetDefaultSize() : SizeCalculatorProvider.PointerFieldSize));
    }

    public override void OnDeserialization(object sender)
    {
      base.OnDeserialization(sender);
      defaultSize = GetPackedStructSize(
        (BaseSizeCalculator1!=null ? BaseSizeCalculator1.GetDefaultSize() : SizeCalculatorProvider.PointerFieldSize) +
        (BaseSizeCalculator2!=null ? BaseSizeCalculator2.GetDefaultSize() : SizeCalculatorProvider.PointerFieldSize) +
        (BaseSizeCalculator3!=null ? BaseSizeCalculator3.GetDefaultSize() : SizeCalculatorProvider.PointerFieldSize));
    }
  }
}