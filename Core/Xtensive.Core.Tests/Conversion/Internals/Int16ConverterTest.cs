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
  public class Int16ConverterTest : ConverterTestBase
  {
    private readonly short[] constants = { 0, -1, 1, 123, 0x7F, 0xFF, 0x7FFF, -32768 };
    private const int iterationCount = 100;

    [Test]
    public void CombinedTest()
    {
      foreach (short constant in constants)
        OneValueTest<short, bool>(constant, iterationCount);
      foreach (short constant in constants)
        OneValueTest<short, byte>(constant, iterationCount);
      foreach (short constant in constants)
        OneValueTest<short, sbyte>(constant, iterationCount);
      foreach (short constant in constants)
        OneValueTest<short, short>(constant, iterationCount);
      foreach (short constant in constants)
        OneValueTest<short, ushort>(constant, iterationCount);
      foreach (short constant in constants)
        OneValueTest<short, int>(constant, iterationCount);
      foreach (short constant in constants)
        OneValueTest<short, uint>(constant, iterationCount);
      foreach (short constant in constants)
        OneValueTest<short, long>(constant, iterationCount);
      foreach (short constant in constants)
        OneValueTest<short, ulong>(constant, iterationCount);
      foreach (short constant in constants)
        OneValueTest<short, float>(constant, iterationCount);
      foreach (short constant in constants)
        OneValueTest<short, double>(constant, iterationCount);
      foreach (short constant in constants)
        OneValueTest<short, decimal>(constant, iterationCount);
      foreach (short constant in constants)
        OneValueTest<short, DateTime>(constant, iterationCount);
      foreach (short constant in constants)
        OneValueTest<short, Guid>(constant, iterationCount);
      foreach (short constant in constants)
        OneValueTest<short, string>(constant, iterationCount);
      foreach (short constant in constants)
        OneValueTest<short, char>(constant, iterationCount);
    }
  }
}
