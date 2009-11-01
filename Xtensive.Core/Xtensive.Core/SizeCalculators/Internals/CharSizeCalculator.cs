// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.Core.SizeCalculators
{
  [Serializable]
  internal class CharSizeCalculator : SizeCalculatorBase<char>
  {
    public override int GetDefaultSize()
    {
      return sizeof (char);
    }

    public override int GetValueSize(char value)
    {
      return sizeof (char);
    }


    // Constructors

    public CharSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
}