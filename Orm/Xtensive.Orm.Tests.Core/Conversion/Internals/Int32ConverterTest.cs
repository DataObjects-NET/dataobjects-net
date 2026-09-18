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
  public class Int32ConverterTest : ConverterTestBase<int>
  {
    private readonly int[] constants = { 0, 1, 123, 0x7F, 0xFF, 0x7FFFFFFF, -2147483648, 0xFFFF, 0x7FFF };
    private readonly HashSet<Type> allowedTargetTypes = new HashSet<Type>() {
      // strict conversions
      typeof (byte),
      typeof (sbyte),
      typeof (short),
      typeof (ushort),
      typeof (uint),
      typeof (long),
      typeof (ulong),
      typeof (double),
      typeof (decimal),
      typeof (DateTime),
      typeof (DateOnly),
      typeof (TimeSpan),
      typeof (string),
      typeof (char),
      // rough coversions
      typeof (bool),
      typeof (float),
    };

    /// <inheritdoc/>
    protected override int[] Constants => constants;

    /// <inheritdoc/>
    protected override HashSet<Type> AllowedTargetTypes => allowedTargetTypes;
  }
}
