// Copyright (C) 2019-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.02.14

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0761_ReadingAverageByDecimalFieldModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0761_ReadingAverageByDecimalFieldModel
{
  [HierarchyRoot]
  public class DecimalValueStructureEntityByRefCase : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public DecimalValueStructureCase Ref { get; set; }

    public DecimalValueStructureEntityByRefCase(Session session)
      : base(session)
    {
    }
  }

  public class DecimalValueStructure : Structure
  {
    [Field(Precision = 19, Scale = 5)]
    public decimal Value { get; set; }

    [Field]
    public int Code { get; set; }

    public DecimalValueStructure(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class DecimalValueStructureCase : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public DecimalValueStructure Struct { get; set; }

    [Field(Precision = 19, Scale = 5)]
    public decimal AdditionalValue { get; set; }

    public DecimalValueStructureCase(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class KeyValueByEntityRefCase : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public KeyExpressionCase Ref { get; set; }

    public KeyValueByEntityRefCase(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class KeyExpressionCase : Entity
  {
    [Field(Precision = 19, Scale = 5), Key]
    public decimal Id { get; private set; }

    [Field]
    public int SomeValue { get; set; }

    [Field(Precision = 19, Scale = 5)]
    public decimal AdditionalValue { get; set; }

    public KeyExpressionCase(Session session, decimal id)
      : base(session, id)
    {
    }
  }

  [HierarchyRoot]
  public class ValueByEntityRefCase : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public DirectFieldValueCase Ref { get; set; }

    public ValueByEntityRefCase(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class DirectFieldValueCase : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Precision = 19, Scale = 5)]
    public decimal Accepted { get; set; }

    [Field(Precision = 19, Scale = 5)]
    public decimal AdditionalValue { get; set; }

    public DirectFieldValueCase(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class Order : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Precision = 29, Scale = 2)]
    public decimal Sum { get; set; }

    [Field(Precision = 29, Scale = 1)]
    public decimal Sum2 { get; set; }

    [Field(Precision = 28, Scale = 2)]
    public decimal Sum3 { get; set; }

    [Field(Precision = 27, Scale = 3)]
    public decimal Sum4 { get; set; }

    [Field(Precision = 26, Scale = 4)]
    public decimal Sum5 { get; set; }

    [Field(Precision = 25, Scale = 5)]
    public decimal Sum6 { get; set; }

    [Field(Precision = 24, Scale = 6)]
    public decimal Sum7 { get; set; }

    [Field(Precision = 23, Scale = 7)]
    public decimal Sum8 { get; set; }

    [Field(Precision = 22, Scale = 8)]
    public decimal Sum9 { get; set; }

    [Field(Precision = 21, Scale = 9)]
    public decimal Sum10 { get; set; }

    [Field(Precision = 10, Scale = 0)]
    public decimal Count { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0761_ReadingAverageByDecimalField : AutoBuildTest
  {
    private const int OrderCount = 100;

    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.SqlServer | StorageProvider.PostgreSql);

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Order).Assembly, typeof(Order).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        for (int i = 0; i < OrderCount; i++) {
          _ = new Order() {
            Sum = (i % 2 == 0) ? 100000000000000000000000000.11m : 100000000000000000000000000.12m,
            Sum2 = 100000000000000000000000000.3m,
            Sum3 = 10000000000000000000000000.33m,
            Sum4 = 100000000000000000000000.333m,
            Sum5 = 1000000000000000000000.3333m,
            Sum6 = 10000000000000000000.33333m,
            Sum7 = 100000000000000000.333333m,
            Sum8 = 1000000000000000.3333333m,
            Sum9 = 10000000000000.33333333m,
            Sum10 = 100000000000.333333333m,
            Count = OrderCount
          };
        }

        foreach (var i in Enumerable.Range(1, 1000)) {
          _ = new ValueByEntityRefCase(session) {
            Ref = new DirectFieldValueCase(session) {
              Accepted = 163767
            }
          };

          _ = new KeyValueByEntityRefCase(session) {
            Ref = new KeyExpressionCase(session, 163767 + i)
          };

          _ = new DecimalValueStructureEntityByRefCase(session) {
            Ref = new DecimalValueStructureCase(session) {
              Struct = new DecimalValueStructure(session) {
                Value = 163767,
                Code = i
              }
            }
          };
        }

        tx.Complete();
      }
    }

    [Test]
    public void AverageComplexTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryResult = session.Query.All<Order>().Average(o => o.Sum);
        var localResult = session.Query.All<Order>().ToArray().Average(o => o.Sum);
        Assert.That(queryResult, Is.EqualTo(localResult + 0.03m));

        queryResult = session.Query.All<Order>().Average(o => o.Sum2);
        localResult = session.Query.All<Order>().ToArray().Average(o => o.Sum2);
        Assert.That(queryResult, Is.EqualTo(localResult + 0.06m));

        queryResult = session.Query.All<Order>().Average(o => o.Sum3);
        localResult = session.Query.All<Order>().ToArray().Average(o => o.Sum3);
        Assert.That(queryResult, Is.EqualTo(localResult + 0.006m));

        queryResult = session.Query.All<Order>().Average(o => o.Sum4);
        localResult = session.Query.All<Order>().ToArray().Average(o => o.Sum4);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Average(o => o.Sum5);
        localResult = session.Query.All<Order>().ToArray().Average(o => o.Sum5);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Average(o => o.Sum6);
        localResult = session.Query.All<Order>().ToArray().Average(o => o.Sum6);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Average(o => o.Sum7);
        localResult = session.Query.All<Order>().ToArray().Average(o => o.Sum7);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Average(o => o.Sum8);
        localResult = session.Query.All<Order>().ToArray().Average(o => o.Sum8);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Average(o => o.Sum9);
        localResult = session.Query.All<Order>().ToArray().Average(o => o.Sum9);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Average(o => o.Sum10);
        localResult = session.Query.All<Order>().ToArray().Average(o => o.Sum10);
        Assert.That(queryResult, Is.EqualTo(localResult));
      }
    }

    [Test]
    public void SumComplexTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryResult = session.Query.All<Order>().Sum(o => o.Sum);
        var localResult = session.Query.All<Order>().ToArray().Sum(o => o.Sum);
        Assert.That(queryResult, Is.EqualTo(localResult + 3));

        queryResult = session.Query.All<Order>().Sum(o => o.Sum2);
        localResult = session.Query.All<Order>().ToArray().Sum(o => o.Sum2);
        Assert.That(queryResult, Is.EqualTo(localResult + 6));

        queryResult = session.Query.All<Order>().Sum(o => o.Sum3);
        localResult = session.Query.All<Order>().ToArray().Sum(o => o.Sum3);
        Assert.That(queryResult, Is.EqualTo(localResult + 0.6m));

        queryResult = session.Query.All<Order>().Sum(o => o.Sum4);
        localResult = session.Query.All<Order>().ToArray().Sum(o => o.Sum4);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Sum(o => o.Sum5);
        localResult = session.Query.All<Order>().ToArray().Sum(o => o.Sum5);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Sum(o => o.Sum6);
        localResult = session.Query.All<Order>().ToArray().Sum(o => o.Sum6);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Sum(o => o.Sum7);
        localResult = session.Query.All<Order>().ToArray().Sum(o => o.Sum7);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Sum(o => o.Sum8);
        localResult = session.Query.All<Order>().ToArray().Sum(o => o.Sum8);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Sum(o => o.Sum9);
        localResult = session.Query.All<Order>().ToArray().Sum(o => o.Sum9);
        Assert.That(queryResult, Is.EqualTo(localResult));

        queryResult = session.Query.All<Order>().Sum(o => o.Sum10);
        localResult = session.Query.All<Order>().ToArray().Sum(o => o.Sum10);
        Assert.That(queryResult, Is.EqualTo(localResult));
      }
    }

    [Test]
    public void DirectFieldValueCase()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var results = session.Query.All<DirectFieldValueCase>().Sum(a => a.Accepted);
        results = session.Query.All<DirectFieldValueCase>().Sum(a => a.Accepted + a.AdditionalValue);
        results = session.Query.All<DirectFieldValueCase>().Sum(a => a.Accepted + 1m);
      }
    }

    [Test]
    public void ValueByEntityRefCase()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var results = session.Query.All<ValueByEntityRefCase>().Sum(a => a.Ref.Accepted);
      }
    }

    [Test]
    public void KeyExpressionCase()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var results = session.Query.All<KeyExpressionCase>().Sum(a => a.Id);
      }
    }

    [Test]
    public void KeyValueByEntityRefCase()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var results = session.Query.All<KeyValueByEntityRefCase>().Sum(a => a.Ref.Id);
      }
    }

    [Test]
    public void DecimalValueStructureCase()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var results = session.Query.All<DecimalValueStructureCase>().Sum(a => a.Struct.Value);
      }
    }

    [Test]
    public void DecimalValueStructureEntityByRefCase()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var results = session.Query.All<DecimalValueStructureEntityByRefCase>().Sum(a => a.Ref.Struct.Value);
      }
    }
  }
}