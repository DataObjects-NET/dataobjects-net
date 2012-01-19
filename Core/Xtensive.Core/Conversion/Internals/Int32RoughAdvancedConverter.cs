// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.08

using System;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class Int32RoughAdvancedConverter :
    RoughAdvancedConverterBase,
    IAdvancedConverter<int, bool>,
    IAdvancedConverter<int, float>
  {
    bool IAdvancedConverter<int, bool>.Convert(int value)
    {
      return Convert.ToBoolean(value);
    }

    float IAdvancedConverter<int, float>.Convert(int value)
    {
      return Convert.ToSingle(value);
    }


    // Constructors

    public Int32RoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}