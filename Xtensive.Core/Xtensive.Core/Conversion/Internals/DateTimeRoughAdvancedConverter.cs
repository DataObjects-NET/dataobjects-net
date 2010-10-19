// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.08

using System;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class DateTimeRoughAdvancedConverter :
    RoughAdvancedConverterBase,
    IAdvancedConverter<DateTime, float>,
    IAdvancedConverter<DateTime, double>
  {
    private readonly long baseDateTimeTicks;

    float IAdvancedConverter<DateTime, float>.Convert(DateTime value)
    {
      return Convert.ToSingle(value.Ticks - baseDateTimeTicks);
    }

    double IAdvancedConverter<DateTime, double>.Convert(DateTime value)
    {
      return Convert.ToDouble(value.Ticks - baseDateTimeTicks);
    }


    // Constructors

    public DateTimeRoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      baseDateTimeTicks = provider.BaseTime.Ticks;
    }
  }
}