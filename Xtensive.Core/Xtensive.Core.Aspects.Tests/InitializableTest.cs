// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.01

using System;
using NUnit.Framework;
using Xtensive.Core.Aspects;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Reflection;
using Xtensive.Core.Testing;

namespace Xtensive.Core.Aspects.Tests
{
  public class WrongInitializableBase
  {
    public bool IsInitialized { get; private set; }

    protected void Initialize(Type ctorType)
    {
      IsInitialized = true;
    }
  }

  public class InitializableBase : IInitializable
  {
    public static int ErrorCount { get; private set; }
    public int InitializeCount { get; private set; }

    protected void Initialize(Type ctorType)
    {
      if (ctorType != GetType())
        return;
      InitializeCount++;
      Log.Info("Initialized: type {0}", ctorType.GetShortName());
    }

    protected void InitializationError(Type ctorType, Exception error)
    {
      ErrorCount++;
      Log.Info("Failed: type {0}", ctorType.GetShortName());
    }
  }

  public class InitializableSample : InitializableBase
  {
    // Constructors

    public InitializableSample()
      : this(0, true)
    {
    }

    public InitializableSample(int i)
      : this(i, true)
    {
      ArgumentValidator.EnsureArgumentIsInRange(i, 0, int.MaxValue, "i");
    }

    public InitializableSample(object arg)
    {
    }

    protected InitializableSample(int i, bool ignored)
    {
    }
  }

  [TestFixture]
  public class InitializableTest
  {
    [Test]
    public void CombinedTest()
    {
      var wi = new WrongInitializableBase(); 
      Assert.IsFalse(wi.IsInitialized);

      var i = new InitializableBase(); 
      Assert.AreEqual(1, i.InitializeCount);
      i = new InitializableSample();
      Assert.AreEqual(1, i.InitializeCount);
      i = new InitializableSample(null);
      Assert.AreEqual(1, i.InitializeCount);

      Assert.AreEqual(0, InitializableBase.ErrorCount);
      AssertEx.Throws<ArgumentException>(() => {
        new InitializableSample(-1);
      });
      Assert.AreEqual(1, InitializableBase.ErrorCount);
    }
  }
}