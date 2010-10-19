// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.27

using System;
using NUnit.Framework;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Tests.Tuples
{
  [TestFixture]
  public class FlagsTest
  {
    [Test]
    public void Main()
    {
      Xtensive.Tuples.Tuple t = Xtensive.Tuples.Tuple.Create<int, string>(0, null);
      Assert.IsTrue(t.GetFieldState(0).IsAvailable());
      Assert.IsFalse(t.GetFieldState(0).IsNull());
      Assert.IsTrue(t.GetFieldState(1).IsAvailable());
      Assert.IsTrue(t.GetFieldState(1).IsNull());
      t.SetValue(0, null);
      t.SetValue(1, null);
      Assert.IsTrue(t.GetFieldState(0).IsAvailable());
      Assert.IsTrue(t.GetFieldState(1).IsAvailable());
      Assert.IsTrue(t.GetFieldState(0).IsNull());
      Assert.IsTrue(t.GetFieldState(1).IsNull());
      t.SetValue(0, new int?(32));
      Assert.IsTrue(t.GetFieldState(0).IsAvailable());
      Assert.IsFalse(t.GetFieldState(0).IsNull());
      int? value = null;
      t.SetValue(0, value);
      Assert.IsTrue(t.GetFieldState(0).IsAvailable());
      Assert.IsTrue(t.GetFieldState(0).IsNull());
      
    }
  }
}