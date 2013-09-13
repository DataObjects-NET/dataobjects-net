// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.09.13

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Model.PersistentPropertyWeavingTestModel;

namespace Xtensive.Orm.Tests.Model
{
  namespace PersistentPropertyWeavingTestModel
  {
    [HierarchyRoot]
    public class Base : Entity
    {
      [Key, Field]
      public long Id { get; private set; }
    }

    // Override virtual property

    public class OverrideFieldBase : Base
    {
      [Field]
      public virtual string Value { get; set; }
    }

    public class OverrideField : OverrideFieldBase
    {
      public override string Value { get; set; }
    }

    public class GenericOverrideFieldBase<T> : Base
    {
      [Field]
      public virtual T Value { get; set; }
    }

    public class GenericOverrideField : GenericOverrideFieldBase<string>
    {
      public override string Value { get; set; }
    }

    public class GenericOverrideField<T> : GenericOverrideFieldBase<T>
    {
      public override T Value { get; set; }
    }

    // Implement persistent interface property

    public interface IImplementField : IEntity
    {
      [Field]
      string Value { get; set; }
    }

    public class ImplementField : Base, IImplementField
    {
      public string Value { get; set; }
    }

    public interface IGenericImplementField<T> : IEntity
    {
      [Field]
      T Value { get; set; }
    }

    public class GenericImplementField : Base, IGenericImplementField<string>
    {
      public string Value { get; set; }
    }

    public class GenericImplementField<T> : Base, IGenericImplementField<T>
    {
      public T Value { get; set; }
    }

    // Implement non-persistent interface property

    public interface INonPersistentImplementField
    {
      [Field]
      string Value { get; set; }
    }

    public class NonPersistentImplementField : Base, INonPersistentImplementField
    {
      public string Value { get; set; }
    }

    public interface INonPersistentGenericImplementField<T>
    {
      [Field]
      T Value { get; set; }
    }

    public class NonPersistentGenericImplementField : Base, INonPersistentGenericImplementField<string>
    {
      public string Value { get; set; }
    }

    public class NonPersistentGenericImplementField<T> : Base, INonPersistentGenericImplementField<T>
    {
      public T Value { get; set; }
    }

    // New property

    public class NewFieldBase : Base
    {
      [Field]
      public string PersistentValue { get; set; }

      [Field]
      public virtual string PersistentVirtualValue { get; set; }

      public string NonPersistentValue { get; set; }

      public static string NonPersistentStaticValue { get; set; }
    }

    public class PersistentNewField : NewFieldBase
    {
      [Field]
      public new string PersistentValue { get; set; }

      [Field]
      public new string PersistentVirtualValue { get; set; }

      [Field]
      public new string NonPersistentValue { get; set; }

      [Field]
      public new string NonPersistentStaticValue { get; set; }
    }

    public class NonPersistentNewField : NewFieldBase
    {
      public new string PersistentValue { get; set; }

      public new string PersistentVirtualValue { get; set; }

      public new string NonPersistentValue { get; set; }

      public new string NonPersistentStaticValue { get; set; }
    }
  }

  [TestFixture]
  public class PersistentPropertyWeavingTest : AutoBuildTest
  {
    private const string Hello = "Hello";
    private const string Hello1 = "Hello1";
    private const string Hello2 = "Hello2";
    private const string Hello3 = "Hello3";
    private const string Hello4 = "Hello4";
    private const string Hello5 = "Hello5";
    private const string Hello6 = "Hello6";

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Base).Assembly, typeof (Base).Namespace);
      configuration.Types.Register(typeof (GenericOverrideField<string>));
      configuration.Types.Register(typeof (GenericImplementField<string>));
      configuration.Types.Register(typeof (NonPersistentGenericImplementField<string>));
      return configuration;
    }

    [Test]
    public void OverrideFieldTest()
    {
      ExecuteTest<OverrideField>(
        e => e.Value = Hello, e => Assert.That(e.Value, Is.EqualTo(Hello)));
    }

    [Test]
    public void GenericOverrideFieldTest()
    {
      ExecuteTest<GenericOverrideField>(
        e => e.Value = Hello, e => Assert.That(e.Value, Is.EqualTo(Hello)));
    }

    [Test]
    public void GenericOverrideFieldTest2()
    {
      ExecuteTest<GenericOverrideField<string>>(
        e => e.Value = Hello, e => Assert.That(e.Value, Is.EqualTo(Hello)));
    }

    [Test]
    public void ImplementFieldTest()
    {
      ExecuteTest<ImplementField>(
        e => e.Value = Hello, e => Assert.That(e.Value, Is.EqualTo(Hello)));
    }

    [Test]
    public void GenericImplementFieldTest()
    {
      ExecuteTest<GenericImplementField>(
        e => e.Value = Hello, e => Assert.That(e.Value, Is.EqualTo(Hello)));
    }

    [Test]
    public void GenericImplementFieldTest2()
    {
      ExecuteTest<GenericImplementField<string>>(
        e => e.Value = Hello, e => Assert.That(e.Value, Is.EqualTo(Hello)));
    }

    [Test]
    public void NonPersistentImplementFieldTest()
    {
      ExecuteTest<NonPersistentImplementField>(
        e => e.Value = Hello, e => Assert.That(e.Value, Is.EqualTo(Hello)));
    }

    [Test]
    public void NonPersistentGenericImplementFieldTest()
    {
      ExecuteTest<NonPersistentGenericImplementField>(
        e => e.Value = Hello, e => Assert.That(e.Value, Is.EqualTo(Hello)));
    }

    [Test]
    public void NonPersistentGenericImplementFieldTest2()
    {
      ExecuteTest<NonPersistentGenericImplementField<string>>(
        e => e.Value = Hello, e => Assert.That(e.Value, Is.EqualTo(Hello)));
    }

    [Test]
    public void PersistentNewFieldTest()
    {
      ExecuteTest<PersistentNewField>(
        entity => {
          var baseEntity = (NewFieldBase) entity;
          baseEntity.PersistentValue = Hello1;
          baseEntity.PersistentVirtualValue = Hello2;
          entity.PersistentValue = Hello3;
          entity.PersistentVirtualValue = Hello4;
          entity.NonPersistentValue = Hello5;
          entity.NonPersistentStaticValue = Hello6;
        },
        entity => {
          var baseEntity = (NewFieldBase) entity;
          Assert.That(baseEntity.PersistentValue, Is.EqualTo(Hello1));
          Assert.That(baseEntity.PersistentVirtualValue, Is.EqualTo(Hello2));
          Assert.That(entity.PersistentValue, Is.EqualTo(Hello3));
          Assert.That(entity.PersistentVirtualValue, Is.EqualTo(Hello4));
          Assert.That(entity.NonPersistentValue, Is.EqualTo(Hello5));
          Assert.That(entity.NonPersistentStaticValue, Is.EqualTo(Hello6));
        });
    }

    [Test]
    public void NonPersistentNewFieldTest()
    {
      ExecuteTest<NonPersistentNewField>(
        entity => {
          var baseEntity = (NewFieldBase) entity;
          baseEntity.PersistentValue = Hello1;
          baseEntity.PersistentVirtualValue = Hello2;
          entity.PersistentValue = Hello3;
          entity.PersistentVirtualValue = Hello4;
          entity.NonPersistentValue = Hello5;
          entity.NonPersistentStaticValue = Hello6;
        },
        entity => {
          var baseEntity = (NewFieldBase) entity;
          Assert.That(baseEntity.PersistentValue, Is.EqualTo(Hello1));
          Assert.That(baseEntity.PersistentVirtualValue, Is.EqualTo(Hello2));
          Assert.That(entity.PersistentValue, Is.EqualTo(null));
          Assert.That(entity.PersistentVirtualValue, Is.EqualTo(null));
          Assert.That(entity.NonPersistentValue, Is.EqualTo(null));
          Assert.That(entity.NonPersistentStaticValue, Is.EqualTo(null));
        });
    }

    private void ExecuteTest<T>(Action<T> initializer, Action<T> checker)
      where T : Entity, new()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entity = new T();
        initializer.Invoke(entity);
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entity = session.Query.All<T>().Single();
        checker.Invoke(entity);
        tx.Complete();
      }
    }
  }
}