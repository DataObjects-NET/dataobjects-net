// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.01

using System;
using NUnit.Framework;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Aspects.Tests
{
  [Initializable]
  public class InitializableBase
  {
    public bool Initializated { get; private set; }

    protected void Initialize(Type ctorType)
    {
      Initializated = true;
      Log.Info("Initialize: type {0}, .ctor of {1}", 
        GetType().GetShortName(), ctorType.GetShortName());
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
      var i = new InitializableBase(); 
      Assert.IsTrue(i.Initializated);
      i = new InitializableSample();
      Assert.IsTrue(i.Initializated);
    }
  }
}