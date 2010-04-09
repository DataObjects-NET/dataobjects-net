// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.01

using System;
using NUnit.Framework;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Aspects.Tests
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