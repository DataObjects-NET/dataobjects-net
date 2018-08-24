// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.19

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Storage.ValidationConstraintsOnGenericTypeTestModel;
using Xtensive.Orm.Validation;

namespace Xtensive.Orm.Tests.Storage
{
  namespace ValidationConstraintsOnGenericTypeTestModel
  {
    public sealed class GenericEntity<T> : EntityBase
    {
      [Field(Length = 255, DefaultValue = "")]
      [NotNullOrEmptyConstraint]
      public string Name { get; set; }

      [Field, RangeConstraint(Min = 0, Max = 100)]
      public T Value { get; set; }
    }

    [HierarchyRoot(InheritanceSchema = InheritanceSchema.ConcreteTable)]
    [Serializable]
    public abstract class EntityBase : Entity
    {
      [Field, Key]
      public long Id { get; private set; }
    }
  }

  public class ValidationConstraintsOnGenericTypeTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (GenericEntity<long>));
      return configuration;
    }

    [Test]
    public void SuccessTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entity = new GenericEntity<long>();
        entity.Name = entity.Id.ToString();
        entity.Value = 5;
        tx.Complete();
      }
    }

    [Test]
    public void Failure1Test()
    {
      Assert.Throws<ValidationFailedException>(() => {
        using (var session = Domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var entity = new GenericEntity<long>();
          entity.Name = entity.Id.ToString();
          entity.Value = -1;
          session.Validate();
          tx.Complete();
        }
      });
    }

    [Test]
    public void Failure2Test()
    {
      Assert.Throws<ValidationFailedException>(() => {
        using (var session = Domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var entity = new GenericEntity<long>();
          session.Validate();
          tx.Complete();
        }
      });
    }
  }
}