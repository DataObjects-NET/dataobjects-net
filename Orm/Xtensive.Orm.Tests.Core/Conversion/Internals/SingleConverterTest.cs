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
  public class SingleConverterTest : ConverterTestBase<float>
  {
    private readonly float[] constants = {
      0x7FFFFFFFFFFF1234, 0, 123, 0x7F, 0xFF, 0x7FFFFFFF, -9223372036854714932,
      0xFFFF, 0x7FFF, 0xFFFFFFFF, 0xFFFFFFFFFFFFFF, -72057594037927935, float.MinValue,
      float.MaxValue, float.NegativeInfinity, float.PositiveInfinity, float.Epsilon, -float.Epsilon,
      2.14748366E+09f, 2.14748366E+11f, 2.14748366E+12f, 2.14748366E+13f, 2.14748366E+14f,
      2.14748366E+15f, 2.14748366E+16f, 2.14748366E+38f, 2.14748366E+37f
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
      typeof(double),
      typeof(decimal),
      typeof(DateTime),
      typeof(TimeOnly),
      typeof(DateOnly),
      typeof(TimeSpan),
      typeof(char),
    };

    /// <inheritdoc/>
    protected override float[] Constants => constants;

    /// <inheritdoc/>
    protected override HashSet<Type> AllowedTargetTypes => allowedTargetTypes;

    [Test]
    public void AdditionalStringTest()
    {
      IInstanceGenerator<float> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<float>();
      Random random = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
      for (int i = 0; i < IterationCount * 100; i++)
        OneValueTest<float, string>(generator.GetInstance(random), 1);
    }

    // Proves that float-to-double is a rough conversion.
    [Test]
    public void AdditionalDoubleTest()
    {
      IInstanceGenerator<double > generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<double>();
      Random random = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
      for (int i = 0; i < IterationCount * 100; i++)
        OneValueTest<double, string>(generator.GetInstance(random), 1);
    }
  }
}
