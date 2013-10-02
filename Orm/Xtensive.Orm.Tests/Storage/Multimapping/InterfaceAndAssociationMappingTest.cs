// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.11.14

using System;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Orm.Tests.Storage.Multimapping.InterfaceAndAssociationMappingTestModel.Entities;
using Xtensive.Orm.Tests.Storage.Multimapping.InterfaceAndAssociationMappingTestModel.Interfaces;
using Xtensive.Orm.Tests.Storage.Multimapping.InterfaceAndAssociationMappingTestModel.Upgrade;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Storage.Multimapping
{
  namespace InterfaceAndAssociationMappingTestModel
  {
    namespace Interfaces
    {
      public interface IMyEntity : IEntity
      {
        [Field, Association(PairTo = "Owner")]
        EntitySet<MyEntityElement> Elements { get; }
      }
    }

    namespace Entities
    {
      [HierarchyRoot]
      public class MyEntityElement : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public IMyEntity Owner { get; set; }
      }

      [HierarchyRoot]
      public class EntityToKeep : Entity, IMyEntity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public EntitySet<MyEntityElement> Elements { get; private set; }
      }

      [HierarchyRoot]
      public class EntityToRemove : Entity, IMyEntity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public EntitySet<MyEntityElement> Elements { get; private set; }
      }
    }

    namespace Upgrade
    {
      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new RemoveTypeHint(typeof (EntityToRemove).FullName));
        }
      }
    }
  }

  public class InterfaceAndAssociationMappingTest : MultidatabaseTest
  {
    [Test]
    public void MainTest()
    {
      using (var initialDomain = BuildInitialDomain()) {
        CheckIMyEntityMapping(initialDomain);
      }

      using (var upgradedDomain = BuildUpgradedDomain()) {
        CheckIMyEntityMapping(upgradedDomain);
      }
    }

    private void CheckIMyEntityMapping(Domain domain)
    {
      var interfaceType = domain.Model.Types[typeof (IMyEntity)];
      var implementorType = domain.Model.Types[typeof (EntityToKeep)];
      Assert.That(interfaceType.MappingDatabase, Is.EqualTo(implementorType.MappingDatabase));
    }

    private Domain BuildUpgradedDomain()
    {
      return BuildDomain(DomainUpgradeMode.PerformSafely, typeof (Upgrader));
    }

    private Domain BuildInitialDomain()
    {
      return BuildDomain(DomainUpgradeMode.Recreate, typeof (EntityToRemove));
    }

    private Domain BuildDomain(DomainUpgradeMode upgradeMode, Type type)
    {
      var configuration = BuildConfiguration();
      configuration.UpgradeMode = upgradeMode;

      configuration.Types.Register(typeof (IMyEntity));
      configuration.Types.Register(typeof (MyEntityElement));
      configuration.Types.Register(typeof (EntityToKeep));
      configuration.Types.Register(type);

      configuration.MappingRules.Map(typeof (IMyEntity).Namespace).ToDatabase(Database1Name);
      configuration.MappingRules.Map(typeof (EntityToKeep).Namespace).ToDatabase(Database2Name);
      return Domain.Build(configuration);
    }
  }
}