// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.25

using System;
using NUnit.Framework;
using Xtensive.Testing;

namespace Xtensive.Tests.Conversion
{
  [TestFixture]
  public class SingleConverterTest : ConverterTestBase
  {
    private readonly float[] constants = {
      0x7FFFFFFFFFFF1234, 0, 123, 0x7F, 0xFF, 0x7FFFFFFF, -9223372036854714932,
      0xFFFF, 0x7FFF, 0xFFFFFFFF, 0xFFFFFFFFFFFFFF, -72057594037927935, float.MinValue,
      float.MaxValue, float.NegativeInfinity, float.PositiveInfinity, float.Epsilon, -float.Epsilon,
      2.14748366E+09f, 2.14748366E+11f, 2.14748366E+12f, 2.14748366E+13f, 2.14748366E+14f,
      2.14748366E+15f, 2.14748366E+16f, 2.14748366E+38f, 2.14748366E+37f
    };
    private const int iterationCount = 100;

    [Test]
    public void StringTest()
    {
      IInstanceGenerator<float> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<float>();
      Random random = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
      for (int i = 0; i < iterationCount * 10000; i++)
        OneValueTest<float, string>(generator.GetInstance(random), 1);
    }

    // Proves that float-to-double is a rough conversion.
    [Test]
    public void DoubleTest()
    {
      IInstanceGenerator<double > generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<double>();
      Random random = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
      for (int i = 0; i < iterationCount * 10000; i++)
        OneValueTest<double, string>(generator.GetInstance(random), 1);
    }

    [Test]
    public void CombinedTest()
    {
      foreach (float constant in constants)
        OneValueTest<float, bool>(constant, iterationCount);
      foreach (float constant in constants)
        OneValueTest<float, byte>(constant, iterationCount);
      foreach (float constant in constants)
        OneValueTest<float, sbyte>(constant, iterationCount);
      foreach (float constant in constants)
        OneValueTest<float, short>(constant, iterationCount);
      foreach (float constant in constants)
        OneValueTest<float, ushort>(constant, iterationCount);
      foreach (float constant in constants)
        OneValueTest<float, int>(constant, iterationCount);
      foreach (float constant in constants)
        OneValueTest<float, uint>(constant, iterationCount);
      foreach (float constant in constants)
        OneValueTest<float, long>(constant, iterationCount);
      foreach (float constant in constants)
        OneValueTest<float, ulong>(constant, iterationCount);
      foreach (float constant in constants)
        OneValueTest<float, float>(constant, iterationCount);
      foreach (float constant in constants)
        OneValueTest<float, double>(constant, iterationCount);
      foreach (float constant in constants)
        OneValueTest<float, decimal>(constant, iterationCount);
      foreach (float constant in constants)
        OneValueTest<float, DateTime>(constant, iterationCount);
      foreach (float constant in constants)
        OneValueTest<float, Guid>(constant, iterationCount);
      foreach (float constant in constants)
        OneValueTest<float, string>(constant, iterationCount);
      foreach (float constant in constants)
        OneValueTest<float, char>(constant, iterationCount);
    }
  }
}
