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
  public class DecimalConverterTest : ConverterTestBase
  {
    private readonly decimal[] constants = { 0x7FFFFFFFFFFF1234, 0, 123, 0x7F, 0xFF, 0x7FFFFFFF, -9223372036854714932,
                               0xFFFF, 0x7FFF, 0xFFFFFFFF, 0xFFFFFFFFFFFFFF, -72057594037927935, decimal.MinValue, decimal.MaxValue,
                               (decimal)2.5, (decimal)1.6E-308d, (decimal)234.14748366, (decimal)-0.4503599627370496};
    private const int iterationCount = 100;

    [Test]
    public void StringTest()
    {
      IInstanceGenerator<decimal> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<decimal>();
      Random random = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
      for (int i = 0; i < iterationCount*1000; i++)
        OneValueTest<decimal, string>(generator.GetInstance(random), 1);
    }

    [Test]
    public void CombinedTest()
    {
      foreach (decimal constant in constants)
        OneValueTest<decimal, bool>(constant, iterationCount);
      foreach (decimal constant in constants)
        OneValueTest<decimal, byte>(constant, iterationCount);
      foreach (decimal constant in constants)
        OneValueTest<decimal, sbyte>(constant, iterationCount);
      foreach (decimal constant in constants)
        OneValueTest<decimal, short>(constant, iterationCount);
      foreach (decimal constant in constants)
        OneValueTest<decimal, ushort>(constant, iterationCount);
      foreach (decimal constant in constants)
        OneValueTest<decimal, int>(constant, iterationCount);
      foreach (decimal constant in constants)
        OneValueTest<decimal, uint>(constant, iterationCount);
      foreach (decimal constant in constants)
        OneValueTest<decimal, long>(constant, iterationCount);
      foreach (decimal constant in constants)
        OneValueTest<decimal, ulong>(constant, iterationCount);
      foreach (decimal constant in constants)
        OneValueTest<decimal, float>(constant, iterationCount);
      foreach (decimal constant in constants)
        OneValueTest<decimal, double>(constant, iterationCount);
      foreach (decimal constant in constants)
        OneValueTest<decimal, decimal>(constant, iterationCount);
      foreach (decimal constant in constants)
        OneValueTest<decimal, DateTime>(constant, iterationCount);
      foreach (decimal constant in constants)
        OneValueTest<decimal, Guid>(constant, iterationCount);
      foreach (decimal constant in constants)
        OneValueTest<decimal, string>(constant, iterationCount);
      foreach (decimal constant in constants)
        OneValueTest<decimal, char>(constant, iterationCount);
    }
  }
}
