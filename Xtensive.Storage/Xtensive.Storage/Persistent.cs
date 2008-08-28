// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Integrity.Atomicity;
using Xtensive.Integrity.Validation;
using Xtensive.Integrity.Validation.Interfaces;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Core.Aspects;

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
    protected abstract internal Tuple Tuple { get; }

    [Infrastructure]
    internal Dictionary<FieldInfo, IFieldHandler> FieldHandlers {
      get {
        if (fieldHandlers == null)
          fieldHandlers = new Dictionary<FieldInfo, IFieldHandler>();
        return fieldHandlers;
      }
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
      else
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
      using (var transactionScope = Session.BeginTransaction()) {
        FieldInfo field = Type.Fields[name];
        OnGettingValue(field);
        T result = field.GetAccessor<T>().GetValue(this, field);
        OnGetValue(field);

        transactionScope.Complete();
        return result;
      }
    }

    [Infrastructure]    
    protected void SetValue<T>(string name, T value)
    {
      using (var transactionScope = Session.BeginTransaction()) {
        FieldInfo field = Type.Fields[name];
        OnSettingValue(field);
        field.GetAccessor<T>().SetValue(this, field, value);

        if (Session.Domain.Configuration.AutoValidation)
          this.Validate();

        OnSetValue(field);

        transactionScope.Complete();
      }
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


    #region IValidationAware members

    /// <inheritdoc/>
    public ValidationContextBase Context
    {
      get 
      {
        // TODO: return ValidationContext of the current Transaction (or Session).
        return ValidationScope.CurrentContext; 
      }
    }

    public bool IsCompatibleWith(AtomicityContextBase context)
    {
      return context==Session.AtomicityContext;
    }

    /// <inheritdoc/>
    public void OnValidate()
    {
      // TODO: Call event-like method for custom validation.
      this.CheckConstraints();
    }

    /// <inheritdoc/>
    public bool IsCompatibleWith(ValidationContextBase context)
    {
      return context is PersistentValidationContext;
    }

    AtomicityContextBase IContextBound<AtomicityContextBase>.Context
    {
      get { return Session.AtomicityContext; }
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Persistent"/> class.
    /// </summary>
    protected Persistent()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Persistent"/> class.
    /// </summary>
    protected Persistent(EntityData data) : this()
    {
    }
  }
}