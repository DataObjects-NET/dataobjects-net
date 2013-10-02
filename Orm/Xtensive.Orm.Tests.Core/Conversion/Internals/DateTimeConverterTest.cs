// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.25

using System;
using System.Globalization;
using NUnit.Framework;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.Conversion
{
  [TestFixture]
  public class DateTimeConverterTest : ConverterTestBase
  {
    private readonly DateTime[] constants = { DateTime.Now, new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Local),
                               new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                               new DateTime(0x7FFFFFFF), new DateTime(223372036854714932),
                               new DateTime(0xFFFF), new DateTime(3155378975999999999),
                               DateTime.MinValue, DateTime.MaxValue};
    private const int iterationCount = 100;

    [Test]
    public void Test()
    {
      TestLog.Info("{0}", DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff %K", CultureInfo.InvariantCulture));
      IInstanceGenerator<DateTime> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<DateTime>();
      Random random = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
      for (int i = 0; i < iterationCount * 1; i++)
        TestLog.Info("{0}", generator.GetInstance(random).ToString("yyyy/MM/dd hh:mm:ss.fff %K", CultureInfo.InvariantCulture));
    }

    [Test]
    public void StringTest()
    {
      IInstanceGenerator<DateTime> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<DateTime>();
      Random random = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
      for (int i = 0; i < iterationCount * 100; i++)
        OneValueTest<DateTime, string>(generator.GetInstance(random), 1);
    }

    [Test]
    public void CombinedTest()
    {
      foreach (DateTime constant in constants)
        OneValueTest<DateTime, bool>(constant, iterationCount);
      foreach (DateTime constant in constants)
        OneValueTest<DateTime, byte>(constant, iterationCount);
      foreach (DateTime constant in constants)
        OneValueTest<DateTime, sbyte>(constant, iterationCount);
      foreach (DateTime constant in constants)
        OneValueTest<DateTime, short>(constant, iterationCount);
      foreach (DateTime constant in constants)
        OneValueTest<DateTime, ushort>(constant, iterationCount);
      foreach (DateTime constant in constants)
        OneValueTest<DateTime, int>(constant, iterationCount);
      foreach (DateTime constant in constants)
        OneValueTest<DateTime, uint>(constant, iterationCount);
      foreach (DateTime constant in constants)
        OneValueTest<DateTime, long>(constant, iterationCount);
      foreach (DateTime constant in constants)
        OneValueTest<DateTime, ulong>(constant, iterationCount);
      foreach (DateTime constant in constants)
        OneValueTest<DateTime, float>(constant, iterationCount);
      foreach (DateTime constant in constants)
        OneValueTest<DateTime, double>(constant, iterationCount);
      foreach (DateTime constant in constants)
        OneValueTest<DateTime, decimal>(constant, iterationCount);
      foreach (DateTime constant in constants)
        OneValueTest<DateTime, DateTime>(constant, iterationCount);
      foreach (DateTime constant in constants)
        OneValueTest<DateTime, Guid>(constant, iterationCount);
      foreach (DateTime constant in constants)
        OneValueTest<DateTime, string>(constant, iterationCount);
      foreach (DateTime constant in constants)
        OneValueTest<DateTime, char>(constant, iterationCount);
    }
  }
}
