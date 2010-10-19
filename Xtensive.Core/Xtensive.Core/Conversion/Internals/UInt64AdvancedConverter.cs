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
  internal class UInt64AdvancedConverter :
    StrictAdvancedConverterBase<ulong>,
    IAdvancedConverter<ulong, byte>,
    IAdvancedConverter<ulong, sbyte>,
    IAdvancedConverter<ulong, short>,
    IAdvancedConverter<ulong, ushort>,
    IAdvancedConverter<ulong, int>,
    IAdvancedConverter<ulong, uint>,
    IAdvancedConverter<ulong, long>,
    IAdvancedConverter<ulong, decimal>,
    IAdvancedConverter<ulong, DateTime>,
    IAdvancedConverter<ulong, TimeSpan>,
    IAdvancedConverter<ulong, string>,
    IAdvancedConverter<ulong, char>
  {
    private readonly long baseDateTimeTicks;

    byte IAdvancedConverter<ulong, byte>.Convert(ulong value)
    {
      checked{
        return (byte)value;
      }
    }

    sbyte IAdvancedConverter<ulong, sbyte>.Convert(ulong value)
    {
      checked{
        return (sbyte)value;
      }
    }

    short IAdvancedConverter<ulong, short>.Convert(ulong value)
    {
      checked{
        return (short)value;
      }
    }

    ushort IAdvancedConverter<ulong, ushort>.Convert(ulong value)
    {
      checked{
        return (ushort)value;
      }
    }

    int IAdvancedConverter<ulong, int>.Convert(ulong value)
    {
      checked{
        return (int)value;
      }
    }

    uint IAdvancedConverter<ulong, uint>.Convert(ulong value)
    {
      checked{
        return (uint)value;
      }
    }

    long IAdvancedConverter<ulong, long>.Convert(ulong value)
    {
      checked{
        return (long)value;
      }
    }

    decimal IAdvancedConverter<ulong, decimal>.Convert(ulong value)
    {
      return System.Convert.ToDecimal(value);
    }

    DateTime IAdvancedConverter<ulong, DateTime>.Convert(ulong value)
    {
      checked{
        return new DateTime((long)value + baseDateTimeTicks, DateTimeKind.Utc);
      }
    }

    TimeSpan IAdvancedConverter<ulong, TimeSpan>.Convert(ulong value)
    {
      checked {
        return new TimeSpan((long)value);
      }
    }

    string IAdvancedConverter<ulong, string>.Convert(ulong value)
    {
      return System.Convert.ToString(value, CultureInfo.InvariantCulture);
    }

    char IAdvancedConverter<ulong, char>.Convert(ulong value)
    {
      return System.Convert.ToChar(value);
    }


    // Constructors

    public UInt64AdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      baseDateTimeTicks = provider.BaseTime.Ticks;
    }
  }
}