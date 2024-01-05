// Copyright (C) 2013-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2013.02.22

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Issues.IssueJira0437_OperationsWithListOfIntModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0437_OperationsWithListOfIntModel
  {
    [HierarchyRoot]
    public class NamedObject : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }
  }

  public class IssueJira0437_OperationsWithListOfInt : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (NamedObject));
      return configuration;
    }

    protected override void CheckRequirements()
      => Require.AnyFeatureSupported(ProviderFeatures.TemporaryTableEmulation | ProviderFeatures.TemporaryTables);

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = new NamedObject {Name = "One"};
        _ = new NamedObject {Name = "Two"};
        _ = new NamedObject {Name = "Three"};
        tx.Complete();
      }
    }

    [Test]
    public void GroupJoinTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.TemporaryTableEmulation | ProviderFeatures.TemporaryTables);
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var items = new List<int> {2, 3};
        var query =
          from o in session.Query.All<NamedObject>()
          join i in items on o.Id equals i
            into j
          select o;
        var result = query.ToList();
        Assert.That(result.Count, Is.EqualTo(3));
      }
    }

    [Test]
    public void JoinTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.TemporaryTableEmulation | ProviderFeatures.TemporaryTables);
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var items = new List<int> {1, 2};
        var query =
          from o in session.Query.All<NamedObject>()
          join i in items on o.Id equals i
          orderby o.Id
          select o;

        var result = query.ToList();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Id, Is.EqualTo(1));
        Assert.That(result[1].Id, Is.EqualTo(2));
      }
    }

    [Test]
    public void ApplyTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.TemporaryTableEmulation | ProviderFeatures.TemporaryTables);
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var items = new List<int> {1, 2, 3};

        var query =
          from o in session.Query.All<NamedObject>()
          from i in items
          select new {Object = o, Item = i};

        Assert.That(query.Count(), Is.EqualTo(9));
      }
    }
  }
}