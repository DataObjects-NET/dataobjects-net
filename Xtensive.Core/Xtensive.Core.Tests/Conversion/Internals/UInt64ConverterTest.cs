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
  public class UInt64ConverterTest : ConverterTestBase
  {
    private readonly ulong[] constants = {
      0x7FFFFFFFFFFF1234, 0, 123, 0x7F, 0xFF, 0x7FFFFFFF, 0x800000000000EDCC,
      0xFFFF, 0x7FFF, 0xFFFFFFFF, 0xFFFFFFFFFFFFFF, 0xFFFFFFFFFFFF1234
    };
    private const int iterationCount = 100;

    [Test]
    public void CombinedTest()
    {
      foreach (ulong constant in constants)
        OneValueTest<ulong, bool>(constant, iterationCount);
      foreach (ulong constant in constants)
        OneValueTest<ulong, byte>(constant, iterationCount);
      foreach (ulong constant in constants)
        OneValueTest<ulong, sbyte>(constant, iterationCount);
      foreach (ulong constant in constants)
        OneValueTest<ulong, short>(constant, iterationCount);
      foreach (ulong constant in constants)
        OneValueTest<ulong, ushort>(constant, iterationCount);
      foreach (ulong constant in constants)
        OneValueTest<ulong, int>(constant, iterationCount);
      foreach (ulong constant in constants)
        OneValueTest<ulong, uint>(constant, iterationCount);
      foreach (ulong constant in constants)
        OneValueTest<ulong, long>(constant, iterationCount);
      foreach (ulong constant in constants)
        OneValueTest<ulong, ulong>(constant, iterationCount);
      foreach (ulong constant in constants)
        OneValueTest<ulong, float>(constant, iterationCount);
      foreach (ulong constant in constants)
        OneValueTest<ulong, double>(constant, iterationCount);
      foreach (ulong constant in constants)
        OneValueTest<ulong, decimal>(constant, iterationCount);
      foreach (ulong constant in constants)
        OneValueTest<ulong, DateTime>(constant, iterationCount);
      foreach (ulong constant in constants)
        OneValueTest<ulong, Guid>(constant, iterationCount);
      foreach (ulong constant in constants)
        OneValueTest<ulong, string>(constant, iterationCount);
      foreach (ulong constant in constants)
        OneValueTest<ulong, char>(constant, iterationCount);
    }
  }
}
