// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Xtensive.Collections;
using System.Runtime.Serialization.Formatters.Binary;

namespace Xtensive.Orm.Tests.Core.Collections
{
  [TestFixture]
  public class ReadOnlyCollectionTest
  {
    private List<string> innerCol;
    private ReadOnlyCollection<string> readOnlyCollection;

#if NETCOREAPP
    [OneTimeSetUp]
#else
    [TestFixtureSetUp]
#endif
    public void Init()
    {
      innerCol = new List<string>();
      readOnlyCollection = new ReadOnlyCollection<string>(innerCol);
    }

    [Test]
    public void ReadOnlyCollection_Add()
    {
      Assert.Throws<NotSupportedException>(() => readOnlyCollection.Add("Add"));
    }

    [Test]
    public void ReadOnlyCollection_Remove()
    {
      Assert.Throws<NotSupportedException>(() => {
        innerCol.Add("Remove");
        try {
          readOnlyCollection.Remove("Remove");
        }
        finally {
          innerCol.Clear();
        }
      });
    }

    [Test]
    public void ReadOnlyCollection_Clear()
    {
      Assert.Throws<NotSupportedException>(() => {
        innerCol.Add("Clear");
        try {
          readOnlyCollection.Clear();
        }
        finally {
          innerCol.Clear();
        }
      });
    }

    [Test]
    public void CopyToTest()
    {
      innerCol.Clear();
      innerCol.Add("element");
      innerCol.Add("element 2");
      string[] array = new string[3];
      readOnlyCollection.CopyTo(array, 1);
      Assert.IsTrue(array[0]==null);
      if (array[1]=="element")
        Assert.IsTrue(array[2]=="element 2");
      else
        Assert.IsTrue(array[2]=="element");
    }

    [Test]
    public void SerializationTest()
    {
      BinaryFormatter serizalizer = new BinaryFormatter();
      MemoryStream stream = new MemoryStream();
      serizalizer.Serialize(stream, readOnlyCollection);
      stream.Position = 0;
      ReadOnlyCollection<string> deserialized = (ReadOnlyCollection<string>)serizalizer.Deserialize(stream);
      Assert.AreEqual(deserialized.Count, readOnlyCollection.Count);
      foreach (string s in deserialized)
        Assert.IsTrue(readOnlyCollection.Contains(s));
      foreach (string s in readOnlyCollection)
        Assert.IsTrue(deserialized.Contains(s));
    }
  }
}