// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.16

using System.Linq;
using NUnit.Framework;
using V1 = Xtensive.Orm.Tests.Storage.Multimapping.CrossRenameModel.Version1;
using V2 = Xtensive.Orm.Tests.Storage.Multimapping.CrossRenameModel.Version2;

namespace Xtensive.Orm.Tests.Storage.Multimapping
{
  public class CrossSchemaTableMoveTest : MultischemaTest
  {
    [Test]
    public void MainTest()
    {
      BuildInitialDomain();
      BuildUpgradedDomain();
    }

    private void BuildInitialDomain()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (V1.UpgradeHandler).Assembly, typeof (V1.UpgradeHandler).Namespace);
      var r = config.MappingRules;
      r.Map(typeof (V1.Namespace1.Renamed1).Namespace).ToSchema(Schema1Name);
      r.Map(typeof (V1.Namespace2.Renamed2).Namespace).ToSchema(Schema2Name);
      PrepareSchema(config.ConnectionInfo);
      var domain = Domain.Build(config);
      using (domain) {
        using (var session = domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          new V1.Namespace1.Renamed1 {Name = "r1"};
          new V1.Namespace2.Renamed2 {Name = "r2"};
          tx.Complete();
        }
      }
    }

    private void BuildUpgradedDomain()
    {
      var config = BuildConfiguration();
      config.Types.Register(typeof (V2.UpgradeHandler).Assembly, typeof (V2.UpgradeHandler).Namespace);
      config.UpgradeMode = DomainUpgradeMode.PerformSafely;
      var r = config.MappingRules;
      r.Map(typeof (V2.Namespace1.Renamed2).Namespace).ToSchema(Schema1Name);
      r.Map(typeof (V2.Namespace2.Renamed1).Namespace).ToSchema(Schema2Name);
      var domain = Domain.Build(config);
      using (domain) {
        using (var session = domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var r1 = session.Query.All<V2.Namespace2.Renamed1>().Single();
          var r2 = session.Query.All<V2.Namespace1.Renamed2>().Single();
          Assert.That(r1.Name, Is.EqualTo("r1"));
          Assert.That(r2.Name, Is.EqualTo("r2"));
          tx.Complete();
        }
      }
    }
  }
}