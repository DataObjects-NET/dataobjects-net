// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.23

using NUnit.Framework;
using Xtensive.Collections;

namespace Xtensive.Orm.Tests.Core.Collections
{
  [TestFixture]
  public class SetTests
  {
    [Test]
    public void AddRemoveTest()
    {
      ISet<int> set = new SetSlim<int>();
      Assert.IsTrue(set.Add(0));
      Assert.IsFalse(set.Add(0));
      Assert.IsTrue(set.Contains(0));
      Assert.AreEqual(set.Count, 1);
      Assert.AreEqual(set[0], 0);
      Assert.IsTrue(set.Remove(0));
      Assert.AreEqual(set.Count, 0);
    }
  }
}