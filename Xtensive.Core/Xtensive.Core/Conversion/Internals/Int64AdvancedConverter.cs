// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;
using System.Globalization;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class Int64AdvancedConverter :
    StrictAdvancedConverterBase<long>,
    IAdvancedConverter<long, byte>,
    IAdvancedConverter<long, sbyte>,
    IAdvancedConverter<long, short>,
    IAdvancedConverter<long, ushort>,
    IAdvancedConverter<long, int>,
    IAdvancedConverter<long, uint>,
    IAdvancedConverter<long, ulong>,
    IAdvancedConverter<long, decimal>,
    IAdvancedConverter<long, DateTime>,
    IAdvancedConverter<long, TimeSpan>,
    IAdvancedConverter<long, string>,
    IAdvancedConverter<long, char>
  {
    private readonly long baseDateTimeTicks;

    byte IAdvancedConverter<long, byte>.Convert(long value)
    {
      checked{
        return (byte)value;
      }
    }

    sbyte IAdvancedConverter<long, sbyte>.Convert(long value)
    {
      checked{
        return (sbyte)value;
      }
    }

    short IAdvancedConverter<long, short>.Convert(long value)
    {
      checked{
        return (short)value;
      }
    }

    ushort IAdvancedConverter<long, ushort>.Convert(long value)
    {
      checked{
        return (ushort)value;
      }
    }

    int IAdvancedConverter<long, int>.Convert(long value)
    {
      checked{
        return (int)value;
      }
    }

    uint IAdvancedConverter<long, uint>.Convert(long value)
    {
      checked{
        return (uint)value;
      }
    }

    ulong IAdvancedConverter<long, ulong>.Convert(long value)
    {
      checked{
        return (ulong)value;
      }
    }

    decimal IAdvancedConverter<long, decimal>.Convert(long value)
    {
      return System.Convert.ToDecimal(value);
    }

    DateTime IAdvancedConverter<long, DateTime>.Convert(long value)
    {
      checked{
        return new DateTime(value + baseDateTimeTicks, DateTimeKind.Utc);
      }
    }

    TimeSpan IAdvancedConverter<long, TimeSpan>.Convert(long value)
    {
      checked {
        return new TimeSpan(value);
      }
    }

    string IAdvancedConverter<long, string>.Convert(long value)
    {
      return value.ToString(CultureInfo.InvariantCulture);
    }

    char IAdvancedConverter<long, char>.Convert(long value)
    {
      return System.Convert.ToChar(value);
    }


    // Constructors

    public Int64AdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      baseDateTimeTicks = provider.BaseTime.Ticks;
    }
  }
}