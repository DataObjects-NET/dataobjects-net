// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.27

using System;
using NUnit.Framework;
using Xtensive.Core.Tuples;

namespace Xtensive.Core.Tests.Tuples
{
  [TestFixture]
  public class FlagsTest
  {
    [Test]
    public void Main()
    {
      Tuple t = Tuple.Create<int, string>(0, null);
      Assert.IsTrue(t.IsAvailable(0));
      Assert.IsFalse(t.IsNull(0));
      Assert.IsTrue(t.IsAvailable(1));
      Assert.IsTrue(t.IsNull(1));
      t.SetValue(0, null);
      t.SetValue(1, null);
      Assert.IsTrue(t.IsAvailable(0));
      Assert.IsTrue(t.IsAvailable(1));
      Assert.IsTrue(t.IsNull(0));
      Assert.IsTrue(t.IsNull(1));
      t.SetValue(0, new int?(32));
      Assert.IsTrue(t.IsAvailable(0));
      Assert.IsFalse(t.IsNull(0));
      int? value = null;
      t.SetValue(0, value);
      Assert.IsTrue(t.IsAvailable(0));
      Assert.IsTrue(t.IsNull(0));
      
    }
  }
}