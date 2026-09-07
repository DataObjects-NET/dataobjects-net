// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2008.02.08

using System;

namespace Xtensive.Conversion
{
  internal class SingleRoughAdvancedConverter :
    RoughAdvancedConverterBase,
    IAdvancedConverter<float, bool>,
    IAdvancedConverter<float, byte>,
    IAdvancedConverter<float, sbyte>,
    IAdvancedConverter<float, short>,
    IAdvancedConverter<float, ushort>,
    IAdvancedConverter<float, int>,
    IAdvancedConverter<float, uint>,
    IAdvancedConverter<float, long>,
    IAdvancedConverter<float, ulong>,
    IAdvancedConverter<float, double>,
    IAdvancedConverter<float, decimal>,
    IAdvancedConverter<float, DateTime>,
    IAdvancedConverter<float, DateOnly>,
    IAdvancedConverter<float, TimeOnly>,
    IAdvancedConverter<float, TimeSpan>,
    IAdvancedConverter<float, char>
  {
    private readonly long baseDateTimeTicks;
    private readonly long ticksPerDay;

    bool IAdvancedConverter<float, bool>.Convert(float value)
    {
      return Convert.ToBoolean(value);
    }

    byte IAdvancedConverter<float, byte>.Convert(float value)
    {
      return Convert.ToByte(value);
    }

    sbyte IAdvancedConverter<float, sbyte>.Convert(float value)
    {
      return Convert.ToSByte(value);
    }

    short IAdvancedConverter<float, short>.Convert(float value)
    {
      return Convert.ToInt16(value);
    }

    ushort IAdvancedConverter<float, ushort>.Convert(float value)
    {
      return Convert.ToUInt16(value);
    }

    int IAdvancedConverter<float, int>.Convert(float value)
    {
      return Convert.ToInt32(value);
    }

    uint IAdvancedConverter<float, uint>.Convert(float value)
    {
      return Convert.ToUInt32(value);
    }

    long IAdvancedConverter<float, long>.Convert(float value)
    {
      // Since "Convert.ToInt64(double)" does not check overflow:
      checked{
        return (long)Math.Round(value);
      }
    }

    ulong IAdvancedConverter<float, ulong>.Convert(float value)
    {
      // Since "Convert.ToUInt64(double)" does not check overflow:
      checked{
        return (ulong)Math.Round(value);
      }
    }

    double IAdvancedConverter<float, double>.Convert(float value)
    {
      return Convert.ToDouble(value);
    }

    decimal IAdvancedConverter<float, decimal>.Convert(float value)
    {
      return Convert.ToDecimal(value);
    }

    DateTime IAdvancedConverter<float, DateTime>.Convert(float value)
    {
      // Since "Convert.ToUInt64(double)" does not check overflow:
      checked{
        return new DateTime((long)Math.Round(value) + baseDateTimeTicks, DateTimeKind.Utc);
      }
    }

    DateOnly IAdvancedConverter<float, DateOnly>.Convert(float value)
    {
      checked {
        return DateOnly.FromDayNumber(Convert.ToInt32(Math.Round(value)));
      }
    }

    TimeOnly IAdvancedConverter<float, TimeOnly>.Convert(float value)
    {
      checked {
        return new TimeOnly((long) Math.Round(value));
      }
    }

    TimeSpan IAdvancedConverter<float, TimeSpan>.Convert(float value)
    {
      // Since "Convert.ToUInt64(double)" does not check overflow:
      checked {
        return new TimeSpan((long)Math.Round(value));
      }
    }

    char IAdvancedConverter<float, char>.Convert(float value)
    {
      // Since "Convert.ToUInt64(double)" does not check overflow:
      checked{
        return Convert.ToChar((long)Math.Round(value));
      }
    }


    // Constructors

    public SingleRoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      baseDateTimeTicks = provider.BaseTime.Ticks;
      ticksPerDay = TimeSpan.FromDays(1).Ticks;
    }
  }
}