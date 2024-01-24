// Copyright (C) 2015-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2015.12.08

using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0619_InvalidExpressionInOrderByModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0619_InvalidExpressionInOrderByModel
{
  public interface IMesObject
  {
    [Key, Field]
    long ID { get; }
  }

  public abstract class MesObject : Entity, IMesObject
  {
    public long ID { get; private set; }
  }

  public class DimensionalField : Structure
  {
    
    [Field(Nullable = false, DefaultValue = "", Length = 20)]
    public string MeasureType { get; private set; }

    [Field]
    public decimal NormalizedValue { get; private set; }

    [Field(DefaultValue = 1.0)]
    public double LastUsedScale { get; private set; }
  }

  [HierarchyRoot]
  public class ProductRequirement : MesObject
  {
    [Field]
    public DimensionalField RequestedProductQuantity { get; set; }

    [Field]
    public DimensionalField QuantityToGetViaInventoryAction { get; set; }
  }

  public sealed class RowTuple
  {
    public const int FieldCount = 128;

    private object[] data = new object[FieldCount];

    public object V0 { get { return GetValue(0); } set { SetValue(0, value); } }
    public object V1 { get { return GetValue(1); } set { SetValue(1, value); } }
    public object V2 { get { return GetValue(2); } set { SetValue(2, value); } }

    public object GetValue(int i)
    {
      return data[i];
    }

    public void SetValue(int i, object value)
    {
      data[i] = value;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0619_InvalidExpressionInOrderBy : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(IMesObject).Assembly, typeof(IMesObject).Namespace);
      return configuration;
    }

    [Test]
    public void Test01()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<ProductRequirement>().Select(c => new RowTuple{
          V0 = c.ID,
          V1 = c.TypeId,
          V2 = (c.QuantityToGetViaInventoryAction.NormalizedValue / c.RequestedProductQuantity.NormalizedValue) > 0.05m
        })
        .OrderBy(c => c.V2);
        Assert.DoesNotThrow(() => query.Run());
      }
    }

    [Test]
    public void Test02()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<ProductRequirement>().Select(c => new RowTuple {
          V0 = c.ID,
          V1 = c.TypeId,
          V2 = (c.QuantityToGetViaInventoryAction.NormalizedValue / c.RequestedProductQuantity.NormalizedValue) > 0.05m
        })
          .OrderBy(c => c.V1);
        Assert.DoesNotThrow(() => query.Run());
      }
    }

    [Test]
    public void Test03()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<ProductRequirement>().Select(c => new RowTuple {
          V0 = c.ID,
          V1 = c.TypeId,
          V2 = (c.QuantityToGetViaInventoryAction.NormalizedValue / c.RequestedProductQuantity.NormalizedValue) > 0.05m
        })
        .OrderBy(c => c.V0);
        Assert.DoesNotThrow(() => query.Run());
      }
    }

    [Test]
    public void Test04()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<ProductRequirement>().Select(c => new {
          V0 = (object) c.ID,
          V1 = (object) c.TypeId,
          V2 = (object) ((c.QuantityToGetViaInventoryAction.NormalizedValue / c.RequestedProductQuantity.NormalizedValue) > 0.05m)
        }).OrderBy(el=>el.V2);

        Assert.DoesNotThrow(()=>query.Run());
      }
    }

    [Test]
    public void Test05()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<ProductRequirement>().Select(c => new {
          V0 = (object)c.ID,
          V1 = (object)c.TypeId,
          V2 = (object)((c.QuantityToGetViaInventoryAction.NormalizedValue / c.RequestedProductQuantity.NormalizedValue) > 0.05m)
        }).OrderBy(el => el.V1);

        Assert.DoesNotThrow(() => query.Run());
      }
    }

    [Test]
    public void Test06()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<ProductRequirement>().Select(c => new {
          V0 = (object)c.ID,
          V1 = (object)c.TypeId,
          V2 = (object)((c.QuantityToGetViaInventoryAction.NormalizedValue / c.RequestedProductQuantity.NormalizedValue) > 0.05m)
        }).OrderBy(el => el.V0);

        Assert.DoesNotThrow(() => query.Run());
      }
    }

    [Test]
    public void Test07()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<ProductRequirement>().Select(c => new {
          V0 = c.ID as object,
          V1 = c.TypeId as object,
          V2 = ((c.QuantityToGetViaInventoryAction.NormalizedValue / c.RequestedProductQuantity.NormalizedValue) >
                  0.05m) as object
        }).OrderBy(el => el.V2);

        Assert.DoesNotThrow(() => query.Run());
      }
    }
  }
}
