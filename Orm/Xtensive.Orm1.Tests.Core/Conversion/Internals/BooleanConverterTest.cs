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
  public class BooleanConverterTest : ConverterTestBase
  {
    private readonly bool[] constants = { false, true };
    private const int iterationCount = 100;

    [Test]
    public void CombinedTest()
    {
      foreach (bool constant in constants)
        OneValueTest<bool, bool>(constant, iterationCount);
      foreach (bool constant in constants)
        OneValueTest<bool, byte>(constant, iterationCount);
      foreach (bool constant in constants)
        OneValueTest<bool, sbyte>(constant, iterationCount);
      foreach (bool constant in constants)
        OneValueTest<bool, short>(constant, iterationCount);
      foreach (bool constant in constants)
        OneValueTest<bool, ushort>(constant, iterationCount);
      foreach (bool constant in constants)
        OneValueTest<bool, int>(constant, iterationCount);
      foreach (bool constant in constants)
        OneValueTest<bool, uint>(constant, iterationCount);
      foreach (bool constant in constants)
        OneValueTest<bool, long>(constant, iterationCount);
      foreach (bool constant in constants)
        OneValueTest<bool, ulong>(constant, iterationCount);
      foreach (bool constant in constants)
        OneValueTest<bool, float>(constant, iterationCount);
      foreach (bool constant in constants)
        OneValueTest<bool, double>(constant, iterationCount);
      foreach (bool constant in constants)
        OneValueTest<bool, decimal>(constant, iterationCount);
      foreach (bool constant in constants)
        OneValueTest<bool, DateTime>(constant, iterationCount);
      foreach (bool constant in constants)
        OneValueTest<bool, Guid>(constant, iterationCount);
      foreach (bool constant in constants)
        OneValueTest<bool, string>(constant, iterationCount);
      foreach (bool constant in constants)
        OneValueTest<bool, char>(constant, iterationCount);
    }
  }
}
