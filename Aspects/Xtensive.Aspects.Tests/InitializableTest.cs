// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.01

using System;
using NUnit.Framework;
using Xtensive.Aspects;
using Xtensive.Aspects.Helpers;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Testing;

namespace Xtensive.Aspects.Tests
{
  public class WrongInitializableBase
  {
    public bool IsInitialized { get; private set; }

    protected void Initialize(Type ctorType)
    {
      IsInitialized = true;
    }
  }

  [Initializable]
  public class InitializableBase
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

    private InitializableSample(int i, bool ignored)
    {
    }
  }

  public class InitializableGeneric<T> : InitializableBase
  {
    public InitializableGeneric(object arg)
    {
    }

    public InitializableGeneric(int i)
      : this(i, true)
    {
      ArgumentValidator.EnsureArgumentIsInRange(i, 0, int.MaxValue, "i");
    }

    private InitializableGeneric(int i, bool ignored)
    {
    }
  }

  public class InitializableDerived : InitializableGeneric<int>
  {
    public InitializableDerived(int i)
      : base(i)
    {
    }

    public InitializableDerived(Guid guid)
      : base(0)
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
      Assert.Throws<ArgumentException>(() => {
        new InitializableSample(-1);
      });
      Assert.AreEqual(1, InitializableBase.ErrorCount);
    }
  }
}