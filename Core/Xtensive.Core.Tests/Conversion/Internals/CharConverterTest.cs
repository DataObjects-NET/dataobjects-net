// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.28

using System;
using NUnit.Framework;
using Xtensive.Testing;

namespace Xtensive.Tests.Conversion
{
  [TestFixture]
  public class CharConverterTest : ConverterTestBase
  {
    private readonly char[] constants = { 'A', '\0', '0', (char)123, (char)0x7F, (char)0xFF, (char)0x7FFF, (char)0xFFFF, 'к'};
    private const int iterationCount = 100;

    [Test]
    public void SingleTest()
    {
      IInstanceGenerator<char> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<char>();
      Random random = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
      for (int i = 0; i < iterationCount * 100; i++)
        OneValueTest<char, float>(generator.GetInstance(random), 1);
    }

    [Test]
    public void DoubleTest()
    {
      IInstanceGenerator<char> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<char>();
      Random random = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
      for (int i = 0; i < iterationCount * 100; i++)
        OneValueTest<char, double>(generator.GetInstance(random), 1);
    }

    [Test]
    public void DecimalTest()
    {
      IInstanceGenerator<char> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<char>();
      Random random = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
      for (int i = 0; i < iterationCount * 100; i++)
        OneValueTest<char, decimal>(generator.GetInstance(random), 1);
    }

    [Test]
    public void CombinedTest()
    {
      foreach (char constant in constants)
        OneValueTest<char, bool>(constant, iterationCount);
      foreach (char constant in constants)
        OneValueTest<char, byte>(constant, iterationCount);
      foreach (char constant in constants)
        OneValueTest<char, sbyte>(constant, iterationCount);
      foreach (char constant in constants)
        OneValueTest<char, short>(constant, iterationCount);
      foreach (char constant in constants)
        OneValueTest<char, ushort>(constant, iterationCount);
      foreach (char constant in constants)
        OneValueTest<char, int>(constant, iterationCount);
      foreach (char constant in constants)
        OneValueTest<char, uint>(constant, iterationCount);
      foreach (char constant in constants)
        OneValueTest<char, long>(constant, iterationCount);
      foreach (char constant in constants)
        OneValueTest<char, ulong>(constant, iterationCount);
      foreach (char constant in constants)
        OneValueTest<char, float>(constant, iterationCount);
      foreach (char constant in constants)
        OneValueTest<char, double>(constant, iterationCount);
      foreach (char constant in constants)
        OneValueTest<char, decimal>(constant, iterationCount);
      foreach (char constant in constants)
        OneValueTest<char, DateTime>(constant, iterationCount);
      foreach (char constant in constants)
        OneValueTest<char, Guid>(constant, iterationCount);
      foreach (char constant in constants)
        OneValueTest<char, string>(constant, iterationCount);
      foreach (char constant in constants)
        OneValueTest<char, char>(constant, iterationCount);
    }
  }
}
