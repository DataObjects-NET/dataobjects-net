// Copyright (C) 2009-20223 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.10.15

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0435_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0435_Model
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

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0435_BatchingFail : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (MyEntity));
      return configuration;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      _ = CreateSessionAndTransaction();
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

      foreach (var e in Session.Demand().Query.All<MyEntity>()) // Batch is sent
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

      var futureCount = Session.Demand().Query.CreateDelayedQuery(qe => qe.All<MyEntity>().Count());

      foreach (var e in Session.Demand().Query.All<MyEntity>()) // Batch is sent
        Console.WriteLine("Entity.Text: {0}", e.Text); 
      Console.WriteLine("Count: {0}", futureCount.Value);
    }
  }
}