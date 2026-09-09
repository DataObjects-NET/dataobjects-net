// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.11.12

using System;

namespace Xtensive.Conversion
{
  internal class TimeSpanRoughAdvancedConverter :
    RoughAdvancedConverterBase,
    IAdvancedConverter<TimeSpan, float>,
    IAdvancedConverter<TimeSpan, double>,
    IAdvancedConverter<TimeSpan, DateOnly>,
    IAdvancedConverter<TimeSpan, DateTimeOffset>
  {
    float IAdvancedConverter<TimeSpan, float>.Convert(TimeSpan value) => Convert.ToSingle(value.Ticks);

    double IAdvancedConverter<TimeSpan, double>.Convert(TimeSpan value) => Convert.ToDouble(value.Ticks);

    DateOnly IAdvancedConverter<TimeSpan, DateOnly>.Convert(TimeSpan value) => DateOnly.FromDayNumber((int) Math.Round(value.TotalDays));

    DateTimeOffset IAdvancedConverter<TimeSpan, DateTimeOffset>.Convert(TimeSpan value) => new DateTimeOffset(value.Ticks, TimeSpan.Zero);

    // Constructors

    public TimeSpanRoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}