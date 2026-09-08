// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Roman Churakov
// Created:    2008.01.25

using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Core.Conversion
{
  public class DateTimeConverterTest : ConverterTestBase<DateTime> 
  {
    private readonly DateTime[] constants = { DateTime.Now, new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Local),
      new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
      new DateTime(0x7FFFFFFF), new DateTime(223372036854714932),
      new DateTime(0xFFFF), new DateTime(3155378975999999999),
      DateTime.MinValue, DateTime.MaxValue};
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
      typeof(decimal),
      typeof(DateTime),
      typeof(TimeSpan),
      typeof(string),
      // rough conversions
      typeof(float),
      typeof(double),
    };


    /// <inheritdoc/>
    protected override DateTime[] Constants => constants;

    /// <inheritdoc/>
    protected override HashSet<Type> AllowedTargetTypes => allowedTargetTypes;


    [Test]
    public void AdditionalStringTest()
    {
      if (UseLog)
        TestLog.Info($" Format of values {DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff %K", CultureInfo.InvariantCulture)}");
      IInstanceGenerator<DateTime> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<DateTime>();
      Random random = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
      for (int i = 0; i < IterationCount * 100; i++) {
        var value = generator.GetInstance(random);
        if (UseLog)
          TestLog.Info($"Trying to convert '{generator.GetInstance(random).ToString("yyyy/MM/dd hh:mm:ss.fff %K", CultureInfo.InvariantCulture)}'");
        OneValueTest<DateTime, string>(value, 1);
      }
    }
  }
}
