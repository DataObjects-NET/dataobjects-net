// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.01

using System;
using NUnit.Framework;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects;
using Xtensive.Core.Reflection;

[assembly:Initializable(AttributeTargetTypes = "*")]

namespace Xtensive.Core.Aspects.Tests
{
  public class WrongInitializableBase
  {
    public bool Initializated { get; private set; }

    protected void Initialize(Type ctorType)
    {
      Initializated = true;
    }
  }

  public class InitializableBase: IInitializable
  {
    public bool Initializated { get; private set; }

    protected void Initialize(Type ctorType)
    {
      if (ctorType!=GetType())
        return;
      Initializated = true;
      Log.Info("Initialized: type {0}", ctorType.GetShortName());
    }
  }

  [Initializable]
  public class InitializableSample : InitializableBase
  {
    // Constructors

    public InitializableSample()
    {
    }

    public InitializableSample(object arg)
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
      Assert.IsFalse(wi.Initializated);

      var i = new InitializableBase(); 
      Assert.IsTrue(i.Initializated);
      i = new InitializableSample();
      Assert.IsTrue(i.Initializated);
    }
  }
}