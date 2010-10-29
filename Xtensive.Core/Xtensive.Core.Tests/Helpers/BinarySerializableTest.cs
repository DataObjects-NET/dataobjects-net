// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.03.18

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using System;
using System.IO;
using Xtensive.Helpers;

namespace Xtensive.Tests.Helpers
{
  [Serializable]
  public class Container
  {
    public BinarySerializable<Item> Item;
  }

  [Serializable]
  public class Item
  {
    public string Name;
  }

  [TestFixture]
  public class BinarySerializableTest
  {
    [Test]
    public void BinaryFormatterTest()
    {
      var formatter = new BinaryFormatter();
      var containter = new Container {Item = new BinarySerializable<Item>(new Item {Name = "Hello"})};
      byte[] serializedContainer;
      using (var stream  = new MemoryStream()) {
        formatter.Serialize(stream, containter);
        serializedContainer = stream.ToArray();
      }
      using (var stream = new MemoryStream(serializedContainer)) {
        var deserializedContainer = (Container) formatter.Deserialize(stream);
        Assert.AreEqual(containter.Item.Value.Name, deserializedContainer.Item.Value.Name);
      }
    }

    [Test]
    public void DataContractSerializerTest()
    {
      var serializer = new DataContractSerializer(typeof (Container));
      var containter = new Container {Item = new BinarySerializable<Item>(new Item {Name = "Hello"})};
      byte[] serializedContainer;
      using (var stream  = new MemoryStream()) {
        serializer.WriteObject(stream, containter);
        serializedContainer = stream.ToArray();
      }
      using (var stream = new MemoryStream(serializedContainer)) {
        var deserializedContainer = (Container) serializer.ReadObject(stream);
        Assert.AreEqual(containter.Item.Value.Name, deserializedContainer.Item.Value.Name);
      }
    }
  }
}