// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.25

using System;
using NUnit.Framework;
using Xtensive.Conversion;
using Xtensive.Reflection;

namespace Xtensive.Orm.Tests.Core.Conversion
{
  [TestFixture]
  public class Int64ConverterTest : ConverterTestBase
  {
    private readonly long[] constants = {
      0x7FFFFFFFFFFF1234, 0, 123, 0x7F, 0xFF, 0x7FFFFFFF, -9223372036854714932,
      0xFFFF, 0x7FFF, 0xFFFFFFFF, 0xFFFFFFFFFFFFFF, -72057594037927935
    };
    private const int iterationCount = 100;

    [Test]
    public void MainTest ()
    {
      foreach (long constant in constants)
        OneValueTest<long, bool>(constant, iterationCount);
      foreach (long constant in constants)
        OneValueTest<long, byte>(constant, iterationCount);
      foreach (long constant in constants)
        OneValueTest<long, sbyte>(constant, iterationCount);
      foreach (long constant in constants)
        OneValueTest<long, short>(constant, iterationCount);
      foreach (long constant in constants)
        OneValueTest<long, ushort>(constant, iterationCount);
      foreach (long constant in constants)
        OneValueTest<long, int>(constant, iterationCount);
      foreach (long constant in constants)
        OneValueTest<long, uint>(constant, iterationCount);
      foreach (long constant in constants)
        OneValueTest<long, long>(constant, iterationCount);
      foreach (long constant in constants)
        OneValueTest<long, ulong>(constant, iterationCount);
      foreach (long constant in constants)
        OneValueTest<long, float>(constant, iterationCount);
      foreach (long constant in constants)
        OneValueTest<long, double>(constant, iterationCount);
      foreach (long constant in constants)
        OneValueTest<long, decimal>(constant, iterationCount);
      foreach (long constant in constants)
        OneValueTest<long, DateTime>(constant, iterationCount);
      foreach (long constant in constants)
        OneValueTest<long, Guid>(constant, iterationCount);
      foreach (long constant in constants)
        OneValueTest<long, string>(constant, iterationCount);
      foreach (long constant in constants)
        OneValueTest<long, char>(constant, iterationCount);
    }
  }
}
