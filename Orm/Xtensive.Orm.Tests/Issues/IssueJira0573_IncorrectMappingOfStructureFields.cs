// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.02.02

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0573_IncorrectMappingOfStructureFieldsModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0573_IncorrectMappingOfStructureFieldsModel
{
  public interface IHasProcess : IEntity, IMesObject
  {
  }

  public interface IHasRecipe : IEntity, IMesObject
  {
  }

  public interface IHasConsumable : IEntity, IMesObject
  {
  }

  public interface IHasPackingMaterial : IMesObject, IEntity
  {
  }

  public interface IMesObject
  {
    [Key, Field]
    int ID { get; }
  }

  [HierarchyRoot]
  public class SalesUnitVersion : Entity, IHasProcess, IHasRecipe, IHasConsumable, IHasPackingMaterial
  {
    [Field]
    public int ID { get; set; }
  }

  [HierarchyRoot]
  public abstract class ProductUsage : MesObject
  {
    [Field]
    public Point Point { get; set; }
  }

  public class Point : Structure
  {
    [Field]
    public int X { get; set; }

    [Field(Precision = 1, Scale = 1)]
    public decimal Y { get; set; }
  }

  public class RecipeProductUsage : ProductUsage
  {
    [Field]
    public IHasRecipe Owner { get; set; }
  }

  public class ConsumableUsage : ProductUsage
  {
    [Field]
    public IHasConsumable Owner { get; set; }
  }

  public class PackingMaterialUsage : ProductUsage
  {
    [Field(Nullable = false)]
    public IHasPackingMaterial Owner { get; set; }
  }

  public abstract class MesObject : Entity, IMesObject
  {
    public int ID { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0573_IncorrectMappingOfStructureFields : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction())
      {
        IHasProcess owner = new SalesUnitVersion();
        IQueryable<ProductUsage> q = null;
        if (owner is IHasRecipe)
          q = SafeUnion(q, Query.All<RecipeProductUsage>().Where(u => u.Owner.ID == owner.ID));
        if (owner is IHasConsumable)
          q = SafeUnion(q, Query.All<ConsumableUsage>().Where(u => u.Owner.ID == owner.ID));
        if (owner is IHasPackingMaterial)
          q = SafeUnion(q, Query.All<PackingMaterialUsage>().Where(u => u.Owner.ID == owner.ID));

        if (q == null)
          throw new InvalidOperationException();

        Assert.DoesNotThrow(()=>q.ToArray());
      }
    }

    private IQueryable<ProductUsage> SafeUnion(IQueryable<ProductUsage> x, IQueryable<ProductUsage> y)
    {
      if (x == null)
        return y;
      if (y == null)
        return x;
      return x.Union(y);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var c = base.BuildConfiguration();
      c.Types.Register(typeof (SalesUnitVersion).Assembly, typeof (SalesUnitVersion).Namespace);
      c.UpgradeMode = DomainUpgradeMode.Recreate;
      return c;
    }
  }
}
