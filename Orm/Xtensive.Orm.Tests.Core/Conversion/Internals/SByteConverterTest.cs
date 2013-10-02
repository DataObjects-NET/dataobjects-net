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
  public class SByteConverterTest : ConverterTestBase
  {
    private readonly sbyte[] constants = { 0, -1, 1, 123, 0x7F, -128 };
    private const int iterationCount = 100;

    [Test]
    public void CombinedTest()
    {
      foreach (sbyte constant in constants)
        OneValueTest<sbyte, bool>(constant, iterationCount);
      foreach (sbyte constant in constants)
        OneValueTest<sbyte, byte>(constant, iterationCount);
      foreach (sbyte constant in constants)
        OneValueTest<sbyte, sbyte>(constant, iterationCount);
      foreach (sbyte constant in constants)
        OneValueTest<sbyte, short>(constant, iterationCount);
      foreach (sbyte constant in constants)
        OneValueTest<sbyte, ushort>(constant, iterationCount);
      foreach (sbyte constant in constants)
        OneValueTest<sbyte, int>(constant, iterationCount);
      foreach (sbyte constant in constants)
        OneValueTest<sbyte, uint>(constant, iterationCount);
      foreach (sbyte constant in constants)
        OneValueTest<sbyte, long>(constant, iterationCount);
      foreach (sbyte constant in constants)
        OneValueTest<sbyte, ulong>(constant, iterationCount);
      foreach (sbyte constant in constants)
        OneValueTest<sbyte, float>(constant, iterationCount);
      foreach (sbyte constant in constants)
        OneValueTest<sbyte, double>(constant, iterationCount);
      foreach (sbyte constant in constants)
        OneValueTest<sbyte, decimal>(constant, iterationCount);
      foreach (sbyte constant in constants)
        OneValueTest<sbyte, DateTime>(constant, iterationCount);
      foreach (sbyte constant in constants)
        OneValueTest<sbyte, Guid>(constant, iterationCount);
      foreach (sbyte constant in constants)
        OneValueTest<sbyte, string>(constant, iterationCount);
      foreach (sbyte constant in constants)
        OneValueTest<sbyte, char>(constant, iterationCount);
    }
  }
}
