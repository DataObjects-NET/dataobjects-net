// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.13

namespace Xtensive.SizeCalculators
{
  internal class BoxSizeCalculator<T>: WrappingSizeCalculator<T, T>,
    ISizeCalculatorBase
    // Actually: where T: struct
  {
    private int defaultSize;

    public override int GetDefaultSize()
    {
      return defaultSize;
    }

    public override int GetValueSize(T value)
    {
      return BaseSizeCalculator.GetValueSize(value) +
        SizeCalculatorProvider.HeapObjectHeaderSize + 
        SizeCalculatorProvider.PointerFieldSize;
    }

    int ISizeCalculatorBase.GetInstanceSize(object instance)
    {
      if (instance==null)
        return SizeCalculatorProvider.PointerFieldSize;
      else
        return BaseSizeCalculator.GetValueSize((T)instance) +
          SizeCalculatorProvider.HeapObjectHeaderSize + 
          SizeCalculatorProvider.PointerFieldSize;
    }


    // Constructors

    public BoxSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
      defaultSize = BaseSizeCalculator.GetDefaultSize() + 
        SizeCalculatorProvider.HeapObjectHeaderSize + 
        SizeCalculatorProvider.PointerFieldSize;
    }

    public override void OnDeserialization(object sender)
    {
      base.OnDeserialization(sender);
      defaultSize = BaseSizeCalculator.GetDefaultSize() + 
        SizeCalculatorProvider.HeapObjectHeaderSize + 
        SizeCalculatorProvider.PointerFieldSize;
    }
  }
}