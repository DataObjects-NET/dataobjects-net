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
  internal class Int16AdvancedConverter :
    StrictAdvancedConverterBase<short>,
    IAdvancedConverter<short, byte>,
    IAdvancedConverter<short, sbyte>,
    IAdvancedConverter<short, ushort>,
    IAdvancedConverter<short, int>,
    IAdvancedConverter<short, uint>,
    IAdvancedConverter<short, long>,
    IAdvancedConverter<short, ulong>,
    IAdvancedConverter<short, float>,
    IAdvancedConverter<short, double>,
    IAdvancedConverter<short, decimal>,
    IAdvancedConverter<short, DateTime>,
    IAdvancedConverter<short, TimeSpan>,
    IAdvancedConverter<short, string>,
    IAdvancedConverter<short, char>
  {
    private readonly long baseDateTimeTicks;

    byte IAdvancedConverter<short, byte>.Convert(short value)
    {
      checked{
        return (byte)value;
      }
    }

    sbyte IAdvancedConverter<short, sbyte>.Convert(short value)
    {
      checked{
        return (sbyte)value;
      }
    }

    ushort IAdvancedConverter<short, ushort>.Convert(short value)
    {
      checked{
        return (ushort)value;
      }
    }

    int IAdvancedConverter<short, int>.Convert(short value)
    {
      return value;
    }

    uint IAdvancedConverter<short, uint>.Convert(short value)
    {
      checked{
        return (uint)value;
      }
    }

    long IAdvancedConverter<short, long>.Convert(short value)
    {
      return value;
    }

    ulong IAdvancedConverter<short, ulong>.Convert(short value)
    {
      checked{
        return (ulong)value;
      }
    }

    float IAdvancedConverter<short, float>.Convert(short value)
    {
      return System.Convert.ToSingle(value);
    }

    double IAdvancedConverter<short, double>.Convert(short value)
    {
      return System.Convert.ToDouble(value);
    }

    decimal IAdvancedConverter<short, decimal>.Convert(short value)
    {
      return System.Convert.ToDecimal(value);
    }

    DateTime IAdvancedConverter<short, DateTime>.Convert(short value)
    {
      checked{
        return new DateTime(value + baseDateTimeTicks, DateTimeKind.Utc);
      }
    }

    TimeSpan IAdvancedConverter<short, TimeSpan>.Convert(short value)
    {
      checked {
        return new TimeSpan(value);
      }
    }

    string IAdvancedConverter<short, string>.Convert(short value)
    {
      return value.ToString(CultureInfo.InvariantCulture);
    }

    char IAdvancedConverter<short, char>.Convert(short value)
    {
      return System.Convert.ToChar(value);
    }


    // Constructors

    public Int16AdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      baseDateTimeTicks = provider.BaseTime.Ticks;
    }
  }
}