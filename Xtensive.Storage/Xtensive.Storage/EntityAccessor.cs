// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.01

using System;
using Xtensive.Core.Diagnostics;
using Xtensive.Integrity.Validation;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  internal sealed class EntityAccessor : PersistentAccessor
  {
    protected override void OnInitializing(Persistent obj)
    {
      base.OnInitializing(obj);
      var entity = (Entity) obj;
      entity.EntityState.Entity = entity;
      if (entity.PersistenceState!=PersistenceState.New) {
        Session.newEntities.Add(entity.EntityState);
        entity.EntityState.PersistenceState = PersistenceState.New;
      }
      obj.Validate();
    }

    protected internal override void OnGettingField(Persistent obj, FieldInfo field)
    {
      base.OnGettingField(obj, field);
      var entity = (Entity) obj;
      if (Session.IsDebugEventLoggingEnabled)
        LogTemplate<Log>.Debug("Session '{0}'. Getting value: Key = '{1}', Field = '{2}'", Session, entity.Key, field);
      entity.EntityState.EnsureIsActual();
      entity.EntityState.EnsureNotRemoved();
      obj.EnsureIsFetched(field);
    }

    protected internal override void OnSettingField(Persistent obj, FieldInfo field)
    {
      base.OnSettingField(obj, field);
      var entity = (Entity) obj;
      if (Session.IsDebugEventLoggingEnabled)
        LogTemplate<Log>.Debug("Session '{0}'. Setting value: Key = '{1}', Field = '{2}'", Session, entity.Key, field);
      if (field.IsPrimaryKey)
        throw new NotSupportedException(string.Format(
          Strings.ExUnableToSetKeyFieldXExplicitly, field.Name));
      entity.EntityState.EnsureIsActual();
      entity.EntityState.EnsureNotRemoved();
    }

    protected internal override void OnSetField(Persistent obj, FieldInfo field)
    {
      base.OnSetField(obj, field);
      var entity = (Entity) obj;
      if (entity.PersistenceState!=PersistenceState.New && 
          entity.PersistenceState!=PersistenceState.Modified) {
        Session.modifiedEntities.Add(entity.EntityState);
        entity.EntityState.PersistenceState = PersistenceState.Modified;
      }
      base.OnSetField(obj, field);
    }


    // Constructors

    public EntityAccessor(Session session)
      : base(session)
    {
    }
  }
}