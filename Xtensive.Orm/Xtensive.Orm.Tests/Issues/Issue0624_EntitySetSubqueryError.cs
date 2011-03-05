// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.24

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0624_EntitySetSubqueryError_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Issue0624_EntitySetSubqueryError_Model
  {
    public abstract class EntityBase : Entity
    {
      [Field, Key]
      public Guid Id { get; private set; }

      protected EntityBase(Guid id)
        : base(id)
      {
      }
    }

    [HierarchyRoot]
    public class Control : EntityBase
    {
      public Control(Guid id)
        : base(id)
      {}

      [Field, Association(PairTo = "Owner")]
      public EntitySet<ControlMessage> Messages { get; private set; }
    }

    [HierarchyRoot]
    public class ControlMessage : TablePartBase<Control>
    {
      public ControlMessage(Guid id)
        : base(id)
      {
      }
    }

    public abstract class TablePartBase<T> : Entity
      where T: Entity
    {
      [Field, Key]
      public Guid Id { get; private set; }

      [Field]
      public T Owner { get; set; }

      protected TablePartBase(Guid id)
        : base(id)
      {
      }
    }
  }

  [Serializable]
  public class Issue0624_EntitySetSubqueryError : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.ProviderIsNot(StorageProvider.Oracle);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(ControlMessage).Assembly, typeof(ControlMessage).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var controlId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var control = new Control(controlId);
        var message = new ControlMessage(messageId) {Owner = control};
        session.SaveChanges();

        var ids = new[] { controlId, messageId };
        var itemsX = session.Query.All<Control>().Where(a => a.Messages.Select(b => b.Id).Any(id => ids.Contains(id))).ToList();
        Assert.AreEqual(1, itemsX.Count);
        Assert.AreSame(control, itemsX[0]);
        var itemsA = session.Query.All<Control>().Where(a => ids.Any(id => a.Messages.Select(b => b.Id).Contains(id))).ToList();
        Assert.AreEqual(1, itemsA.Count);
        Assert.AreSame(control, itemsA[0]);
        var itemsB = session.Query.All<Control>().Where(a => ids.ContainsAny(a.Messages.Select(b => b.Id))).ToList();
        Assert.AreEqual(1, itemsB.Count);
        Assert.AreSame(control, itemsB[0]);
        t.Complete();
      }  
    }
  }
}