// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Globalization;

namespace Xtensive.Conversion
{
  internal class DateOnlyAdvancedConverter :
    StrictAdvancedConverterBase<DateOnly>,
    IAdvancedConverter<DateOnly, int>,
    IAdvancedConverter<DateOnly, uint>,
    IAdvancedConverter<DateOnly, long>,
    IAdvancedConverter<DateOnly, ulong>,
    IAdvancedConverter<DateOnly, decimal>,
    IAdvancedConverter<DateOnly, DateTime>,
    IAdvancedConverter<DateOnly, TimeSpan>,
    IAdvancedConverter<DateOnly, string>
  {
    int IAdvancedConverter<DateOnly, int>.Convert(DateOnly value)
    {
      checked {
        return value.DayNumber;
      }
    }

    uint IAdvancedConverter<DateOnly, uint>.Convert(DateOnly value)
    {
      checked {
        return (uint) value.DayNumber;
      }
    }

    long IAdvancedConverter<DateOnly, long>.Convert(DateOnly value)
    {
      checked {
        return value.DayNumber;
      }
    }

    ulong IAdvancedConverter<DateOnly, ulong>.Convert(DateOnly value)
    {
      checked {
        return (ulong) value.DayNumber;
      }
    }

    decimal IAdvancedConverter<DateOnly, decimal>.Convert(DateOnly value) => System.Convert.ToDecimal(value.DayNumber);

    DateTime IAdvancedConverter<DateOnly, DateTime>.Convert(DateOnly value) => value.ToDateTime(TimeOnly.MinValue);

    TimeSpan IAdvancedConverter<DateOnly, TimeSpan>.Convert(DateOnly value)
      => TimeSpan.FromDays(value.DayNumber);

    string IAdvancedConverter<DateOnly, string>.Convert(DateOnly value)
      => value.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);

    public DateOnlyAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}