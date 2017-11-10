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
  public class UInt32ConverterTest : ConverterTestBase
  {
    private readonly uint[] constants = { 0, 1, 123, 0x7F, 0xFF, 0x7FFFFFFF, 0xFFFF, 0x7FFF, 0xFFFFFFFF };
    private const int iterationCount = 100;

    [Test]
    public void CombinedTest()
    {
      foreach (uint constant in constants)
        OneValueTest<uint, bool>(constant, iterationCount);
      foreach (uint constant in constants)
        OneValueTest<uint, byte>(constant, iterationCount);
      foreach (uint constant in constants)
        OneValueTest<uint, sbyte>(constant, iterationCount);
      foreach (uint constant in constants)
        OneValueTest<uint, short>(constant, iterationCount);
      foreach (uint constant in constants)
        OneValueTest<uint, ushort>(constant, iterationCount);
      foreach (uint constant in constants)
        OneValueTest<uint, int>(constant, iterationCount);
      foreach (uint constant in constants)
        OneValueTest<uint, uint>(constant, iterationCount);
      foreach (uint constant in constants)
        OneValueTest<uint, long>(constant, iterationCount);
      foreach (uint constant in constants)
        OneValueTest<uint, ulong>(constant, iterationCount);
      foreach (uint constant in constants)
        OneValueTest<uint, float>(constant, iterationCount);
      foreach (uint constant in constants)
        OneValueTest<uint, double>(constant, iterationCount);
      foreach (uint constant in constants)
        OneValueTest<uint, decimal>(constant, iterationCount);
      foreach (uint constant in constants)
        OneValueTest<uint, DateTime>(constant, iterationCount);
      foreach (uint constant in constants)
        OneValueTest<uint, Guid>(constant, iterationCount);
      foreach (uint constant in constants)
        OneValueTest<uint, string>(constant, iterationCount);
      foreach (uint constant in constants)
        OneValueTest<uint, char>(constant, iterationCount);
    }
  }
}
