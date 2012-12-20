// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.19

using System;
using NUnit.Framework;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Storage.ValidationConstraintsOnGenericTypeTestModel;
using Xtensive.Orm.Validation;
using Xtensive.Testing;

namespace Xtensive.Orm.Tests.Storage
{
  namespace ValidationConstraintsOnGenericTypeTestModel
  {
    public sealed class Folder<T> : EntityBase
    {
      [Field(Length = 255, DefaultValue = "")]
      [NotNullOrEmptyConstraint]
      public string Name { get; set; }

      [Field]
      [Association(PairTo = "Parent")]
      public EntitySet<Folder<T>> Folders { get; set; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Deny)]
      public Folder<T> Parent { get; set; }
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
    protected override Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Folder<int>));
      return configuration;
    }

    [Test, ExpectedException(typeof (Core.AggregateException))]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entity = new Folder<int>();
        tx.Complete();
      }
    }
  }
}