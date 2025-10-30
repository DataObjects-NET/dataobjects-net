// Copyright (C) 2013-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2013.06.25

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Issues.IssueJira0446_TypeAsOnSubqueryOperandModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0446_TypeAsOnSubqueryOperandModel
  {
    [HierarchyRoot]
    public class Owner : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field, Association(PairTo = "Owner")]
      public EntitySet<Item> Items { get; set; }
    }

    [HierarchyRoot]
    public class Item : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public Owner Owner { get; set; }
    }

    public class Item2 : Item
    {
      [Field]
      public string Info { get; set; }
    }
  }

  [TestFixture]
  public class IssueJira0446_TypeAsOnSubqueryOperand : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.RegisterCaching(typeof(Owner).Assembly, typeof(Owner).Namespace);
      return configuration;
    }

    protected override void CheckRequirements() => Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var q = session.Query.All<Owner>().Select(o => (o.Items.First() as Item2).Info).ToList();
      }
    }
  }
}