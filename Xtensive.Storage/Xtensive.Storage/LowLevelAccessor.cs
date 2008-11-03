// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.02

using System;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Tuples;
using Xtensive.Integrity.Validation;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.PairIntegrity;
using Xtensive.Storage.ReferentialIntegrity;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage
{
  public sealed class LowLevelAccessor : SessionBound
  {
    #region Public methods

    [Infrastructure]
    public Persistent CreateInstance(Type type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      
      TypeInfo typeInfo = Session.Domain.Model.Types[type];
      if (typeInfo.IsEntity) {
        KeyGenerator keyGenerator = Session.Domain.KeyGenerators[typeInfo.Hierarchy];
        Key key = new Key(typeInfo.Hierarchy, keyGenerator.Next());
        EntityState state = Session.Cache.Add(key);
        Initialize(state.Entity, state);
        return state.Entity;
      }
      throw new NotImplementedException();
    }

    [Infrastructure]
    public Persistent CreateInstance(Type type, Tuple tuple)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNull(tuple, "tuple");

      TypeInfo typeInfo = Session.Domain.Model.Types[type];
      if (typeInfo.IsEntity) {
        Key key = new Key(typeInfo.Hierarchy, tuple);
        EntityState state = Session.Cache.Add(key);
        Initialize(state.Entity, state);
        return state.Entity;
      }
      throw new NotImplementedException();
    }

    [Infrastructure]
    public T GetField<T>(Persistent target, FieldInfo field)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentNotNull(field, "field");

      OnGettingField(target, field);
      T result = field.GetAccessor<T>().GetValue(target, field);
      OnGetField(target, field);

      return result;
    }

    [Infrastructure]
    public void SetField<T>(Persistent target, FieldInfo field, T value)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentNotNull(field, "field");

      OnSettingField(target, field);

      AssociationInfo association = field.Association;
      if (association!=null && association.IsPaired) {
        Key currentRef = GetKey(target, field);
        Key newRef = null;
        Entity newEntity = (Entity) (object) value;
        if (newEntity != null)
          newRef = newEntity.Key;
        if (currentRef != newRef) {
          SyncManager.Enlist(OperationType.Set, (Entity) target, newEntity, association);
          field.GetAccessor<T>().SetValue(target, field, value);
        }
      }
      else
        field.GetAccessor<T>().SetValue(target, field, value);

      OnSetField(target, field);
    }

    [Infrastructure]
    public Key GetKey(Persistent target, FieldInfo field)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentNotNull(field, "field");

      // TODO: Refactor
      return EntityFieldAccessor<Entity>.ExtractKey(target, field);
    }

    [Infrastructure]
    public void Remove(Entity target)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");

      if (Session.IsDebugEventLoggingEnabled)
        LogTemplate<Log>.Debug("Session '{0}'. Removing: Key = '{1}'", Session, target.Key);

      target.EntityState.EnsureIsActual();
      target.EntityState.EnsureNotRemoved();

      Session.Persist();
      ReferenceManager.ClearReferencesTo(target);
      target.EntityState.PersistenceState = PersistenceState.Removed;
      Session.State.Register(target.EntityState);
      Session.Cache.Remove(target.EntityState);
    }

    [Infrastructure]
    public RecordSet GetRecordSet(EntitySet target)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");

      throw new NotImplementedException();
    }

    [Infrastructure]
    public bool Add(EntitySet target, Entity item)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentNotNull(item, "item");

      throw new NotImplementedException();
    }

    [Infrastructure]
    public bool Remove(EntitySet target, Entity item)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      ArgumentValidator.EnsureArgumentNotNull(item, "item");

      throw new NotImplementedException();
    }

    [Infrastructure]
    public void Clear(EntitySet target)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");

      throw new NotImplementedException();
    }

    #endregion

    #region Entity initialization methods

    [Infrastructure]
    internal void Initialize(Entity target)
    {
      TypeInfo type = Session.Domain.Model.Types[target.GetType()];
      KeyGenerator keyGenerator = Session.Domain.KeyGenerators[type.Hierarchy];
      Initialize(target, type, keyGenerator.Next());
    }

    [Infrastructure]
    internal void Initialize(Entity target, Tuple tuple)
    {
      TypeInfo type = Session.Domain.Model.Types[target.GetType()];
      Initialize(target, type, tuple);
    }

    private void Initialize(Entity target, TypeInfo type, Tuple tuple)
    {
      Key key = new Key(type, tuple);

      if (Session.IsDebugEventLoggingEnabled)
        LogTemplate<Log>.Debug("Session '{0}'. Creating entity: Key = '{1}'", Session, key);

      var state = Session.Cache.Add(key, target);
      Initialize(target, state);
    }

    private void Initialize(Entity target, EntityState state)
    {
      target.EntityState = state;
      state.PersistenceState = PersistenceState.New;
      Session.State.Register(state);
      target.Initialize();
    }

    #endregion

    #region Before/After methods

    [Infrastructure]
    private void OnGettingField(Persistent target, FieldInfo field)
    {
      Structure structure = target as Structure;
      Entity entity = target as Entity;

      if (structure != null) {
        if (structure.Owner!=null)
          structure.Owner.Accessor.OnGettingField(structure.Owner, structure.Field);
      }
      else {
        if (Session.IsDebugEventLoggingEnabled)
          LogTemplate<Log>.Debug("Session '{0}'. Getting value: Key = '{1}', Field = '{2}'", Session, entity.Key, field);
        entity.EntityState.EnsureIsActual();
        entity.EntityState.EnsureNotRemoved();
        target.EnsureIsFetched(field);
      }
    }

    [Infrastructure]
    private void OnGetField(Persistent target, FieldInfo field)
    {
      Structure structure = target as Structure;

      if (structure != null) {
        if (structure.Owner!=null)
          structure.Owner.Accessor.OnGetField(structure.Owner, structure.Field);
      }
    }

    [Infrastructure]
    private void OnSettingField(Persistent target, FieldInfo field)
    {
      Structure structure = target as Structure;
      Entity entity = target as Entity;

      if (structure != null) {
        if (structure.Owner!=null)
          structure.Owner.Accessor.OnSettingField(structure.Owner, structure.Field);
      }
      else {
        if (Session.IsDebugEventLoggingEnabled)
          LogTemplate<Log>.Debug("Session '{0}'. Setting value: Key = '{1}', Field = '{2}'", Session, entity.Key, field);
        if (field.IsPrimaryKey)
          throw new NotSupportedException(string.Format(Strings.ExUnableToSetKeyFieldXExplicitly, field.Name));
        entity.EntityState.EnsureIsActual();
        entity.EntityState.EnsureNotRemoved();
      }
    }

    [Infrastructure]
    private void OnSetField(Persistent target, FieldInfo field)
    {
      Structure structure = target as Structure;
      Entity entity = target as Entity;

      if (structure != null) {
        if (structure.Owner!=null)
          structure.Owner.Accessor.OnSetField(structure.Owner, structure.Field);
      }
      else {
        if (entity.PersistenceState!=PersistenceState.New && entity.PersistenceState!=PersistenceState.Modified) {
          entity.EntityState.PersistenceState = PersistenceState.Modified;
          Session.State.Register(entity.EntityState);
        }
      }

      if (Session.Domain.Configuration.AutoValidation)
        target.Validate();
    }

    [Infrastructure]
    private void OnRemoving(Persistent target)
    {
    }

    [Infrastructure]
    private void OnRemoved(Persistent target)
    {
    }

    #endregion


    // Constructor

    internal LowLevelAccessor(Session session)
      : base(session)
    {
    }
  }
}