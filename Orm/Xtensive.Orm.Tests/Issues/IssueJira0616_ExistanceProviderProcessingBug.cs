// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.12.04

using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0616_ExistanceProviderProcessingBugModel;

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0616_ExistanceProviderProcessingBug : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var stockFlow = new LogisticFlow();
        var margin = 2;
        Query.All<PickingProductRequirement>().Select(p => new {
          V2 = (int)(p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow==stockFlow ? margin : 1))
        }).OrderBy(t => t.V2).Run();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(InventoryAction).Assembly, typeof(InventoryAction).Namespace);
      return configuration;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0616_ExistanceProviderProcessingBugModel
{
  [HierarchyRoot]
  public class InventoryAction : MesObject
  {
    [Field]
    public Product Product { get; set; }

    [Field]
    public LogisticFlow LogisticFlow { get; set; }
  }

  [HierarchyRoot]
  public class LogisticFlow : MesObject
  {
  }

  [HierarchyRoot]
  public class PickingPoolMember : MesObject
  {
    [Field]
    public Product Product { get; private set; }

    [Field]
    public PickingProductRequirement Requirement { get; private set; }

    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<PickingPoolMemberDetail> Details { get; private set; }
  }

  [HierarchyRoot]
  public class Product : MesObject
  {
    [Field]
    public string MeasureType { get; set; }
  }

  [HierarchyRoot]
  public class PickingProductRequirement : MesObject
  {
    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Deny)]
    public Product Product { get; private set; }

    [Field]
    public DimensionalField Quantity { get; private set; }

    [Field]
    public InventoryAction InventoryAction { get; set; }
  }

  public class DimensionalField : Structure
  {
    [Field]
    public decimal NormalizedValue { get; private set; }
  }

  [HierarchyRoot]
  public class PickingPoolMemberDetail : MesObject
  {
    [Field]
    public DimensionalField Quantity { get; set; }
  }

  public abstract class MesObject : Entity
  {
    [Field, Key]
    public int ID { get; set; }
  }

}
