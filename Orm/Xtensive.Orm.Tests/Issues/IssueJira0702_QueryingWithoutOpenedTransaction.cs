using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0702_QueryingWithoutOpenedTransactionModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0702_QueryingWithoutOpenedTransactionModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Text { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0702_QueryingWithoutOpenedTransaction: AutoBuildTest
  {
    [Test]
    public void NonTransactionalReadsIsEnabledTest()
    {
      var sessionConfiguration = new SessionConfiguration(SessionOptions.ServerProfile | SessionOptions.NonTransactionalReads);

      using (var session = Domain.OpenSession(sessionConfiguration))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          new TestEntity {Text = "abc"};
        }

        Assert.DoesNotThrow(() => Query.All<TestEntity>().Any());
        Assert.DoesNotThrow(() => session.Query.All<TestEntity>().Any());
      }
    }

    [Test]
    public void NonTransactionalReadsIsDisabledTest()
    {
      var sessionConfiguration = new SessionConfiguration(SessionOptions.ServerProfile);

      using (var session = Domain.OpenSession(sessionConfiguration))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          new TestEntity { Text = "abc" };
        }

        Assert.Throws<InvalidOperationException>(() => Query.All<TestEntity>().Any());
        Assert.Throws<InvalidOperationException>(() => session.Query.All<TestEntity>().Any());
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TestEntity).Assembly, typeof (TestEntity).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}
