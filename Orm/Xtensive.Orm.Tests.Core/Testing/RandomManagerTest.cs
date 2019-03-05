// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.09

using System;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Core.Testing
{
  [TestFixture]
  public class RandomManagerTest
  {
    private class RandomManagerInvoker
    {
      [MethodImpl(MethodImplOptions.NoInlining)]
      public virtual Random CreateRandom1(SeedVariatorType variatorType)
      {
        return RandomManager.CreateRandom(variatorType);
      }

      [MethodImpl(MethodImplOptions.NoInlining)]
      public virtual Random CreateRandom2(SeedVariatorType variatorType)
      {
        return RandomManager.CreateRandom(variatorType);
      }
    }

    private class RandomManagerInvoker2: RandomManagerInvoker
    {
      [MethodImpl(MethodImplOptions.NoInlining)]
      public override Random CreateRandom1(SeedVariatorType variatorType)
      {
        return RandomManager.CreateRandom(variatorType);
      }

      [MethodImpl(MethodImplOptions.NoInlining)]
      public override Random CreateRandom2(SeedVariatorType variatorType)
      {
        return RandomManager.CreateRandom(variatorType);
      }
    }

    [Test]
    public virtual void SeedVariationTest_None()
    {
      RandomManagerInvoker r1 = new RandomManagerInvoker();
      RandomManagerInvoker r2 = new RandomManagerInvoker2();
      Assert.AreEqual(
        r1.CreateRandom1(SeedVariatorType.None).Next(),
        r2.CreateRandom1(SeedVariatorType.None).Next());
      Assert.AreEqual(
        r1.CreateRandom1(SeedVariatorType.None).Next(),
        r1.CreateRandom2(SeedVariatorType.None).Next());
      Assert.AreEqual(
        r1.CreateRandom1(SeedVariatorType.None).Next(),
        r2.CreateRandom1(SeedVariatorType.None).Next());
    }

    [Test]
    public virtual void SeedVariationTest_CallingMethod()
    {
      RandomManagerInvoker r1 = new RandomManagerInvoker();
      RandomManagerInvoker r2 = new RandomManagerInvoker2();
      Assert.AreEqual(
        r1.CreateRandom1(SeedVariatorType.CallingMethod).Next(),
        r1.CreateRandom1(SeedVariatorType.CallingMethod).Next());
      Assert.AreNotEqual(
        r1.CreateRandom1(SeedVariatorType.CallingMethod).Next(),
        r1.CreateRandom2(SeedVariatorType.CallingMethod).Next());
      Assert.AreNotEqual(
        r1.CreateRandom1(SeedVariatorType.CallingMethod).Next(),
        r2.CreateRandom1(SeedVariatorType.CallingMethod).Next());
    }

    [Test]
    public virtual void SeedVariationTest_CallingType()
    {
      RandomManagerInvoker r1 = new RandomManagerInvoker();
      RandomManagerInvoker r2 = new RandomManagerInvoker2();
      Assert.AreEqual(
        r1.CreateRandom1(SeedVariatorType.CallingType).Next(),
        r1.CreateRandom1(SeedVariatorType.CallingType).Next());
      Assert.AreEqual(
        r1.CreateRandom1(SeedVariatorType.CallingType).Next(),
        r1.CreateRandom2(SeedVariatorType.CallingType).Next());
      Assert.AreNotEqual(
        r1.CreateRandom1(SeedVariatorType.CallingType).Next(),
        r2.CreateRandom1(SeedVariatorType.CallingType).Next());
    }
  }
}