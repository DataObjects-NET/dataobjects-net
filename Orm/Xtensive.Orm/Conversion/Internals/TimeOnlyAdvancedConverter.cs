// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Globalization;

namespace Xtensive.Conversion
{
  internal class TimeOnlyAdvancedConverter :
    StrictAdvancedConverterBase<TimeOnly>,
    IAdvancedConverter<TimeOnly, long>,
    IAdvancedConverter<TimeOnly, ulong>,
    IAdvancedConverter<TimeOnly, decimal>,
    IAdvancedConverter<TimeOnly, TimeSpan>,
    IAdvancedConverter<TimeOnly, string>
  {
    long IAdvancedConverter<TimeOnly, long>.Convert(TimeOnly value) => value.Ticks;

    ulong IAdvancedConverter<TimeOnly, ulong>.Convert(TimeOnly value)
    {
      checked {
        return (ulong) (value.Ticks);
      }
    }

    decimal IAdvancedConverter<TimeOnly, decimal>.Convert(TimeOnly value) => System.Convert.ToDecimal(value.Ticks);

    TimeSpan IAdvancedConverter<TimeOnly, TimeSpan>.Convert(TimeOnly value) => value.ToTimeSpan();

    string IAdvancedConverter<TimeOnly, string>.Convert(TimeOnly value)
      => value.ToString("hh:mm:ss.fffffff tt K ", CultureInfo.InvariantCulture);


    // Constructors

    public TimeOnlyAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}