// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Elena Vakhtina
// Created:    2008.11.11

using System;

namespace Xtensive.Conversion
{
  internal class TimeSpanAdvancedConverter :
    StrictAdvancedConverterBase<TimeSpan>,
    IAdvancedConverter<TimeSpan, byte>,
    IAdvancedConverter<TimeSpan, sbyte>,
    IAdvancedConverter<TimeSpan, short>,
    IAdvancedConverter<TimeSpan, ushort>,
    IAdvancedConverter<TimeSpan, int>,
    IAdvancedConverter<TimeSpan, uint>,
    IAdvancedConverter<TimeSpan, long>,
    IAdvancedConverter<TimeSpan, ulong>,
    IAdvancedConverter<TimeSpan, decimal>,
    IAdvancedConverter<TimeSpan, TimeOnly>,
    IAdvancedConverter<TimeSpan, TimeSpan>,
    IAdvancedConverter<TimeSpan, string>
  {
    byte IAdvancedConverter<TimeSpan, byte>.Convert(TimeSpan value)
    {
      checked {
        return (byte) (value.Ticks);
      }
    }

    sbyte IAdvancedConverter<TimeSpan, sbyte>.Convert(TimeSpan value)
    {
      checked {
        return (sbyte) (value.Ticks);
      }
    }

    short IAdvancedConverter<TimeSpan, short>.Convert(TimeSpan value)
    {
      checked {
        return (short) (value.Ticks);
      }
    }

    ushort IAdvancedConverter<TimeSpan, ushort>.Convert(TimeSpan value)
    {
      checked {
        return (ushort) (value.Ticks);
      }
    }

    int IAdvancedConverter<TimeSpan, int>.Convert(TimeSpan value)
    {
      checked {
        return (int) (value.Ticks);
      }
    }

    uint IAdvancedConverter<TimeSpan, uint>.Convert(TimeSpan value)
    {
      checked {
        return (uint) (value.Ticks);
      }
    }

    long IAdvancedConverter<TimeSpan, long>.Convert(TimeSpan value)
    {
      checked {
        return value.Ticks;
      }
    }

    ulong IAdvancedConverter<TimeSpan, ulong>.Convert(TimeSpan value)
    {
      checked {
        return (ulong) (value.Ticks);
      }
    }

    decimal IAdvancedConverter<TimeSpan, decimal>.Convert(TimeSpan value) => System.Convert.ToDecimal(value.Ticks);

    TimeSpan IAdvancedConverter<TimeSpan, TimeSpan>.Convert(TimeSpan value) => value;

    TimeOnly IAdvancedConverter<TimeSpan, TimeOnly>.Convert(TimeSpan value) => TimeOnly.FromTimeSpan(value);

    string IAdvancedConverter<TimeSpan, string>.Convert(TimeSpan value) => value.ToString();


    // Constructors

    public TimeSpanAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}