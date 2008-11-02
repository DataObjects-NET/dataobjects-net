// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.01

using Xtensive.Core.Aspects;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Tuples;
using Xtensive.Integrity.Validation;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.PairIntegrity;

namespace Xtensive.Storage
{
  public abstract class PersistentAccessor : SessionBound
  {
    [Infrastructure]
    internal void Initialize(Persistent obj)
    {
      Domain domain = Session.Domain;
      TypeInfo type = domain.Model.Types[obj.GetType()];

      KeyGenerator keyGenerator = domain.KeyGenerators[type.Hierarchy];
      Initialize((Entity)obj, type, keyGenerator.Next());
    }

    [Infrastructure]
    internal void Initialize(Persistent obj, Tuple tuple)
    {
      Domain domain = Session.Domain;
      TypeInfo type = domain.Model.Types[obj.GetType()];
      Initialize((Entity)obj, type, tuple);
    }

    private void Initialize(Entity entity, TypeInfo type, Tuple tuple)
    {
      Key key = new Key(type, tuple);

      if (Session.IsDebugEventLoggingEnabled)
        LogTemplate<Log>.Debug("Session '{0}'. Creating entity: Key = '{1}'", Session, key);

      entity.EntityState = Session.Cache.Create(key, Session.Transaction);
    }

    [Infrastructure]
    public T GetField<T>(Persistent obj, FieldInfo field)
    {
      OnGettingField(obj, field);
      T result = field.GetAccessor<T>().GetValue(obj, field);
      OnGetField(obj, field);

      return result;
    }

    [Infrastructure]
    public void SetField<T>(Persistent obj, FieldInfo field, T value)
    {
      OnSettingField(obj, field);

      AssociationInfo association = field.Association;
      if (association!=null && association.IsPaired) {
        Key currentRef = GetKey(obj, field);
        Key newRef = null;
        Entity newEntity = (Entity) (object) value;
        if (newEntity != null)
          newRef = newEntity.Key;
        if (currentRef != newRef) {
          SyncManager.Enlist(OperationType.Set, (Entity) obj, newEntity, association);
          field.GetAccessor<T>().SetValue(obj, field, value);
        }
      }
      else
        field.GetAccessor<T>().SetValue(obj, field, value);

      OnSetField(obj, field);
    }

    [Infrastructure]
    internal Key GetKey(Persistent obj, FieldInfo field)
    {
      return EntityFieldAccessor<Entity>.ExtractKey(obj, field);
    }

    [Infrastructure]
    protected virtual void OnInitializing(Persistent obj)
    {
    }

    [Infrastructure]
    protected virtual void OnInitialized(Persistent obj)
    {
    }

    [Infrastructure]
    protected internal virtual void OnGettingField(Persistent obj, FieldInfo field)
    {
    }

    [Infrastructure]
    protected internal virtual void OnGetField(Persistent obj, FieldInfo field)
    {
    }

    [Infrastructure]
    protected internal virtual void OnSettingField(Persistent obj, FieldInfo field)
    {
    }

    [Infrastructure]
    protected internal virtual void OnSetField(Persistent obj, FieldInfo field)
    {
      if (Session.Domain.Configuration.AutoValidation)
        obj.Validate();
      obj.NotifyPropertyChanged(field);
    }

    [Infrastructure]
    protected virtual void OnRemoving(Persistent obj)
    {
    }

    [Infrastructure]
    protected virtual void OnRemoved(Persistent obj)
    {
    }

    internal PersistentAccessor(Session session)
      : base(session)
    {
    }
  }
}