// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.11.11

using System;

namespace Xtensive.Conversion
{
  [Serializable]
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

    decimal IAdvancedConverter<TimeSpan, decimal>.Convert(TimeSpan value)
    {
      return System.Convert.ToDecimal(value.Ticks);
    }

    TimeSpan IAdvancedConverter<TimeSpan, TimeSpan>.Convert(TimeSpan value)
    {
      return value;
    }

    string IAdvancedConverter<TimeSpan, string>.Convert(TimeSpan value)
    {
      return value.ToString();
    }


    // Constructors

    public TimeSpanAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}