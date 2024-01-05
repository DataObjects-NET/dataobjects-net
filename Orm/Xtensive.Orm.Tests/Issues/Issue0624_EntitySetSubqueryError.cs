// Copyright (C) 2010-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2010.03.24

using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0624_EntitySetSubqueryError_Model;
using Xtensive.Orm.Providers;

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
      Require.ProviderIsNot(StorageProvider.Oracle | StorageProvider.MySql);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(ControlMessage).Assembly, typeof(ControlMessage).Namespace);
      return config;
    }

    [Test]
    public void Test01()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var controlId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var control = new Control(controlId);
        var message = new ControlMessage(messageId) {Owner = control};
        session.SaveChanges();

        var ids = new[] { controlId, messageId };
        var itemsB = session.Query.All<Control>().Where(a => ids.ContainsAny(a.Messages.Select(b => b.Id))).ToList();
        Assert.AreEqual(1, itemsB.Count);
        Assert.AreSame(control, itemsB[0]);
        t.Complete();
      }
    }

    [Test]
    public void Test02()
    {
      Require.AnyFeatureSupported(ProviderFeatures.TemporaryTableEmulation | ProviderFeatures.TemporaryTables);

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var controlId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var control = new Control(controlId);
        var message = new ControlMessage(messageId) { Owner = control };
        session.SaveChanges();

        var ids = new[] { controlId, messageId };
        var itemsA = session.Query.All<Control>().Where(a => ids.Any(id => a.Messages.Select(b => b.Id).Contains(id))).ToList();
        Assert.AreEqual(1, itemsA.Count);
        Assert.AreSame(control, itemsA[0]);
      }
    }

    [Test]
    public void Test03()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var controlId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var control = new Control(controlId);
        var message = new ControlMessage(messageId) { Owner = control };
        session.SaveChanges();

        var ids = new[] { controlId, messageId };
        var itemsX = session.Query.All<Control>().Where(a => a.Messages.Select(b => b.Id).Any(id => ids.Contains(id))).ToList();
        Assert.AreEqual(1, itemsX.Count);
        Assert.AreSame(control, itemsX[0]);
      }
    }
  }
}