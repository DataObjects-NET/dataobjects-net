// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.04.26

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Model.OnRemoveActionForNonPairedAssociationsTestModel;

namespace Xtensive.Orm.Tests.Model
{
  namespace OnRemoveActionForNonPairedAssociationsTestModel
  {
    [HierarchyRoot]
    public class Owner : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field, Association(OnTargetRemove = OnRemoveAction.Cascade)]
      public Target T { get; set; }
    }

    [HierarchyRoot]
    public class Target : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }
  }

  public class OnRemoveActionForNonPairedAssociationsTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Owner).Assembly, typeof (Owner).Namespace);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      var owner = Domain.Model.Types[typeof (Owner)];
      var association = owner.GetOwnerAssociations().Single();

      Assert.That(association.OnOwnerRemove, Is.EqualTo(OnRemoveAction.None));
    }
  }
}