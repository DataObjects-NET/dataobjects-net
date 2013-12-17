// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.12.13

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.NullableEnumQueryTestModel;

namespace Xtensive.Orm.Tests.Linq
{
  namespace NullableEnumQueryTestModel
  {
    public enum MyEnum
    {
      Foo = 0,
      Bar = 1,
      Qux = 2,
    }

    [HierarchyRoot]
    public class EntityWithNullableEnum : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public MyEnum? Value { get; set; }
    }
  }

  [TestFixture]
  public class NullableEnumMaterializationTest : AutoBuildTest
  {
    protected override Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (EntityWithNullableEnum));
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new EntityWithNullableEnum {Value = MyEnum.Foo};
        new EntityWithNullableEnum {Value = MyEnum.Bar};
        new EntityWithNullableEnum {Value = null};
        tx.Complete();
      }
    }

    [Test]
    public void SelectContainsTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var values = new[] {MyEnum.Bar, MyEnum.Foo};
        var query = session.Query.All<EntityWithNullableEnum>()
          .Select(e => new {
            Id = e.Id,
            Match = e.Value.HasValue && values.Contains(e.Value.Value)
          });
        var result = query.ToList();
        Assert.That(result.Count(r => r.Match), Is.EqualTo(2));
        tx.Complete();
      }
    }

    [Test]
    public void SelectConditionTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var values = new[] {MyEnum.Bar, MyEnum.Foo};
        var query = session.Query.All<EntityWithNullableEnum>()
          .Select(e => new {
            Id = e.Id,
            Match = e.Value.HasValue && (e.Value.Value==MyEnum.Foo || e.Value.Value==MyEnum.Bar)
          });
        var result = query.ToList();
        Assert.That(result.Count(r => r.Match), Is.EqualTo(2));
        tx.Complete();
      }
    }
  }
}