// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.08

using System;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class DoubleRoughAdvancedConverter :
    RoughAdvancedConverterBase,
    IAdvancedConverter<double, bool>,
    IAdvancedConverter<double, byte>,
    IAdvancedConverter<double, sbyte>,
    IAdvancedConverter<double, short>,
    IAdvancedConverter<double, ushort>,
    IAdvancedConverter<double, int>,
    IAdvancedConverter<double, uint>,
    IAdvancedConverter<double, long>,
    IAdvancedConverter<double, ulong>,
    IAdvancedConverter<double, float>,
    IAdvancedConverter<double, decimal>,
    IAdvancedConverter<double, DateTime>,
    IAdvancedConverter<double, TimeSpan>,
    IAdvancedConverter<double, char>
  {
    private readonly long baseDateTimeTicks;

    bool IAdvancedConverter<double, bool>.Convert(double value)
    {
      return Convert.ToBoolean(value);
    }

    byte IAdvancedConverter<double, byte>.Convert(double value)
    {
      return Convert.ToByte(value);
    }

    sbyte IAdvancedConverter<double, sbyte>.Convert(double value)
    {
      return Convert.ToSByte(value);
    }

    short IAdvancedConverter<double, short>.Convert(double value)
    {
      return Convert.ToInt16(value);
    }

    ushort IAdvancedConverter<double, ushort>.Convert(double value)
    {
      return Convert.ToUInt16(value);
    }

    int IAdvancedConverter<double, int>.Convert(double value)
    {
      return Convert.ToInt32(value);
    }

    uint IAdvancedConverter<double, uint>.Convert(double value)
    {
      return Convert.ToUInt32(value);
    }

    long IAdvancedConverter<double, long>.Convert(double value)
    {
      // Since "Convert.ToInt64(double)" does not check overflow:
      checked {
        return (long) Math.Round(value);
      }
    }

    ulong IAdvancedConverter<double, ulong>.Convert(double value)
    {
      // Since "Convert.ToUInt64(double)" does not check overflow:
      checked {
        return (ulong) Math.Round(value);
      }
    }

    float IAdvancedConverter<double, float>.Convert(double value)
    {
      return Convert.ToSingle(value);
    }

    decimal IAdvancedConverter<double, decimal>.Convert(double value)
    {
      return Convert.ToDecimal(value);
    }

    DateTime IAdvancedConverter<double, DateTime>.Convert(double value)
    {
      checked {
        return new DateTime((long) Math.Round(value) + baseDateTimeTicks, DateTimeKind.Utc);
      }
    }

    TimeSpan IAdvancedConverter<double, TimeSpan>.Convert(double value)
    {
      checked {
        return new TimeSpan((long) Math.Round(value));
      }
    }

    char IAdvancedConverter<double, char>.Convert(double value)
    {
      checked {
        return Convert.ToChar((long) Math.Round(value));
      }
    }


    // Constructors

    public DoubleRoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      baseDateTimeTicks = provider.BaseTime.Ticks;
    }
  }
}