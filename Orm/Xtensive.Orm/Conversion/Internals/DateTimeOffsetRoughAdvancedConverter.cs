// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;

namespace Xtensive.Conversion
{
  internal class DateTimeOffsetRoughAdvancedConverter :
    RoughAdvancedConverterBase,
    
    IAdvancedConverter<DateTimeOffset, long>,
    IAdvancedConverter<DateTimeOffset, ulong>,
    IAdvancedConverter<DateTimeOffset, decimal>,
    IAdvancedConverter<DateTimeOffset, DateTime>,
    IAdvancedConverter<DateTimeOffset, TimeSpan>
  {
    long IAdvancedConverter<DateTimeOffset, long>.Convert(DateTimeOffset value) => value.UtcTicks;
    ulong IAdvancedConverter<DateTimeOffset, ulong>.Convert(DateTimeOffset value) => (ulong)value.UtcTicks;
    decimal IAdvancedConverter<DateTimeOffset, decimal>.Convert(DateTimeOffset value) => value.UtcTicks;
    DateTime IAdvancedConverter<DateTimeOffset, DateTime>.Convert(DateTimeOffset value) => value.UtcDateTime;
    TimeSpan IAdvancedConverter<DateTimeOffset, TimeSpan>.Convert(DateTimeOffset value) => TimeSpan.FromTicks(value.UtcTicks);


    public DateTimeOffsetRoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}