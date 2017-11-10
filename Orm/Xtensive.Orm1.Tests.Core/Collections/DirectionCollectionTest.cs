// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.11.23

using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Orm.Tests.Core.Collections
{
  [TestFixture]
  public class DirectionCollectionTest
  {
    public void Test()
    {
      DirectionCollection<string> collection = new DirectionCollection<string>();
      collection["a"] = Direction.Negative;
      collection["b"] = Direction.Negative;
      collection["c"] = Direction.Positive;
      collection["d"] = Direction.Positive;
      collection["e"] = Direction.Negative;
      Assert.AreEqual(Direction.Negative, collection["a"]);
      Assert.AreEqual(Direction.Negative, collection["b"]);
      Assert.AreEqual(Direction.Positive, collection["c"]);
      Assert.AreEqual(Direction.Positive, collection["d"]);
      Assert.AreEqual(Direction.Negative, collection["e"]);
    }
  }
}