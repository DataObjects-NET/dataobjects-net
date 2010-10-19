// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.22

using System;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class BooleanAdvancedConverter :
    StrictAdvancedConverterBase<bool>,
    IAdvancedConverter<bool, byte>,
    IAdvancedConverter<bool, sbyte>,
    IAdvancedConverter<bool, short>,
    IAdvancedConverter<bool, ushort>,
    IAdvancedConverter<bool, int>,
    IAdvancedConverter<bool, uint>,
    IAdvancedConverter<bool, long>,
    IAdvancedConverter<bool, ulong>,
    IAdvancedConverter<bool, float>,
    IAdvancedConverter<bool, double>,
    IAdvancedConverter<bool, decimal>,
    IAdvancedConverter<bool, string>
  {
    byte IAdvancedConverter<bool, byte>.Convert(bool value)
    {
      return System.Convert.ToByte(value);
    }

    sbyte IAdvancedConverter<bool, sbyte>.Convert(bool value)
    {
      return System.Convert.ToSByte(value);
    }

    short IAdvancedConverter<bool, short>.Convert(bool value)
    {
      return System.Convert.ToInt16(value);
    }

    ushort IAdvancedConverter<bool, ushort>.Convert(bool value)
    {
      return System.Convert.ToUInt16(value);
    }

    int IAdvancedConverter<bool, int>.Convert(bool value)
    {
      return System.Convert.ToInt32(value);
    }

    uint IAdvancedConverter<bool, uint>.Convert(bool value)
    {
      return System.Convert.ToUInt32(value);
    }

    long IAdvancedConverter<bool, long>.Convert(bool value)
    {
      return System.Convert.ToInt64(value);
    }

    ulong IAdvancedConverter<bool, ulong>.Convert(bool value)
    {
      return System.Convert.ToUInt64(value);
    }

    float IAdvancedConverter<bool, float>.Convert(bool value)
    {
      return System.Convert.ToSingle(value);
    }

    double IAdvancedConverter<bool, double>.Convert(bool value)
    {
      return System.Convert.ToDouble(value);
    }

    decimal IAdvancedConverter<bool, decimal>.Convert(bool value)
    {
      return System.Convert.ToDecimal(value);
    }

    string IAdvancedConverter<bool, string>.Convert(bool value)
    {
      return value.ToString(); // Culture is not used for Boolean string
    }


    // Constructors

    public BooleanAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}