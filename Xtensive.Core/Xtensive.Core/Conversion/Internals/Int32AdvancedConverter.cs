// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.21

using System;
using System.Globalization;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class Int32AdvancedConverter :
    StrictAdvancedConverterBase<int>,
    IAdvancedConverter<int, byte>,
    IAdvancedConverter<int, sbyte>,
    IAdvancedConverter<int, short>,
    IAdvancedConverter<int, ushort>,
    IAdvancedConverter<int, uint>,
    IAdvancedConverter<int, long>,
    IAdvancedConverter<int, ulong>,
    IAdvancedConverter<int, double>,
    IAdvancedConverter<int, decimal>,
    IAdvancedConverter<int, DateTime>,
    IAdvancedConverter<int, TimeSpan>,
    IAdvancedConverter<int, string>,
    IAdvancedConverter<int, char>
  {
    private readonly long baseDateTimeTicks;

    byte IAdvancedConverter<int, byte>.Convert(int value)
    {
      checked{
        return (byte)value;
      }
    }

    sbyte IAdvancedConverter<int, sbyte>.Convert(int value)
    {
      checked{
        return (sbyte)value;
      }
    }

    short IAdvancedConverter<int, short>.Convert(int value)
    {
      checked{
        return (short)value;
      }
    }

    ushort IAdvancedConverter<int, ushort>.Convert(int value)
    {
      checked{
        return (ushort)value;
      }
    }

    uint IAdvancedConverter<int, uint>.Convert(int value)
    {
      checked{
        return (uint)value;
      }
    }

    long IAdvancedConverter<int, long>.Convert(int value)
    {
      return value;
    }

    ulong IAdvancedConverter<int, ulong>.Convert(int value)
    {
      checked{
        return (ulong)value;
      }
    }

    double IAdvancedConverter<int, double>.Convert(int value)
    {
      return System.Convert.ToDouble(value);
    }

    decimal IAdvancedConverter<int, decimal>.Convert(int value)
    {
      return System.Convert.ToDecimal(value);
    }

    DateTime IAdvancedConverter<int, DateTime>.Convert(int value)
    {
      checked{
        return new DateTime(value + baseDateTimeTicks, DateTimeKind.Utc);
      }
    }

    TimeSpan IAdvancedConverter<int, TimeSpan>.Convert(int value)
    {
      checked {
        return new TimeSpan(value);
      }
    }


    string IAdvancedConverter<int, string>.Convert(int value)
    {
      return value.ToString(CultureInfo.InvariantCulture);
    }

    char IAdvancedConverter<int, char>.Convert(int value)
    {
      return System.Convert.ToChar(value);
    }


    // Constructors

    public Int32AdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      baseDateTimeTicks = provider.BaseTime.Ticks;
    }
  }
}