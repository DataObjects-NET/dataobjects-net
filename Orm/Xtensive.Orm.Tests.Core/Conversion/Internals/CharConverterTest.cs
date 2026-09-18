// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Roman Churakov
// Created:    2008.01.28

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Core.Conversion
{
  public class CharConverterTest : ConverterTestBase<char>
  {
    private readonly char[] constants = { 'A', '\0', '0', (char)123, (char)0x7F, (char)0xFF, (char)0x7FFF, (char)0xFFFF, 'к'};
    private readonly HashSet<Type> allowedTargetTypes = new() {
      // strict conversions
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
      typeof(decimal),
      typeof(string),
    };

    /// <inheritdoc/>
    protected override char[] Constants => constants;

    /// <inheritdoc/>
    protected override HashSet<Type> AllowedTargetTypes => allowedTargetTypes;


    [Test]
    public void AdditionalFloatTest()
    {
      IInstanceGenerator<char> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<char>();
      Random random = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
      for (int i = 0; i < IterationCount * 100; i++)
        OneValueTest<char, float>(generator.GetInstance(random), 1);
    }

    [Test]
    public void AddtionalDoubleTest()
    {
      IInstanceGenerator<char> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<char>();
      Random random = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
      for (int i = 0; i < IterationCount * 100; i++)
        OneValueTest<char, double>(generator.GetInstance(random), 1);
    }

    [Test]
    public void AdditionalDecimalTest()
    {
      IInstanceGenerator<char> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<char>();
      Random random = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
      for (int i = 0; i < IterationCount * 100; i++)
        OneValueTest<char, decimal>(generator.GetInstance(random), 1);
    }
  }
}
