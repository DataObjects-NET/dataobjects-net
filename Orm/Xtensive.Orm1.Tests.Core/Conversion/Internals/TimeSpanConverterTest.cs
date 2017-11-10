// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.11.12

using System;
using System.Globalization;
using NUnit.Framework;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.Conversion
{
  [TestFixture]
  public class TimeSpanConverterTest : ConverterTestBase
  {
    private readonly TimeSpan[] constants = { new TimeSpan( 1 ),
                               new TimeSpan( 10, 20, 30, 40, 50 ),
                               new TimeSpan( 1111, 2222, 3333, 4444, 5555 ),
                               TimeSpan.FromDays( 20.84745602 ),
                               new TimeSpan(0x7FFFFFFF), new TimeSpan(223372036854714932),
                               new TimeSpan(0xFFFF), new TimeSpan(3155378975999999999),
                               TimeSpan.MinValue, TimeSpan.MaxValue};
    private const int iterationCount = 100;

    [Test]
    public void CombinedTest()
    {
      foreach (TimeSpan constant in constants)
        OneValueTest<TimeSpan, bool>(constant, iterationCount);
      foreach (TimeSpan constant in constants)
        OneValueTest<TimeSpan, byte>(constant, iterationCount);
      foreach (TimeSpan constant in constants)
        OneValueTest<TimeSpan, sbyte>(constant, iterationCount);
      foreach (TimeSpan constant in constants)
        OneValueTest<TimeSpan, short>(constant, iterationCount);
      foreach (TimeSpan constant in constants)
        OneValueTest<TimeSpan, ushort>(constant, iterationCount);
      foreach (TimeSpan constant in constants)
        OneValueTest<TimeSpan, int>(constant, iterationCount);
      foreach (TimeSpan constant in constants)
        OneValueTest<TimeSpan, uint>(constant, iterationCount);
      foreach (TimeSpan constant in constants)
        OneValueTest<TimeSpan, long>(constant, iterationCount);
      foreach (TimeSpan constant in constants)
        OneValueTest<TimeSpan, ulong>(constant, iterationCount);
      foreach (TimeSpan constant in constants)
        OneValueTest<TimeSpan, float>(constant, iterationCount);
      foreach (TimeSpan constant in constants)
        OneValueTest<TimeSpan, double>(constant, iterationCount);
      foreach (TimeSpan constant in constants)
        OneValueTest<TimeSpan, decimal>(constant, iterationCount);
      foreach (TimeSpan constant in constants)
        OneValueTest<TimeSpan, TimeSpan>(constant, iterationCount);
      foreach (TimeSpan constant in constants)
        OneValueTest<TimeSpan, Guid>(constant, iterationCount);
      foreach (TimeSpan constant in constants)
        OneValueTest<TimeSpan, string>(constant, iterationCount);
      foreach (TimeSpan constant in constants)
        OneValueTest<TimeSpan, char>(constant, iterationCount);
    }
  }
}
