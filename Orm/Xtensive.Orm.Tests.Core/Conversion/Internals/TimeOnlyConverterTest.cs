// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Core.Conversion
{
  public class TimeOnlyConverterTest : ConverterTestBase<TimeOnly>
  {
    private const long SecondMultiplier =  1_0000000;
    private const long MinuteMultiplier = 60_0000000;
    private const long HourMultiplier = 3600_0000000;

    private readonly TimeOnly[] constants = {
      new TimeOnly(1, 15),
      new TimeOnly(1, 15, 18),
      new TimeOnly(1, 15, 18, 192, 021),
      new TimeOnly(18 * HourMultiplier),
      new TimeOnly(11 * HourMultiplier + 22 * MinuteMultiplier),
      new TimeOnly(9 * HourMultiplier + 22 * MinuteMultiplier + 46 * SecondMultiplier),
      new TimeOnly(3 * HourMultiplier + 22 * MinuteMultiplier + 46 * SecondMultiplier + 7848942),
      TimeOnly.MinValue, TimeOnly.MaxValue
    };

    private readonly HashSet<Type> allowedTargetTypes = new() {
      // strict conversions
      typeof(int),
      typeof(uint),
      typeof(long),
      typeof(ulong),
      typeof(decimal),
      typeof(DateTime),
      typeof(TimeSpan),
      typeof(string),
      // rough conversions
      typeof(float),
      typeof(double),
    };

    /// <inheritdoc/>
    protected override TimeOnly[] Constants => constants;

    /// <inheritdoc/>
    protected override HashSet<Type> AllowedTargetTypes => allowedTargetTypes;
  }
}
