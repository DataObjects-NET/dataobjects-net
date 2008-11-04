// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.09.17

using System.Reflection;
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
