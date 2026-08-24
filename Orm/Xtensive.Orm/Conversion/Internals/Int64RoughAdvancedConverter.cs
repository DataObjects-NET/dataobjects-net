// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.08

using System;

namespace Xtensive.Conversion
{
  internal class Int64RoughAdvancedConverter:
    RoughAdvancedConverterBase,
    IAdvancedConverter<long, bool>,
    IAdvancedConverter<long, float>,
    IAdvancedConverter<long, double>
  {
    bool IAdvancedConverter<long, bool>.Convert(long value) => Convert.ToBoolean(value);

    float IAdvancedConverter<long, float>.Convert(long value) => Convert.ToSingle(value);

    double IAdvancedConverter<long, double>.Convert(long value) => Convert.ToDouble(value);


    // Constructors

    public Int64RoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}