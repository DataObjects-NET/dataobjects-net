// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.11

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.ActivatorModel;
using System.Linq;

namespace Xtensive.Orm.Tests.Storage.ActivatorModel
{
  [Serializable]
  [HierarchyRoot]
  public abstract class Ancestor : Entity
  {
    [Field, Key]
    public int ID { get; private set; }
  }

  [Serializable]
  public class Descendant : Ancestor
  {
    [Field]
    public int Number { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class InitializebleClass : Entity
  {
    public object syncRoot = new object();

    protected override void OnInitialize()
    {
      base.OnInitialize();
      syncRoot = new object();
    }

    public InitializebleClass()
    {
      syncRoot = new object();
      // Ëîãèêà, êîòîðàÿ þçàåò syncRoot.
      Assert.That(syncRoot, Is.Not.Null);
    }

    [Field, Key]
    public int ID { get; private set; }
  }

  
}

namespace Xtensive.Orm.Tests.Storage
{
  

  public class  ActivatorTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.RegisterCaching(Assembly.GetExecutingAssembly(), "Xtensive.Orm.Tests.Storage.ActivatorModel");
      return config;
    }

    [Test]
    public void TestFieldInitializer()
    {
      using (var session = Domain.OpenSession())
      {
        using (var t = session.OpenTransaction())
        {
          var obj1 = new  InitializebleClass();
          Assert.That(obj1.syncRoot, Is.Not.Null);
          t.Complete();
        }
      }
      using (var session = Domain.OpenSession())
      {
        using (var t = session.OpenTransaction())
        {
          var obj1 = session.Query.All<InitializebleClass>().First();
          Assert.That(obj1, Is.Not.Null);
          Assert.That(obj1.syncRoot, Is.Not.Null);
          t.Complete();
        }
      }
    }

    [Test]
    public void Test()
    {
      Key key;
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var descendant = new Descendant();
          key = descendant.Key;          
          t.Complete();
        }        
      }
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var ancestor = session.Query.SingleOrDefault<Ancestor>(key);
          Assert.That(ancestor, Is.Not.Null);

          var descendant = session.Query.SingleOrDefault<Descendant>(key);
          Assert.That(descendant, Is.Not.Null);
        }        
      }
    }
  }
}