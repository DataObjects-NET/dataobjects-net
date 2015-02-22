// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.01

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Xtensive.Caching;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Model;
using Xtensive.Orm.Operations;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Serialization;
using Xtensive.Orm.Validation;
using Xtensive.Reflection;
using Xtensive.Tuples;
using Xtensive.Tuples.Transform;
using FieldInfo = Xtensive.Orm.Model.FieldInfo;
using OperationType = Xtensive.Orm.Operations.OperationType;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm
{
  /// <summary>
  /// Abstract base class for any persistent entity.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <see cref="Entity"/> class encapsulates infrastructure to store persistent transactional data.
  /// It has <see cref="Key"/> property that uniquely identifies the instance within its <see cref="Session"/>.
  /// </para>
  /// <para>All entities in a model should be inherited from this class.
  /// </para>
  /// </remarks>
  /// <example>
  /// <code>
  /// [HierarchyRoot]
  /// public class Customer : Entity
  /// {
  ///   [Field, Key]
  ///   public int Id { get; set; }
  ///   
  ///   [Field]
  ///   public string Name { get; set; }
  /// }
  /// </code>
  /// </example>
  /// <seealso cref="Structure">Structure class</seealso>
  /// <seealso cref="EntitySet{TItem}"><c>EntitySet</c> class</seealso>
  [Serializable]
  [SystemType]
  [DebuggerDisplay("{Key}")]
  public abstract class Entity : Persistent,
    IEntity,
    ISerializable,
    IDeserializationCallback
  {
    private static readonly Parameter<Tuple> keyParameter = new Parameter<Tuple>(WellKnown.KeyFieldName);
    private EntityState state;

    #region Internal properties

    /// <exception cref="InvalidOperationException">Entity is already detached from Session.</exception>
    internal EntityState State {
      get {
        if (state==null)
          return null;
        var entityBoundToState = state.TryGetEntity();
        if (entityBoundToState!=this && entityBoundToState!=null)
          throw new InvalidOperationException(Strings.ExEntityIsAlreadyDetachedFromSession);
        return state;
      }
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        if (state!=null)
          throw Exceptions.AlreadyInitialized("State");
        state = value;
        state.Entity = this;
      }
    }

    /// <exception cref="Exception">Property is already initialized.</exception>
    public int TypeId
    {
      [DebuggerStepThrough]
      get { return GetFieldValue<int>(WellKnown.TypeIdFieldName); }
    }

    #endregion

    #region Properties: Key, TypeInfo, Tuple, PersistenceState, VersionInfo, etc.

    /// <summary>
    /// Gets the <see cref="Key"/> that identifies this entity.
    /// </summary>
    public Key Key
    {
      [DebuggerStepThrough]
      get { return State.Key; }
    }

    /// <inheritdoc/>
    public VersionInfo VersionInfo {
      get {
        if (IsRemoved)
          return VersionInfo.Void;
        if (TypeInfo.HasVersionRoots) {
          var version = VersionInfo.Void;
          foreach (var root in ((IHasVersionRoots) this).GetVersionRoots()) {
            if (root is IHasVersionRoots)
              throw new InvalidOperationException(Strings.ExVersionRootObjectCantImplementIHasVersionRoots);
            version = version.Combine(root.Key, root.VersionInfo);
          }
          return version;
        }
        if (TypeInfo.VersionExtractor==null)
          return VersionInfo.Void;
        var tuple = State.Tuple;
        var versionColumns = TypeInfo.GetVersionColumns();
        List<PrefetchFieldDescriptor> columnsToPrefetch = null;
        foreach (var columnInfo in versionColumns) {
          if (!tuple.GetFieldState(columnInfo.Field.MappingInfo.Offset).IsAvailable()) {
            if (columnsToPrefetch==null)
              columnsToPrefetch = new List<PrefetchFieldDescriptor>();
            columnsToPrefetch.Add(new PrefetchFieldDescriptor(columnInfo.Field));
          }
        }
        if (columnsToPrefetch!=null) {
          Session.Handler.Prefetch(Key, TypeInfo, columnsToPrefetch);
          Session.Handler.ExecutePrefetchTasks(true);
        }
        var versionTuple = TypeInfo.VersionExtractor.Apply(TupleTransformType.Tuple, State.Tuple);
        return new VersionInfo(versionTuple);
      }
    }

    /// <inheritdoc/>
    public override sealed TypeInfo TypeInfo
    {
      [DebuggerStepThrough]
      get { return State.Type; }
    }

    /// <inheritdoc/>
    protected internal override sealed Tuple Tuple
    {
      [DebuggerStepThrough]
      get { return State.Tuple; }
    }

    /// <summary>
    /// Gets persistence state of the entity.
    /// </summary>
    public PersistenceState PersistenceState
    {
      [DebuggerStepThrough]
      get { return State.PersistenceState; }
    }

    /// <inheritdoc/>
    public bool IsRemoved {
      get {
        if (Session == null || state == null || state.TryGetEntity()!=this)
          return true;
        if (Session.IsPersisting)
          // Removed = "already removed from storage" here
          return State.IsNotAvailable;
        else
          // Removed = "either already removed, or marked as removed" here
          return State.IsNotAvailableOrMarkedAsRemoved;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is materializing.
    /// </summary>
    protected bool IsMaterializing { get; private set; }

    #endregion

    #region IIdentified members

    /// <inheritdoc/>
    Key IIdentified<Key>.Identifier
    {
      [DebuggerStepThrough]
      get { return Key; }
    }

    /// <inheritdoc/>
    object IIdentified.Identifier
    {
      [DebuggerStepThrough]
      get { return Key; }
    }

    #endregion

    #region IHasVersion members

    /// <inheritdoc/>
    VersionInfo IHasVersion<VersionInfo>.Version {
      [DebuggerStepThrough]
      get { return VersionInfo; }
    }

    /// <inheritdoc/>
    object IHasVersion.Version {
      [DebuggerStepThrough]
      get { return VersionInfo; }
    }

    #endregion

    #region Public members

    /// <inheritdoc/>
    public override sealed event PropertyChangedEventHandler PropertyChanged
    {
      add {
        Session.EntityEvents.AddSubscriber(
          Key, EntityEventBroker.PropertyChangedEventKey, value);
      }
      remove {
        Session.EntityEvents.RemoveSubscriber(Key, 
          EntityEventBroker.PropertyChangedEventKey, value);
      }
    }

    /// <summary>
    /// Removes this entity.
    /// </summary>
    /// <exception cref="ReferentialIntegrityException">
    /// Entity is associated with another entity with <see cref="OnRemoveAction.Deny"/> on-remove action.</exception>
    /// <seealso cref="IsRemoved"/>
    public void Remove()
    {
      Session.RemovalProcessor.Remove(EnumerableUtils.One(this));
    }

    /// <summary>
    /// Register the entity in removing queue. Removal operation will be postponed 
    /// until <see cref="Session.SaveChanges"/> method is called; some query is executed 
    /// or current transaction is being committed.
    /// </summary>
    public void RemoveLater()
    {
      Session.RemovalProcessor.EnqueueForRemoval(EnumerableUtils.One(this));
    }

    /// <inheritdoc/>
    public void Lock(LockMode lockMode, LockBehavior lockBehavior)
    {
      using (new ParameterContext().Activate()) {
        keyParameter.Value = Key.Value;
        var domain = Session.Domain;
        object key = new Triplet<TypeInfo, LockMode, LockBehavior>(TypeInfo, lockMode, lockBehavior);
        Func<object, object> generator = tripletObj => {
          var triplet = (Triplet<TypeInfo, LockMode, LockBehavior>) tripletObj;
          IndexInfo index = triplet.First.Indexes.PrimaryIndex;
          var query = index.GetQuery()
            .Seek(keyParameter.Value)
            .Lock(() => triplet.Second, () => triplet.Third)
            .Select();
          return Session.Compile(query);
        };
        var source = (ExecutableProvider) Session.StorageNode.InternalQueryCache.GetOrAdd(key, generator);
        var recordSet = source.GetRecordSet(Session);
        recordSet.FirstOrDefault();
      }
    }

    /// <inheritdoc/>
    public void IdentifyAs(EntityIdentifierType identifierType)
    {
      var operations = Session.Operations;
      if (!operations.CanRegisterOperation)
        return;
      switch (identifierType) {
      case EntityIdentifierType.Auto:
        string identifier = string.Format("#{0}", operations.GetNextIdentifier().ToString("0000"));
        operations.RegisterEntityIdentifier(Key, identifier);
        break;
      case EntityIdentifierType.None:
        operations.RegisterEntityIdentifier(Key, null);
        break;
      default:
        throw new ArgumentOutOfRangeException("identifierType");
      }
    }

    /// <inheritdoc/>
    public void IdentifyAs(string identifier)
    {
      var operations = Session.Operations;
      if (!operations.CanRegisterOperation)
        return;
      operations.RegisterEntityIdentifier(Key, identifier);
    }

    #endregion

    #region Protected event-like methods

    /// <inheritdoc/>
    protected internal override bool CanBeValidated {
      get { return !IsRemoved; }
    }

    /// <summary>
    /// Called when entity is about to be removed.
    /// </summary>
    /// <remarks>
    /// Override it to perform some actions when entity is about to be removed.
    /// </remarks>
    protected virtual void OnRemoving()
    {
    }

    /// <summary>
    /// Called when entity becomes removed.
    /// </summary>
    /// <remarks>
    /// Override this method to perform some actions when entity is removed.
    /// </remarks>
    protected virtual void OnRemove()
    {
    }

    /// <summary>
    /// Invoked to update <see cref="VersionInfo"/>.
    /// </summary>
    /// <param name="changedEntity">The changed entity.</param>
    /// <param name="changedField">The changed field.</param>
    /// <returns>
    /// <see langword="True"/>, if <see cref="VersionInfo"/> was changed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="NotSupportedException">Version root can't implement
    /// <see cref="IHasVersionRoots"/>.</exception>
    protected internal bool UpdateVersionInfo(Entity changedEntity, FieldInfo changedField)
    {
      if (State.IsVersionInfoUpdated || IsRemoved || changedEntity.TypeInfo.IsSystem)
        return true;
      bool changed = false;
      try {
        State.IsVersionInfoUpdated = true; // Prevents recursion
        if (!TypeInfo.HasVersionRoots) 
          changed = SystemUpdateVersionInfo(changedEntity, changedField);
        else {
          foreach (var root in ((IHasVersionRoots) this).GetVersionRoots()) {
            if (root.TypeInfo.HasVersionRoots)
              throw new NotSupportedException(Strings.ExVersionRootObjectCantImplementIHasVersionRoots);
            changed |= root.UpdateVersionInfo(changedEntity, changedField);
          }
        }
        return changed;
      }
      finally {
        State.IsVersionInfoUpdated = changed;
      }
    }

    /// <summary>
    /// Called to update the fields describing <see cref="Entity"/>'s version.
    /// </summary>
    /// <param name="changedEntity">The changed entity.</param>
    /// <param name="changedField">The changed field.</param>
    /// <returns>
    /// <see langword="True"/>, if <see cref="VersionInfo"/> was changed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected virtual bool UpdateVersion(Entity changedEntity, FieldInfo changedField)
    {
      foreach (var field in TypeInfo.GetVersionFields().Where(f => f.AutoVersion))
        SetFieldValue(field, VersionGenerator.GenerateNextVersion(GetFieldValue(field)));
      return true;
    }

    /// <summary>
    /// Performs immediate validation of this instance.
    /// </summary>
    public void Validate()
    {
      if (CanBeValidated)
        Session.ValidationContext.Validate(this);
    }

    #endregion

    #region Private / internal methods

    /// <exception cref="InvalidOperationException">Entity is removed.</exception>
    internal void EnsureNotRemoved()
    {
      if (IsRemoved)
        throw new InvalidOperationException(Strings.ExEntityIsRemoved);
    }

    internal override sealed ValidationResult GetValidationResult()
    {
      return Session.ValidationContext.ValidateOnceAndGetErrors(this).FirstOrDefault(r => r.Field==null);
    }

    internal override sealed ValidationResult GetValidationResult(string fieldName)
    {
      return Session.ValidationContext.ValidateOnceAndGetErrors(this).FirstOrDefault(f => f.Field!=null && f.Field.Name==fieldName);
    }

    internal override sealed void EnsureIsFetched(FieldInfo field)
    {
      var state = State;

      if (state.PersistenceState==PersistenceState.New)
        return;

      var tuple = state.Tuple;
      // tuple is already cleared
      if (tuple == null && (Session.Configuration.Options & SessionOptions.ReadRemovedObjects)==SessionOptions.ReadRemovedObjects)
        return;

      if (tuple.GetFieldState(field.MappingInfo.Offset).IsAvailable())
        return;

      EnsureNotRemoved();
      Session.Handler.FetchField(Key, field);
    }

    internal IEnumerable<FieldInfo> GetKeyFieldsOfEntityType()
    {
      return TypeInfo.Key.Fields.Where(field => field.IsEntity && !field.IsNested).Select(field => field);
    }

    internal void RegisterKeyFieldsOfEntityTypeForRemap(Key keyOfThisEntity, params object[] values)
    {
      foreach (var fieldInfo in GetKeyFieldsOfEntityType()) {
        var referencedEntity = values[fieldInfo.MappingInfo.Offset] as Entity;
        Session.ReferenceFieldsChangesRegistry.Register(keyOfThisEntity, referencedEntity.Key, fieldInfo);
        Session.EntityReferenceChangesRegistry.RegisterAddedReference(referencedEntity.State, State);
      }
    }

    /// <summary>
    /// Checks that reference from <paramref name="entity"/> was removed since last saving.
    /// </summary>
    /// <param name="entity">Entity to check.</param>
    /// <returns><see langword="true"/> if <see cref="Session.EntityReferenceChangesRegistry"/> contains information about removed reference, otherwise, <see langword="false"/>.</returns>
    internal bool IsReferenceDeleted(Entity entity)
    {
      return Session.EntityReferenceChangesRegistry.IsReferenceDeleted(State, entity.State);
    }

    /// <summary>
    /// Gets all entities which have new references to this entity until <see cref="Session.Persist(PersistReason)"/> or <see cref="Session.CancelChanges()"/> executed.
    /// </summary>
    /// <returns>All entities which have new references to this entity until persist.</returns>
    internal IEnumerable<Entity> GetNewReferencesFromEntities()
    {
      return Session.EntityReferenceChangesRegistry.GetAddedReferences(State).Keys.Select(referencingState => referencingState.Entity);
    }

    /// <summary>
    /// Gets all entities which have new references to this entity until <see cref="Session.Persist(PersistReason)"/> or <see cref="Session.CancelChanges()"/> executed.
    /// </summary>
    /// <returns>All entities which have new references to this entity until persist.</returns>
    internal IEnumerable<Entity> GetRemovedReferencesFromEntities()
    {
      return Session.EntityReferenceChangesRegistry.GetAddedReferences(State).Keys.Select(referensingState => referensingState.Entity);
    }

    #endregion

    #region System-level event-like members & GetSubscription members

    private bool SystemUpdateVersionInfo(Entity changedEntity, FieldInfo changedField)
    {
      Session.SystemEvents.NotifyEntityVersionInfoChanging(changedEntity, changedField, false);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyEntityVersionInfoChanging(changedEntity, changedField, false);
      }

      var changed = TypeInfo.HasExplicitVersionFields
        ? UpdateVersion(changedEntity, changedField)
        : !changedField.IsEntitySet;

      Session.SystemEvents.NotifyEntityVersionInfoChanged(changedEntity, changedField, changed);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyEntityVersionInfoChanged(changedEntity, changedField, changed);
      }

      return changed;
    }

    internal void SystemBeforeRemove()
    {
      OrmLog.Debug(Strings.LogSessionXRemovingKeyY, Session, Key);

      Session.SystemEvents.NotifyEntityRemoving(this);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyEntityRemoving(this);

        if (Session.IsSystemLogicOnly)
          return;

        var subscriptionInfo = GetSubscription(EntityEventBroker.RemovingEntityEventKey);
        if (subscriptionInfo.Second!=null)
          ((Action<Key>) subscriptionInfo.Second)
            .Invoke(subscriptionInfo.First);
        OnRemoving();
      }
    }

    internal void SystemRemove()
    {
      Session.SystemEvents.NotifyEntityRemove(this);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyEntityRemove(this);

        if (Session.IsSystemLogicOnly)
          return;

        var subscriptionInfo = GetSubscription(EntityEventBroker.RemoveEntityEventKey);
        if (subscriptionInfo.Second!=null)
          ((Action<Key>) subscriptionInfo.Second).Invoke(subscriptionInfo.First);
        OnRemove();
      }
    }

    internal void SystemRemoveCompleted(Exception exception)
    {
      Session.SystemEvents.NotifyEntityRemoveCompleted(this, exception);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyEntityRemoveCompleted(this, exception);
        NotifyPropertyChanged(WellKnown.PersistenceStatePropertyName);
      }
    }

    internal override sealed void SystemBeforeInitialize(bool materialize)
    {
      State.Entity = this;
      OrmLog.Debug(Strings.LogSessionXMaterializingYKeyZ, Session, GetType().GetShortName(), State.Key);

      if (Session.IsSystemLogicOnly || materialize) 
        return;

      // Not necessity to use
      // Session.Operations.EnableSystemOperationRegistration()
      // here, because there is no higher level system operation
      // for this one, or system op. registration is already enabled.
      
      bool hasKeyGenerator = Session.Domain.KeyGenerators[TypeInfo.Key]!=null;
      if (hasKeyGenerator) {
        Session.SystemEvents.NotifyKeyGenerated(Key);
        Session.Events.NotifyKeyGenerated(Key);
      }
      Session.SystemEvents.NotifyEntityCreated(this);
      Session.Events.NotifyEntityCreated(this);

      var operations = Session.Operations;
      // Operation 1:
      using (var scope = operations.BeginRegistration(OperationType.System)) {
        if (operations.CanRegisterOperation)
          operations.RegisterOperation(new KeyGenerateOperation(Key), true);
        scope.Complete();
      }
      // Operation 2:
      using (var scope = operations.BeginRegistration(OperationType.System)) {
        if (operations.CanRegisterOperation)
          operations.RegisterOperation(new EntityCreateOperation(Key), true);
        IdentifyAs(EntityIdentifierType.Auto);
        scope.Complete();
      }

      var subscriptionInfo = GetSubscription(EntityEventBroker.InitializingPersistentEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key>) subscriptionInfo.Second).Invoke(subscriptionInfo.First);
    }

    internal override sealed void SystemInitialize(bool materialize)
    {
      if (Session.IsSystemLogicOnly)
        return;

      // Not necessity to use
      // Session.Operations.EnableSystemOperationRegistration()
      // here, because there is no higher level system operation
      // for this one, or system op. registration is already enabled.

      var subscriptionInfo = GetSubscription(EntityEventBroker.InitializePersistentEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key>) subscriptionInfo.Second).Invoke(subscriptionInfo.First);

      OnInitialize();

      if (!materialize && CanBeValidated)
        Session.ValidationContext.RegisterForValidation(this);
    }

    internal override sealed void SystemInitializationError(Exception error)
    {
      if (Session.IsSystemLogicOnly)
        return;

      // Not necessity to use
      // Session.Operations.EnableSystemOperationRegistration()
      // here, because there is no higher level system operation
      // for this one, or system op. registration is already enabled.

      var subscriptionInfo = GetSubscription(EntityEventBroker.InitializationErrorPersistentEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First);
      OnInitializationError(error);

      if (State == null)
        return;
      State.PersistenceState = PersistenceState.Removed;
      NotifyPropertyChanged(WellKnown.PersistenceStatePropertyName);
      ((IInvalidatable)State).Invalidate();
      Session.EntityStateCache.Remove(State);
    }

    internal override sealed void SystemBeforeGetValue(FieldInfo field)
    {
      if (!Session.Configuration.Supports(SessionOptions.ReadRemovedObjects))
        EnsureNotRemoved();
      OrmLog.Debug(Strings.LogSessionXGettingValueKeyYFieldZ, Session, Key, field);
      EnsureIsFetched(field);

      Session.SystemEvents.NotifyFieldValueGetting(this, field);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyFieldValueGetting(this, field);

        if (Session.IsSystemLogicOnly)
          return;

        var subscriptionInfo = GetSubscription(EntityEventBroker.GettingFieldEventKey);
        if (subscriptionInfo.Second!=null)
          ((Action<Key, FieldInfo>) subscriptionInfo.Second)
            .Invoke(subscriptionInfo.First, field);
        OnGettingFieldValue(field);
      }
    }

    internal override sealed void SystemGetValue(FieldInfo field, object value)
    {
      Session.SystemEvents.NotifyFieldValueGet(this, field, value);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyFieldValueGet(this, field, value);

        if (Session.IsSystemLogicOnly)
          return;

        var subscriptionInfo = GetSubscription(EntityEventBroker.GetFieldEventKey);
        if (subscriptionInfo.Second!=null)
          ((Action<Key, FieldInfo, object>) subscriptionInfo.Second)
            .Invoke(subscriptionInfo.First, field, value);
        OnGetFieldValue(field, value);
      }
    }

    internal override sealed void SystemGetValueCompleted(FieldInfo field, object value, Exception exception)
    {
      Session.SystemEvents.NotifyFieldValueGetCompleted(this, field, value, exception);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyFieldValueGetCompleted(this, field, value, exception);
      }
    }

    internal override sealed void SystemSetValueAttempt(FieldInfo field, object value)
    {
      EnsureNotRemoved();

      OrmLog.Debug(Strings.LogSessionXSettingValueKeyYFieldZ, Session, Key, field);

      if (field.IsPrimaryKey)
        throw new NotSupportedException(string.Format(Strings.ExUnableToSetKeyFieldXExplicitly, field.Name));

      UpdateVersionInfo(this, field);

      Session.SystemEvents.NotifyFieldValueSettingAttempt(this, field, value);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyFieldValueSettingAttempt(this, field, value);

        if (Session.IsSystemLogicOnly)
          return;

        var subscriptionInfo = GetSubscription(EntityEventBroker.SettingFieldAttemptEventKey);
        if (subscriptionInfo.Second != null)
          ((Action<Key, FieldInfo, object>)subscriptionInfo.Second).Invoke(subscriptionInfo.First, field, value);
        OnSettingFieldValueAttempt(field, value);
        Session.ValidationContext.ValidateSetAttempt(this, field, value);
      }
    }

    internal override sealed void SystemBeforeSetValue(FieldInfo field, object value)
    {
      EnsureNotRemoved();

      Session.SystemEvents.NotifyFieldValueSetting(this, field, value);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyFieldValueSetting(this, field, value);

        if (Session.IsSystemLogicOnly)
          return;

        var subscriptionInfo = GetSubscription(EntityEventBroker.SettingFieldEventKey);
        if (subscriptionInfo.Second!=null)
          ((Action<Key, FieldInfo, object>) subscriptionInfo.Second).Invoke(subscriptionInfo.First, field, value);
        OnSettingFieldValue(field, value);
      }
    }

    internal override sealed void SystemBeforeTupleChange()
    {
      Session.SystemEvents.NotifyEntityChanging(this);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyEntityChanging(this);
      }

      if (PersistenceState != PersistenceState.New) {
        // Ensures there will be a DifferentialTuple, not the regular one
        var dTuple = State.DifferentialTuple;
      }
    }

    internal override void SystemTupleChange()
    {
      if (PersistenceState!=PersistenceState.New && PersistenceState!=PersistenceState.Modified) {
        Session.EnforceChangeRegistrySizeLimit(); // Must be done before the next line 
        // to avoid post-first property set flush.
        State.PersistenceState = PersistenceState.Modified;
      }
    }

    internal override sealed void SystemSetValue(FieldInfo field, object oldValue, object newValue)
    {
      Session.SystemEvents.NotifyFieldValueSet(this, field, oldValue, newValue);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyFieldValueSet(this, field, oldValue, newValue);

        if (Session.IsSystemLogicOnly)
          return;

        if (CanBeValidated)
          Session.ValidationContext.RegisterForValidation(this);

        var subscriptionInfo = GetSubscription(EntityEventBroker.SetFieldEventKey);
        if (subscriptionInfo.Second!=null)
          ((Action<Key, FieldInfo, object, object>) subscriptionInfo.Second)
            .Invoke(subscriptionInfo.First, field, oldValue, newValue);
        NotifyFieldChanged(field);
        OnSetFieldValue(field, oldValue, newValue);
      }
    }

    internal override sealed void SystemSetValueCompleted(FieldInfo field, object oldValue, object newValue, Exception exception)
    {
      Session.SystemEvents.NotifyFieldValueSetCompleted(this, field, oldValue, newValue, exception);
      using (Session.Operations.EnableSystemOperationRegistration()) {
        Session.Events.NotifyFieldValueSetCompleted(this, field, oldValue, newValue, exception);
      }
    }

    /// <inheritdoc/>
    protected override sealed Pair<Key, Delegate> GetSubscription(object eventKey)
    {
      if (state==null || state.TryGetEntity()!=this)
        return new Pair<Key, Delegate>();
      var entityKey = Key;
      return new Pair<Key, Delegate>(entityKey,
        Session.EntityEvents.GetSubscriber(entityKey, eventKey));
    }

    #endregion

    #region Serialization-related methods

    [SecurityCritical]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      using (Session.OpenSystemLogicOnlyRegion()) {
        SerializationContext.Demand().GetEntityData(this, info, context);
      }
    }

    void IDeserializationCallback.OnDeserialization(object sender)
    {
      using (Session.OpenSystemLogicOnlyRegion()) {
        DeserializationContext.Demand().OnDeserialization();
      }
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return Key.ToString();
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    protected Entity()
    {
      try {
        var key = Key.Generate(Session, GetType());
        State = Session.CreateEntityState(key, true);
        SystemBeforeInitialize(false);
      }
      catch (Exception error) {
        InitializationError(GetType(), error); 
        // GetType() call is correct here: no code will be executed further,
        // if base constructor will fail, but since descendant's constructor is aspected,
        // we must "simulate" its own call of InitializationError method.
        throw;
      }
    }

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="session">The session.</param>
    protected Entity(Session session)
      : base(session)
    {
      try
      {
        var key = Key.Generate(Session, GetType());
        State = Session.CreateEntityState(key, true);
        SystemBeforeInitialize(false);
      }
      catch (Exception error)
      {
        InitializationError(GetType(), error);
        // GetType() call is correct here: no code will be executed further,
        // if base constructor will fail, but since descendant's constructor is aspected,
        // we must "simulate" its own call of InitializationError method.
        throw;
      }
    }

    // Is used for EntitySetItem<,> instance construction
    internal Entity(Session session, Tuple keyTuple)
      : base(session)
    {
      try {
        ArgumentValidator.EnsureArgumentNotNull(keyTuple, "keyTuple");
        var key = Key.Create(Session.Domain, Session.StorageNodeId, GetTypeInfo(), TypeReferenceAccuracy.ExactType, keyTuple);
        State = Session.CreateEntityState(key, true);
        SystemBeforeInitialize(false);
        Initialize(GetType());
      }
      catch (Exception error) {
        InitializationError(GetType(), error);
        throw;
      }
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="values">The field values that will be used for key building.</param>
    /// <remarks>Use this kind of constructor when you need to explicitly set key for this instance.</remarks>
    /// <example>
    /// <code>
    /// [HierarchyRoot]
    /// public class Book : Entity
    /// {
    ///   [Field, KeyField]
    ///   public string ISBN { get; set; }
    ///   
    ///   public Book(string isbn) : base(isbn) { }
    /// }
    /// </code>
    /// </example>
    protected Entity(params object[] values)
    {
      try {
        ArgumentValidator.EnsureArgumentNotNull(values, "values");
        var key = Key.Create(Session.Domain, Session.StorageNodeId, GetTypeInfo(), TypeReferenceAccuracy.ExactType, values);
        State = Session.CreateEntityState(key, true);
        RegisterKeyFieldsOfEntityTypeForRemap(key, values);
        var operations = Session.Operations;
        using (operations.BeginRegistration(OperationType.System)) {
          if (operations.CanRegisterOperation)
            operations.RegisterOperation(new EntityInitializeOperation(key), true);
          var references = TypeInfo.Key.Fields.Where(f => f.IsEntity && f.Associations.Any(a => a.IsPaired)).ToList();
          if (references.Count > 0) {
            using (Session.DisableSaveChanges(this)) {
              foreach (var referenceField in references) {
                var referenceValue = (Entity)GetFieldValue(referenceField);
                Session.PairSyncManager.ProcessRecursively(null, null,
                  PairIntegrity.OperationType.Set, referenceField.GetAssociation(referenceValue.TypeInfo), this, referenceValue, null);
              }
            }
          }
        }
        SystemBeforeInitialize(false);
      }
      catch (Exception error) {
        InitializationError(GetType(), error); 
        // GetType() call is correct here: no code will be executed further,
        // if base constructor will fail, but since descendant's constructor is aspected,
        // we must "simulate" its own call of InitializationError method.
        throw;
      }
    }

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="values">The field values that will be used for key building.</param>
    /// <remarks>Use this kind of constructor when you need to explicitly set key for this instance.</remarks>
    /// <example>
    /// 	<code>
    /// [HierarchyRoot]
    /// public class Book : Entity
    /// {
    /// [Field, KeyField]
    /// public string ISBN { get; set; }
    /// public Book(string isbn) : base(isbn) { }
    /// }
    /// </code>
    /// </example>
    protected Entity(Session session, params object[] values)
      : base(session)
    {
      try {
        ArgumentValidator.EnsureArgumentNotNull(values, "values");
        var key = Key.Create(Session.Domain, Session.StorageNodeId, GetTypeInfo(), TypeReferenceAccuracy.ExactType, values);
        State = Session.CreateEntityState(key, true);
        RegisterKeyFieldsOfEntityTypeForRemap(key, values);
        var operations = Session.Operations;
        using (operations.BeginRegistration(OperationType.System)) {
          if (operations.CanRegisterOperation)
            operations.RegisterOperation(new EntityInitializeOperation(key), true);
          var references = TypeInfo.Key.Fields.Where(f => f.IsEntity && f.Associations.Any(a => a.IsPaired)).ToList();
          if (references.Count > 0) {
            using (Session.DisableSaveChanges(this)) {
              foreach (var referenceField in references) {
                var referenceValue = (Entity) GetFieldValue(referenceField);
                Session.PairSyncManager.ProcessRecursively(null, null, 
                  PairIntegrity.OperationType.Set, referenceField.GetAssociation(referenceValue.TypeInfo), this, referenceValue, null);
              }
            }
          }
        }
        SystemBeforeInitialize(false);
      }
      catch (Exception error) {
        InitializationError(GetType(), error); 
        // GetType() call is correct here: no code will be executed further,
        // if base constructor will fail, but since descendant's constructor is aspected,
        // we must "simulate" its own call of InitializationError method.
        throw;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity"/> class.
    /// </summary>
    /// <param name="state">The initial state of this instance fetched from storage.</param>
    /// <remarks>
    /// Used internally to initialize the entity on materialization.
    /// </remarks>
    protected Entity(EntityState state)
    {
      try {
        State = state;
        IsMaterializing = true;
        SystemBeforeInitialize(true);
        InitializeOnMaterialize();
      }
      catch (Exception error) {
        InitializationErrorOnMaterialize(error);
        throw;
      }
      finally {
        IsMaterializing = false;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity"/> class.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="state">The initial state of this instance fetched from storage.</param>
    /// <remarks>
    /// Used internally to initialize the entity on materialization.
    /// </remarks>
    protected Entity(Session session, EntityState state)
      : base(session)
    {
      try {
        State = state;
        IsMaterializing = true;
        SystemBeforeInitialize(true);
        InitializeOnMaterialize();
      }
      catch (Exception error) {
        InitializationErrorOnMaterialize(error);
        throw;
      }
      finally {
        IsMaterializing = false;
      }
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/>.</param>
    /// <param name="context">The <see cref="StreamingContext"/>.</param>
    protected Entity(SerializationInfo info, StreamingContext context)
    {
      using (Session.OpenSystemLogicOnlyRegion()) {
        DeserializationContext.Demand().SetObjectData(this, info, context);
      }
    }
  }
}