// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.08

using System;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class DecimalRoughAdvancedConverter :
    RoughAdvancedConverterBase,
    IAdvancedConverter<decimal, bool>,
    IAdvancedConverter<decimal, byte>,
    IAdvancedConverter<decimal, sbyte>,
    IAdvancedConverter<decimal, short>,
    IAdvancedConverter<decimal, ushort>,
    IAdvancedConverter<decimal, int>,
    IAdvancedConverter<decimal, uint>,
    IAdvancedConverter<decimal, long>,
    IAdvancedConverter<decimal, ulong>,
    IAdvancedConverter<decimal, float>,
    IAdvancedConverter<decimal, double>,
    IAdvancedConverter<decimal, DateTime>,
    IAdvancedConverter<decimal, TimeSpan>,
    IAdvancedConverter<decimal, char>
  {
    private readonly long baseDateTimeTicks;

    /// <inheritdoc/>
    bool IAdvancedConverter<decimal, bool>.Convert(decimal value)
    {
      return Convert.ToBoolean(value);
    }

    /// <inheritdoc/>
    byte IAdvancedConverter<decimal, byte>.Convert(decimal value)
    {
      return Convert.ToByte(value);
    }

    /// <inheritdoc/>
    sbyte IAdvancedConverter<decimal, sbyte>.Convert(decimal value)
    {
      return Convert.ToSByte(value);
    }

    /// <inheritdoc/>
    short IAdvancedConverter<decimal, short>.Convert(decimal value)
    {
      return Convert.ToInt16(value);
    }

    /// <inheritdoc/>
    ushort IAdvancedConverter<decimal, ushort>.Convert(decimal value)
    {
      return Convert.ToUInt16(value);
    }

    /// <inheritdoc/>
    int IAdvancedConverter<decimal, int>.Convert(decimal value)
    {
      return Convert.ToInt32(value);
    }

    /// <inheritdoc/>
    uint IAdvancedConverter<decimal, uint>.Convert(decimal value)
    {
      return Convert.ToUInt32(value);
    }

    /// <inheritdoc/>
    long IAdvancedConverter<decimal, long>.Convert(decimal value)
    {
      return Convert.ToInt64(value);
    }

    /// <inheritdoc/>
    ulong IAdvancedConverter<decimal, ulong>.Convert(decimal value)
    {
      return Convert.ToUInt64(value);
    }

    /// <inheritdoc/>
    float IAdvancedConverter<decimal, float>.Convert(decimal value)
    {
      return Convert.ToSingle(value);
    }

    /// <inheritdoc/>
    double IAdvancedConverter<decimal, double>.Convert(decimal value)
    {
      return Convert.ToDouble(value);
    }

    /// <inheritdoc/>
    DateTime IAdvancedConverter<decimal, DateTime>.Convert(decimal value)
    {
      return new DateTime(Convert.ToInt64(value) + baseDateTimeTicks, DateTimeKind.Utc);
    }

    /// <inheritdoc/>
    TimeSpan IAdvancedConverter<decimal, TimeSpan>.Convert(decimal value)
    {
      return new TimeSpan(Convert.ToInt64(value));
    }


    /// <inheritdoc/>
    char IAdvancedConverter<decimal, char>.Convert(decimal value)
    {
      return Convert.ToChar(Convert.ToInt64(value));
    }


    // Constructors

    public DecimalRoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      baseDateTimeTicks = provider.BaseTime.Ticks;
    }
  }
}