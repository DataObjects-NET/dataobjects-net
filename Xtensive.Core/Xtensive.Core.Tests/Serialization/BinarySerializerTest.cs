// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using NUnit.Framework;
using Xtensive.Core.Serialization.Binary;

namespace Xtensive.Core.Tests.Serialization
{
  [TestFixture]
  public class BinarySerializerTest
  {
    [Test]
    public void CombinedTest()
    {
      object o = 1;
      Assert.AreEqual(o, CloneBySerialization(o));
    }

    private static object CloneBySerialization(object source)
    {
      return BinarySerializer.Instance.Clone(source);
    }
  }
}