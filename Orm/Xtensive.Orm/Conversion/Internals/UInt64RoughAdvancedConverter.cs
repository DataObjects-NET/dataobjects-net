// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.08

using System;

namespace Xtensive.Conversion
{
  internal class UInt64RoughAdvancedConverter :
    RoughAdvancedConverterBase,
    IAdvancedConverter<ulong, bool>,
    IAdvancedConverter<ulong, float>,
    IAdvancedConverter<ulong, double>,
    IAdvancedConverter<ulong, DateTimeOffset>
  {
    bool IAdvancedConverter<ulong, bool>.Convert(ulong value) => Convert.ToBoolean(value);

    float IAdvancedConverter<ulong, float>.Convert(ulong value) => Convert.ToSingle(value);

    double IAdvancedConverter<ulong, double>.Convert(ulong value) => Convert.ToDouble(value);

    DateTimeOffset IAdvancedConverter<ulong, DateTimeOffset>.Convert(ulong value) => new DateTimeOffset(Convert.ToInt64(value), TimeSpan.Zero);


    // Constructors

    public UInt64RoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}