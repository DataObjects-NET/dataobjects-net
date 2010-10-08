// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.11

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.ActivatorModel;
using System.Linq;

namespace Xtensive.Storage.Tests.Storage.ActivatorModel
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
      // Логика, которая юзает syncRoot.
      Assert.IsNotNull(syncRoot);
    }

    [Field, Key]
    public int ID { get; private set; }
  }

  
}

namespace Xtensive.Storage.Tests.Storage
{
  

  public class  ActivatorTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.ActivatorModel");
      return config;
    }

    [Test]
    public void TestFieldInitializer()
    {
      using (Domain.OpenSession())
      {
        using (var t = Transaction.Open())
        {
          var obj1 = new  InitializebleClass();
          Assert.IsNotNull(obj1.syncRoot);
          t.Complete();
        }
      }
      using (Domain.OpenSession())
      {
        using (var t = Transaction.Open())
        {
          var obj1 = Query.All<InitializebleClass>().First();
          Assert.IsNotNull(obj1);
          Assert.IsNotNull(obj1.syncRoot);
          t.Complete();
        }
      }
    }

    [Test]
    public void Test()
    {
      Key key;
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var descendant = new Descendant();
          key = descendant.Key;          
          t.Complete();
        }        
      }
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var ancestor = Query.SingleOrDefault<Ancestor>(key);
          Assert.IsNotNull(ancestor);

          var descendant = Query.SingleOrDefault<Descendant>(key);
          Assert.IsNotNull(descendant);
        }        
      }
    }
  }
}