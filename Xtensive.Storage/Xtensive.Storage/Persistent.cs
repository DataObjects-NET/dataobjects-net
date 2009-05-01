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
  /// <summary>
  /// Base class for all persistent classes.
  /// </summary>
  /// <seealso cref="Entity"/>
  /// <seealso cref="Structure"/>
  [Initializable]
  [SystemType]
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

    /// <summary>
    /// Gets the underlying tuple.
    /// </summary>
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

    internal TypeInfo GetTypeInfo()
    {
      return Session.Domain.Model.Types[GetType()];
    }

    #region this[...], GetProperty, SetProperty members

    /// <summary>
    /// Gets or sets the value of the field with specified name.
    /// </summary>
    /// <value>Field value.</value>
    [Infrastructure]
    public object this[string name]
    {
      get { return GetProperty<object>(name); }
      set { SetProperty(name, value); }
    }

    /// <summary>
    /// Gets the property value.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="fieldName">The field name.</param>
    /// <returns>Property value.</returns>
    /// <remarks>
    /// Method calls property getter thought the reflection to perform its business logic
    /// or calls <see cref="GetFieldValue{T}(string)"/> directly if there is no property declared for this field.
    /// </remarks>
    /// <seealso cref="SetProperty{T}"/>
    [Infrastructure]
    public T GetProperty<T>(string fieldName)
    {
      FieldInfo field = Type.Fields[fieldName];
      // TODO: Improve (use DelegateHelper)
      if (field.UnderlyingProperty!=null)
        return (T) field.UnderlyingProperty.GetValue(this, null);
      return GetFieldValue<T>(fieldName);
    }

    /// <summary>
    /// Sets the property value.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="fieldName">The field name.</param>
    /// <param name="value">The value to set.</param>
    /// <remarks>
    /// Method calls property setter thought the reflection to perform its business logic
    /// or calls <see cref="SetFieldValue{T}(string,T)"/> directly if there is no property declared for this field.
    /// </remarks>
    /// <seealso cref="GetProperty{T}"/>
    [Infrastructure]
    public void SetProperty<T>(string fieldName, T value)
    {
      FieldInfo field = Type.Fields[fieldName];
      // TODO: Improve (use DelegateHelper)
      if (field.UnderlyingProperty!=null)
        field.UnderlyingProperty.SetValue(this, value, null);
      else
        SetFieldValue(fieldName, value);
    }

    #endregion

    #region User-level GetFieldValue, SetFieldValue members

    /// <summary>
    /// Gets the field value.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="fieldName">The field name.</param>
    /// <returns>Field value.</returns>
    [Infrastructure]
    protected T GetFieldValue<T>(string fieldName)
    {
      return GetFieldValue<T>(Type.Fields[fieldName]);
    }

    /// <summary>
    /// Gets the field value.
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="field">The field.</param>
    /// <returns>Field value.</returns>
    [Infrastructure]
    protected T GetFieldValue<T>(FieldInfo field)
    {
      return GetFieldValue<T>(field, true);
    }

    /// <summary>
    /// Sets the field value.
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="fieldName">The field name.</param>
    /// <param name="value">The value to set.</param>
    [Infrastructure]
    protected void SetFieldValue<T>(string fieldName, T value)
    {
      SetFieldValue(Type.Fields[fieldName], value);
    }

    /// <summary>
    /// Sets the field value.
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="field">The field.</param>
    /// <param name="value">The value to set.</param>
    [Infrastructure]
    protected void SetFieldValue<T>(FieldInfo field, T value)
    {
      SetFieldValue(field, value, true);
    }

    [Infrastructure]
    internal protected bool IsFieldAvailable(string name)
    {
      return IsFieldAvailable(Type.Fields[name]);
    }

    [Infrastructure]
    internal protected bool IsFieldAvailable(FieldInfo field)
    {
      return Tuple.IsAvailable(field.MappingInfo.Offset);
    }

    #endregion

    #region User-level event-like members
    
    [Infrastructure]
    protected virtual void OnInitialize()
    {
    }

    /// <summary>
    /// Called before field value is about to be read.
    /// </summary>
    /// <remarks>
    /// Override it to perform some actions before reading field value, e.g. to check access permissions.
    /// </remarks>
    [Infrastructure]
    protected virtual void OnGettingFieldValue(FieldInfo field)
    {
    }

    /// <summary>
    /// Called when field value has been read.
    /// </summary>
    /// <remarks>
    /// Override it to perform some actions when field value has been read, e.g. for logging purposes.
    /// </remarks>
    [Infrastructure]
    protected virtual void OnGetFieldValue(FieldInfo field, object value)
    {
    }

    /// <summary>
    /// Called before field value is about to be changed.
    /// </summary>
    /// <remarks>
    /// Override it to perform some actions before changing field value, e.g. to check access permissions.
    /// </remarks>
    [Infrastructure]
    protected virtual void OnSettingFieldValue(FieldInfo field, object value)
    {
    }

    /// <summary>
    /// Called when field value has been changed.
    /// </summary>
    /// <remarks>
    /// Override it to perform some actions when field value has been changed, e.g. for logging purposes.
    /// </remarks>
    [Infrastructure]
    protected virtual void OnSetFieldValue(FieldInfo field, object oldValue, object newValue)
    {
    }

    /// <summary>
    /// Called when entity should be validated.
    /// </summary>    
    /// <remarks>
    /// Override this method to perform custom validation.
    /// </remarks>
    /// <example>
    /// <code>
    /// public override void OnValidate()
    /// {
    ///   base.OnValidate();
    ///   if (Age &lt;= 0) 
    ///     throw new Exception("Age should be positive.");
    /// }
    /// </code>
    /// </example>
    [Infrastructure]
    public virtual void OnValidate()
    {
    }

    #endregion

    #region System-level GetField, SetField, GetKey, Remove members

    [Infrastructure]
    internal T GetFieldValue<T>(FieldInfo field, bool notify)
    {
      OnGettingFieldValue(field, notify);
      T result = field.GetAccessor<T>().GetValue(this, field, notify);
      OnGetFieldValue(field, result, notify);

      return result;
    }

    [Infrastructure]
    internal void SetFieldValue<T>(string fieldName, T value, bool notify)
    {
      SetFieldValue(Type.Fields[fieldName], value, false);
    }

    [Infrastructure]
    internal void SetFieldValue<T>(FieldInfo field, T value, bool notify)
    {
      OnSettingFieldValue(field, value, notify);
      var oldValue = GetFieldValue<T>(field, false);
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
      OnSetFieldValue(field, oldValue, value, notify);
    }

    [Infrastructure]
    internal Key GetKey(FieldInfo field)
    {
      if (!field.IsEntity)
        throw new InvalidOperationException(string.Format(Resources.Strings.ExFieldIsNotAnEntityField, field.Name, field.ReflectedType.Name));

      OnGettingFieldValue(field, false);
      var type = Session.Domain.Model.Types[field.ValueType];
      var tuple = field.ExtractValue(Tuple);
      Key result = tuple.ContainsEmptyValues() ? null : Key.Create(type, tuple);
      OnGetFieldValue(field, result, false);

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
    }

    [Infrastructure]
    internal virtual void OnGettingFieldValue(FieldInfo field, bool notify)
    {
      if (!notify)
        return;
      OnGettingFieldValue(field);
    }

    [Infrastructure]
    internal virtual void OnGetFieldValue(FieldInfo field, object value, bool notify)
    {
      if (!notify)
        return;
      OnGetFieldValue(field, value);
    }

    [Infrastructure]
    internal virtual void OnSettingFieldValue(FieldInfo field, object value, bool notify)
    {
      if (!notify)
        return;
      OnSettingFieldValue(field, value);
    }

    [Infrastructure]
    internal virtual void OnSetFieldValue(FieldInfo field, object oldValue, object newValue, bool notify)
    {
      if (!notify)
        return;
      OnSetFieldValue(field, oldValue, newValue);
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
    /// <remarks>
    /// This method is called when custom constructor is finished.
    /// </remarks>
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
