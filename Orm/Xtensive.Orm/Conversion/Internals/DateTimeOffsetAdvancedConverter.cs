// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Globalization;

namespace Xtensive.Conversion
{
  internal class DateTimeOffsetAdvancedConverter :
    StrictAdvancedConverterBase<DateTimeOffset>,
    IAdvancedConverter<DateTimeOffset, string>
  {
    string IAdvancedConverter<DateTimeOffset, string>.Convert(DateTimeOffset value)
      => value.ToString("yyyy/MM/dd hh:mm:ss.fffffff tt zzz", CultureInfo.InvariantCulture);

    public DateTimeOffsetAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}