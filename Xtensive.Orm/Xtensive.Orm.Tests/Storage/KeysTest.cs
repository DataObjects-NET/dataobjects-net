// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.09.17

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Web;
using NUnit.Framework;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Storage.Keys
{ 
  public class KeysTest : AutoBuildTest
  {
    #region Model

    [Serializable]
    [KeyGenerator(KeyGeneratorKind.None)]
    [HierarchyRoot]
    public abstract class Fruit : Entity
    {
      [Field(Length = 50), Key] 
      public string Tag { get; private set;}    

      public Fruit(string tag)
        : base(tag) {}
    }

    [Serializable]
    public class Banana : Fruit
    {
      public Banana(string tag)
        : base(tag) {}
    }

    [Serializable]
    public class Apple : Fruit
    {
      public Apple(string tag)
        : base(tag) {}
    }

    [Serializable]
    [KeyGenerator(KeyGeneratorKind.None)]
    [HierarchyRoot]
    public class Test : Entity
    {
      [Field(Length = 50), Key(0)]
      public string Key1 { get; private set; }
      [Field, Key(1)]
      public Byte Key2 { get; private set; }
      [Field, Key(2)]
      public SByte Key3 { get; private set; }
      [Field, Key(3)]
      public DateTime Key4 { get; private set; }
      [Field]
      public Int32 Key5 { get; private set; }
      [Field]
      public Int64 Key6 { get; private set; }
      [Field]
      public UInt16 Key7 { get; private set; }
      [Field]
      public UInt32 Key8 { get; private set; }
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
      public Guid ID { get; private set; }
      
      [Field(Length = 128)]
      public Key StringKey { get; set; } 
    }

    #endregion

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration(); 
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Orm.Tests.Storage.Keys");
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Key k1 = Key.Create<Apple>(Domain, "1");
          Key k2 = Key.Create<Apple>(Domain, "1");
          Assert.AreEqual(k1, k2);

          Key kk = Key.Create<Apple>(Domain, "");
          var s = kk.Format();
          var k = Key.Parse(Domain, s);
          Assert.AreEqual(k, kk);
          t.Complete();
        }
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
        tuple.SetValue<Byte>(1, 1);
        tuple.SetValue<SByte>(2, -1);
        tuple.SetValue(3, DateTime.Now);

        Key k1 = Key.Create<Test>(Domain, tuple);
        var stringValue = k1.Format();
        var k2 = Key.Parse(Domain, stringValue);
        Assert.AreEqual(k1, k2);
        t.Complete();
      }
    }



    [Test]
    public void ResolveNotExistingKeyTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Key key = Key.Create(Domain, typeof (Fruit), "NotExistingFruit");
          var entity = session.Query.SingleOrDefault(key);
          var entity2 = session.Query.SingleOrDefault<Fruit>("NotExistingFruit");
          Assert.IsNull(entity);
          Assert.IsNull(entity2);
        }
      }
    }

    [Test]
    public void StoreKeyTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var a = new Apple("1");
          var b = new Banana("2");
          var c = new Container();

          c.StringKey = a.Key;
          Session.Current.SaveChanges();

          Assert.AreEqual(c.StringKey, a.Key);
          c.StringKey = b.Key;
          Assert.AreEqual(c.StringKey, b.Key);

          c.StringKey = null;
          Session.Current.SaveChanges();
          Assert.AreEqual(c.StringKey, null);

          var appleEntity1 = session.Query.SingleOrDefault<Apple>("1");
          var appleEntity2 = session.Query.SingleOrDefault<Apple>(a);
          var appleEntity3 = session.Query.SingleOrDefault<Apple>((object)a);

          Assert.AreEqual(a.Key, appleEntity1.Key);
          Assert.AreEqual(a.Key, appleEntity2.Key);
          Assert.AreEqual(a.Key, appleEntity3.Key);

          Assert.AreEqual(appleEntity1, appleEntity2);
          Assert.AreEqual(appleEntity1, appleEntity3);

          t.Complete();
        }
      }
    }

    [Test]
    [Ignore("Erroneous behavior")]
    public void CombinedTest()
    {
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          Apple myApple = new Apple("My fruit");
          Key appleKey = myApple.Key;

          using (var session2 = Domain.OpenSession()) {
            using (session2.OpenTransaction()) {
              Key bananaKey = new Banana("My fruit").Key;
              Assert.AreEqual(typeof (Banana), bananaKey.TypeInfo.UnderlyingType);
            }
          }
          Assert.AreEqual(typeof (Apple), appleKey.TypeInfo.UnderlyingType);
        }
      }
    }
  }
}
