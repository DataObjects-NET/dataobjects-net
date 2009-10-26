// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.ComponentModel;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.IoC;
using Xtensive.Core.Tuples;
using Xtensive.Integrity.Atomicity;
using Xtensive.Integrity.Validation;
using Xtensive.Storage.Aspects;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.PairIntegrity;
using Xtensive.Storage.Resources;

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
    IInitializable,
    IDataErrorInfo
  {
    private IFieldValueAdapter[] fieldAdapters;

    /// <summary>
    /// Gets the type of this instance.
    /// </summary>
    [Infrastructure]
    public abstract TypeInfo Type { get; }

    /// <summary>
    /// Gets the underlying tuple.
    /// </summary>
    [Infrastructure]
    protected internal abstract Tuple Tuple { get; }

    /// <inheritdoc/>
    [Infrastructure]
    public abstract event PropertyChangedEventHandler PropertyChanged;

    internal IFieldValueAdapter GetFieldValueAdapter (FieldInfo field, Func<Persistent, FieldInfo, IFieldValueAdapter> ctor)
    {
      // Building adapter container if necessary
      if (fieldAdapters==null) {
        int maxAdapterIndex = Type.Fields.Select(f => f.AdapterIndex).Max();
        fieldAdapters = new IFieldValueAdapter[maxAdapterIndex + 1];
      }

      // Building adapter
      var adapter = fieldAdapters[field.AdapterIndex];
      if (adapter != null)
        return adapter;
      adapter = ctor(this, field);
      fieldAdapters[field.AdapterIndex] = adapter;
      return adapter;
    }

    [Infrastructure]
    internal abstract void EnsureIsFetched(FieldInfo field);

    [Infrastructure]
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

    #region User-level GetFieldValue, SetFieldValue & similar members

    /// <summary>
    /// Gets the field value.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="fieldName">The field name.</param>
    /// <returns>Field value.</returns>
    protected internal T GetFieldValue<T>(string fieldName)
    {
      return GetFieldValue<T>(Type.Fields[fieldName]);
    }

    /// <summary>
    /// Gets the field value.
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="field">The field.</param>
    /// <returns>Field value.</returns>
    [AspectBehavior]
    protected internal T GetFieldValue<T>(FieldInfo field)
    {
      T result = default(T);
      try {
        SystemBeforeGetValue(field);
        result = GetAccessor<T>(field).GetValue(this, field);
        SystemGetValue(field, result);
        return result;
      }
      catch(Exception e) {
        SystemGetValueCompleted(field, result, e);
        throw;
      }
      finally {
        SystemGetValueCompleted(field, result, null);
      }
    }

    /// <summary>
    /// Gets the key of the entity, that is referenced by specified field 
    /// of the target persistent object.
    /// </summary>
    /// <remarks>
    /// Result is the same as <c>GetValue&lt;Entity&gt;(field).Key</c>, 
    /// but referenced entity will not be materialized.
    /// </remarks>
    /// <param name="field">The reference field. Field value type must be 
    /// <see cref="Entity"/> descendant.</param>
    /// <returns>Referenced entity key.</returns>
    /// <exception cref="InvalidOperationException">Field is not a reference field.</exception>
    protected internal Key GetReferenceKey(FieldInfo field)
    {
      Key key = null;
      try {
        SystemBeforeGetValue(field);
        if (!field.IsEntity)
          throw new InvalidOperationException(
            String.Format(Strings.ExFieldIsNotAnEntityField, field.Name, field.ReflectedType.Name));

        var types = Session.Domain.Model.Types;
        var type = types[field.ValueType];
        var tuple = Tuple;
        if (tuple.ContainsEmptyValues(field.MappingInfo))
          return null;

        int typeIdColumnIndex = type.KeyProviderInfo.TypeIdColumnIndex;
        var accuracy = TypeReferenceAccuracy.BaseType;
        if (typeIdColumnIndex >= 0)
          accuracy = TypeReferenceAccuracy.ExactType;
        var keyValue = field.ExtractValue(tuple);
        if (accuracy == TypeReferenceAccuracy.ExactType) {
          int typeId = keyValue.GetValueOrDefault<int>(typeIdColumnIndex);
          if (typeId != TypeInfo.NoTypeId) // != default(int) != 0
            type = types[typeId];
          else
            // This may happen if reference is null
            accuracy = TypeReferenceAccuracy.BaseType;
        }
        key = Key.Create(Session.Domain, type, accuracy, keyValue);
        SystemGetValue(field, key);
        return key;
      }
      catch(Exception e) {
        SystemGetValueCompleted(field, key, e);
        throw;
      }
      finally {
        SystemGetValueCompleted(field, key, null);
      }
    }

    /// <summary>
    /// Sets the field value.
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="fieldName">The field name.</param>
    /// <param name="value">The value to set.</param>
    protected internal void SetFieldValue<T>(string fieldName, T value)
    {
      SetFieldValue(Type.Fields[fieldName], value);
    }

    /// <summary>
    /// Sets the field value.
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="field">The field.</param>
    /// <param name="value">The value to set.</param>
    [AspectBehavior]
    protected internal void SetFieldValue<T>(FieldInfo field, T value)
    {
      T oldValue = default(T);
      try {
        SystemBeforeSetValue(field, value);
        oldValue = GetFieldValue<T>(field);
        var structure = value as Structure;
        var association = field.Association;
        if (association != null && association.IsPaired) {
          Key currentKey = GetReferenceKey(field);
          Key newKey = null;
          var newReference = (Entity) (object) value;
          if (newReference != null)
            newKey = newReference.Key;
          if (currentKey != newKey) {
            Session.PairSyncManager.Enlist(OperationType.Set, (Entity) this, newReference, association);
            PrepareToSetField();
            GetAccessor<T>(field).SetValue(this, field, value);
          }
        }
        else {
          if (!Equals(value, oldValue) || field.IsStructure) {
            PrepareToSetField();
            GetAccessor<T>(field).SetValue(this, field, value);
          }
        }
        SystemSetValue(field, oldValue, value);
      }
      catch (Exception e) {
        SystemSetValueCompleted(field, oldValue, value, e);
        throw;
      }
      finally {
        SystemSetValueCompleted(field, oldValue, value, null);
      }
    }

    internal protected bool IsFieldAvailable(string name)
    {
      return IsFieldAvailable(Type.Fields[name]);
    }

    internal protected bool IsFieldAvailable(FieldInfo field)
    {
      return Tuple.GetFieldState(field.MappingInfo.Offset).IsAvailable();
    }

    #endregion

    #region User-level event-like members
    
    protected virtual void OnInitialize()
    {
    }

    /// <summary>
    /// Called before field value is about to be read.
    /// </summary>
    /// <remarks>
    /// Override it to perform some actions before reading field value, e.g. to check access permissions.
    /// </remarks>
    protected virtual void OnGettingFieldValue(FieldInfo field)
    {
    }

    /// <summary>
    /// Called when field value has been read.
    /// </summary>
    /// <remarks>
    /// Override it to perform some actions when field value has been read, e.g. for logging purposes.
    /// </remarks>
    protected virtual void OnGetFieldValue(FieldInfo field, object value)
    {
    } 

    /// <summary>
    /// Called before field value is about to be changed.
    /// </summary>
    /// <remarks>
    /// Override it to perform some actions before changing field value, e.g. to check access permissions.
    /// </remarks>
    protected virtual void OnSettingFieldValue(FieldInfo field, object value)
    {
    }

    /// <summary>
    /// Called when field value has been changed.
    /// </summary>
    /// <remarks>
    /// Override it to perform some actions when field value has been changed, e.g. for logging purposes.
    /// </remarks>
    protected virtual void OnSetFieldValue(FieldInfo field, object oldValue, object newValue)
    {
    }

    /// <summary>
    /// Called when entity should be validated.
    /// </summary>
    /// <remarks>
    /// Override this method to perform custom object validation.
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
    protected virtual void OnValidate()
    {
    }

    #endregion


    #region System-level event-like members

    internal abstract void SystemBeforeInitialize();

    internal abstract void SystemInitialize();

    internal abstract void SystemBeforeGetValue(FieldInfo field);

    internal abstract void SystemGetValue(FieldInfo field, object value);

    internal abstract void SystemGetValueCompleted(FieldInfo fieldInfo, object value, Exception exception);

    internal abstract void SystemBeforeSetValue(FieldInfo field, object value);

    internal abstract void SystemSetValue(FieldInfo field, object oldValue, object newValue);

    internal abstract void SystemSetValueCompleted(FieldInfo fieldInfo, object oldValue, object newValue, Exception exception);
//    OnSetValue
//    {
//      if (Session.Domain.Configuration.AutoValidation)
//        this.Validate();
//      NotifyPropertyChanged(field);
//    }

    

//    [Infrastructure]
//    protected internal abstract void NotifyPropertyChanged(FieldInfo field);
    

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

    [Infrastructure]
    AtomicityContextBase IContextBound<AtomicityContextBase>.Context
    {
      get { return Session.AtomicityContext; }
    }

    [Infrastructure]
    bool IAtomicityAware.IsCompatibleWith(AtomicityContextBase context)
    {
      return context==Session.AtomicityContext;
    }

    #endregion

    #region IValidationAware members

    /// <summary>
    /// Gets a value indicating whether validation can be performed for this entity.
    /// </summary>
    protected internal abstract bool CanBeValidated { get; }

    /// <inheritdoc/>
    [Infrastructure]
    void IValidationAware.OnValidate()
    {
      InnerOnValidate();
    }

    [AspectBehavior]
    private void InnerOnValidate()
    {
      if (!CanBeValidated) // True for Structures which aren't bound to entities
        return;
      this.CheckConstraints(); // Ensures all PropertyConstraintAspects will be executed
                               // CheckConstraints is an extension method provided by Integrity
      OnValidate(); // Runs custom validation logic: this OnValidate can be overriden
    }

    /// <inheritdoc/>
    [Infrastructure]
    ValidationContextBase IContextBound<ValidationContextBase>.Context {
      get {
        return (Session ?? Session.Demand()).ValidationContext;
      }
    }

    #endregion

    #region Private \ Internal methods

    internal abstract void PrepareToSetField();

    internal static FieldAccessor<T> GetAccessor<T>(FieldInfo field)
    {
      if (field.IsEntity)
        return EntityFieldAccessor<T>.Instance;
      if (field.IsStructure)
        return StructureFieldAccessor<T>.Instance;
      if (field.IsEnum)
        return EnumFieldAccessor<T>.Instance;
      if (field.IsEntitySet)
        return EntitySetFieldAccessor<T>.Instance;
      if (field.ValueType==typeof(Key))
        return KeyFieldAccessor<T>.Instance;
      return DefaultFieldAccessor<T>.Instance;
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
    [Infrastructure]
    protected void Initialize(Type ctorType)
    {
      if (ctorType!=GetType())
        return;
      SystemInitialize();
    }

    #endregion

    #region IDataErrorInfo members

    string IDataErrorInfo.this[string columnName]
    {
      get { return this.GetPropertyError(columnName); }
    }

    string IDataErrorInfo.Error
    {
      get 
      { 
        try {
          OnValidate();
        }
        catch(Exception error) {
          return error.Message;
        }
        return null;
      }
    }

    #endregion

    internal Persistent()
    {
    }
  }
}
