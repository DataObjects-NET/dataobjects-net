// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.01

using System;
using NUnit.Framework;
using Xtensive.Aspects.Helpers;
using Xtensive.Collections;
using Xtensive.Reflection;

namespace Xtensive.Aspects.Tests
{
  [TestFixture]
  public class ImplementConstructorTest
  {
    [Test]
    public void CombinedTest()
    {
      var tDescendant = typeof (Descendant);
      var instance = tDescendant.Activate(null, "Created") as Descendant;
      Assert.IsNotNull(instance);
      Assert.AreEqual("Created", instance.Message);
    }
  }
}