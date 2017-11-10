// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.09.28

using System;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;
using V1 = Xtensive.Orm.Tests.Upgrade.KeyGeneratorTableCleanUpTestTestModel.Version1;
using V2 = Xtensive.Orm.Tests.Upgrade.KeyGeneratorTableCleanUpTestTestModel.Version2;

namespace Xtensive.Orm.Tests.Upgrade
{
  namespace KeyGeneratorTableCleanUpTestTestModel
  {
    namespace Version1
    {
      [HierarchyRoot]
      public class MyEntity : Entity
      {
        [Key, Field]
        public long Id { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        protected override string DetectAssemblyVersion()
        {
          return "1";
        }

        public override void OnStage()
        {
          base.OnStage();

          new MyEntity();
        }

        public override void OnComplete(Domain domain)
        {
          KeyGeneratorTableCleanUpTestTest.CheckKeyGeneratorTableIsEmpty(UpgradeContext);
        }
      }
    }

    namespace Version2
    {
      [HierarchyRoot]
      public class MyEntity : Entity
      {
        [Key, Field]
        public long Id { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        protected override string DetectAssemblyVersion()
        {
          return "2";
        }

        public override void OnStage()
        {
          base.OnStage();
          new MyEntity();
        }

        public override void OnComplete(Domain domain)
        {
          KeyGeneratorTableCleanUpTestTest.CheckKeyGeneratorTableIsEmpty(UpgradeContext);
        }

        protected override void AddUpgradeHints(Collections.ISet<UpgradeHint> hints)
        {
          hints.Add(new RenameTypeHint(typeof (V1.MyEntity).FullName, typeof (MyEntity)));
        }
      }
    }
  }

  [TestFixture]
  public class KeyGeneratorTableCleanUpTestTest
  {
#if NETCOREAPP
    [OneTimeSetUp]
#else
    [TestFixtureSetUp]
#endif
    public void TestFixtureSetUp()
    {
      Require.ProviderIs(StorageProvider.SqlServer, "Uses native SQL");
      Require.AllFeaturesNotSupported(ProviderFeatures.Sequences);
    }

    [Test]
    public void MainTest()
    {
      using (var domain = BuildInitialDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        // One entity is created in OnStage method which is called once
        Assert.That(session.Query.All<V1.MyEntity>().Count(), Is.EqualTo(1));
        tx.Complete();
      }

      using (var domain = BuildUpgradedDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        // On entity is created in OnStage method which is called twice
        // plus one entity from previous domain build.
        Assert.That(session.Query.All<V2.MyEntity>().Count(), Is.EqualTo(3));
        tx.Complete();
      }
    }

    private Domain BuildInitialDomain()
    {
      return BuildDomain(DomainUpgradeMode.Recreate, typeof (V1.MyEntity));
    }

    private Domain BuildUpgradedDomain()
    {
      return BuildDomain(DomainUpgradeMode.PerformSafely, typeof (V2.MyEntity));
    }

    private Domain BuildDomain(DomainUpgradeMode upgradeMode, Type sampleType)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(sampleType.Assembly, sampleType.Namespace);
      return Domain.Build(configuration);
    }

    public static void CheckKeyGeneratorTableIsEmpty(UpgradeContext context)
    {
      var connection = context.Connection;
      var transaction = context.Transaction;

      int count;
      using (var command = connection.CreateCommand()) {
        command.Transaction = transaction;
        command.CommandText = "select count(*) from [Int64-Generator]";
        count = Convert.ToInt32(command.ExecuteScalar());
      }

      Assert.That(count, Is.EqualTo(0));
    }
  }
}