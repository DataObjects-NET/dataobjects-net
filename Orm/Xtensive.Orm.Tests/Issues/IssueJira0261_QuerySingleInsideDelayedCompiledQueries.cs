// Copyright (C) 2013-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2013.07.28

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
    public void QuerySingle()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        var expected = new Exception(Strings.ExNonLinqCallsAreNotSupportedWithinQueryExecuteDelayed);

        Assert.Throws<NotSupportedException>(() => {
          var result = session.Query.CreateDelayedQuery(query => query.SingleOrDefault<TestEntity>(instanceKey));
          Assert.That(result.Value.FirstName, Is.EqualTo("Jeremy"));
        });
      }
    }
  }
}
