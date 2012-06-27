// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.23

using System;

namespace Xtensive.Indexing.SizeCalculators
{
  [Serializable]
  internal class StringSizeCalculator : SizeCalculatorBase<string>
  {
    private static readonly int minimalStringSize = 
      SizeCalculatorProvider.PointerFieldSize + 
      SizeCalculatorProvider.HeapObjectHeaderSize + 
      sizeof (int) * 2 + sizeof (char);

    public override int GetValueSize(string value)
    {
      if (value==null) 
        return SizeCalculatorProvider.PointerFieldSize;

      return minimalStringSize + (value.Length << 1);
    } 


    // Constructors

    public StringSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
}