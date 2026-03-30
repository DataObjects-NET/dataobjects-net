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
      Assert.That(c.Count, Is.EqualTo(0));
      Assert.That(c.Get<ExtensionCollectionTest>(), Is.Null);
      AssertEx.ThrowsArgumentException(() => c.Set(typeof(int), 1));
      c.Set(this);
      Assert.That(c.Count, Is.EqualTo(1));
      Assert.That(c.Get<ExtensionCollectionTest>(), Is.SameAs(this));
      
      c.Lock();
      AssertEx.Throws<InstanceIsLockedException>(() => c.Set(this));

      var cc = (ExtensionCollection) c.Clone();
      AssertEx.HasSameElements(c, cc);

      var o = new object();
      cc.Set(o);
      Assert.That(cc.Count, Is.EqualTo(2));
      Assert.That(cc.Get<ExtensionCollectionTest>(), Is.SameAs(this));
      Assert.That(cc.Get<object>(), Is.SameAs(o));
    }
  }
}