// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.23

using System;
using System.Globalization;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class UInt16AdvancedConverter :
    StrictAdvancedConverterBase<ushort>,
    IAdvancedConverter<ushort, byte>,
    IAdvancedConverter<ushort, sbyte>,
    IAdvancedConverter<ushort, short>,
    IAdvancedConverter<ushort, int>,
    IAdvancedConverter<ushort, uint>,
    IAdvancedConverter<ushort, long>,
    IAdvancedConverter<ushort, ulong>,
    IAdvancedConverter<ushort, float>,
    IAdvancedConverter<ushort, double>,
    IAdvancedConverter<ushort, decimal>,
    IAdvancedConverter<ushort, DateTime>,
    IAdvancedConverter<ushort, TimeSpan>,
    IAdvancedConverter<ushort, string>,
    IAdvancedConverter<ushort, char>
  {
    private readonly long baseDateTimeTicks;


    byte IAdvancedConverter<ushort, byte>.Convert(ushort value)
    {
      checked{
        return (byte)value;
      }
    }

    sbyte IAdvancedConverter<ushort, sbyte>.Convert(ushort value)
    {
      checked{
        return (sbyte)value;
      }
    }

    short IAdvancedConverter<ushort, short>.Convert(ushort value)
    {
      checked{
        return (short)value;
      }
    }

    int IAdvancedConverter<ushort, int>.Convert(ushort value)
    {
      return value;
    }

    uint IAdvancedConverter<ushort, uint>.Convert(ushort value)
    {
      return value;
    }

    long IAdvancedConverter<ushort, long>.Convert(ushort value)
    {
      return value;
    }

    ulong IAdvancedConverter<ushort, ulong>.Convert(ushort value)
    {
      return value;
    }

    float IAdvancedConverter<ushort, float>.Convert(ushort value)
    {
      return System.Convert.ToSingle(value);
    }

    double IAdvancedConverter<ushort, double>.Convert(ushort value)
    {
      return System.Convert.ToDouble(value);
    }

    decimal IAdvancedConverter<ushort, decimal>.Convert(ushort value)
    {
      return System.Convert.ToDecimal(value);
    }

    DateTime IAdvancedConverter<ushort, DateTime>.Convert(ushort value)
    {
      checked{
        return new DateTime(value + baseDateTimeTicks, DateTimeKind.Utc);
      }
    }

    TimeSpan IAdvancedConverter<ushort, TimeSpan>.Convert(ushort value)
    {
      checked {
        return new TimeSpan(value);
      }
    }

    string IAdvancedConverter<ushort, string>.Convert(ushort value)
    {
      return System.Convert.ToString(value, CultureInfo.InvariantCulture);
    }

    char IAdvancedConverter<ushort, char>.Convert(ushort value)
    {
      return System.Convert.ToChar(value);
    }


    // Constructors

    public UInt16AdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      baseDateTimeTicks = provider.BaseTime.Ticks;
    }
  }
}