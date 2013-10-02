// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.25

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.Conversion
{
  [TestFixture]
  public class DoubleConverterTest : ConverterTestBase
  {
    private readonly double[] constants = {
      0x7FFFFFFFFFFF1234, 0, 123, 0x7F, 0xFF, 0x7FFFFFFF, -9223372036854714932,
      0xFFFF, 0x7FFF, 0xFFFFFFFF, 0xFFFFFFFFFFFFFF, -72057594037927935, double.MinValue,
      double.MaxValue, double.NegativeInfinity, double.PositiveInfinity, double.Epsilon, -double.Epsilon,
      2.14748366444444444444444444444444444444444444444444444444444444444444444444444444444444444444E-323d,
      2.14748366E+11d, 2.14748366E+12d, 2.14748366E+13d, 2.14748366E+14d, 2.14748366E+15d, 2.14748366E+16d,
      1.6E+308d, 2.14748366E+37d, 0.4503599627370496
    };
    private const int iterationCount = 100;

    [Test]
    public void StringTest()
    {
      IInstanceGenerator<double> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<double>();
      Random random = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
      for (int i = 0; i < iterationCount * 10000; i++)
        OneValueTest<double, string>(generator.GetInstance(random), 1);
    }

    [Test]
    public void CombinedTest()
    {
      foreach (double constant in constants)
        OneValueTest<double, bool>(constant, iterationCount);
      foreach (double constant in constants)
        OneValueTest<double, byte>(constant, iterationCount);
      foreach (double constant in constants)
        OneValueTest<double, sbyte>(constant, iterationCount);
      foreach (double constant in constants)
        OneValueTest<double, short>(constant, iterationCount);
      foreach (double constant in constants)
        OneValueTest<double, ushort>(constant, iterationCount);
      foreach (double constant in constants)
        OneValueTest<double, int>(constant, iterationCount);
      foreach (double constant in constants)
        OneValueTest<double, uint>(constant, iterationCount);
      foreach (double constant in constants)
        OneValueTest<double, long>(constant, iterationCount);
      foreach (double constant in constants)
        OneValueTest<double, ulong>(constant, iterationCount);
      foreach (double constant in constants)
        OneValueTest<double, float>(constant, iterationCount);
      foreach (double constant in constants)
        OneValueTest<double, double>(constant, iterationCount);
      foreach (double constant in constants)
        OneValueTest<double, decimal>(constant, iterationCount);
      foreach (double constant in constants)
        OneValueTest<double, DateTime>(constant, iterationCount);
      foreach (double constant in constants)
        OneValueTest<double, Guid>(constant, iterationCount);
      foreach (double constant in constants)
        OneValueTest<double, string>(constant, iterationCount);
      foreach (double constant in constants)
        OneValueTest<double, char>(constant, iterationCount);
    }
  }
}
