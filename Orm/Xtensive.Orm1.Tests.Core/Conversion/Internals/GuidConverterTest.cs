// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.25


using System;
using NUnit.Framework;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.Conversion
{
  [TestFixture]
  public class GuidConverterTest : ConverterTestBase
  {
    private Guid[] constants;
    private const int iterationCount = 100;

    [Test]
    public void StringTest()
    {
      IInstanceGenerator<Guid> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<Guid>();
      Random random = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
      for (int i = 0; i < iterationCount * 100; i++)
        OneValueTest<Guid, string>(generator.GetInstance(random), 1);
    }

    [Test]
    public void CombinedTest()
    {
      constants = new Guid[10];
      IInstanceGenerator<Guid> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<Guid>();
      Random random = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
      for (int i = 0; i < 10; i++)
        constants[i] = generator.GetInstance(random);

      foreach (Guid constant in constants)
        OneValueTest<Guid, bool>(constant, iterationCount);
      foreach (Guid constant in constants)
        OneValueTest<Guid, byte>(constant, iterationCount);
      foreach (Guid constant in constants)
        OneValueTest<Guid, sbyte>(constant, iterationCount);
      foreach (Guid constant in constants)
        OneValueTest<Guid, short>(constant, iterationCount);
      foreach (Guid constant in constants)
        OneValueTest<Guid, ushort>(constant, iterationCount);
      foreach (Guid constant in constants)
        OneValueTest<Guid, int>(constant, iterationCount);
      foreach (Guid constant in constants)
        OneValueTest<Guid, uint>(constant, iterationCount);
      foreach (Guid constant in constants)
        OneValueTest<Guid, long>(constant, iterationCount);
      foreach (Guid constant in constants)
        OneValueTest<Guid, ulong>(constant, iterationCount);
      foreach (Guid constant in constants)
        OneValueTest<Guid, float>(constant, iterationCount);
      foreach (Guid constant in constants)
        OneValueTest<Guid, double>(constant, iterationCount);
      foreach (Guid constant in constants)
        OneValueTest<Guid, decimal>(constant, iterationCount);
      foreach (Guid constant in constants)
        OneValueTest<Guid, DateTime>(constant, iterationCount);
      foreach (Guid constant in constants)
        OneValueTest<Guid, Guid>(constant, iterationCount);
      foreach (Guid constant in constants)
        OneValueTest<Guid, string>(constant, iterationCount);
      foreach (Guid constant in constants)
        OneValueTest<Guid, char>(constant, iterationCount);
    }
  }
}
