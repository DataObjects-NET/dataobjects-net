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
  public class DecimalConverterTest : ConverterTestBase<decimal>
  {
    private readonly decimal[] constants = {
      0x7FFFFFFFFFFF1234, 0, 123, 0x7F, 0xFF, 0x7FFFFFFF, -9223372036854714932,
      0xFFFF, 0x7FFF, 0xFFFFFFFF, 0xFFFFFFFFFFFFFF, -72057594037927935, decimal.MinValue, decimal.MaxValue,
      (decimal)2.5, (decimal)1.6E-308d, (decimal)234.14748366, (decimal)-0.4503599627370496
    };

    private readonly HashSet<Type> allowedTargetTypes = new() {
      // strict conversions
      typeof(string),
      // rough coversions
      typeof(bool),
      typeof(byte),
      typeof(sbyte),
      typeof(short),
      typeof(ushort),
      typeof(int),
      typeof(uint),
      typeof(long),
      typeof(ulong),
      typeof(float),
      typeof(double),
      typeof(DateTime),
      typeof(DateOnly),
      typeof(TimeOnly),
      typeof(TimeSpan),
      typeof(char),
    };

    /// <inheritdoc/>
    protected override decimal[] Constants => constants;

    /// <inheritdoc/>
    protected override HashSet<Type> AllowedTargetTypes => allowedTargetTypes;

    [Test]
    public void AdditionaStringTest()
    {
      IInstanceGenerator<decimal> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<decimal>();
      Random random = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
      for (int i = 0; i < IterationCount * 100; i++)
        OneValueTest<decimal, string>(generator.GetInstance(random), 1);
    }
  }
}
