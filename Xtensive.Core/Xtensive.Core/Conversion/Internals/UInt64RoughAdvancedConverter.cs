// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.08

using System;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class UInt64RoughAdvancedConverter :
    RoughAdvancedConverterBase,
    IAdvancedConverter<ulong, bool>,
    IAdvancedConverter<ulong, float>,
    IAdvancedConverter<ulong, double>
  {
    bool IAdvancedConverter<ulong, bool>.Convert(ulong value)
    {
      return Convert.ToBoolean(value);
    }

    float IAdvancedConverter<ulong, float>.Convert(ulong value)
    {
      return Convert.ToSingle(value);
    }

    double IAdvancedConverter<ulong, double>.Convert(ulong value)
    {
      return Convert.ToDouble(value);
    }


    // Constructors

    public UInt64RoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}