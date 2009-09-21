// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using NUnit.Framework;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Core.Tuples;

namespace Xtensive.Core.Tests.Serialization
{
  [TestFixture]
  public class SerializedTupleTest
  {
    [Test]
    public void CombinedTest()
    {
      var t = Tuple.Create(1, false);
      Assert.AreEqual(t, CloneBySerialization(t));
      
      t = Tuple.Create(t.Descriptor);
      t.SetValue(1, true);
      Assert.AreEqual(t, CloneBySerialization(t));
    }

    private static Tuple CloneBySerialization(Tuple source)
    {
      return ((SerializedTuple) LegacyBinarySerializer.Instance.Clone(new SerializedTuple(source))).Value;
    }
  }
}