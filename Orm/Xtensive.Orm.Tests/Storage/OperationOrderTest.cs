// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.12

using System;
using System.Diagnostics;
using NUnit.Framework;

using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Reflection;
using Xtensive.Orm.Operations;
using Xtensive.Core;
using System.Linq;

namespace Xtensive.Orm.Tests.Storage.OperationOrderTest
{
  [Serializable]
  [HierarchyRoot]
  public class Book : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field]
    public EntitySet<Book> Referenecs { get; private set; }

    public override string ToString()
    {
      return Title;
    }
  }

  [TestFixture]
  public class OperationOrderTest : AutoBuildTest
  {
    private static bool disabled = false;
    private static object expectedTarget = null;
    private static string[] expectedOperations = null;
    private static int lastOperationIndex = -1;
    private static int lastOperationState = -2;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Book).Assembly, typeof(Book).Namespace);
      return configuration;
    }

    [Test]
    public void StandardTest()
    {
      using (var session = Domain.OpenSession()) {
        BindEvents(session);
        using (var tx = session.OpenTransaction()) {
          Book b;
          using (DisableEvents()) {
            b = new Book();
          }
          
          AssertOperations(b, new [] {"Entity.SetValue"});
          b.Title = "Book";

          AssertOperations(b.Referenecs, new [] {"EntitySet.AddItem"});
          b.Referenecs.Add(b);

          AssertOperations(b.Referenecs, new [] {"EntitySet.RemoveItem"});
          b.Referenecs.Remove(b);

          AssertOperations(b.Referenecs, new [] {"EntitySet.Clear"});
          b.Referenecs.Clear();

          AssertOperations(b, new [] {"Entity.Remove"});
          b.Remove();

          // tx.Complete();
        }
      }
    }

    #region Helper methods

    private static void BindEvents(Session session)
    {
      var operations = session.Operations;
      operations.OutermostOperationStarting += (s, ea) => InOperation(s, ea);
      operations.NestedOperationStarting    += (s, ea) => InOperation(s, ea);

      var events = session.Events;

      events.EntityFieldValueSetting      += (s, ea) => BeforeOperation(ea.Entity, "Entity.SetValue");
      events.EntityFieldValueSet          += (s, ea) => AfterOperation(ea.Entity, "Entity.SetValue");
      events.EntityFieldValueSetCompleted += (s, ea) => AfterOperation(ea.Entity, "Entity.SetValue");

      events.EntityRemoving        += (s, ea) => BeforeOperation(ea.Entity, "Entity.Remove");
      events.EntityRemove          += (s, ea) => AfterOperation(ea.Entity, "Entity.Remove");
      events.EntityRemoveCompleted += (s, ea) => AfterOperation(ea.Entity, "Entity.Remove");

      events.EntitySetItemAdding        += (s, ea) => BeforeOperation(ea.EntitySet, "EntitySet.AddItem");
      events.EntitySetItemAdd           += (s, ea) => AfterOperation(ea.EntitySet, "EntitySet.AddItem");
      events.EntitySetItemAddCompleted  += (s, ea) => AfterOperation(ea.EntitySet, "EntitySet.AddItem");

      events.EntitySetItemRemoving         += (s, ea) => BeforeOperation(ea.EntitySet, "EntitySet.RemoveItem");
      events.EntitySetItemRemove           += (s, ea) => AfterOperation(ea.EntitySet, "EntitySet.RemoveItem");
      events.EntitySetItemRemoveCompleted  += (s, ea) => AfterOperation(ea.EntitySet, "EntitySet.RemoveItem");

      events.EntitySetClearing        += (s, ea) => BeforeOperation(ea.EntitySet, "EntitySet.Clear");
      events.EntitySetClear           += (s, ea) => AfterOperation(ea.EntitySet, "EntitySet.Clear");
      events.EntitySetClearCompleted  += (s, ea) => AfterOperation(ea.EntitySet, "EntitySet.Clear");
    }

    private static void InOperation(object sender, OperationEventArgs ea)
    {
      if (disabled)
        return;

      object target = null;
      if (ea.Operation is EntitySetOperation) {
        var eso = (EntitySetOperation) ea.Operation;
        var entity = Session.Demand().Query.Single(eso.Key);
        target = entity.GetProperty<object>(eso.Field.Name);
      }
      else if (ea.Operation is EntityOperation) {
        var eo = (EntityOperation) ea.Operation;
        target = Session.Demand().Query.Single(eo.Key);
      }
      else if (ea.Operation is EntitiesRemoveOperation) {
        var ero = (EntitiesRemoveOperation) ea.Operation;
        target = Session.Demand().Query.Single(ero.Keys.Single());
      }
      else
        return;

      if (target!=expectedTarget)
        return;
      if ((ea.Operation.Type & OperationType.System)!=OperationType.System)
        return;

      string prefix = null;
      if (target is Entity)
        prefix = "Entity";
      else if (target is EntitySetBase)
        prefix = "EntitySet";
      else
        return;

      string suffix = null;
      if (ea.Operation.GetType()==typeof(EntityFieldSetOperation))
        suffix = "SetValue";
      else if (ea.Operation.GetType()==typeof(EntitiesRemoveOperation))
        suffix = "Remove";

      else if (ea.Operation.GetType()==typeof(EntitySetItemAddOperation))
        suffix = "AddItem";
      else if (ea.Operation.GetType()==typeof(EntitySetItemRemoveOperation))
        suffix = "RemoveItem";
      else if (ea.Operation.GetType()==typeof (EntitySetClearOperation))
        suffix = "Clear";
      else
        return;

      string operation = "{0}.{1}".FormatWith(prefix, suffix);
      InOperation(target, operation);
    }

    public static IDisposable DisableEvents()
    {
      var oldDisabled = disabled;
      disabled = true;
      return new Disposable(disposing => disabled = oldDisabled);
    }

    public static void AssertOperations(object target, string[] operations)
    {
      expectedTarget     = target;
      expectedOperations = operations;
      lastOperationIndex = -1;
      lastOperationState = -2;
    }

    public static void BeforeOperation(object target, string operation)
    {
      if (disabled)
        return;
      if (target!=expectedTarget)
        return;

      Log.Info("Before '{0}'", operation);
      var lastOperation = (lastOperationIndex < 0) ? null : expectedOperations[lastOperationIndex];
      if (lastOperation != operation) {
        if (lastOperationState!=-2 && lastOperationState!=1)
          Assert.Fail("Invalid notification order: no 'after' event.");
        lastOperationIndex++;
        lastOperation = expectedOperations[lastOperationIndex];
        if (lastOperation != operation)
          Assert.Fail("Invalid operation order: expected operation is '{0}'.", lastOperation);
        lastOperationState = -1;
      }
      else {
        if (lastOperationState>=0)
          Assert.Fail("Invalid notification order: 'before' event is unexpected here.");
      }
    }

    public static void InOperation(object target, string operation)
    {
      if (disabled)
        return;
      if (target!=expectedTarget)
        return;

      Log.Info("In     '{0}'", operation);
      var lastOperation = (lastOperationIndex < 0) ? null : expectedOperations[lastOperationIndex];
      if (lastOperation != operation)
        Assert.Fail("Invalid notification order: no 'before' event.");
      else {
        if (lastOperationState==-1)
          lastOperationState = 0;
        if (lastOperationState!=0)
          Assert.Fail("Invalid notification order: 'in' event is unexpected here.");
      }
    }

    public static void AfterOperation(object target, string operation)
    {
      if (disabled)
        return;
      if (target!=expectedTarget)
        return;

      Log.Info("After  '{0}'", operation);
      var lastOperation = (lastOperationIndex < 0) ? null : expectedOperations[lastOperationIndex];
      if (lastOperation != operation)
        Assert.Fail("Invalid notification order: no 'before' event.");
      else {
        if (lastOperationState==0)
          lastOperationState = 1;
        if (lastOperationState!=1)
          Assert.Fail("Invalid notification order: 'after' event is unexpected here.");
      }
    }

    #endregion
  }
}