// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.22

using System;
using System.Globalization;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class ByteAdvancedConverter :
    StrictAdvancedConverterBase<byte>,
    IAdvancedConverter<byte, sbyte>,
    IAdvancedConverter<byte, short>,
    IAdvancedConverter<byte, ushort>,
    IAdvancedConverter<byte, int>,
    IAdvancedConverter<byte, uint>,
    IAdvancedConverter<byte, long>,
    IAdvancedConverter<byte, ulong>,
    IAdvancedConverter<byte, float>,
    IAdvancedConverter<byte, double>,
    IAdvancedConverter<byte, decimal>,
    IAdvancedConverter<byte, DateTime>,
    IAdvancedConverter<byte, TimeSpan>,
    IAdvancedConverter<byte, string>,
    IAdvancedConverter<byte, char>
  {
    private readonly long baseDateTimeTicks;

    sbyte IAdvancedConverter<byte, sbyte>.Convert(byte value)
    {
      checked {
        return (sbyte) value;
      }
    }

    short IAdvancedConverter<byte, short>.Convert(byte value)
    {
      return value;
    }

    ushort IAdvancedConverter<byte, ushort>.Convert(byte value)
    {
      return value;
    }

    int IAdvancedConverter<byte, int>.Convert(byte value)
    {
      return value;
    }

    uint IAdvancedConverter<byte, uint>.Convert(byte value)
    {
      return value;
    }

    long IAdvancedConverter<byte, long>.Convert(byte value)
    {
      return value;
    }

    ulong IAdvancedConverter<byte, ulong>.Convert(byte value)
    {
      return value;
    }

    float IAdvancedConverter<byte, float>.Convert(byte value)
    {
      return System.Convert.ToSingle(value);
    }

    double IAdvancedConverter<byte, double>.Convert(byte value)
    {
      return System.Convert.ToDouble(value);
    }

    decimal IAdvancedConverter<byte, decimal>.Convert(byte value)
    {
      return System.Convert.ToDecimal(value);
    }

    DateTime IAdvancedConverter<byte, DateTime>.Convert(byte value)
    {
      checked {
        return new DateTime(value + baseDateTimeTicks, DateTimeKind.Utc);
      }
    }

    TimeSpan IAdvancedConverter<byte, TimeSpan>.Convert(byte value)
    {
      checked {
        return new TimeSpan(value);
      }
    }

    string IAdvancedConverter<byte, string>.Convert(byte value)
    {
      return value.ToString(CultureInfo.InvariantCulture);
    }

    char IAdvancedConverter<byte, char>.Convert(byte value)
    {
      return System.Convert.ToChar(value);
    }


    // Constructors

    public ByteAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      baseDateTimeTicks = provider.BaseTime.Ticks;
    }
  }
}