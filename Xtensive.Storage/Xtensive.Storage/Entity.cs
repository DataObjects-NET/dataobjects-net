// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.01

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Integrity.Validation;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Internals.Prefetch;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Serialization;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage
{
  /// <summary>
  /// Base class for all entities in a model.
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
  [SystemType]
  [DebuggerDisplay("{Key}")]
  public abstract class Entity : Persistent,
    IEntity,
    ISerializable,
    IDeserializationCallback,
    IHasVersion<VersionInfo>
  {
    private static readonly Parameter<Tuple> keyParameter = new Parameter<Tuple>(WellKnown.KeyFieldName);

    #region Internal properties

    internal EntityState State { get; set; }

    /// <exception cref="Exception">Property is already initialized.</exception>
    [Field]
    internal int TypeId
    {
      [DebuggerStepThrough]
      get { return GetFieldValue<int>(WellKnown.TypeIdFieldName); }
    }

    #endregion

    #region Properties: Key, Type, Tuple, PersistenceState, Version

    /// <summary>
    /// Gets the <see cref="Key"/> that identifies this entity.
    /// </summary>
    [Infrastructure]
    public Key Key
    {
      [DebuggerStepThrough]
      get { return State.Key; }
    }

    /// <summary>
    /// Gets a value indicating whether this entity is removed.
    /// </summary>
    /// <seealso cref="Remove"/>
    public bool IsRemoved
    {
      get
      {
        if (Session.IsPersisting)
          // Removed = "already removed from storage" here
          return State.IsNotAvailable;
        else
          // Removed = "either already removed, or marked as removed" here
          return State.IsNotAvailableOrMarkedAsRemoved;
      }
    }

    /// <inheritdoc/>
    public override sealed TypeInfo Type
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
    [Infrastructure]
    public PersistenceState PersistenceState
    {
      [DebuggerStepThrough]
      get { return State.PersistenceState; }
    }

    /// <inheritdoc/>
    [Infrastructure]
    object IHasVersion.Version
    {
      [DebuggerStepThrough]
      get { return GetVersion(); }
    }

    /// <inheritdoc/>
    [Infrastructure]
    VersionInfo IHasVersion<VersionInfo>.Version
    {
      [DebuggerStepThrough]
      get { return GetVersion(); }
    }

    #endregion

    #region IIdentifier members

    /// <inheritdoc/>
    [Infrastructure] // Proxy
      Key IIdentified<Key>.Identifier
    {
      [DebuggerStepThrough]
      get { return Key; }
    }

    /// <inheritdoc/>
    [Infrastructure] // Proxy
      object IIdentified.Identifier
    {
      [DebuggerStepThrough]
      get { return Key; }
    }

    #endregion

    #region Public members

    /// <summary>
    /// Gets the current version.
    /// </summary>
    /// <returns>Current version.</returns>
    public VersionInfo GetVersion()
    {
      if (Type.HasVersionRoots) {
        var version = new VersionInfo();
        foreach (var root in ((IHasVersionRoots) this).GetVersionRoots()) {
          if (root is IHasVersionRoots)
            throw new InvalidOperationException(Strings.ExVersionRootObjectCantImplementIHasVersionRoots);
          version = version.Join(root.Key, root.GetVersion());
        }
        return version;
      }
      if (Type.VersionExtractor==null)
        return new VersionInfo(); // returns empty VersionInfo
      var tuple = State.Tuple;
      var fieldsToLoad = Type.GetVersionColumns()
        .Where(pair => !tuple.GetFieldState(pair.Second).IsAvailable())
        .Select(pair => new PrefetchFieldDescriptor(pair.First.Field, false))
        .ToArray();
      if (fieldsToLoad.Length > 0) {
        Session.Handler.Prefetch(Key, Type, fieldsToLoad);
        Session.Handler.ExecutePrefetchTasks();
      }
      var versionTuple = Type.VersionExtractor.Apply(TupleTransformType.Tuple, State.Tuple);
      return new VersionInfo(versionTuple);
    }
    
    /// <summary>
    /// Removes this entity.
    /// </summary>
    /// <exception cref="ReferentialIntegrityException">
    /// Entity is associated with another entity with <see cref="OnRemoveAction.Deny"/> on-remove action.</exception>
    /// <seealso cref="IsRemoved"/>
    public void Remove()
    {
      EnsureNotRemoved();
      using (var region = Validation.Disable()) {
        NotifyRemoving();

        if (Session.IsDebugEventLoggingEnabled)
          Log.Debug(Strings.LogSessionXRemovingKeyY, Session, Key);
        Session.RemovalProcessor.Remove(this);

        NotifyRemove();

        region.Complete();
      }
    }

    /// <inheritdoc/>
    public override event PropertyChangedEventHandler PropertyChanged
    {
      add { Session.EntityEventBroker.AddSubscriber(Key, EntityEventBroker.PropertyChangedEventKey, value); }
      remove { Session.EntityEventBroker.RemoveSubscriber(Key, EntityEventBroker.PropertyChangedEventKey, value); }
    }

    /// <summary>
    /// Locks this instance in the storage.
    /// </summary>
    /// <param name="lockMode">The lock mode.</param>
    /// <param name="lockBehavior">The lock behavior.</param>
    public void Lock(LockMode lockMode, LockBehavior lockBehavior)
    {
      using (new ParameterContext().Activate()) {
        keyParameter.Value = Key.Value;
        var recordSet = (RecordSet) Session.Domain.GetCachedItem(
          new Triplet<TypeInfo, LockMode, LockBehavior>(Type, lockMode, lockBehavior),
          tripletObj => {
            var triplet = (Triplet<TypeInfo, LockMode, LockBehavior>) tripletObj;
            return IndexProvider.Get(triplet.First.Indexes.PrimaryIndex).Result.Seek(keyParameter.Value)
              .Lock(() => triplet.Second, () => triplet.Third).Select();
          });
        recordSet.First();
      }
    }

    #endregion

    #region Protected event-like methods

    /// <inheritdoc/>
    protected internal override bool CanBeValidated
    {
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
    /// Called when version is updated.
    /// </summary>
    protected virtual void UpdateVersion()
    {
      foreach (var field in Type.GetVersionFields())
        this[field.Name] = IncrementalVersionGenerator.GetNext(this[field.Name]);
    }

    #endregion

    #region Private \ internal methods

    /// <exception cref="InvalidOperationException">Entity is removed.</exception>
    private void EnsureNotRemoved()
    {
      if (IsRemoved)
        throw new InvalidOperationException(Strings.ExEntityIsRemoved);
    }

    internal override sealed void EnsureIsFetched(FieldInfo field)
    {
      var state = State;

      if (state.PersistenceState==PersistenceState.New)
        return;

      var tuple = state.Tuple;
      if (tuple.GetFieldState(field.MappingInfo.Offset).IsAvailable())
        return;

      Session.Handler.FetchField(Key, field);
    }

    internal override void PrepareToSetField()
    {
      if (PersistenceState!=PersistenceState.New) {
        var dTuple = State.DifferentialTuple;
      }
    }

    internal virtual void NotifyRemoving()
    {
      if (Session.IsSystemLogicOnly)
        return;
      Session.NotifyEntityRemoving(this);
      var subscriptionInfo = GetSubscription(EntityEventBroker.RemovingEntityEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First);
      OnRemoving();
    }

    internal virtual void NotifyRemove()
    {
      if (Session.IsSystemLogicOnly)
        return;
      Session.NotifyEntityRemoved(this);
      var subscriptionInfo = GetSubscription(EntityEventBroker.RemoveEntityEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key>) subscriptionInfo.Second).Invoke(subscriptionInfo.First);
      OnRemove();
    }

    /// <exception cref="NotSupportedException"><c>NotSupportedException</c>.</exception>
    internal void UpdateVersionInternal()
    {
      if ((!Type.HasVersionFields
        && !Type.HasVersionRoots)
        || IsRemoved
        || State.IsVersionUpdated)
        return;

      try {
        State.IsVersionUpdated = true;
        if (!Type.HasVersionRoots)
          UpdateVersion();
        else
          foreach (var root in ((IHasVersionRoots) this).GetVersionRoots()) {
            if (root.Type.HasVersionRoots)
              throw new NotSupportedException(Strings.ExVersionRootObjectCantImplementIHasVersionRoots);
            root.UpdateVersionInternal();
          }
      }
      catch {
        State.IsVersionUpdated = false;
        throw;
      }
    }

    #endregion

    #region NotifyXxx & GetSubscription members

    internal override sealed void NotifyInitializing()
    {
      if (!Session.IsSystemLogicOnly) {
        Session.NotifyEntityCreated(this);
        var subscriptionInfo = GetSubscription(EntityEventBroker.InitializingPersistentEventKey);
        if (subscriptionInfo.Second!=null)
          ((Action<Key>) subscriptionInfo.Second).Invoke(subscriptionInfo.First);
      }
      State.Entity = this;
      if (Session.IsDebugEventLoggingEnabled)
        Log.Debug(Strings.LogSessionXMaterializingYKeyZ,
          Session, GetType().GetShortName(), State.Key);
    }

    internal override sealed void NotifyInitialize()
    {
      if (Session.IsSystemLogicOnly)
        return;
      var subscriptionInfo = GetSubscription(EntityEventBroker.InitializePersistentEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First);
      OnInitialize();
    }

    internal override sealed void NotifyGettingFieldValue(FieldInfo fieldInfo)
    {
      EnsureNotRemoved();
      if (Session.IsDebugEventLoggingEnabled)
        Log.Debug(Strings.SessionXGettingValueKeyYFieldZ, Session, Key, fieldInfo);
      EnsureIsFetched(fieldInfo);

      if (Session.IsSystemLogicOnly)
        return;

      Session.NotifyFieldValueGetting(this, fieldInfo);
      var subscriptionInfo = GetSubscription(EntityEventBroker.GettingFieldEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, fieldInfo);
      OnGettingFieldValue(fieldInfo);
    }

    internal override sealed void NotifyGetFieldValue(FieldInfo field, object value)
    {
      if (Session.IsSystemLogicOnly)
        return;
      Session.NotifyFieldValueGet(this, field, value);
      var subscriptionInfo = GetSubscription(EntityEventBroker.GetFieldEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo, object>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, field, value);
      OnGetFieldValue(field, value);
    }

    internal override sealed void NotifySettingFieldValue(FieldInfo field, object value)
    {
      EnsureNotRemoved();
      if (Session.IsDebugEventLoggingEnabled)
        Log.Debug(Strings.LogSessionXSettingValueKeyYFieldZ, Session, Key, field);
      if (field.IsPrimaryKey)
        throw new NotSupportedException(string.Format(Strings.ExUnableToSetKeyFieldXExplicitly, field.Name));
      if (Session.IsSystemLogicOnly)
        return;
      Session.NotifyFieldValueSetting(this, field, value);
      var subscriptionInfo = GetSubscription(EntityEventBroker.SettingFieldEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo, object>) subscriptionInfo.Second).Invoke(subscriptionInfo.First, field, value);
      OnSettingFieldValue(field, value);
    }

    internal override sealed void NotifySetFieldValue(FieldInfo field, object oldValue, object newValue)
    {
      if (!Equals(oldValue, newValue) || field.IsStructure) 
        if (PersistenceState!=PersistenceState.New && PersistenceState!=PersistenceState.Modified) {
          Session.EnforceChangeRegistrySizeLimit(); // Must be done before the next line 
          // to avoid post-first property set flush.
          State.PersistenceState = PersistenceState.Modified;
        }
      UpdateVersionInternal();
      
      if (Session.IsSystemLogicOnly)
        return;
      Session.NotifyEntityFieldValueSet(this, field, oldValue, newValue);
      var subscriptionInfo = GetSubscription(EntityEventBroker.SetFieldEventKey);
      if (subscriptionInfo.Second!=null)
        ((Action<Key, FieldInfo, object, object>) subscriptionInfo.Second)
          .Invoke(subscriptionInfo.First, field, oldValue, newValue);
      OnSetFieldValue(field, oldValue, newValue);
      base.NotifySetFieldValue(field, oldValue, newValue);
    }

    protected internal override void NotifyPropertyChanged(FieldInfo field)
    {
      if (!Session.EntityEventBroker.HasSubscribers)
        return;
      var subscriber = GetSubscription(EntityEventBroker.PropertyChangedEventKey);
      if (subscriber.Second!=null)
        ((PropertyChangedEventHandler) subscriber.Second).Invoke(this,
          new PropertyChangedEventArgs(field.Name));
    }

    protected Pair<Key, Delegate> GetSubscription(object eventKey)
    {
      var entityKey = Key;
      return new Pair<Key, Delegate>(entityKey,
        Session.EntityEventBroker.GetSubscriber(entityKey, eventKey));
    }

    #endregion

    #region Serialization-related methods

    [Infrastructure]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      using (CoreServices.OpenSystemLogicOnlyRegion()) {
        SerializationContext.Demand().GetEntityData(this, info, context);
      }
    }

    [Infrastructure]
    void IDeserializationCallback.OnDeserialization(object sender)
    {
      using (CoreServices.OpenSystemLogicOnlyRegion()) {
        DeserializationContext.Demand().OnDeserialization();
      }
    }

    #endregion

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected Entity()
    {
      var key = Key.Create(Session.Domain, GetType());
      State = Session.CreateEntityState(key);
      NotifyInitializing();
      this.Validate();
    }

    // Is used for EntitySetItem<,> instance construction
    internal Entity(Tuple keyTuple)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyTuple, "keyTuple");
      Key key = Key.Create(Session.Domain, GetTypeInfo(), TypeReferenceAccuracy.ExactType, keyTuple);
      State = Session.CreateEntityState(key);
      NotifyInitializing();
      // TODO: Add Session.NotifyEntityCreated()?
      this.Validate();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
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
      ArgumentValidator.EnsureArgumentNotNull(values, "values");
      Key key = Key.Create(Session.Domain, GetTypeInfo(), TypeReferenceAccuracy.ExactType, values);
      State = Session.CreateEntityState(key);
      NotifyInitializing();
      this.Validate();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate()" copy="true"/>
    /// </summary>
    /// <param name="state">The initial state of this instance fetched from storage.</param>
    protected Entity(EntityState state)
    {
      State = state;
      NotifyInitializing();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/>.</param>
    /// <param name="context">The <see cref="StreamingContext"/>.</param>
    protected Entity(SerializationInfo info, StreamingContext context)
    {
      using (CoreServices.OpenSystemLogicOnlyRegion()) {
        DeserializationContext.Demand().SetEntityData(this, info, context);
      }
    }

    
  }
}