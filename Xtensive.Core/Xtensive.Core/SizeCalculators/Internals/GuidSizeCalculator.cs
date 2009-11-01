// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.04.23

using System;

namespace Xtensive.Core.SizeCalculators
{
  internal class GuidSizeCalculator : SizeCalculatorBase<Guid>
  {
    public override int GetValueSize(Guid value)
    {
      return DefaultSize;
    }


    // Constructors

    public GuidSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }  
  }
}