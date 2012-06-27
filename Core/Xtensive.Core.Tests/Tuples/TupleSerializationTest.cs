// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Testing;
using Xtensive.Tuples;

namespace Xtensive.Tests.Tuples
{
  [TestFixture]
  public class TupleSerializationTest
  {
    [Test]
    public void BaseTest()
    {
      var tuple = Tuple.Create(1, false);
      var clone = CloneBySerialization(tuple);
      Assert.IsFalse(clone==null);
      Assert.AreEqual(tuple, clone);
    }

    [Test]
    public void CombinedTest()
    {
      var t = Tuple.Create(1, false);
      Assert.AreEqual(t, CloneBySerialization(t));
      
      t = Tuple.Create(t.Descriptor);
      t.SetValue(1, true);
      Assert.AreEqual(t, CloneBySerialization(t));
    }

    [Test]
    public void SerializationWithNullValuesTest()
    {
      var t = Tuple.Create(typeof (string), typeof (int));
      t.SetValue(0, null);
      t.SetValue(1, null);

      Assert.AreEqual(t, CloneBySerialization(t));
    }

    [Test]
    public void DifferentialTupleSerializationTest()
    {
      var origin = Tuple.Create(1, 2);
      var dt1 = new DifferentialTuple(origin);
      dt1.SetValue(1,3);
      var dt2 = new DifferentialTuple(origin);
      dt1.SetValue(1,4);
      var all = new[] {dt1, dt2};
      
      var clone = Cloner.Clone(all);
      AssertEx.AreEqual(all, clone);

      var dt1Clone = clone[0];
      var dt2Clone = clone[1];
      var originClone = dt1Clone.Origin;
      Assert.AreSame(originClone, dt2Clone.Origin);
      originClone.SetValue(0,2);
      Assert.AreEqual(2, dt1Clone.GetValue(0));
      Assert.AreEqual(2, dt2Clone.GetValue(0));
    }

    private static Tuple CloneBySerialization(Tuple source)
    {
      return Cloner.Clone(source);
    }
  }
}