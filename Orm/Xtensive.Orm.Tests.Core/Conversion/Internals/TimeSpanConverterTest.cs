// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Elena Vakhtina
// Created:    2008.11.12

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Core.Conversion
{
  public class TimeSpanConverterTest : ConverterTestBase<TimeSpan>
  {
    private readonly TimeSpan[] constants = new TimeSpan[] { new TimeSpan(1),
      new TimeSpan(10, 20, 30, 40, 50),
      new TimeSpan(1111, 2222, 3333, 4444, 5555),
      TimeSpan.FromDays(20.84745602),
      new TimeSpan(0x7FFFFFFF), new TimeSpan(223372036854714932),
      new TimeSpan(0xFFFF), new TimeSpan(3155378975999999999),
      TimeSpan.MinValue, TimeSpan.MaxValue
    };
    private readonly HashSet<Type> allowedTargetTypes = new HashSet<Type> {
      // strict conversions
      typeof(byte),
      typeof(sbyte),
      typeof(short),
      typeof(ushort),
      typeof(int),
      typeof(uint),
      typeof(long),
      typeof(ulong),
      typeof(decimal),
      typeof(TimeOnly),
      typeof(TimeSpan),
      typeof(string),
      // rough coversions
      typeof(float),
      typeof(double),
      typeof(DateOnly),
    };

    /// <inheritdoc/>
    protected override TimeSpan[] Constants => constants;

    /// <inheritdoc/>
    protected override HashSet<Type> AllowedTargetTypes => allowedTargetTypes;

  }
}
