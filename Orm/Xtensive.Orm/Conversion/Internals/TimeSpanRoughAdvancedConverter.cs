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
    IAdvancedConverter<TimeSpan, double>
  {
    float IAdvancedConverter<TimeSpan, float>.Convert(TimeSpan value) => Convert.ToSingle(value.Ticks);

    double IAdvancedConverter<TimeSpan, double>.Convert(TimeSpan value) => Convert.ToDouble(value.Ticks);


    // Constructors

    public TimeSpanRoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}