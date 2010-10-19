// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.22

using System;
using System.Globalization;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class SByteAdvancedConverter :
    StrictAdvancedConverterBase<sbyte>,
    IAdvancedConverter<sbyte, byte>,
    IAdvancedConverter<sbyte, short>,
    IAdvancedConverter<sbyte, ushort>,
    IAdvancedConverter<sbyte, int>,
    IAdvancedConverter<sbyte, uint>,
    IAdvancedConverter<sbyte, long>,
    IAdvancedConverter<sbyte, ulong>,
    IAdvancedConverter<sbyte, float>,
    IAdvancedConverter<sbyte, double>,
    IAdvancedConverter<sbyte, decimal>,
    IAdvancedConverter<sbyte, DateTime>,
    IAdvancedConverter<sbyte, TimeSpan>,
    IAdvancedConverter<sbyte, string>,
    IAdvancedConverter<sbyte, char>
  {
    private readonly long baseDateTimeTicks;

    byte IAdvancedConverter<sbyte, byte>.Convert(sbyte value)
    {
      checked{
        return (byte)value;
      }
    }

    short IAdvancedConverter<sbyte, short>.Convert(sbyte value)
    {
      return value;
    }

    ushort IAdvancedConverter<sbyte, ushort>.Convert(sbyte value)
    {
      checked{
        return (ushort)value;
      }
    }

    int IAdvancedConverter<sbyte, int>.Convert(sbyte value)
    {
      return value;
    }

    uint IAdvancedConverter<sbyte, uint>.Convert(sbyte value)
    {
      checked{
        return (uint)value;
      }
    }

    long IAdvancedConverter<sbyte, long>.Convert(sbyte value)
    {
      return value;
    }

    ulong IAdvancedConverter<sbyte, ulong>.Convert(sbyte value)
    {
      checked{
        return (ulong)value;
      }
    }

    float IAdvancedConverter<sbyte, float>.Convert(sbyte value)
    {
      return System.Convert.ToSingle(value);
    }

    double IAdvancedConverter<sbyte, double>.Convert(sbyte value)
    {
      return System.Convert.ToDouble(value);
    }

    decimal IAdvancedConverter<sbyte, decimal>.Convert(sbyte value)
    {
      return System.Convert.ToDecimal(value);
    }

    DateTime IAdvancedConverter<sbyte, DateTime>.Convert(sbyte value)
    {
      checked{
        return new DateTime(value + baseDateTimeTicks, DateTimeKind.Utc);
      }
    }

    TimeSpan IAdvancedConverter<sbyte, TimeSpan>.Convert(sbyte value)
    {
      checked {
        return new TimeSpan(value);
      }
    }

    string IAdvancedConverter<sbyte, string>.Convert(sbyte value)
    {
      return value.ToString(CultureInfo.InvariantCulture);
    }

    char IAdvancedConverter<sbyte, char>.Convert(sbyte value)
    {
      return System.Convert.ToChar(value);
    }


    // Constructors

    public SByteAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      baseDateTimeTicks = provider.BaseTime.Ticks;
    }
  }
}