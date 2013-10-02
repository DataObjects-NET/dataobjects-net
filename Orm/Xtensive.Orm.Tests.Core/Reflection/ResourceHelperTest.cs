// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.12.08

using System;
using NUnit.Framework;
using Xtensive.Reflection;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Tests.Core.Resources;

namespace Xtensive.Orm.Tests.Core.Reflection
{
  [TestFixture]
  public class ResourceHelperTest
  {
    [Test]
    public void SuccessfullTest()
    {
      var resourceValue = ResourceHelper.GetStringResource(typeof(TestResources), "TestKey");
      Assert.AreEqual(TestResources.TestKey, resourceValue);
    }

    [Test]
    public void WrongKeyTest()
    {
      AssertEx.Throws<InvalidOperationException>(() => 
        ResourceHelper.GetStringResource(typeof(TestResources), "WrongKey"));
    }
  }
}