// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Tuples;
using Xtensive.Integrity.Atomicity;
using Xtensive.Integrity.Validation;
using Xtensive.Integrity.Validation.Interfaces;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.PairIntegrity;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage
{
  public abstract class Persistent : SessionBound,
    IAtomicityAware,
    IValidationAware,
    INotifyPropertyChanged
  {
    private Dictionary<FieldInfo, IFieldHandler> fieldHandlers;

    /// <summary>
    /// Gets the type of this instance.
    /// </summary>
    [Infrastructure]
    public abstract TypeInfo Type { get; }

    [Infrastructure]
    protected internal abstract Tuple Data { get; }

    [Infrastructure]
    internal Dictionary<FieldInfo, IFieldHandler> FieldHandlers
    {
      get
      {
        if (fieldHandlers==null)
          fieldHandlers = new Dictionary<FieldInfo, IFieldHandler>();
        return fieldHandlers;
      }
    }

    [Infrastructure]
    internal abstract void EnsureIsFetched(FieldInfo field);

    #region this[...], GetProperty, SetProperty members

    [Infrastructure]
    public object this[string name]
    {
      get { return GetProperty<object>(name); }
      set { SetProperty(name, value); }
    }

    [Infrastructure]
    public T GetProperty<T>(string name)
    {
      FieldInfo field = Type.Fields[name];
      // TODO: Improve (use DelegateHelper)
      if (field.UnderlyingProperty!=null)
        return (T) field.UnderlyingProperty.GetValue(this, null);
      return GetField<T>(name);
    }

    [Infrastructure]
    public void SetProperty<T>(string name, T value)
    {
      FieldInfo field = Type.Fields[name];
      // TODO: Improve (use DelegateHelper)
      if (field.UnderlyingProperty!=null)
        field.UnderlyingProperty.SetValue(this, value, null);
      else
        SetField(name, value);
    }

    #endregion

    #region Public user-level GetField, SetField members

    [Infrastructure]
    protected T GetField<T>(string name)
    {
      return GetField<T>(Type.Fields[name]);
    }

    [Infrastructure]
    protected T GetField<T>(FieldInfo field)
    {
      return GetField<T>(field, true);
    }

    [Infrastructure]
    protected void SetField<T>(string name, T value)
    {
      SetField(Type.Fields[name], value);
    }

    [Infrastructure]
    protected void SetField<T>(FieldInfo field, T value)
    {
      SetField(field, value, true);
    }

    #endregion

    #region Protected user-level event-like members

    [Infrastructure]
    protected virtual void OnInitialize()
    {
    }

    [Infrastructure]
    protected virtual void OnGettingField(FieldInfo field)
    {
    }

    [Infrastructure]
    protected virtual void OnGetField<T>(FieldInfo field, T value)
    {
    }

    [Infrastructure]
    protected virtual void OnSettingField<T>(FieldInfo field, T value)
    {
    }

    [Infrastructure]
    protected virtual void OnSetField<T>(FieldInfo field, T oldValue, T newValue)
    {
    }

    /// <summary>
    /// Called when entity should be validated.
    /// Override this method to perform custom validation.
    /// </summary>    
    [Infrastructure]
    public virtual void OnValidate()
    {
    }

    #endregion

    #region System-level GetField, SetField, GetKey, Remove members

    [Infrastructure]
    internal void Initialize(bool notify)
    {
      if (Session.IsDebugEventLoggingEnabled && this is Entity)
        Log.Debug("Session '{0}'. Materializing {1}: Key = '{2}'", 
          Session, GetType().GetShortName(), (this as Entity).State.Key);
      OnBeforeInitialize();
      if (notify) {
        OnInitialize();
        this.Validate();
      }
      OnAfterInitialize();
    }

    [Infrastructure]
    internal T GetField<T>(FieldInfo field, bool notify)
    {
      if (notify)
        OnGettingField(field);

      OnBeforeGetField(field);
      T result = field.GetAccessor<T>().GetValue(this, field);
      OnAfterGetField(field);

      if (notify)
        OnGetField(field, result);

      return result;
    }

    [Infrastructure]
    internal void SetField<T>(FieldInfo field, T value, bool notify)
    {
      if (notify)
        OnSettingField(field, value);

      OnBeforeSetField(field);
      T oldValue = default(T);
      if (notify)
        oldValue = GetField<T>(field, false);
      AssociationInfo association = field.Association;
      if (association!=null && association.IsPaired) {
        Key currentRef = GetKey(field);
        Key newRef = null;
        Entity newEntity = (Entity) (object) value;
        if (newEntity!=null)
          newRef = newEntity.Key;
        if (currentRef!=newRef) {
          SyncManager.Enlist(OperationType.Set, (Entity) this, newEntity, association);
          field.GetAccessor<T>().SetValue(this, field, value);
        }
      }
      else
        field.GetAccessor<T>().SetValue(this, field, value);
      OnAfterSetField(field);

      if (notify) {
        OnSetField(field, oldValue, value);
        if (Session.Domain.Configuration.AutoValidation)
          this.Validate();
        NotifyPropertyChanged(field);
      }
    }

    [Infrastructure]
    internal Key GetKey(FieldInfo field)
    {
      if (!field.IsEntity)
        throw new InvalidOperationException(string.Format("Field '{0}' is not an Entity field.", field.Name));

      OnBeforeGetField(field);
      // TODO: Refactor
      Key result = EntityFieldAccessor<Entity>.ExtractKey(this, field);
      OnAfterGetField(field);

      return result;
    }

    #endregion

    #region System-level event-like members

    [Infrastructure]
    protected internal virtual void OnBeforeInitialize()
    {
    }

    [Infrastructure]
    protected internal virtual void OnAfterInitialize()
    {
    }

    [Infrastructure]
    protected internal virtual void OnBeforeGetField(FieldInfo field)
    {
    }

    [Infrastructure]
    protected internal virtual void OnAfterGetField(FieldInfo field)
    {
    }

    [Infrastructure]
    protected internal virtual void OnBeforeSetField(FieldInfo field)
    {
    }

    [Infrastructure]
    protected internal virtual void OnAfterSetField(FieldInfo field)
    {
    }

    #endregion

    #region Equals & GetHashCode (just to ensure they're marked as [Infrastructure])

    /// <inheritdoc/>
    [Infrastructure]
    public override bool Equals(object obj)
    {
      return base.Equals(obj);
    }

    /// <inheritdoc/>
    [Infrastructure]
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    #endregion

    #region IAtomicityAware members

    AtomicityContextBase IContextBound<AtomicityContextBase>.Context
    {
      get { return Session.AtomicityContext; }
    }

    bool IAtomicityAware.IsCompatibleWith(AtomicityContextBase context)
    {
      return context==Session.AtomicityContext;
    }

    #endregion

    #region IValidationAware members

    /// <summary>
    /// Gets a value indicating whether validation should be skipped for this entity.
    /// </summary>
    [Infrastructure]
    protected internal abstract bool SkipValidation { get; }

    /// <inheritdoc/>
    [Infrastructure]
    void IValidationAware.OnValidate()
    {
      if (SkipValidation)
        return;

      this.CheckConstraints();
      OnValidate();
    }

    /// <inheritdoc/>
    [Infrastructure]
    ValidationContextBase IContextBound<ValidationContextBase>.Context
    {
      get {
        return Session.ValidationContext;
      }
    }

    #endregion

    /// <inheritdoc/>
    public event PropertyChangedEventHandler PropertyChanged;

    protected internal void NotifyPropertyChanged(FieldInfo field)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(field.Name));
    }

    internal Persistent()
    {
    }
  }
}
