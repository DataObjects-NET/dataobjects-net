// Copyright (C) 2012-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.07.05

using System;
using NUnit.Framework;
using Xtensive.Orm.Upgrade;
using V1 = Xtensive.Orm.Tests.Upgrade.RemoveColumnWithRenameTableTestModel.Version1;
using V2 = Xtensive.Orm.Tests.Upgrade.RemoveColumnWithRenameTableTestModel.Version2;

namespace Xtensive.Orm.Tests.Upgrade
{
  namespace RemoveColumnWithRenameTableTestModel.Version1
  {
    [HierarchyRoot]
    public class EntityToRemove : Entity
    {
      [Field, Key]
      public long Id { get; private set; }
    }

    [HierarchyRoot]
    public class EntityWithFieldToRemove : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public EntityToRemove Ref { get; set; }
    }

    public class Upgrader : UpgradeHandler
    {
      protected override string DetectAssemblyVersion()
      {
        return "1";
      }
    }
  }

  namespace RemoveColumnWithRenameTableTestModel.Version2
  {
    [HierarchyRoot]
    public class EntityWithRemovedField : Entity
    {
      [Field, Key]
      public long Id { get; private set; }
    }

    public class Upgrader : UpgradeHandler
    {
      protected override string DetectAssemblyVersion()
      {
        return "2";
      }

      public override bool CanUpgradeFrom(string oldVersion)
      {
        return true;
      }

      protected override void AddUpgradeHints(Collections.ISet<UpgradeHint> hints)
      {
        _ = hints.Add(new RenameTypeHint(typeof (V1.EntityWithFieldToRemove).FullName, typeof (EntityWithRemovedField)));
        _ = hints.Add(new RemoveTypeHint(typeof (V1.EntityToRemove).FullName));
        _ = hints.Add(new RemoveFieldHint(typeof (V1.EntityWithFieldToRemove).FullName, "Ref"));
      }
    }
  }

  [TestFixture]
  public class RemoveColumnWithRenameTableTest
  {
    private Domain BuildDomain(Type sampleType, DomainUpgradeMode mode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      configuration.Types.Register(sampleType.Assembly, sampleType.Namespace);
      return Domain.Build(configuration);
    }

    [Test]
    [IgnoreIfGithubActions(StorageProvider.Firebird)]
    public void MainTest()
    {
      using (var domain = BuildDomain(typeof (V1.Upgrader), DomainUpgradeMode.Recreate))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = new V1.EntityWithFieldToRemove {Ref = new V1.EntityToRemove()};
        tx.Complete();
      }

      using (var domain = BuildDomain(typeof (V2.Upgrader), DomainUpgradeMode.PerformSafely))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var count = session.Query.All<V2.EntityWithRemovedField>().Count();
        Assert.That(count, Is.EqualTo(1));
        tx.Complete();
      }
    }
  }
}