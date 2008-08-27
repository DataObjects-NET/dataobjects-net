// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.13

using System;

namespace Xtensive.Core.SizeCalculators
{
  [Serializable]
  internal class DecimalSizeCalculator : SizeCalculatorBase<decimal> 
  {
    public override int GetValueSize(decimal value)
    {
      return DefaultSize;
    }


    // Constructors

    public DecimalSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
}