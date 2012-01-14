// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.23

using System;
using System.Globalization;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class DateTimeAdvancedConverter :
    StrictAdvancedConverterBase<DateTime>,
    IAdvancedConverter<DateTime, byte>,
    IAdvancedConverter<DateTime, sbyte>,
    IAdvancedConverter<DateTime, short>,
    IAdvancedConverter<DateTime, ushort>,
    IAdvancedConverter<DateTime, int>,
    IAdvancedConverter<DateTime, uint>,
    IAdvancedConverter<DateTime, long>,
    IAdvancedConverter<DateTime, ulong>,
    IAdvancedConverter<DateTime, decimal>,
    IAdvancedConverter<DateTime, DateTime>,
    IAdvancedConverter<DateTime, TimeSpan>,
    IAdvancedConverter<DateTime, string>
  {
    private readonly long baseDateTimeTicks;

    byte IAdvancedConverter<DateTime, byte>.Convert(DateTime value)
    {
      checked{
        return (byte)(value.Ticks - baseDateTimeTicks);
      }
    }

    sbyte IAdvancedConverter<DateTime, sbyte>.Convert(DateTime value)
    {
      checked{
        return (sbyte)(value.Ticks - baseDateTimeTicks);
      }
    }

    short IAdvancedConverter<DateTime, short>.Convert(DateTime value)
    {
      checked{
        return (short)(value.Ticks - baseDateTimeTicks);
      }
    }

    ushort IAdvancedConverter<DateTime, ushort>.Convert(DateTime value)
    {
      checked{
        return (ushort)(value.Ticks - baseDateTimeTicks);
      }
    }

    int IAdvancedConverter<DateTime, int>.Convert(DateTime value)
    {
      checked{
        return (int)(value.Ticks - baseDateTimeTicks);
      }
    }

    uint IAdvancedConverter<DateTime, uint>.Convert(DateTime value)
    {
      checked{
        return (uint)(value.Ticks - baseDateTimeTicks);
      }
    }

    long IAdvancedConverter<DateTime, long>.Convert(DateTime value)
    {
      checked{
        return value.Ticks - baseDateTimeTicks;
      }
    }

    ulong IAdvancedConverter<DateTime, ulong>.Convert(DateTime value)
    {
      checked{
        return (ulong)(value.Ticks - baseDateTimeTicks);
      }
    }

    decimal IAdvancedConverter<DateTime, decimal>.Convert(DateTime value)
    {
      return System.Convert.ToDecimal(value.Ticks - baseDateTimeTicks);
    }

    DateTime IAdvancedConverter<DateTime, DateTime>.Convert(DateTime value)
    {
      return value;
    }

    TimeSpan IAdvancedConverter<DateTime, TimeSpan>.Convert(DateTime value)
    {
      return new TimeSpan(value.Ticks - baseDateTimeTicks);
    }

    string IAdvancedConverter<DateTime, string>.Convert(DateTime value)
    {
      return value.ToString("yyyy/MM/dd hh:mm:ss.fffffff tt K ", CultureInfo.InvariantCulture);
    }


    // Constructors

    public DateTimeAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      baseDateTimeTicks = provider.BaseTime.Ticks;
    }
  }
}