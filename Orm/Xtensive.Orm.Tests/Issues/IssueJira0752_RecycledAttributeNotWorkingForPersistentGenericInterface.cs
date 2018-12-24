// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2018.12.24

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Modelling.Actions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Issues
{
  using IssuesIssueJira0752_RecycledAttributeNotWorkingForPersistentGenericInterfaceModels;

  [TestFixture]
  public class IssueJira0752_RecycledAttributeNotWorkingForPersistentGenericInterface : AutoBuildTest
  {
    [Test]
    [TestCase(typeof (RecycledWithInterfaceEntity), true)]
    [TestCase(typeof (NonRecycledWithInterfaceEntity), false)]
    [TestCase(typeof (RecycledEntity), true)]
    public void MainTest(Type type, bool isRecycled)
    {
      TypeInfo typeInfo;
      Domain.Model.Types.TryGetValue(type, out typeInfo);
      var expectedTableName = (typeInfo==null ? null : typeInfo.MappingName) ?? type.Name;
      var actionSequence = Domain.Extensions.Get<ActionSequence>();
      var actions = actionSequence.Flatten().Where(x => x.Path.StartsWith("Tables/" + expectedTableName)).ToArray();

      Assert.That(actions, isRecycled ? Is.Empty : Is.Not.Empty);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (RecycledEntity).Assembly, typeof (RecycledEntity).Namespace);
      config.Types.Register(typeof (UpgradeCatcherHandler));
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    private class UpgradeCatcherHandler : UpgradeHandler
    {
      public override void OnComplete(Domain domain)
      {
        domain.Extensions.Set(UpgradeContext.SchemaUpgradeActions);
        base.OnComplete(domain);
      }
    }
  }
}

namespace Xtensive.Orm.Tests.IssuesIssueJira0752_RecycledAttributeNotWorkingForPersistentGenericInterfaceModels
{
  [Serializable]
  [HierarchyRoot]
  [Recycled]
  public class RecycledEntity : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }
  }

  interface ITestInterface<T> : IEntity
  {
  }

  [Serializable]
  [HierarchyRoot]
  [Recycled]
  public class RecycledWithInterfaceEntity :
    Entity,
    ITestInterface<RecycledWithInterfaceEntity>
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class NonRecycledWithInterfaceEntity :
    Entity,
    ITestInterface<RecycledWithInterfaceEntity>
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }
  }
}