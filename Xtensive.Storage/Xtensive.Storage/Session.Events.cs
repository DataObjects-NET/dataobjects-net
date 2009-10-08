// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.29

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  public partial class Session
  {
    /// <summary>
    /// Occurs when <see cref="Session"/> is about to be disposed.
    /// </summary>
    public event EventHandler Disposing;

    /// <summary>
    /// Occurs when <see cref="Session"/> is about to <see cref="Persist"/> changes.
    /// </summary>
    public event EventHandler Persisting;

    /// <summary>
    /// Occurs when <see cref="Session"/> persisted.
    /// </summary>
    public event EventHandler Persisted;

    /// <summary>
    /// Occurs when <see cref="Entity"/> created.
    /// </summary>
    public event EventHandler<EntityEventArgs> EntityCreated;

    /// <summary>
    /// Occurs when field value is about to be read.
    /// </summary>
    public event EventHandler<FieldEventArgs> FieldValueReading;

    /// <summary>
    /// Occurs when field value is about to be read.
    /// </summary>
    public event EventHandler<FieldValueEventArgs> FieldValueRead;

    /// <summary>
    /// Occurs when is field value is about to be changed.
    /// </summary>
    public event EventHandler<FieldValueEventArgs> FieldValueChanging;

    /// <summary>
    /// Occurs when field value was changed.
    /// </summary>
    public event EventHandler<ChangeFieldValueEventArgs> FieldValueChanged;

    /// <summary>
    /// Occurs when <see cref="Entity"/> is about to remove.
    /// </summary>
    public event EventHandler<EntityEventArgs> EntityRemoving;

    /// <summary>
    /// Occurs when <see cref="Entity"/> removed.
    /// </summary>
    public event EventHandler<EntityEventArgs> EntityRemoved;


    /// <summary>
    /// The manager of <see cref="Entity"/>'s events.
    /// </summary>
    public EntityEventBroker EntityEventBroker { get; private set; }

    private void NotifyDisposing()
    {
      if (!IsSystemLogicOnly && Disposing!=null)
        Disposing(this, EventArgs.Empty);
    }

    private void NotifyPersisting()
    {
      if (!IsSystemLogicOnly && Persisting!=null)
        Persisting(this, EventArgs.Empty);
    }

    private void NotifyPersisted()
    {
      if (!IsSystemLogicOnly && Persisted!=null)
        Persisted(this, EventArgs.Empty);
    }

    internal void NotifyEntityCreated(Entity entity)
    {
      if (!IsSystemLogicOnly && EntityCreated!=null)
        EntityCreated(this, new EntityEventArgs(entity));
    }

    internal void NotifyFieldValueReading(Entity entity, FieldInfo field)
    {
      if (FieldValueReading!=null)
        FieldValueReading(this, new FieldEventArgs(entity, field));
    }

    internal void NotifyFieldValueRead(Entity entity, FieldInfo field, object value)
    {
      if (FieldValueRead!=null)
        FieldValueRead(this, new FieldValueEventArgs(entity, field, value));
    }

    internal void NotifyFieldValueChanging(Entity entity, FieldInfo field, object value)
    {
      if (FieldValueChanging!=null)
        FieldValueChanging(this, new FieldValueEventArgs(entity, field, value));
    }

    internal void NotifyFieldValueChanged(Entity entity, FieldInfo field, object oldValue, object newValue)
    {
      if (FieldValueChanged!=null)
        FieldValueChanged(this, new ChangeFieldValueEventArgs(entity, field, oldValue, newValue));
    }

    internal void NotifyEntityRemoving(Entity entity)
    {
      if (EntityRemoving!=null)
        EntityRemoving(this, new EntityEventArgs(entity));
    }

    internal void NotifyEntityRemoved(Entity entity)
    {
      if (!IsSystemLogicOnly && EntityRemoved!=null)
        EntityRemoved(this, new EntityEventArgs(entity));
    }
  }
}