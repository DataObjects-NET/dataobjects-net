// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using NUnit.Framework;
using System.Collections;
using Xtensive.Collections;

namespace Xtensive.Orm.Tests.Core.Collections
{
  [TestFixture]
  public class ReadOnlyDictionaryTest
  {
    private Dictionary<int, string> innerDictionary;
    private IDictionary<int, string> readOnlyDictionary;

    [TestFixtureSetUp]
    public void Init()
    {
      this.innerDictionary = new Dictionary<int, string>();
      this.readOnlyDictionary = new ReadOnlyDictionary<int, string>(innerDictionary);
    }

    [Test]
    [ExpectedException(typeof (NotSupportedException))]
    public void Test_Set()
    {
      this.innerDictionary[1] = "value";
      try {
        this.readOnlyDictionary[1] = "newValue";
      }
      finally {
        Assert.IsTrue((string)readOnlyDictionary[1] == "value");
      }
    }

    [Test]
    [ExpectedException(typeof (NotSupportedException))]
    public void Test_Clear()
    {
      this.innerDictionary[1] = "value";
      try {
        this.readOnlyDictionary.Clear();
      }
      finally {
        Assert.IsTrue((string)readOnlyDictionary[1] == "value");
      }
    }

    [Test]
    public void SerializationTest()
    {
      BinaryFormatter serizalizer = new BinaryFormatter();
      MemoryStream stream = new MemoryStream();
      serizalizer.Serialize(stream, readOnlyDictionary);
      stream.Position = 0;
      ReadOnlyDictionary<int, string> deserialized = (ReadOnlyDictionary<int, string>)serizalizer.Deserialize(stream);
      Assert.AreEqual(deserialized.Count, readOnlyDictionary.Count);
      foreach (KeyValuePair<int, string> pair in readOnlyDictionary)
        Assert.IsTrue(deserialized.Contains(pair));
      foreach (KeyValuePair<int, string> pair in deserialized)
        Assert.IsTrue(readOnlyDictionary.Contains(pair));
    }
  }
}