// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;

namespace Xtensive.Conversion
{
  internal class DateOnlyRoughAdvancedConverter :
    RoughAdvancedConverterBase,
    IAdvancedConverter<DateOnly, float>,
    IAdvancedConverter<DateOnly, double>
  {
    float IAdvancedConverter<DateOnly, float>.Convert(DateOnly value) => Convert.ToSingle(value.DayNumber);
    double IAdvancedConverter<DateOnly, double>.Convert(DateOnly value) => Convert.ToDouble(value.DayNumber);


    // Constructors

    public DateOnlyRoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}