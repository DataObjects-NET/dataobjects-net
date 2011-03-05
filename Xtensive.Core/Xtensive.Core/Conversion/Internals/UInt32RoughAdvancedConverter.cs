// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.08

using System;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class UInt32RoughAdvancedConverter :
    RoughAdvancedConverterBase,
    IAdvancedConverter<uint, bool>,
    IAdvancedConverter<uint, float>
  {
    bool IAdvancedConverter<uint, bool>.Convert(uint value)
    {
      return Convert.ToBoolean(value);
    }

    float IAdvancedConverter<uint, float>.Convert(uint value)
    {
      return Convert.ToSingle(value);
    }


    // Constructors

    public UInt32RoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}