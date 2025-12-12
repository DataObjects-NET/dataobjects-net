// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.09.17

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.Keys;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tests.Storage
{
  namespace Keys
  {
    [HierarchyRoot]
    public class FruitRef : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public Fruit Ref { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    public abstract class Fruit : Entity
    {
      [Field(Length = 50), Key]
      public string Tag { get; private set; }

      public Fruit(string tag)
        : base(tag)
      {
      }
    }

    public class Banana : Fruit
    {
      public Banana(string tag)
        : base(tag)
      {
      }
    }

    public class Apple : Fruit
    {
      public Apple(string tag)
        : base(tag)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class Test : Entity
    {
      [Field(Length = 50), Key(0)]
      public string Key1 { get; private set; }

      [Field, Key(1)]
      public byte Key2 { get; private set; }

      [Field, Key(2)]
      public sbyte Key3 { get; private set; }

      [Field, Key(3)]
      public DateTime Key4 { get; private set; }

      [Field]
      public int Key5 { get; private set; }

      [Field]
      public long Key6 { get; private set; }

      [Field]
      public ushort Key7 { get; private set; }

      [Field]
      public uint Key8 { get; private set; }

      [Field]
      public Guid Key9 { get; private set; }

      [Field]
      public float Key10 { get; private set; }

      [Field]
      public double Key11 { get; private set; }

      [Field]
      public decimal Key12 { get; private set; }

      [Field]
      public bool Key13 { get; private set; }

      [Field]
      public string Key14 { get; private set; }

      [Field]
      public TimeSpan Key15 { get; private set; }
    }

    [Serializable]
    [HierarchyRoot]
    public class Container : Entity
    {
      [Field, Key]
      public Guid Id { get; private set; }

      [Field(Length = 128)]
      public Key StringKey { get; set; }
    }
  }

  public class KeysTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.RegisterCaching(typeof (Apple).Assembly, typeof (Apple).Namespace);
      return configuration;
    }

    [Test]
    public void CreateFormatParseTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        Key k1 = Key.Create<Apple>(Domain, "1");
        Key k2 = Key.Create<Apple>(Domain, "1");
        Assert.That(k2, Is.EqualTo(k1));

        Key kk = Key.Create<Apple>(Domain, "");
        var s = kk.Format();
        var k = Key.Parse(Domain, s);
        Assert.That(kk, Is.EqualTo(k));
        t.Complete();
      }
    }

    [Test]
    public void ResolveKeyTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var descriptor = Domain.Model.Types[typeof (Test)].Hierarchy.Key.TupleDescriptor;

        Tuple tuple = Tuple.Create(descriptor);
        tuple.SetValue(0, " , ");
        tuple.SetValue<byte>(1, 1);
        tuple.SetValue<sbyte>(2, -1);
        tuple.SetValue(3, DateTime.Now);

        Key k1 = Key.Create<Test>(Domain, tuple);
        var stringValue = k1.Format();
        var k2 = Key.Parse(Domain, stringValue);
        Assert.That(k2, Is.EqualTo(k1));
        t.Complete();
      }
    }

    [Test]
    public void ResolveNotExistingKeyTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        Key key = Key.Create(Domain, typeof (Fruit), "NotExistingFruit");
        var entity = session.Query.SingleOrDefault(key);
        var entity2 = session.Query.SingleOrDefault<Fruit>("NotExistingFruit");
        Assert.That(entity, Is.Null);
        Assert.That(entity2, Is.Null);
      }
    }

    [Test]
    public void StoreKeyTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var a = new Apple("1");
        var b = new Banana("2");
        var c = new Container();

        c.StringKey = a.Key;
        session.SaveChanges();

        Assert.That(a.Key, Is.EqualTo(c.StringKey));
        c.StringKey = b.Key;
        Assert.That(b.Key, Is.EqualTo(c.StringKey));

        c.StringKey = null;
        session.SaveChanges();
        Assert.That(c.StringKey, Is.Null);

        var appleEntity1 = session.Query.SingleOrDefault<Apple>("1");
        var appleEntity2 = session.Query.SingleOrDefault<Apple>(a);
        var appleEntity3 = session.Query.SingleOrDefault<Apple>((object) a);

        Assert.That(appleEntity1.Key, Is.EqualTo(a.Key));
        Assert.That(appleEntity2.Key, Is.EqualTo(a.Key));
        Assert.That(appleEntity3.Key, Is.EqualTo(a.Key));

        Assert.That(appleEntity2, Is.EqualTo(appleEntity1));
        Assert.That(appleEntity3, Is.EqualTo(appleEntity1));

        t.Complete();
      }
    }

    [Test]
    public void NodeIdCreateFormatParseTest()
    {
      var nodeId = "{59EE3D42-C207-4E65-8EC0-220058B4F8F2}";
      var key = Key.Create(Domain, nodeId, typeof (Apple), TypeReferenceAccuracy.ExactType, "1");
      Assert.That(key.NodeId, Is.EqualTo(nodeId));

      var keyString = key.Format();
      var key2 = Key.Parse(Domain, keyString);
      Assert.That(key2.NodeId, Is.EqualTo(nodeId));
    }

    [Test]
    public void NodeIdCreateFormatParseDefaultTest()
    {
      var key = Key.Create(Domain, typeof (Apple), TypeReferenceAccuracy.ExactType, "1");
      Assert.That(key.NodeId, Is.EqualTo(WellKnown.DefaultNodeId));

      var keyString = key.Format();
      var key2 = Key.Parse(Domain, keyString);
      Assert.That(key2.NodeId, Is.EqualTo(WellKnown.DefaultNodeId));
    }
  }
}
