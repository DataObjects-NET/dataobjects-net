// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.26

using System;
using NUnit.Framework;
using Xtensive.Testing;

namespace Xtensive.Tests.Conversion
{
  [TestFixture]
  public class StringConverterTest : ConverterTestBase
  {
    private readonly string[] numericConstants = {
      "0x7FFFFFFFFFFF1234", "123", "012", "0x7F", "0x0a", "0xFF", "0x7FFFFFFF", "-9223372036854714932",
      "0xFFFF", "0x7FFF", "0xFFFFFFFF", "0xFFFFFFFFFFFFFF", "-72057594037927935", "", "0", "\n", "2.5", "abcd"
    };

    private readonly string[] fractionalConstants = {
      "2.5", "1.6E-308d", "234.14748366", "-0.4503599627370496",
    };
    private readonly string[] guidConstants = { "12345678-9012-3456-78990-123456789022" };
    private readonly string[] booleanConstants = {"false", "FALSE", "true", "tRue"};
    private readonly string[] dateTimeConstants = {};
    private const int iterationCount = 100;

    [Test]
    public void BooleanTest()
    {
      foreach (string constant in booleanConstants)
        OneValueTest<string, bool>(constant, iterationCount);
    }

    [Test]
    [Explicit]
    public void BooleanTestWithLog()
    {
      foreach (string constant in booleanConstants)
        OneValueTest<string, bool>(constant, iterationCount);
    }

    [Test]
    public void NumericTest()
    {
      foreach (string constant in numericConstants)
        OneValueTest<string, byte>(constant, iterationCount);
      foreach (string constant in numericConstants)
        OneValueTest<string, sbyte>(constant, iterationCount);
      foreach (string constant in numericConstants)
        OneValueTest<string, short>(constant, iterationCount);
      foreach (string constant in numericConstants)
        OneValueTest<string, ushort>(constant, iterationCount);
      foreach (string constant in numericConstants)
        OneValueTest<string, int>(constant, iterationCount);
      foreach (string constant in numericConstants)
        OneValueTest<string, uint>(constant, iterationCount);
      foreach (string constant in numericConstants)
        OneValueTest<string, long>(constant, iterationCount);
      foreach (string constant in numericConstants)
        OneValueTest<string, ulong>(constant, iterationCount);
    }

    [Test]
    public void FractionalTest()
    {
      foreach (string constant in fractionalConstants)
        OneValueTest<string, float>(constant, iterationCount);
      foreach (string constant in fractionalConstants)
        OneValueTest<string, double>(constant, iterationCount);
      foreach (string constant in fractionalConstants)
        OneValueTest<string, decimal>(constant, iterationCount);
    }

    [Test]
    public void DateTimeTest()
    {
      foreach (string constant in dateTimeConstants)
        OneValueTest<string, DateTime>(constant, iterationCount);
    }

    [Test]
    public void GuidTest()
    {
      foreach (string constant in guidConstants)
        OneValueTest<string, Guid>(constant, iterationCount);
    }

    [Test]
    public void StringTest()
    {
      IInstanceGenerator<string> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<string>();
      Random random = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
      for (int i = 0; i < iterationCount * 1000; i++)
        OneValueTest<string, string>(generator.GetInstance(random), 1);
    }
  }
}
