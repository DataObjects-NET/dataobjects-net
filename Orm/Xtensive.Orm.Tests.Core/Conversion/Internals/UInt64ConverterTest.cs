// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Roman Churakov
// Created:    2008.01.25

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Core.Conversion
{
  public class UInt64ConverterTest : ConverterTestBase<ulong>
  {
    private readonly ulong[] constants = new ulong[] {
      0x7FFFFFFFFFFF1234, 0, 123, 0x7F, 0xFF, 0x7FFFFFFF, 0x800000000000EDCC,
      0xFFFF, 0x7FFF, 0xFFFFFFFF, 0xFFFFFFFFFFFFFF, 0xFFFFFFFFFFFF1234
    };

    private readonly HashSet<Type> allowedTargetTypes = new() {
      // strict conversions
      typeof(byte),
      typeof(sbyte),
      typeof(short),
      typeof(ushort),
      typeof(int),
      typeof(uint),
      typeof(long),
      typeof(decimal),
      typeof(DateTime),
      typeof(DateOnly),
      typeof(TimeOnly),
      typeof(TimeSpan),
      typeof(string),
      typeof(char),
      // rough coversions
      typeof(float),
      typeof(double),
      typeof(bool),
      typeof(DateTimeOffset)
    };

    /// <inheritdoc/>
    protected override ulong[] Constants => constants;

    /// <inheritdoc/>
    protected override HashSet<Type> AllowedTargetTypes => allowedTargetTypes;
  }
}
