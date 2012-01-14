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
  public class ByteConverterTest : ConverterTestBase
  {
    private readonly byte[] constants = { 0, 1, 123, 0x7F, 0xFF};
    private const int iterationCount = 100;

    [Test]
    public void CombinedTest()
    {
      foreach (byte constant in constants)
        OneValueTest<byte, bool>(constant, iterationCount);
      foreach (byte constant in constants)
        OneValueTest<byte, byte>(constant, iterationCount);
      foreach (byte constant in constants)
        OneValueTest<byte, sbyte>(constant, iterationCount);
      foreach (byte constant in constants)
        OneValueTest<byte, short>(constant, iterationCount);
      foreach (byte constant in constants)
        OneValueTest<byte, ushort>(constant, iterationCount);
      foreach (byte constant in constants)
        OneValueTest<byte, int>(constant, iterationCount);
      foreach (byte constant in constants)
        OneValueTest<byte, uint>(constant, iterationCount);
      foreach (byte constant in constants)
        OneValueTest<byte, long>(constant, iterationCount);
      foreach (byte constant in constants)
        OneValueTest<byte, ulong>(constant, iterationCount);
      foreach (byte constant in constants)
        OneValueTest<byte, float>(constant, iterationCount);
      foreach (byte constant in constants)
        OneValueTest<byte, double>(constant, iterationCount);
      foreach (byte constant in constants)
        OneValueTest<byte, decimal>(constant, iterationCount);
      foreach (byte constant in constants)
        OneValueTest<byte, DateTime>(constant, iterationCount);
      foreach (byte constant in constants)
        OneValueTest<byte, Guid>(constant, iterationCount);
      foreach (byte constant in constants)
        OneValueTest<byte, string>(constant, iterationCount);
      foreach (byte constant in constants)
        OneValueTest<byte, char>(constant, iterationCount);
    }
  }
}
