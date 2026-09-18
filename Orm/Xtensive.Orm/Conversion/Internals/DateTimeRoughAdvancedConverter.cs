// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.08

using System;

namespace Xtensive.Conversion
{
  internal class DateTimeRoughAdvancedConverter :
    RoughAdvancedConverterBase,
    IAdvancedConverter<DateTime, float>,
    IAdvancedConverter<DateTime, double>,
    IAdvancedConverter<DateTime, DateTimeOffset>

  {
    private readonly long baseDateTimeTicks;

    float IAdvancedConverter<DateTime, float>.Convert(DateTime value) => Convert.ToSingle(value.Ticks - baseDateTimeTicks);
    double IAdvancedConverter<DateTime, double>.Convert(DateTime value) => Convert.ToDouble(value.Ticks - baseDateTimeTicks);
    DateTimeOffset IAdvancedConverter<DateTime, DateTimeOffset>.Convert(DateTime value)
    {
      if (value.Kind == DateTimeKind.Utc) {
        return new DateTimeOffset(value, TimeSpan.Zero);
      }
      if (value.Kind == DateTimeKind.Local) {
        return new DateTimeOffset(value, DateTimeOffset.Now.Offset);
      }
      return new DateTimeOffset(value.Ticks, TimeSpan.Zero);
    }


    // Constructors

    public DateTimeRoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      baseDateTimeTicks = provider.BaseTime.Ticks;
    }
  }
}