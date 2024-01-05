// Copyright (C) 2012-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.01.25

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Issues.IssueJira0240_SortingInSubqueryIsOmittedModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0240_SortingInSubqueryIsOmittedModel
{
  [HierarchyRoot]
  public class Container : Entity
  {
    [Field, Key]
    public long Id { get; private set; }
  }

  [HierarchyRoot]
  public class StoredContainer : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public Container Container { get; set; }

    [Field]
    public string Address { get; set; }

    [Field]
    public DateTime CreationTime { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0240_SortingInSubqueryIsOmitted : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Container).Assembly, typeof(Container).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var c = new Container();
        _ = new StoredContainer { Container = c, CreationTime = new DateTime(2012, 1, 1), Address = "1" };
        _ = new StoredContainer { Container = c, CreationTime = new DateTime(2012, 1, 2), Address = "2" };
        _ = new StoredContainer { Container = c, CreationTime = new DateTime(2012, 1, 3), Address = "3" };
        tx.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var r = session.Query.All<Container>()
          .Select(c => new {
            c.Id,
            LastLocation = Query.All<StoredContainer>()
              .Where(s => s.Container == c)
              .OrderByDescending(s => s.CreationTime)
              .Select(s => s.Address)
              .FirstOrDefault()
          })
          .OrderBy(c => c.Id)
          .Single();
        Assert.That(r.LastLocation, Is.EqualTo("3"));
      }
    }

    [Test]
    public void RegressionTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var container = session.Query.All<Container>().Single();
        var now = DateTime.Now;
        var lastStoredContainer = session.Query.All<StoredContainer>()
          .Where(s => s.Container == container && s.CreationTime <= now)
          .GroupBy(s => s.CreationTime)
          .OrderByDescending(s => s.Key)
          .FirstOrDefault();

        Assert.That(lastStoredContainer, Is.Not.Null);
        Assert.That(lastStoredContainer.Key.Day, Is.EqualTo(3));
      }
    }
  }
}