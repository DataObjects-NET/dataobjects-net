// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Transactions;
using Xtensive.Aspects;
using Xtensive.Core;
using Xtensive.Disposing;
using Xtensive.IoC;
using Xtensive.Orm.Validation;
using Xtensive.Reflection;
using Xtensive.Tuples;
using Xtensive.Orm;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Orm.Operations;
using Xtensive.Orm.PairIntegrity;
using Xtensive.Orm.ReferentialIntegrity;
using Xtensive.Orm.Resources;
using Xtensive.Orm.Services;
using Activator = Xtensive.Orm.Internals.Activator;
using AggregateException = Xtensive.Core.AggregateException;
using FieldInfo = Xtensive.Storage.Model.FieldInfo;
using OperationType = Xtensive.Orm.PairIntegrity.OperationType;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm
{
  /// <summary>
  /// Abstract base class for any persistent type (<see cref="Entity"/> or <see cref="Structure"/>).
  /// </summary>
  /// <seealso cref="Entity"/>
  /// <seealso cref="Structure"/>
  [SystemType]
  [Initializable]
  [PersistentAspect]
  public abstract class Persistent : SessionBound,
    IValidationAware,
    INotifyPropertyChanged,
    IDataErrorInfo
  {
    #region Nested type: CtorTransactionInfo

    // [DebuggerDisplay("Id = {Id}")]
    private sealed class CtorTransactionInfo
    {
      [ThreadStatic]
      public static CtorTransactionInfo Current;
      // public static int CurrentId;

      // public int Id;
      public TransactionScope TransactionScope;
      public ICompletableScope InconsistentRegion;
      public CompletableScope OperationScope;
      public CtorTransactionInfo Previous;

      public CtorTransactionInfo()
      {
        // Id = Interlocked.Increment(ref CurrentId);
      }
    }

    #endregion

    private IFieldValueAdapter[] fieldAdapters;

    /// <summary>
    /// Gets <see cref="Xtensive.Orm.Model.TypeInfo"/> object describing structure of persistent object.
    /// </summary>
    [Infrastructure]
    public abstract TypeInfo TypeInfo { get; }

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
      if (field.ReflectedType.IsInterface)
        field = TypeInfo.FieldMap[field];
      // Building adapter container if necessary
      if (fieldAdapters==null) {
        int maxAdapterIndex = TypeInfo.Fields.Select(f => f.AdapterIndex).Max();
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
    /// <exception cref="ArgumentException">There is no persistent property with provided name.</exception>
    [Infrastructure]
    public T GetProperty<T>(string fieldName)
    {
      object value;
      var pair = fieldName.RevertibleSplitFirstAndTail(';', '.');
      var field = TypeInfo.Fields[pair.First];
      // TODO: Improve (use DelegateHelper)
      if (field.UnderlyingProperty!=null) {
        var mi = field.UnderlyingProperty.GetGetMethod(true);
        if (mi == null && field.UnderlyingProperty.ReflectedType != field.UnderlyingProperty.DeclaringType) {
          var p = field.UnderlyingProperty;  
          var dt = p.DeclaringType;
          mi = dt.GetProperty(p.Name, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).GetGetMethod(true);
        }
        value = mi.Invoke(this, null);
      }
      else
        value = GetFieldValue(pair.First); // Untyped, since T might be wrong
      if (pair.Second == null) // There is no tail, so we are going to return a value here
        return (T) value;
      var persistent = (Persistent) value;
      return persistent.GetProperty<T>(pair.Second);
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
    /// <exception cref="ArgumentException">There is no persistent property with provided name.</exception>
    [Infrastructure]
    public void SetProperty<T>(string fieldName, T value)
    {
      var pair = fieldName.RevertibleSplitFirstAndTail(';', '.');
      if (pair.Second == null) { // There is no tail, so we are setting a value on the current instance
        var field = TypeInfo.Fields[pair.First];
        // TODO: Improve (use DelegateHelper)
        if (field.UnderlyingProperty != null) {
          var mi = field.UnderlyingProperty.GetSetMethod(true);
          if (mi == null && field.UnderlyingProperty.ReflectedType != field.UnderlyingProperty.DeclaringType) {
            var p = field.UnderlyingProperty;  
            var dt = p.DeclaringType;
            mi = dt.GetProperty(p.Name, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).GetSetMethod(true);
          }
          mi.Invoke(this, new object[]{value});
        }
        else
          SetFieldValue(pair.First, (object)value); // Untyped, since T might be wrong
      }
      else {
        var persistent = GetProperty<Persistent>(pair.First);
        persistent.SetProperty(pair.Second, value);
      }
    }

    #endregion

    #region User-level GetFieldValue, SetFieldValue & similar members

    /// <summary>
    /// Gets the field value.
    /// Field value type must be specified precisely. 
    /// E.g. usage of <see cref="Object"/> instead of <see cref="IEntity"/> might lead to unpredictable effects.
    /// </summary>
    /// <typeparam name="T">Field value type.</typeparam>
    /// <param name="fieldName">The field name.</param>
    /// <returns>Field value.</returns>
    protected internal T GetFieldValue<T>(string fieldName)
    {
      return GetFieldValue<T>(TypeInfo.Fields[fieldName]);
    }

    /// <summary>
    /// Gets the field value.
    /// </summary>
    /// <param name="fieldName">The field name.</param>
    /// <returns>Field value.</returns>
    protected internal object GetFieldValue(string fieldName)
    {
      return GetFieldValue(TypeInfo.Fields[fieldName]);
    }

    /// <summary>
    /// Gets the field value.
    /// Field value type must be specified precisely. 
    /// E.g. usage of <see cref="Object"/> instead of <see cref="IEntity"/> might lead to unpredictable effects.
    /// </summary>
    /// <typeparam name="T">Field value type.</typeparam>
    /// <param name="field">The field.</param>
    /// <returns>Field value.</returns>
    [Transactional(TransactionalBehavior.Auto)]
    protected internal T GetFieldValue<T>(FieldInfo field)
    {
      if (field.ReflectedType.IsInterface)
        field = TypeInfo.FieldMap[field];
      var fieldAccessor = GetFieldAccessor<T>(field);
      T result = default(T);
      try {
        SystemBeforeGetValue(field);
        result = fieldAccessor.GetValue(this);
        SystemGetValue(field, result);
        SystemGetValueCompleted(field, result, null);
        return result;
      }
      catch(Exception e) {
        SystemGetValueCompleted(field, result, e);
        throw;
      }
    }

    /// <summary>
    /// Gets the field value.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <returns>Field value.</returns>
    [Transactional(TransactionalBehavior.Auto)]
    protected internal object GetFieldValue(FieldInfo field)
    {
      if (field.ReflectedType.IsInterface)
        field = TypeInfo.FieldMap[field];
      var fieldAccessor = GetFieldAccessor(field);
      object result = fieldAccessor.DefaultUntypedValue;
      try {
        SystemBeforeGetValue(field);
        result = fieldAccessor.GetUntypedValue(this);
        SystemGetValue(field, result);
        SystemGetValueCompleted(field, result, null);
        return result;
      }
      catch(Exception e) {
        SystemGetValueCompleted(field, result, e);
        throw;
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
        if (field.ReflectedType.IsInterface)
          field = TypeInfo.FieldMap[field];
        SystemBeforeGetValue(field);
        if (!field.IsEntity)
          throw new InvalidOperationException(
            String.Format(Strings.ExFieldIsNotAnEntityField, field.Name, field.ReflectedType.Name));

        var types = Session.Domain.Model.Types;
        var type = types[field.ValueType];
        var tuple = Tuple;
        if (tuple.ContainsEmptyValues(field.MappingInfo))
          return null;

        int typeIdColumnIndex = type.Key.TypeIdColumnIndex;
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
        SystemGetValueCompleted(field, key, null);
        return key;
      }
      catch(Exception e) {
        SystemGetValueCompleted(field, key, e);
        throw;
      }
    }

    /// <summary>
    /// Sets the field value.
    /// Field value type must be specified precisely. 
    /// E.g. usage of <see cref="Object"/> instead of <see cref="IEntity"/> might lead to unpredictable effects.
    /// </summary>
    /// <typeparam name="T">Field value type.</typeparam>
    /// <param name="fieldName">The field name.</param>
    /// <param name="value">The value to set.</param>
    protected internal void SetFieldValue<T>(string fieldName, T value)
    {
      SetFieldValue(TypeInfo.Fields[fieldName], value);
    }

    /// <summary>
    /// Sets the field value.
    /// </summary>
    /// <param name="fieldName">The field name.</param>
    /// <param name="value">The value to set.</param>
    protected internal void SetFieldValue(string fieldName, object value)
    {
      SetFieldValue(TypeInfo.Fields[fieldName], value);
    }

    /// <summary>
    /// Sets the field value.
    /// Field value type must be specified precisely. 
    /// E.g. usage of <see cref="Object"/> instead of <see cref="IEntity"/> might lead to unpredictable effects.
    /// </summary>
    /// <typeparam name="T">Field value type.</typeparam>
    /// <param name="field">The field.</param>
    /// <param name="value">The value to set.</param>
    [Transactional(TransactionalBehavior.Auto)]
    protected internal void SetFieldValue<T>(FieldInfo field, T value)
    {
      if (field.ReflectedType.IsInterface)
        field = TypeInfo.FieldMap[field];
      SetFieldValue(field, (object) value);
    }

    /// <summary>
    /// Sets the field value.
    /// Field value type must be specified precisely. 
    /// E.g. usage of <see cref="Object"/> instead of <see cref="IEntity"/> might lead to unpredictable effects.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <param name="value">The value to set.</param>
    [Transactional(TransactionalBehavior.Auto)]
    protected internal void SetFieldValue(FieldInfo field, object value)
    {
      SetFieldValue(field, value, null);
    }

    internal void SetFieldValue(FieldInfo field, object value, SyncContext syncContext)
    {
      SetFieldValue(field, value, syncContext, null);
    }

    internal void SetFieldValue(FieldInfo field, object value, SyncContext syncContext, RemovalContext removalContext)
    {
      if (field.ReflectedType.IsInterface)
        field = TypeInfo.FieldMap[field];
      SystemSetValueAttempt(field, value);
      var fieldAccessor = GetFieldAccessor(field);
      object oldValue = GetFieldValue(field);

      // Handling 'OldValue != NewValue' problem for structures
      var o = oldValue as Structure;
      if (o != null) {
        oldValue = Activator.CreateStructure(Session, o.GetType(), o.Tuple.ToRegular());
      }

      try {
        var operations = Session.Operations;
        var scope = operations.BeginRegistration(Operations.OperationType.System);
        try {
          if (operations.CanRegisterOperation) {
            var entity = this as Entity;
            if (entity != null)
              operations.RegisterOperation(new EntityFieldSetOperation(entity.Key, field, value));
            else {
              var persistent = this;
              var currentField = field;
              var structure = persistent as Structure;
              while (structure != null && structure.Owner != null) {
                var pair = new Pair<FieldInfo>(structure.Field, currentField);
                currentField = structure.Owner.TypeInfo.StructureFieldMapping[pair];
                persistent = structure.Owner;
                structure = persistent as Structure;
              }
              entity = persistent as Entity;
              if (entity != null)
                operations.RegisterOperation(new EntityFieldSetOperation(entity.Key, currentField, value));
            }
          }
          if (fieldAccessor.AreSameValues(oldValue, value)) {
            operations.NotifyOperationStarting(false);
            scope.Complete();
            return;
          }
          {
            SystemBeforeSetValue(field, value);
            operations.NotifyOperationStarting(false);
            AssociationInfo association = null;
            var entity = value as Entity ?? oldValue as Entity;
            if (entity != null)
              association = field.GetAssociation(entity.TypeInfo);
            if (association!=null && association.IsPaired) {
              Key currentKey = GetReferenceKey(field);
              Key newKey = null;
              var newReference = (Entity) (object) value;
              if (newReference!=null)
                newKey = newReference.Key;
              if (currentKey!=newKey) {
                Session.PairSyncManager.ProcessRecursively(syncContext, removalContext,
                  OperationType.Set, association, (Entity) this, newReference, () => {
                    SystemBeforeTupleChange();
                    fieldAccessor.SetUntypedValue(this, value);
                    SystemTupleChange();
                  });
              }
            }
            else {
              if (!Equals(value, oldValue) || field.IsStructure) {
                SystemBeforeTupleChange();
                fieldAccessor.SetUntypedValue(this, value);
                SystemTupleChange();
              }
            }

            if (removalContext!=null) {
              // Postponing finalizers (events)
              removalContext.EnqueueFinalizer(() => {
                try {
                  try {
                    SystemSetValue(field, oldValue, value);
                    SystemSetValueCompleted(field, oldValue, value, null);
                    scope.Complete();
                  }
                  finally {
                    scope.DisposeSafely();
                  }
                }
                catch (Exception e) {
                  SystemSetValueCompleted(field, oldValue, value, e);
                  throw;
                }
              });
              return;
            }

            SystemSetValue(field, oldValue, value);
            SystemSetValueCompleted(field, oldValue, value, null);
          }
          scope.Complete();
        }
        finally {
          if (removalContext==null)
            scope.DisposeSafely();
        }
      }
      catch (Exception e) {
        SystemSetValueCompleted(field, oldValue, value, e);
        throw;
      }
    }

    internal protected bool IsFieldAvailable(string name)
    {
      return IsFieldAvailable(TypeInfo.Fields[name]);
    }

    internal protected bool IsFieldAvailable(FieldInfo field)
    {
      if (field.ReflectedType.IsInterface)
        field = TypeInfo.FieldMap[field];
      return Tuple.GetFieldState(field.MappingInfo.Offset).IsAvailable();
    }

    #endregion

    #region User-level event-like members

    /// <summary>
    /// Called when instance is initialized (right after constructor).
    /// </summary>
    protected virtual void OnInitialize()
    {
    }

    /// <summary>
    /// Called on instance initialization error (constructor failure).
    /// </summary>
    /// <param name="error">The actual error.</param>
    protected virtual void OnInitializationError(Exception error)
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
    /// Called before field value is about to be set.
    /// This event is raised on any set attempt (even if new value is the same as the current one).
    /// </summary>
    /// <remarks>
    /// Override it to perform some actions before setting field value, e.g. to check access permissions.
    /// </remarks>
    protected virtual void OnSettingFieldValueAttempt(FieldInfo field, object value)
    {
    }

    /// <summary>
    /// Called before field value is about to be changed.
    /// This event is raised only on actual change attempt (i.e. when new value differs from the current one).
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
    ///     throw new InvalidOperationException("Age should be positive.");
    /// }
    /// </code>
    /// </example>
    [Infrastructure]
    protected virtual void OnValidate()
    {
    }

    #endregion

    #region System-level event-like members

    internal abstract void SystemBeforeInitialize(bool materialize);

    internal abstract void SystemInitialize(bool materialize);

    internal abstract void SystemInitializationError(Exception error);

    internal abstract void SystemBeforeGetValue(FieldInfo field);

    internal abstract void SystemGetValue(FieldInfo field, object value);

    internal abstract void SystemGetValueCompleted(FieldInfo field, object value, Exception exception);

    internal abstract void SystemSetValueAttempt(FieldInfo field, object value);

    internal abstract void SystemBeforeSetValue(FieldInfo field, object value);

    internal abstract void SystemBeforeTupleChange();

    internal abstract void SystemTupleChange();

    internal abstract void SystemSetValue(FieldInfo field, object oldValue, object newValue);

    internal abstract void SystemSetValueCompleted(FieldInfo field, object oldValue, object newValue, Exception exception);

    #endregion

    #region INotifyPropertyChanged & event support related methods

    /// <summary>
    /// Raises <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
    /// </summary>
    /// <param name="field">The field, which value is changed.</param>
    protected internal void NotifyFieldChanged(FieldInfo field)
    {
      if (!Session.EntityEvents.HasSubscribers)
        return;

      var rootField = field;
      while (rootField.Parent != null)
        rootField = rootField.Parent;

      NotifyPropertyChanged(rootField.Name);
    }

    /// <summary>
    /// Raises <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">Name of the changed property.</param>
    protected internal void NotifyPropertyChanged(string propertyName)
    {
      var subscription = GetSubscription(EntityEventBroker.PropertyChangedEventKey);
      if (subscription.Second != null) {
        ((PropertyChangedEventHandler)subscription.Second)
          .Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    /// <summary>
    /// Gets the subscription for the specified event key.
    /// </summary>
    /// <param name="eventKey">The event key.</param>
    /// <returns>Event subscription (delegate) for the specified event key.</returns>
    protected abstract Pair<Key, Delegate> GetSubscription(object eventKey);

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

    [Transactional(TransactionalBehavior.Auto)]
    private void InnerOnValidate()
    {
      if (!CanBeValidated) // True for Structures which aren't bound to entities & removed entities
        return;
      this.CheckConstraints(); // Ensures all PropertyConstraintAspects will be executed
                               // CheckConstraints is an extension method provided by Integrity
      OnValidate(); // Runs custom validation logic: this OnValidate can be overriden
    }

    /// <inheritdoc/>
    [Infrastructure]
    ValidationContext IValidationAware.Context
    {
      get {
        return (Session ?? Session.Demand()).ValidationContext;
      }
    }

    #endregion

    #region IDataErrorInfo members

    /// <inheritdoc/>
    [Transactional(TransactionalBehavior.Auto)]
    string IDataErrorInfo.this[string columnName] {
      get {
        return GetErrorMessage(this.GetPropertyValidationError(columnName));
      }
    }

    /// <inheritdoc/>
    [Transactional(TransactionalBehavior.Auto)]
    string IDataErrorInfo.Error {
      get { 
        try {
          OnValidate();
          return string.Empty;
        }
        catch (Exception error) {
          return GetErrorMessage(error);
        }
      }
    }

    #endregion

    #region Private \ Internal methods

    internal FieldAccessor GetFieldAccessor(FieldInfo field)
    {
      if (field.ReflectedType.IsInterface)
        field = TypeInfo.FieldMap[field];
      return Session.Domain.TypeLevelCaches[field.ReflectedType.TypeId].GetFieldAccessor(field);
    }

    internal FieldAccessor<T> GetFieldAccessor<T>(FieldInfo field)
    {
      if (field.ReflectedType.IsInterface)
        field = TypeInfo.FieldMap[field];
      return (FieldAccessor<T>) Session.Domain.TypeLevelCaches[field.ReflectedType.TypeId].GetFieldAccessor(field);
    }

    internal PersistentFieldState GetFieldState(string fieldName)
    {
      return GetFieldState(TypeInfo.Fields[fieldName]);
    }

    /// <exception cref="ArgumentException"><paramref name="field"/> belongs to a different type.</exception>
    internal PersistentFieldState GetFieldState(FieldInfo field)
    {
      if (field.ReflectedType.IsInterface)
        field = TypeInfo.FieldMap[field];
      if (field.ReflectedType!=TypeInfo)
        throw new ArgumentException(Strings.ExFieldBelongsToADifferentType, "field");
      var mappingInfo = field.MappingInfo;
      if (mappingInfo.Length==0)
        return 0; // EntitySet or another proxy

      PersistentFieldState state = 0;
      var tuple = Tuple;
      if (tuple!=null && tuple.AreAllColumnsAvalilable(mappingInfo))
        state = PersistentFieldState.Loaded;
      var diffTuple = tuple as DifferentialTuple;
      if (diffTuple!=null && diffTuple.Difference.IsAtLeastOneColumAvailable(mappingInfo))
        state = PersistentFieldState.Modified;
      return state;
    }

    private static string GetErrorMessage(Exception error)
    {
      string result;
      if (error==null)
        result = string.Empty;
      else if (error is AggregateException) {
        var ae = error as AggregateException;
        var errors = ae.GetFlatExceptions();
        if (errors.Count==1)
          result = errors[0].Message;
        else
          result = ae.Message;
      }
      else 
        result = error.Message;
      return result ?? string.Empty;
    }

    #endregion

    #region Initializable aspect support

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    /// <param name="ctorType">Type of the instance that is being constructed.</param>
    /// <remarks>
    /// This method is called when custom constructor is finished.
    /// </remarks>
    [Infrastructure]
    protected void Initialize(Type ctorType)
    {
      var type = GetType();
      if (ctorType != type)
        return;
      var successfully = false;
      try {
        SystemInitialize(false);
        successfully = true;
      }
      finally {
        LeaveCtorTransactionScope(successfully);
      }
    }

    /// <summary>
    /// Called on initialization error.
    /// </summary>
    /// <param name="ctorType">Type of the instance that is being constructed.</param>
    /// <param name="error">The error that happened on initialization.</param>
    /// <remarks>
    /// This method is called when custom constructor is finished.
    /// </remarks>
    [Infrastructure]
    protected void InitializationError(Type ctorType, Exception error)
    {
      var type = GetType();
      if (ctorType != type)
        return;
      try {
        SystemInitializationError(error);
      }
      finally {
        LeaveCtorTransactionScope(false);
      }
    }

    #endregion

    #region Initialization on materialization

    /// <summary>
    /// Initializes this instance on materialization.
    /// </summary>
    [Infrastructure]
    protected void InitializeOnMaterialize()
    {
      var successfully = false;
      try {
        SystemInitialize(true);
        successfully = true;
      }
      finally {
        LeaveCtorTransactionScope(successfully);
      }
    }

    /// <summary>
    /// Called on initialization error on materialization.
    /// </summary>
    /// <param name="error">The error that happened on initialization.</param>
    /// <remarks>
    /// This method is called when custom constructor is finished.
    /// </remarks>
    [Infrastructure]
    protected void InitializationErrorOnMaterialize(Exception error)
    {
      try {
        SystemInitializationError(error);
      }
      finally {
        LeaveCtorTransactionScope(false);
      }
    }

    #endregion

    #region Enter/LeaveCtorTransactionScope methods

    internal void EnterCtorTransactionScope()
    {
      CtorTransactionInfo.Current = new CtorTransactionInfo() {
        TransactionScope = Session.OpenAutoTransaction(),
        InconsistentRegion = Session.DisableValidation(),
        Previous = CtorTransactionInfo.Current,
      };
    }

    internal void BindCtorTransactionScopeToOperationScope(CompletableScope scope)
    {
      var ctorTransactionInfo = CtorTransactionInfo.Current;
      ctorTransactionInfo.OperationScope = scope;
    }

    internal void LeaveCtorTransactionScope(bool successfully)
    {
      var cti = CtorTransactionInfo.Current;
      if (cti == null)
        return;
      CtorTransactionInfo.Current = cti.Previous;
      bool inconsistentRegionDisposed = false;
      try {
        if (successfully) {
          try {
            cti.InconsistentRegion.Complete();
            inconsistentRegionDisposed = true;
            cti.InconsistentRegion.Dispose();
            cti.InconsistentRegion = null;
          } 
          catch {
            successfully = false;
            throw;
          }
        }
        if (cti.OperationScope!=null)
          cti.OperationScope.Complete();
      }
      finally {
        try {
          if (!inconsistentRegionDisposed)
            cti.InconsistentRegion.Dispose();
        }
        catch {
          successfully = false;
          throw;
        }
        finally {
          try {
            if (cti.OperationScope != null)
              cti.OperationScope.Dispose();
          }
          catch {
            successfully = false;
            throw;
          }
          finally {
            var transactionScope = cti.TransactionScope;
            if (transactionScope!=null) {
              if (successfully)
                transactionScope.Complete();
              transactionScope.Dispose();
            }
          }
        }
      }
    }

    #endregion


    // Constructors

    internal Persistent()
    {
      EnterCtorTransactionScope();
    }

    internal Persistent(Session session)
      : base(session)
    {
      EnterCtorTransactionScope();
    }
  }
}
