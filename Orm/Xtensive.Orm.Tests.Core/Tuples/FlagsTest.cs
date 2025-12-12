// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.27

using NUnit.Framework;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tests.Core.Tuples
{
  [TestFixture]
  public class FlagsTest
  {
    [Test]
    public void Main()
    {
      Tuple t = Tuple.Create<int, string>(0, null);
      Assert.That(t.GetFieldState(0).IsAvailable(), Is.True);
      Assert.That(t.GetFieldState(0).IsNull(), Is.False);
      Assert.That(t.GetFieldState(1).IsAvailable(), Is.True);
      Assert.That(t.GetFieldState(1).IsNull(), Is.True);
      t.SetValue(0, null);
      t.SetValue(1, null);
      Assert.That(t.GetFieldState(0).IsAvailable(), Is.True);
      Assert.That(t.GetFieldState(1).IsAvailable(), Is.True);
      Assert.That(t.GetFieldState(0).IsNull(), Is.True);
      Assert.That(t.GetFieldState(1).IsNull(), Is.True);
      t.SetValue(0, new int?(32));
      Assert.That(t.GetFieldState(0).IsAvailable(), Is.True);
      Assert.That(t.GetFieldState(0).IsNull(), Is.False);
      t.SetValue(0, (int?) null);
      Assert.That(t.GetFieldState(0).IsAvailable(), Is.True);
      Assert.That(t.GetFieldState(0).IsNull(), Is.True);
      
    }
  }
}