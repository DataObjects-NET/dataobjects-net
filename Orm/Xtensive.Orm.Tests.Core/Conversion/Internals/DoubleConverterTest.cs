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
  public class DoubleConverterTest : ConverterTestBase<double>
  {
    private readonly double[] constants = {
      0x7FFFFFFFFFFF1234, 0, 123, 0x7F, 0xFF, 0x7FFFFFFF, -9223372036854714932,
      0xFFFF, 0x7FFF, 0xFFFFFFFF, 0xFFFFFFFFFFFFFF, -72057594037927935, double.MinValue,
      double.MaxValue, double.NegativeInfinity, double.PositiveInfinity, double.Epsilon, -double.Epsilon,
      2.14748366444444444444444444444444444444444444444444444444444444444444444444444444444444444444E-323d,
      2.14748366E+11d, 2.14748366E+12d, 2.14748366E+13d, 2.14748366E+14d, 2.14748366E+15d, 2.14748366E+16d,
      1.6E+308d, 2.14748366E+37d, 0.4503599627370496
    };
    private readonly HashSet<Type> allowedTargetTypes = new HashSet<Type>() {
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
      typeof(decimal),
      typeof(DateTime),
      typeof(DateOnly),
      typeof(TimeOnly),
      typeof(TimeSpan),
      typeof(char),
    };

    /// <inheritdoc/>
    protected override double[] Constants => constants;

    /// <inheritdoc/>
    protected override HashSet<Type> AllowedTargetTypes => allowedTargetTypes;


    [Test]
    public void AdditionalStringTest()
    {
      IInstanceGenerator<double> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<double>();
      Random random = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
      for (int i = 0; i < IterationCount * 100; i++)
        OneValueTest<double, string>(generator.GetInstance(random), 1);
    }
  }
}
