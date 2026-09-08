// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.25

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Core.Conversion
{
  public class DateOnlyConverterTest : ConverterTestBase<DateOnly>
  {
    private readonly DateOnly[] constants = {
      DateOnly.FromDateTime(DateTime.Now.Date),
      new DateOnly(2000, 1, 1),
      DateOnly.FromDayNumber(0xFFFFF),
      DateOnly.FromDayNumber(2237236),
      DateOnly.FromDayNumber(0xFFFF),
      DateOnly.FromDayNumber(3153599),
      DateOnly.MinValue, DateOnly.MaxValue
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
    protected override DateOnly[] Constants => constants;

    /// <inheritdoc/>
    protected override HashSet<Type> AllowedTargetTypes => allowedTargetTypes;
  }
}
