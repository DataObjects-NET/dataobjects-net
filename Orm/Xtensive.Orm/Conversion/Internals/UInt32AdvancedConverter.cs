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
  internal class UInt32AdvancedConverter :
    StrictAdvancedConverterBase<uint>,
    IAdvancedConverter<uint, byte>,
    IAdvancedConverter<uint, sbyte>,
    IAdvancedConverter<uint, short>,
    IAdvancedConverter<uint, ushort>,
    IAdvancedConverter<uint, int>,
    IAdvancedConverter<uint, long>,
    IAdvancedConverter<uint, ulong>,
    IAdvancedConverter<uint, double>,
    IAdvancedConverter<uint, decimal>,
    IAdvancedConverter<uint, DateTime>,
    IAdvancedConverter<uint, TimeSpan>,
    IAdvancedConverter<uint, string>,
    IAdvancedConverter<uint, char>
  {
    private readonly long baseDateTimeTicks;

    byte IAdvancedConverter<uint, byte>.Convert(uint value)
    {
      checked{
        return (byte)value;
      }
    }

    sbyte IAdvancedConverter<uint, sbyte>.Convert(uint value)
    {
      checked{
        return (sbyte)value;
      }
    }

    short IAdvancedConverter<uint, short>.Convert(uint value)
    {
      checked{
        return (short)value;
      }
    }

    ushort IAdvancedConverter<uint, ushort>.Convert(uint value)
    {
      checked{
        return (ushort)value;
      }
    }

    int IAdvancedConverter<uint, int>.Convert(uint value)
    {
      checked{
        return (int)value;
      }
    }

    long IAdvancedConverter<uint, long>.Convert(uint value)
    {
      return value;
    }

    ulong IAdvancedConverter<uint, ulong>.Convert(uint value)
    {
      return value;
    }


    double IAdvancedConverter<uint, double>.Convert(uint value)
    {
      return System.Convert.ToDouble(value);
    }

    decimal IAdvancedConverter<uint, decimal>.Convert(uint value)
    {
      return System.Convert.ToDecimal(value);
    }

    DateTime IAdvancedConverter<uint, DateTime>.Convert(uint value)
    {
      checked{
        return new DateTime(value + baseDateTimeTicks, DateTimeKind.Utc);
      }
    }

    TimeSpan IAdvancedConverter<uint, TimeSpan>.Convert(uint value)
    {
      checked {
        return new TimeSpan(value);
      }
    }

    string IAdvancedConverter<uint, string>.Convert(uint value)
    {
      return System.Convert.ToString(value, CultureInfo.InvariantCulture);
    }

    char IAdvancedConverter<uint, char>.Convert(uint value)
    {
      return System.Convert.ToChar(value);
    }


    // Constructors

    public UInt32AdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      baseDateTimeTicks = provider.BaseTime.Ticks;
    }
  }
}