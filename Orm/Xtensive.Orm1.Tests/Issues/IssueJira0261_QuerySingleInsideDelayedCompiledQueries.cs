// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.07.28

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0261_QuerySingleInsideDelayedCompiledQueriesModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0261_QuerySingleInsideDelayedCompiledQueriesModel
{
  [HierarchyRoot]
  class TestEntity : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0261_QuerySingleInsideDelayedCompiledQueries : AutoBuildTest
  {
    private Key instanceKey;
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(TestEntity).Assembly, typeof(TestEntity).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()){
        var instance = new TestEntity {FirstName = "Jeremy", LastName = "Clarkson"};
        instanceKey = instance.Key;
        transaction.Complete();
      }
    }
    
    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void QuerySingle()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        var expected = new Exception(Strings.ExNonLinqCallsAreNotSupportedWithinQueryExecuteDelayed);

        var result = session.Query.ExecuteDelayed(query => query.SingleOrDefault<TestEntity>(instanceKey));

        Assert.That(result.Value.FirstName, Is.EqualTo("Jeremy"));
      }
    }
  }
}
