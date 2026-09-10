// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.25

using System;
using System.Collections.Generic;

namespace Xtensive.Orm.Tests.Core.Conversion
{
  public class DateTimeOffsetConverterTest : ConverterTestBase<DateTimeOffset>
  {
    private readonly DateTimeOffset[] constants = {
      new DateTimeOffset(DateTime.MinValue, TimeSpan.FromHours(-3)),
      new DateTimeOffset(DateTime.MaxValue, TimeSpan.FromHours(+3)),
      new DateTimeOffset(new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), TimeSpan.Zero),
      new DateTimeOffset(new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), TimeSpan.FromHours(-3)),
      new DateTimeOffset(new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), TimeSpan.FromHours(+3)),
      new DateTimeOffset(new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(2, 30, 0)),
      new DateTimeOffset(new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(2, 30, 0).Negate()),
      new DateTimeOffset(new DateTime(223372036854714932), TimeSpan.Zero),
      new DateTimeOffset(new DateTime(223372036854714932), TimeSpan.FromHours(-3)),
      new DateTimeOffset(new DateTime(223372036854714932), TimeSpan.FromHours(+3)),
      new DateTimeOffset(new DateTime(223372036854714932), new TimeSpan(2, 30, 0)),
      new DateTimeOffset(new DateTime(223372036854714932), new TimeSpan(2, 30, 0).Negate()),
      DateTimeOffset.Now, DateTimeOffset.UtcNow,
      DateTimeOffset.MinValue, DateTimeOffset.MaxValue
    };

    private readonly HashSet<Type> allowedTargetTypes = new() {
      // strict conversions
      typeof(string),
      // rough conversions
      typeof(long),
      typeof(ulong),
      typeof(decimal),
      typeof(DateTime),
      typeof(TimeSpan)
    };


    protected override DateTimeOffset[] Constants => constants;

    protected override HashSet<Type> AllowedTargetTypes => allowedTargetTypes;

    protected override int IterationCount => 10;
  }
}
