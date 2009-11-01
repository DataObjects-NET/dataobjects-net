// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.Core.SizeCalculators
{
  [Serializable]
  internal class ObjectSizeCalculator<T> : SizeCalculatorBase<T> 
  {
    // Constructors

    public ObjectSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
}