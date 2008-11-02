// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

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

    [Infrastructure]
    internal void Initialize()
    {
      OnInitialized();
    }

    #region this[...], GetProperty, SetProperty methods

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

    #region GetField, SetField methods

    [Infrastructure]
    protected T GetField<T>(string name)
    {
      return GetField<T>(Type.Fields[name]);
    }

    [Infrastructure]
    protected T GetField<T>(FieldInfo field)
    {
      OnGettingField(field);
      T result = Accessor.GetField<T>(this, field);
      OnGetField(field, result);

      return result;
    }

    [Infrastructure]
    protected void SetField<T>(string name, T value)
    {
      SetField(Type.Fields[name], value);
    }

    [Infrastructure]
    protected void SetField<T>(FieldInfo field, T value)
    {
      OnSettingField(field, value);
      T oldValue = Accessor.GetField<T>(this, field);
      Accessor.SetField(this, field, value);
      OnSetField(field, oldValue, value);
      NotifyPropertyChanged(field);
    }

    #endregion

    #region Protected event-like methods

    [Infrastructure]
    protected virtual void OnInitialized()
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
      this.OnValidate();
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
