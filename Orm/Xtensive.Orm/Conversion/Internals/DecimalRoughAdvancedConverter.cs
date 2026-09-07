// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2008.02.08

using System;

namespace Xtensive.Conversion
{
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
    IAdvancedConverter<decimal, DateOnly>,
    IAdvancedConverter<decimal, TimeOnly>,
    IAdvancedConverter<decimal, TimeSpan>,
    IAdvancedConverter<decimal, char>
  {
    private readonly long baseDateTimeTicks;

    /// <inheritdoc/>
    bool IAdvancedConverter<decimal, bool>.Convert(decimal value) => Convert.ToBoolean(value);

    /// <inheritdoc/>
    byte IAdvancedConverter<decimal, byte>.Convert(decimal value) => Convert.ToByte(value);

    /// <inheritdoc/>
    sbyte IAdvancedConverter<decimal, sbyte>.Convert(decimal value) => Convert.ToSByte(value);

    /// <inheritdoc/>
    short IAdvancedConverter<decimal, short>.Convert(decimal value) => Convert.ToInt16(value);

    /// <inheritdoc/>
    ushort IAdvancedConverter<decimal, ushort>.Convert(decimal value) => Convert.ToUInt16(value);

    /// <inheritdoc/>
    int IAdvancedConverter<decimal, int>.Convert(decimal value) => Convert.ToInt32(value);

    /// <inheritdoc/>
    uint IAdvancedConverter<decimal, uint>.Convert(decimal value) => Convert.ToUInt32(value);

    /// <inheritdoc/>
    long IAdvancedConverter<decimal, long>.Convert(decimal value) => Convert.ToInt64(value);

    /// <inheritdoc/>
    ulong IAdvancedConverter<decimal, ulong>.Convert(decimal value) => Convert.ToUInt64(value);

    /// <inheritdoc/>
    float IAdvancedConverter<decimal, float>.Convert(decimal value) => Convert.ToSingle(value);

    /// <inheritdoc/>
    double IAdvancedConverter<decimal, double>.Convert(decimal value) => Convert.ToDouble(value);

    /// <inheritdoc/>
    DateTime IAdvancedConverter<decimal, DateTime>.Convert(decimal value)
      => new DateTime(Convert.ToInt64(value) + baseDateTimeTicks, DateTimeKind.Utc);

    /// <inheritdoc/>
    DateOnly IAdvancedConverter<decimal, DateOnly>.Convert(decimal value)
      => DateOnly.FromDayNumber(Convert.ToInt32(value));

    /// <inheritdoc/>
    TimeOnly IAdvancedConverter<decimal, TimeOnly>.Convert(decimal value)
      => new TimeOnly(Convert.ToInt64(value));

    /// <inheritdoc/>
    TimeSpan IAdvancedConverter<decimal, TimeSpan>.Convert(decimal value) => new TimeSpan(Convert.ToInt64(value));

    /// <inheritdoc/>
    char IAdvancedConverter<decimal, char>.Convert(decimal value) => Convert.ToChar(Convert.ToInt64(value));


    // Constructors

    public DecimalRoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      baseDateTimeTicks = provider.BaseTime.Ticks;
    }
  }
}