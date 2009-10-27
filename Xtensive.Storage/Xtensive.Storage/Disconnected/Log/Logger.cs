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
    private readonly IOperationLog log;
    private Entity currentEntity;
    private EntityOperationType currentOperationType;

    public void Dispose()
    {
      DetachEventHandlers();
    }

    private void AttachEventHandlers()
    {
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

    void EntitySetItemRemoveCompleted(object sender, EntitySetItemActionCompletedEventArgs e)
    {
      if (currentEntity != e.Entity || currentOperationType != EntityOperationType.RemoveItem)
        return;

      currentEntity = null;
      var operation = new EntitySetOperation(e.Entity.Key, e.EntitySet.Owner.Key, e.EntitySet.Field, EntityOperationType.RemoveItem);
      log.Register(operation);
    }

    void EntitySetItemRemoving(object sender, EntitySetItemEventArgs e)
    {
      if (currentEntity != null)
        return;

      currentEntity = e.Entity;
      currentOperationType = EntityOperationType.RemoveItem;
    }

    void EntitySetItemAddCompleted(object sender, EntitySetItemActionCompletedEventArgs e)
    {
      if (currentEntity != e.Entity || currentOperationType != EntityOperationType.AddItem)
        return;

      currentEntity = null;
      var operation = new EntitySetOperation(e.Entity.Key, e.EntitySet.Owner.Key, e.EntitySet.Field, EntityOperationType.AddItem);
      log.Register(operation);
    }

    void EntitySetItemAdding(object sender, EntitySetItemEventArgs e)
    {
      if (currentEntity != null)
        return;

      currentEntity = e.Entity;
      currentOperationType = EntityOperationType.AddItem;
    }

    void EntityRemoveCompleted(object sender, EntityRemoveCompletedEventArgs e)
    {
      if (currentEntity != e.Entity || currentOperationType != EntityOperationType.Remove)
        return;

      currentEntity = null;
      var operation = new EntityOperation(e.Entity.Key, EntityOperationType.Remove);
      log.Register(operation);
    }

    void EntityRemoving(object sender, EntityEventArgs e)
    {
      if (currentEntity != null)
        return;

      currentEntity = e.Entity;
      currentOperationType = EntityOperationType.Remove;
    }

    void EntityFieldValueSetCompleted(object sender, FieldValueSetCompletedEventArgs e)
    {
      if (currentEntity != e.Entity || currentOperationType != EntityOperationType.Update) 
        return;

      currentEntity = null;
      var operation = new UpdateEntityOperation(e.Entity.Key, e.Field, e.NewValue);
      log.Register(operation);
    }

    void EntityFieldValueSetting(object sender, FieldValueEventArgs e)
    {
      if (currentEntity != null) 
        return;

      currentEntity = e.Entity;
      currentOperationType = EntityOperationType.Update;
    }

    void EntityCreated(object sender, EntityEventArgs e)
    {
      if (currentEntity != null)
        return;

      var operation = new EntityOperation(e.Entity.Key, EntityOperationType.Create);
      log.Register(operation);
    }

    #endregion


    // Constructors

    public Logger(Session session, IOperationLog log)
    {
      this.session = session;
      this.log = log;

      AttachEventHandlers();
    }
  }
}