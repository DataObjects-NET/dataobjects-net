// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections.Generic;
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
    IValidationAware
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
      return GetValue<T>(name);
    }

    [Infrastructure]
    public void SetProperty<T>(string name, T value)
    {
      FieldInfo field = Type.Fields[name];
      // TODO: Improve (use DelegateHelper)
      if (field.UnderlyingProperty!=null)
        field.UnderlyingProperty.SetValue(this, value, null);
      else
        SetValue(name, value);
    }

    #endregion

    #region GetValue, SetValue methods

    [Infrastructure]
    protected T GetValue<T>(string name)
    {
      FieldInfo field = Type.Fields[name];

      OnGettingValue(field);
      T result = field.GetAccessor<T>().GetValue(this, field);
      OnGetValue(field);

      return result;
    }

    [Infrastructure]
    protected void SetValue<T>(string name, T value)
    {
      FieldInfo field = Type.Fields[name];
      OnSettingValue(field);
      
      AssociationInfo association = field.Association;
      if (Type.IsEntity && association != null && association.Multiplicity == Multiplicity.OneToOne) {
        var setter = (Action<Entity, T, FieldInfo, Action<FieldInfo,T>>)(association.IsMaster ? association.SetMaster : association.MasterAssociation.SetSlave);
        setter((Entity)this, value, field, SetValueInternal);
      }
      else {
        SetValueInternal(field, value);
      }
    }


    private void SetValueInternal<T>(FieldInfo field, T value)
    {
      field.GetAccessor<T>().SetValue(this, field, value);
      OnSetValue(field);
    }

    #endregion

    #region Protected event-like methods

    [Infrastructure]
    protected internal virtual void OnCreating()
    {
    }

    [Infrastructure]
    protected internal virtual void OnCreated(Type constructedType)
    {
    }

    [Infrastructure]
    protected internal virtual void OnGettingValue(FieldInfo field)
    {
    }

    [Infrastructure]
    protected internal virtual void OnGetValue(FieldInfo field)
    {
    }

    [Infrastructure]
    protected internal virtual void OnSettingValue(FieldInfo field)
    {
    }

    [Infrastructure]
    protected internal virtual void OnSetValue(FieldInfo field)
    {
      if (Session.Domain.Configuration.AutoValidation)
        this.Validate();
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
  }
}
