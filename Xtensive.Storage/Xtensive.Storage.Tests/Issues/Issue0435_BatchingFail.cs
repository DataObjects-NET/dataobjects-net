// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.15

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.Issues.Issue0435_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0435_Model
{
  [Serializable]
  [HierarchyRoot]
  public class MyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Text { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0435_BatchingFail : AutoBuildTest
  {
    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (MyEntity));
      return configuration;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      CreateSessionAndTransaction();
    }

    [Test]
    public void Test1()
    {
      var entity1 = new MyEntity {
        Text = "Entity 1"
      }; // Nothing is sent to server yet
          
      var entity2 = new MyEntity {
        Text = "Entity 2"
      }; // Nothing is sent to server yet

      foreach (var e in Query.All<MyEntity>()) // Batch is sent
        Console.WriteLine("Entity.Text: {0}", e.Text); 
    }

    [Test]
    public void Test2()
    {
      var entity1 = new MyEntity {
          Text = "Entity 1"
      }; // Nothing is sent to server yet
        
      var entity2 = new MyEntity {
          Text = "Entity 2"
      }; // Nothing is sent to server yet

      var futureCount = Query.ExecuteFutureScalar(() => Query.All<MyEntity>().Count());

      foreach (var e in Query.All<MyEntity>()) // Batch is sent
        Console.WriteLine("Entity.Text: {0}", e.Text); 
      Console.WriteLine("Count: {0}", futureCount.Value);
    }
  }
}