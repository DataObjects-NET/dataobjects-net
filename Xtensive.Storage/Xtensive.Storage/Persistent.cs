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

namespace Xtensive.Storage
{
  [Initializable]
  public abstract class Persistent : SessionBound,
    IAtomicityAware,
    IValidationAware,
    INotifyPropertyChanged,
    IInitializable
  {
    private Dictionary<FieldInfo, IFieldValueAdapter> fieldHandlers;

    /// <summary>
    /// Gets the type of this instance.
    /// </summary>
    [Infrastructure]
    internal abstract TypeInfo Type { get; }

    [Infrastructure]
    protected internal abstract Tuple Tuple { get; }

    [Infrastructure]
    internal Dictionary<FieldInfo, IFieldValueAdapter> FieldHandlers {
      get {
        if (fieldHandlers==null)
          fieldHandlers = new Dictionary<FieldInfo, IFieldValueAdapter>();
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

    #region User-level GetField, SetField members

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

    [Infrastructure]
    protected bool IsFieldAvailable(string name)
    {
      return IsFieldAvailable(Type.Fields[name]);
    }

    [Infrastructure]
    protected bool IsFieldAvailable(FieldInfo field)
    {
      return Tuple.IsAvailable(field.MappingInfo.Offset);
    }

    #endregion

    #region User-level event-like members

    [Infrastructure]
    protected virtual void OnInitialize()
    {
    }

    [Infrastructure]
    protected virtual void OnGettingField(FieldInfo field)
    {
    }

    [Infrastructure]
    protected virtual void OnGetField(FieldInfo field, object value)
    {
    }

    [Infrastructure]
    protected virtual void OnSettingField(FieldInfo field, object value)
    {
    }

    [Infrastructure]
    protected virtual void OnSetField(FieldInfo field, object oldValue, object newValue)
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
    internal T GetField<T>(FieldInfo field, bool notify)
    {
      OnGettingField(field, notify);
      T result = field.GetAccessor<T>().GetValue(this, field, notify);
      OnGetField(field, result, notify);

      return result;
    }

    [Infrastructure]
    internal void SetField<T>(FieldInfo field, T value, bool notify)
    {
      OnSettingField(field, value, notify);
      var oldValue = GetField<T>(field, false);
      AssociationInfo association = field.Association;
      if (association!=null && association.IsPaired) {
        Key currentKey = GetKey(field);
        Key newKey = null;
        var newReference = (Entity) (object) value;
        if (newReference!=null)
          newKey = newReference.Key;
        if (currentKey!=newKey) {
          Session.PairSyncManager.Enlist(OperationType.Set, (Entity) this, newReference, association, notify);
          field.GetAccessor<T>().SetValue(this, field, value, notify);
        }
      }
      else
        field.GetAccessor<T>().SetValue(this, field, value, notify);
      OnSetField(field, oldValue, value, notify);
    }

    [Infrastructure]
    internal Key GetKey(FieldInfo field)
    {
      if (!field.IsEntity)
        throw new InvalidOperationException(string.Format("Field '{0}' is not an Entity field in Type '{1}'.", field.Name, field.ReflectedType.Name));

      OnGettingField(field, false);
      var type = Session.Domain.Model.Types[field.ValueType];
      var tuple = field.ExtractValue(Tuple);
      Key result = tuple.ContainsEmptyValues() ? null : Key.Create(type, tuple);
      OnGetField(field, result, false);

      return result;
    }

    #endregion

    #region System-level event-like members

    [Infrastructure]
    internal virtual void OnInitializing(bool notify)
    {
    }

    [Infrastructure]
    internal virtual void OnInitialize(bool notify)
    {
      if (!notify)
        return;
      OnInitialize();
      this.Validate();
    }

    [Infrastructure]
    internal virtual void OnGettingField(FieldInfo field, bool notify)
    {
      if (!notify)
        return;
      OnGettingField(field);
    }

    [Infrastructure]
    internal virtual void OnGetField(FieldInfo field, object value, bool notify)
    {
      if (!notify)
        return;
      OnGetField(field, value);
    }

    [Infrastructure]
    internal virtual void OnSettingField(FieldInfo field, object value, bool notify)
    {
      if (!notify)
        return;
      OnSettingField(field, value);
    }

    [Infrastructure]
    internal virtual void OnSetField(FieldInfo field, object oldValue, object newValue, bool notify)
    {
      if (!notify)
        return;
      OnSetField(field, oldValue, newValue);
      if (Session.Domain.Configuration.AutoValidation)
        this.Validate();
      NotifyPropertyChanged(field);
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

    #region Initializable aspect support

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    /// <param name="ctorType">Type of the instance that is being constructed.</param>
    protected void Initialize(Type ctorType)
    {
      if (ctorType!=GetType())
        return;
      OnInitialize(true);
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
