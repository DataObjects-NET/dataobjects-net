// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using NUnit.Framework;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;

namespace Xtensive.Core.Tests.Serialization
{
  [TestFixture]
  public class SerializableTupleTest
  {
    [Test]
    public void BaseTest()
    {
      var tuple = Tuple.Create(1, false);
      var serializedTuple = new SerializableTuple(tuple);
      var clone = (SerializableTuple) LegacyBinarySerializer.Instance.Clone(serializedTuple);
      Assert.IsFalse(clone.Value==null);
      Assert.AreEqual(tuple, clone.Value);
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

    private static Tuple CloneBySerialization(Tuple source)
    {
      return ((SerializableTuple) LegacyBinarySerializer.Instance.Clone(new SerializableTuple(source))).Value;
    }
  }
}