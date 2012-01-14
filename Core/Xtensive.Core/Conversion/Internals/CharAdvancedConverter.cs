// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.23

using System;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class CharAdvancedConverter :
    StrictAdvancedConverterBase<char>,
    IAdvancedConverter<char, byte>,
    IAdvancedConverter<char, sbyte>,
    IAdvancedConverter<char, short>,
    IAdvancedConverter<char, ushort>,
    IAdvancedConverter<char, int>,
    IAdvancedConverter<char, uint>,
    IAdvancedConverter<char, long>,
    IAdvancedConverter<char, ulong>,
    IAdvancedConverter<char, float>,
    IAdvancedConverter<char, double>,
    IAdvancedConverter<char, decimal>
  {
    byte IAdvancedConverter<char, byte>.Convert(char value)
    {
      return System.Convert.ToByte(value);
    }

    sbyte IAdvancedConverter<char, sbyte>.Convert(char value)
    {
      return System.Convert.ToSByte(value);
    }

    short IAdvancedConverter<char, short>.Convert(char value)
    {
      return System.Convert.ToInt16(value);
    }

    ushort IAdvancedConverter<char, ushort>.Convert(char value)
    {
      return System.Convert.ToUInt16(value);
    }

    int IAdvancedConverter<char, int>.Convert(char value)
    {
      return System.Convert.ToInt32(value);
    }

    uint IAdvancedConverter<char, uint>.Convert(char value)
    {
      return System.Convert.ToUInt32(value);
    }

    long IAdvancedConverter<char, long>.Convert(char value)
    {
      return System.Convert.ToInt64(value);
    }

    ulong IAdvancedConverter<char, ulong>.Convert(char value)
    {
      return System.Convert.ToUInt64(value);
    }

    float IAdvancedConverter<char, float>.Convert(char value)
    {
      return System.Convert.ToSingle(System.Convert.ToUInt16(value));
    }

    double IAdvancedConverter<char, double>.Convert(char value)
    {
      return System.Convert.ToDouble(System.Convert.ToUInt16(value));
    }

    decimal IAdvancedConverter<char, decimal>.Convert(char value)
    {
      return System.Convert.ToDecimal(System.Convert.ToUInt16(value));
    }


    // Constructors

    public CharAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}