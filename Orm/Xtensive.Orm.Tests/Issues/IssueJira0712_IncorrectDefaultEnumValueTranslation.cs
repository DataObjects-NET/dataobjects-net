// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Julian Mamokin
// Created:    2017.09.15

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using model = Xtensive.Orm.Tests.Issues.IssueJira0712_IncorrectDefaultEnumValueTranslationModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0712_IncorrectDefaultEnumValueTranslationModel
{
  namespace ConcreteTable
  {
    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public class TestEntity : Entity
    {
      [Key, Field]
      public Guid Id { get; set; }

      [Field(DefaultValue = OneTwo.One)]
      public OneTwo Num { get; set; }

      [Field(DefaultValue = OneTwo.Two)]
      public OneTwo? NullableNum { get; set; }
    }

    public class EntDerived : TestEntity
    {}
  }

  namespace SingleTable
  {
    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public class TestEntity : Entity
    {
      [Key, Field]
      public Guid Id { get; set; }

      [Field(DefaultValue = OneTwo.One)]
      public OneTwo Num { get; set; }

      [Field(DefaultValue = OneTwo.Two)]
      public OneTwo? NullableNum { get; set; }
    }

    public class EntDerived : TestEntity
    {}
  }

  namespace ClassTable
  {
    [HierarchyRoot(InheritanceSchema.ClassTable)]
    public class TestEntity : Entity
    {
      [Key, Field]
      public Guid Id { get; set; }

      [Field(DefaultValue = OneTwo.One)]
      public OneTwo Num { get; set; }

      [Field(DefaultValue = OneTwo.Two)]
      public OneTwo? NullableNum { get; set; }
    }

    public class EntDerived : TestEntity
    {}
  }

  public enum OneTwo
  {
    One = 1,
    Two = 2,
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0712_IncorrectDefaultEnumValueTranslation
  {
    [Test]
    public void ConcreteTableTest()
    {
      var configuration = BuildConfiguration(typeof (model.ConcreteTable.TestEntity));
      Domain domain = null;
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      Key baseKey, childKey;
      using (domain) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          baseKey = new model.ConcreteTable.TestEntity().Key;
          childKey = new model.ConcreteTable.EntDerived().Key;
          transaction.Complete();
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var baseEntity = session.Query.Single<model.ConcreteTable.TestEntity>(baseKey);
          var childEntity = session.Query.Single<model.ConcreteTable.EntDerived>(childKey);
          Assert.That(baseEntity.Num, Is.EqualTo(model.OneTwo.One));
          Assert.That(baseEntity.Num, Is.EqualTo(model.OneTwo.One));
          Assert.That(baseEntity.NullableNum, Is.EqualTo(model.OneTwo.Two));
          Assert.That(baseEntity.NullableNum, Is.EqualTo(model.OneTwo.Two));

          Assert.That(childEntity.Num, Is.EqualTo(model.OneTwo.One));
          Assert.That(childEntity.Num, Is.EqualTo(model.OneTwo.One));
          Assert.That(childEntity.NullableNum, Is.EqualTo(model.OneTwo.Two));
          Assert.That(childEntity.NullableNum, Is.EqualTo(model.OneTwo.Two));
        }
      }
    }

    [Test]
    public void ClassTableTest()
    {
      var configuration = BuildConfiguration(typeof (model.ClassTable.TestEntity));
      Domain domain = null;
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      Key baseKey, childKey;
      using (domain) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          baseKey = new model.ClassTable.TestEntity().Key;
          childKey = new model.ClassTable.EntDerived().Key;
          transaction.Complete();
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var baseEntity = session.Query.Single<model.ClassTable.TestEntity>(baseKey);
          var childEntity = session.Query.Single<model.ClassTable.EntDerived>(childKey);
          Assert.That(baseEntity.Num, Is.EqualTo(model.OneTwo.One));
          Assert.That(baseEntity.Num, Is.EqualTo(model.OneTwo.One));
          Assert.That(baseEntity.NullableNum, Is.EqualTo(model.OneTwo.Two));
          Assert.That(baseEntity.NullableNum, Is.EqualTo(model.OneTwo.Two));

          Assert.That(childEntity.Num, Is.EqualTo(model.OneTwo.One));
          Assert.That(childEntity.Num, Is.EqualTo(model.OneTwo.One));
          Assert.That(childEntity.NullableNum, Is.EqualTo(model.OneTwo.Two));
          Assert.That(childEntity.NullableNum, Is.EqualTo(model.OneTwo.Two));
        }
      }
    }

    [Test]
    public void SingleTableTest()
    {
      var configuration = BuildConfiguration(typeof (model.SingleTable.TestEntity));
      Domain domain = null;
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      Key baseKey, childKey;
      using (domain) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()){
          baseKey = new model.SingleTable.TestEntity().Key;
          childKey = new model.SingleTable.EntDerived().Key;
          transaction.Complete();
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var baseEntity = session.Query.Single<model.SingleTable.TestEntity>(baseKey);
          var childEntity = session.Query.Single<model.SingleTable.EntDerived>(childKey);
          Assert.That(baseEntity.Num, Is.EqualTo(model.OneTwo.One));
          Assert.That(baseEntity.Num, Is.EqualTo(model.OneTwo.One));
          Assert.That(baseEntity.NullableNum, Is.EqualTo(model.OneTwo.Two));
          Assert.That(baseEntity.NullableNum, Is.EqualTo(model.OneTwo.Two));

          Assert.That(childEntity.Num, Is.EqualTo(model.OneTwo.One));
          Assert.That(childEntity.Num, Is.EqualTo(model.OneTwo.One));
          Assert.That(childEntity.NullableNum, Is.EqualTo(model.OneTwo.Two));
          Assert.That(childEntity.NullableNum, Is.EqualTo(model.OneTwo.Two));
        }
      }
    }

    private DomainConfiguration BuildConfiguration(Type t)
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(t.Assembly, t.Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }
  }
}
