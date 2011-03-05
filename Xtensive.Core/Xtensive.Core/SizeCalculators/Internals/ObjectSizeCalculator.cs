// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.SizeCalculators
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