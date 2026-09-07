// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2008.01.21

using System;
using System.Globalization;

namespace Xtensive.Conversion
{
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
    IAdvancedConverter<int, DateOnly>,
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

    long IAdvancedConverter<int, long>.Convert(int value) => value;

    ulong IAdvancedConverter<int, ulong>.Convert(int value)
    {
      checked{
        return (ulong)value;
      }
    }

    double IAdvancedConverter<int, double>.Convert(int value) => System.Convert.ToDouble(value);

    decimal IAdvancedConverter<int, decimal>.Convert(int value) => System.Convert.ToDecimal(value);

    DateTime IAdvancedConverter<int, DateTime>.Convert(int value)
    {
      checked{
        return new DateTime(value + baseDateTimeTicks, DateTimeKind.Utc);
      }
    }

    DateOnly IAdvancedConverter<int, DateOnly>.Convert(int value)
    {
      return DateOnly.FromDayNumber(value);
    }

    TimeSpan IAdvancedConverter<int, TimeSpan>.Convert(int value)
    {
      checked {
        return new TimeSpan(value);
      }
    }

    string IAdvancedConverter<int, string>.Convert(int value) => value.ToString(CultureInfo.InvariantCulture);

    char IAdvancedConverter<int, char>.Convert(int value) => System.Convert.ToChar(value);


    // Constructors

    public Int32AdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      baseDateTimeTicks = provider.BaseTime.Ticks;
    }
  }
}