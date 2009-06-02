// Copyright (C) 2008 Xtensive LLC.
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
using Xtensive.Core.Tuples;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.Keys
{ 
  public class KeysTest : AutoBuildTest
  {
    #region Model

    [KeyGenerator(null)]
    [HierarchyRoot]
    public abstract class Fruit : Entity
    {
      [Field(Length = 50), KeyField] 
      public string Tag { get; private set;}    

      public Fruit(string tag)
        : base(tag) {}
    }

    public class Banana : Fruit
    {
      public Banana(string tag)
        : base(tag) {}
    }

    public class Apple : Fruit
    {
      public Apple(string tag)
        : base(tag) {}
    }

    [KeyGenerator(null)]
    [HierarchyRoot]
    public class Test : Entity
    {
      [Field, KeyField(0)]
      public string Key1 { get; private set; }
      [Field, KeyField(1)]
      public Byte Key2 { get; private set; }
      [Field, KeyField(2)]
      public SByte Key3 { get; private set; }
      [Field, KeyField(3)]
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

    [HierarchyRoot]
    public class Container : Entity
    {
      [Field, KeyField]
      public Guid ID { get; private set; }
      
      [Field]
      public Key StringKey { get; set; } 
    }

    #endregion

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration(); 
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.Keys");
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          Key k1 = Key.Create<Apple>("1");
          Key k2 = Key.Create<Apple>("1");
          Assert.AreEqual(k1, k2);

          Key kk = Key.Create<Apple>("");
          var s = kk.Format();
          var k = Key.Parse(s);
          Assert.AreEqual(k, kk);
          t.Complete();
        }
      }
    }

    [Test]
    public void ResolveKeyTest()
    {
      using (Domain.OpenSession())
      {
        using (var t = Transaction.Open())
        {
          TupleDescriptor descriptor = TupleDescriptor.Create(new[] { typeof (string), typeof (Byte), typeof (SByte),
              typeof (DateTime)});

          Tuple tuple = Tuple.Create(descriptor);
          tuple.SetValue(0, " , ");
          tuple.SetValue<Byte>(1, 1);
          tuple.SetValue<SByte>(2, -1);
          tuple.SetValue(3, DateTime.Now);

          Key k1 = Key.Create<Test>(tuple);
          var stringValue = k1.Format();
          var k2 = Key.Parse(stringValue);
          Assert.AreEqual(k1, k2);
          t.Complete();
        }
      }
    }

    [Test]
    public void ResolveNotExistingKeyTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          Key key = Key.Create(typeof (Fruit), "NotExistingFruit");
          var entity = key.Resolve();
          Assert.IsNull(entity);
        }
      }
    }

    [Test]
    public void StoreKeyTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {

          var a = new Apple("1");
          var b = new Banana("2");
          var c = new Container();

          c.StringKey = a.Key;
          Session.Current.Persist();

          Assert.AreEqual(c.StringKey, a.Key);
          c.StringKey = b.Key;
          Assert.AreEqual(c.StringKey, b.Key);

          c.StringKey = null;
          Session.Current.Persist();
          Assert.AreEqual(c.StringKey, null);
          t.Complete();
        }
      }
    }

    [Test]
    [Ignore("Erroneous behavior")]
    public void CombinedTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          Apple myApple = new Apple("My fruit");
          Key appleKey = myApple.Key;

          using (Domain.OpenSession()) {
            using (Transaction.Open()) {
              Key bananaKey = new Banana("My fruit").Key;
              Assert.AreEqual(typeof (Banana), bananaKey.EntityType.UnderlyingType);
            }
          }
          Assert.AreEqual(typeof (Apple), appleKey.EntityType.UnderlyingType);
        }
      }
    }
  }
}
