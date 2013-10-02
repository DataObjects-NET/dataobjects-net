// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.03

using System;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.Collections
{
  [TestFixture]
  public class ExtensionCollectionTest
  {
    [Test]
    public void CombinedTest()
    {
      var c = new ExtensionCollection();
      Assert.AreEqual(0, c.Count);
      Assert.IsNull(c.Get<ExtensionCollectionTest>());
      AssertEx.ThrowsArgumentException(() => c.Set(typeof(int), 1));
      c.Set(this);
      Assert.AreEqual(1, c.Count);
      Assert.AreSame(this, c.Get<ExtensionCollectionTest>());
      
      c.Lock();
      AssertEx.Throws<InstanceIsLockedException>(() => c.Set(this));

      var cc = (ExtensionCollection) c.Clone();
      AssertEx.AreEqual(c, cc);

      var o = new object();
      cc.Set(o);
      Assert.AreEqual(2, cc.Count);
      Assert.AreSame(this, cc.Get<ExtensionCollectionTest>());
      Assert.AreSame(o, cc.Get<object>());
    }
  }
}