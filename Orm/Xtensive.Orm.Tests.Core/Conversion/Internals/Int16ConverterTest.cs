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
  public class Int16ConverterTest : ConverterTestBase<short>
  {
    private readonly short[] constants = { 0, -1, 1, 123, 0x7F, 0xFF, 0x7FFF, -32768 };
    private readonly HashSet<Type> allowedTargetTypes = new HashSet<Type>() {
      // strict conversions
      typeof(byte),
      typeof(sbyte),
      typeof(ushort),
      typeof(int),
      typeof(uint),
      typeof(long),
      typeof(ulong),
      typeof(float),
      typeof(double),
      typeof(decimal),
      typeof(DateTime),
      typeof(TimeSpan),
      typeof(string),
      typeof(char),
      // rough coversions
      typeof(bool),
    };

    /// <inheritdoc/>
    protected override short[] Constants => constants;

    /// <inheritdoc/>
    protected override HashSet<Type> AllowedTargetTypes => allowedTargetTypes;
  }
}
