// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Roman Churakov
// Created:    2008.01.23

using System;
using System.Globalization;

namespace Xtensive.Conversion
{
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
    IAdvancedConverter<uint, DateOnly>,
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

    long IAdvancedConverter<uint, long>.Convert(uint value) => value;

    ulong IAdvancedConverter<uint, ulong>.Convert(uint value) => value;

    double IAdvancedConverter<uint, double>.Convert(uint value) => System.Convert.ToDouble(value);

    decimal IAdvancedConverter<uint, decimal>.Convert(uint value) => System.Convert.ToDecimal(value);

    DateTime IAdvancedConverter<uint, DateTime>.Convert(uint value)
    {
      checked{
        return new DateTime(value + baseDateTimeTicks, DateTimeKind.Utc);
      }
    }

    DateOnly IAdvancedConverter<uint, DateOnly>.Convert(uint value)
    {
      checked {
        return DateOnly.FromDayNumber((int) value);
      }
    }

    TimeSpan IAdvancedConverter<uint, TimeSpan>.Convert(uint value)
    {
      checked {
        return new TimeSpan(value);
      }
    }

    string IAdvancedConverter<uint, string>.Convert(uint value) => System.Convert.ToString(value, CultureInfo.InvariantCulture);

    char IAdvancedConverter<uint, char>.Convert(uint value) => System.Convert.ToChar(value);


    // Constructors

    public UInt32AdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      baseDateTimeTicks = provider.BaseTime.Ticks;
    }
  }
}