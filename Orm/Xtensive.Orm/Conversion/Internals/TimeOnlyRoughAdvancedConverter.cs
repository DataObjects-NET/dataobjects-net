// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;

namespace Xtensive.Conversion
{
  internal class TimeOnlyRoughAdvancedConverter :
    RoughAdvancedConverterBase,
    IAdvancedConverter<TimeOnly, float>,
    IAdvancedConverter<TimeOnly, double>
  {
    float IAdvancedConverter<TimeOnly, float>.Convert(TimeOnly value) => Convert.ToSingle(value.Ticks);
    double IAdvancedConverter<TimeOnly, double>.Convert(TimeOnly value) => Convert.ToDouble(value.Ticks);


    // Constructors

    public TimeOnlyRoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}