// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.09.17

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.Keys
{ 
  public class KeysTest : AutoBuildTest
  {
    #region Model

    [HierarchyRoot("Tag")]        
    public abstract class Fruit : Entity
    {
      [Field(Length = 50)] 
      public string Tag { get; private set;}    

      public Fruit(string tag)
        : base(Tuple.Create(tag)) {}
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

    [HierarchyRoot("Key1", "Key2", "Key3", "Key4", "Key5", "Key6", "Key7", "Key8",
      "Key9", "Key10", "Key11", "Key12", "Key13", "Key14", "Key15")]
    public class Test : Entity
    {
      [Field]
      public string Key1 { get; private set; }
      [Field]
      public Byte Key2 { get; private set; }
      [Field]
      public SByte Key3 { get; private set; }
      [Field]
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
          Key k1 = Key.Create<Apple>(Tuple.Create("1"));
          Key k2 = Key.Create<Apple>(Tuple.Create("1"));
          Assert.AreEqual(k1, k2);

          Key kk = Key.Create<Apple>(Tuple.Create(""));
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
              typeof (DateTime), typeof (Int32), typeof (Int64), typeof (UInt16), typeof (UInt32), typeof (Guid), 
              typeof (float), typeof (double), typeof (decimal), typeof (bool), typeof (string), typeof(TimeSpan)
            });

          Tuple tuple = Tuple.Create(descriptor);
          tuple.SetValue(0," , ");
          tuple.SetValue<Byte>(1, 1);
          tuple.SetValue<SByte>(2, -1);
          tuple.SetValue(3, DateTime.Now);
          tuple.SetValue(4, -1);
          tuple.SetValue<Int64>(5, -1);
          tuple.SetValue<UInt16>(6, 1);
          tuple.SetValue<UInt32>(7, 1);
          tuple.SetValue(8, new Guid(new byte[] {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}));
          tuple.SetValue<float>(9, 1);
          tuple.SetValue<double>(10, 1);
          tuple.SetValue<decimal>(11, 1);
          tuple.SetValue(12, true);
          tuple.SetValue(13, " , ");
          tuple.SetValue(14, new TimeSpan());

          Key k1 = Key.Create<Test>(tuple);
          var stringValue = k1.Format();
          var k2 = Key.Parse(stringValue);
          Assert.AreEqual(k1, k2);
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
              Assert.AreEqual(typeof (Banana), bananaKey.Type.UnderlyingType);
            }
          }
          Assert.AreEqual(typeof (Apple), appleKey.Type.UnderlyingType);
        }
      }
    }
  }
}
