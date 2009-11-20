// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Diagnostics;
using Xtensive.Storage.Disconnected.Log.Operations;

namespace Xtensive.Storage.Disconnected.Log
{
  public class Logger : IDisposable
  {
    private readonly Session session;
    private readonly IOperationSet set;
    private Entity currentEntity;
    private OperationType currentOperationType;

    public void Dispose()
    {
      DetachEventHandlers();
    }

    private void AttachEventHandlers()
    {
      session.LocalKeyCreated += LocalKeyCreated;
      session.EntityCreated += EntityCreated;
      session.EntityFieldValueSetting += EntityFieldValueSetting;
      session.EntityFieldValueSetCompleted += EntityFieldValueSetCompleted;
      session.EntityRemoving += EntityRemoving;
      session.EntityRemoveCompleted += EntityRemoveCompleted;
      session.EntitySetItemAdding += EntitySetItemAdding;
      session.EntitySetItemAddCompleted += EntitySetItemAddCompleted;
      session.EntitySetItemRemoving += EntitySetItemRemoving;
      session.EntitySetItemRemoveCompleted += EntitySetItemRemoveCompleted;
    }

    private void DetachEventHandlers()
    {
      session.LocalKeyCreated -= LocalKeyCreated;
      session.EntityCreated -= EntityCreated;
      session.EntityFieldValueSetting -= EntityFieldValueSetting;
      session.EntityFieldValueSetCompleted -= EntityFieldValueSetCompleted;
      session.EntityRemoving -= EntityRemoving;
      session.EntityRemoveCompleted -= EntityRemoveCompleted;
      session.EntitySetItemAdding -= EntitySetItemAdding;
      session.EntitySetItemAddCompleted -= EntitySetItemAddCompleted;
      session.EntitySetItemRemoving -= EntitySetItemRemoving;
      session.EntitySetItemRemoveCompleted -= EntitySetItemRemoveCompleted;
    }

    #region Session event handlers

    void LocalKeyCreated(object sender, KeyEventArgs e)
    {
      set.RegisterKeyForRemap(e.Key);
    }

    void EntitySetItemRemoveCompleted(object sender, EntitySetItemActionCompletedEventArgs e)
    {
      if (currentEntity != e.Entity || currentOperationType != OperationType.RemoveEntitySetItem)
        return;

      currentEntity = null;
      var operation = new EntitySetItemOperation(e.EntitySet.Owner.Key, e.EntitySet.Field, OperationType.RemoveEntitySetItem, e.Entity.Key);
      set.Register(operation);
    }

    void EntitySetItemRemoving(object sender, EntitySetItemEventArgs e)
    {
      if (currentEntity != null)
        return;

      currentEntity = e.Entity;
      currentOperationType = OperationType.RemoveEntitySetItem;
    }

    void EntitySetItemAddCompleted(object sender, EntitySetItemActionCompletedEventArgs e)
    {
      if (currentEntity != e.Entity || currentOperationType != OperationType.AddEntitySetItem)
        return;

      currentEntity = null;
      var operation = new EntitySetItemOperation(e.EntitySet.Owner.Key, e.EntitySet.Field, OperationType.AddEntitySetItem, e.Entity.Key);
      set.Register(operation);
    }

    void EntitySetItemAdding(object sender, EntitySetItemEventArgs e)
    {
      if (currentEntity != null)
        return;

      currentEntity = e.Entity;
      currentOperationType = OperationType.AddEntitySetItem;
    }

    void EntityRemoveCompleted(object sender, EntityRemoveCompletedEventArgs e)
    {
      if (currentEntity != e.Entity || currentOperationType != OperationType.RemoveEntity)
        return;

      currentEntity = null;
      var operation = new EntityOperation(e.Entity.Key, OperationType.RemoveEntity);
      set.Register(operation);
    }

    void EntityRemoving(object sender, EntityEventArgs e)
    {
      if (currentEntity != null)
        return;

      currentEntity = e.Entity;
      currentOperationType = OperationType.RemoveEntity;
    }

    void EntityFieldValueSetCompleted(object sender, FieldValueSetCompletedEventArgs e)
    {
      if (currentEntity != e.Entity || currentOperationType != OperationType.SetEntityField) 
        return;

      currentEntity = null;
      var operation = new EntityFieldSetOperation(e.Entity.Key, e.Field, e.NewValue);
      set.Register(operation);
    }

    void EntityFieldValueSetting(object sender, FieldValueEventArgs e)
    {
      if (currentEntity != null) 
        return;

      currentEntity = e.Entity;
      currentOperationType = OperationType.SetEntityField;
    }

    void EntityCreated(object sender, EntityEventArgs e)
    {
      if (currentEntity != null)
        return;

      var operation = new EntityOperation(e.Entity.Key, OperationType.CreateEntity);
      set.Register(operation);
    }

    #endregion


    // Constructors

    public Logger(Session session, IOperationSet set)
    {
      this.session = session;
      this.set = set;

      AttachEventHandlers();
    }
  }
}