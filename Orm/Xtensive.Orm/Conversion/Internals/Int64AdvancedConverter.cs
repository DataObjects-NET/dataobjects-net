// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;
using System.Globalization;

namespace Xtensive.Conversion
{
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
    IAdvancedConverter<long, DateOnly>,
    IAdvancedConverter<long, TimeOnly>,
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

    decimal IAdvancedConverter<long, decimal>.Convert(long value) => System.Convert.ToDecimal(value);

    DateTime IAdvancedConverter<long, DateTime>.Convert(long value)
    {
      checked{
        return new DateTime(value + baseDateTimeTicks, DateTimeKind.Utc);
      }
    }

    DateOnly IAdvancedConverter<long, DateOnly>.Convert(long value)
    {
      checked {
        return DateOnly.FromDayNumber((int) value);
      }
    }

    TimeOnly IAdvancedConverter<long, TimeOnly>.Convert(long value)
    {
      checked {
        return new TimeOnly(value);
      }
    }

    TimeSpan IAdvancedConverter<long, TimeSpan>.Convert(long value)
    {
      checked {
        return new TimeSpan(value);
      }
    }

    string IAdvancedConverter<long, string>.Convert(long value) => value.ToString(CultureInfo.InvariantCulture);

    char IAdvancedConverter<long, char>.Convert(long value) => System.Convert.ToChar(value);


    // Constructors

    public Int64AdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      baseDateTimeTicks = provider.BaseTime.Ticks;
    }
  }
}