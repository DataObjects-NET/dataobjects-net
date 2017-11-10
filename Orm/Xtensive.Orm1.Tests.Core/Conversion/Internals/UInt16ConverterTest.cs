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
  public class UInt16ConverterTest : ConverterTestBase
  {
    private readonly ushort[] constants = { 0, 1, 123, 0x7F, 0xFF, 0xFFFF, 0x7FFF };
    private const int iterationCount = 100;

    [Test]
    public void CombinedTest()
    {
      foreach (ushort constant in constants)
        OneValueTest<ushort, bool>(constant, iterationCount);
      foreach (ushort constant in constants)
        OneValueTest<ushort, byte>(constant, iterationCount);
      foreach (ushort constant in constants)
        OneValueTest<ushort, sbyte>(constant, iterationCount);
      foreach (ushort constant in constants)
        OneValueTest<ushort, short>(constant, iterationCount);
      foreach (ushort constant in constants)
        OneValueTest<ushort, ushort>(constant, iterationCount);
      foreach (ushort constant in constants)
        OneValueTest<ushort, int>(constant, iterationCount);
      foreach (ushort constant in constants)
        OneValueTest<ushort, uint>(constant, iterationCount);
      foreach (ushort constant in constants)
        OneValueTest<ushort, long>(constant, iterationCount);
      foreach (ushort constant in constants)
        OneValueTest<ushort, ulong>(constant, iterationCount);
      foreach (ushort constant in constants)
        OneValueTest<ushort, float>(constant, iterationCount);
      foreach (ushort constant in constants)
        OneValueTest<ushort, double>(constant, iterationCount);
      foreach (ushort constant in constants)
        OneValueTest<ushort, decimal>(constant, iterationCount);
      foreach (ushort constant in constants)
        OneValueTest<ushort, DateTime>(constant, iterationCount);
      foreach (ushort constant in constants)
        OneValueTest<ushort, Guid>(constant, iterationCount);
      foreach (ushort constant in constants)
        OneValueTest<ushort, string>(constant, iterationCount);
      foreach (ushort constant in constants)
        OneValueTest<ushort, char>(constant, iterationCount);
    }
  }
}
