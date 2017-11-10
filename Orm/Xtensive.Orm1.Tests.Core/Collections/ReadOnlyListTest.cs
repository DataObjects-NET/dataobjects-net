// Copyright (C) YEAR Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Ilyin
// Created:    8 θώνÿ 2007 γ.

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Collections;
using System;
namespace Xtensive.Orm.Tests.Core.Collections
{
  [TestFixture]
  public class ReadOnlyListTest
  {
    private IList<string> innerList;
    private IList<string> readOnlyList;

    [TestFixtureSetUp]
    public void Init()
    {
      List<string> localInnerSet = new List<string>();
      innerList = localInnerSet;
      this.readOnlyList = new ReadOnlyList<string>(localInnerSet);
    }

    [Test]
    public void Test_Add()
    {
      Assert.Throws<NotSupportedException>(() => {
        innerList.Clear();
        try {
          readOnlyList.Add("element");
        }
        finally {
          Assert.IsTrue(readOnlyList.Count == 0);
        }
      });
    }

    [Test]
    public void Test_Remove()
    {
      Assert.Throws<NotSupportedException>(() => {
        innerList.Clear();
        innerList.Add("element");
        try {
          readOnlyList.Remove("element");
        }
        finally {
          Assert.IsTrue(readOnlyList.Count == 1);
        }
      });
    }

    [Test]
    public void Test_RemoveAt()
    {
      Assert.Throws<NotSupportedException>(() => {
        innerList.Clear();
        innerList.Add("element");
        try {
          readOnlyList.RemoveAt(0);
        }
        finally {
          Assert.IsTrue(readOnlyList.Count == 1);
        }
      });
    }

    [Test]
    public void Test_Clear()
    {
      Assert.Throws<NotSupportedException>(() => {
        innerList.Clear();
        innerList.Add("element");
        try {
          readOnlyList.Clear();
        }
        finally {
          Assert.IsTrue(readOnlyList.Count == 1);
        }
      });
    }

    [Test]
    public void CopyToTest()
    {
      innerList.Clear();
      innerList.Add("element");
      innerList.Add("element 2");
      string[] array = new string[3];
      readOnlyList.CopyTo(array, 1);
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
      serizalizer.Serialize(stream, readOnlyList);
      stream.Position = 0;
      ReadOnlyList<string> deserialized = (ReadOnlyList<string>)serizalizer.Deserialize(stream);
      Assert.AreEqual(deserialized.Count, readOnlyList.Count);
      foreach (string s in deserialized)
        Assert.IsTrue(readOnlyList.Contains(s));
      foreach (string s in readOnlyList)
        Assert.IsTrue(deserialized.Contains(s));
    }
  }
}