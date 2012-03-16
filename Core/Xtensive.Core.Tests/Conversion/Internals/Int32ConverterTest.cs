// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.25

using System;
using NUnit.Framework;
using Xtensive.Conversion;
using Xtensive.Reflection;

namespace Xtensive.Tests.Conversion
{
  [TestFixture]
  public class Int32ConverterTest : ConverterTestBase
  {
    private readonly int[] constants = { 0, 1, 123, 0x7F, 0xFF, 0x7FFFFFFF, -2147483648, 0xFFFF, 0x7FFF };
    private const int iterationCount = 100;

    [Test]
    public void CombinedTest()
    {
      foreach (int constant in constants)
        OneValueTest<int, bool>(constant, iterationCount);
      foreach (int constant in constants)
        OneValueTest<int, byte>(constant, iterationCount);
      foreach (int constant in constants)
        OneValueTest<int, sbyte>(constant, iterationCount);
      foreach (int constant in constants)
        OneValueTest<int, short>(constant, iterationCount);
      foreach (int constant in constants)
        OneValueTest<int, ushort>(constant, iterationCount);
      foreach (int constant in constants)
        OneValueTest<int, int>(constant, iterationCount);
      foreach (int constant in constants)
        OneValueTest<int, uint>(constant, iterationCount);
      foreach (int constant in constants)
        OneValueTest<int, long>(constant, iterationCount);
      foreach (int constant in constants)
        OneValueTest<int, ulong>(constant, iterationCount);
      foreach (int constant in constants)
        OneValueTest<int, float>(constant, iterationCount);
      foreach (int constant in constants)
        OneValueTest<int, double>(constant, iterationCount);
      foreach (int constant in constants)
        OneValueTest<int, decimal>(constant, iterationCount);
      foreach (int constant in constants)
        OneValueTest<int, DateTime>(constant, iterationCount);
      foreach (int constant in constants)
        OneValueTest<int, Guid>(constant, iterationCount);
      foreach (int constant in constants)
        OneValueTest<int, string>(constant, iterationCount);
      foreach (int constant in constants)
        OneValueTest<int, char>(constant, iterationCount);
    }
  }
}
