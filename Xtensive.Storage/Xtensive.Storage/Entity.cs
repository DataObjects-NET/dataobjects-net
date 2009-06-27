// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.01

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Integrity.Validation;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Serialization;

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
  public abstract class Entity : Persistent,
    IEntity, ISerializable, IDeserializationCallback
  {
    #region Internal properties

    [Infrastructure]
    internal EntityState State { get; set; }

    /// <exception cref="Exception">Property is already initialized.</exception>
    [Field]
    internal int TypeId
    {
      [DebuggerStepThrough]
      get { return GetFieldValue<int>(WellKnown.TypeIdFieldName); }
    }

    #endregion

    #region Properties: Key, Type, Tuple, PersistenceState

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
    [Infrastructure]
    public bool IsRemoved
    {
      get { return State.IsRemoved; }
    }

    /// <inheritdoc/>
    internal override sealed TypeInfo Type
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

    #endregion

    #region IIdentifier members

    /// <inheritdoc/>
    [Infrastructure]
    Key IIdentified<Key>.Identifier
    {
      [DebuggerStepThrough]
      get { return Key; }
    }

    /// <inheritdoc/>
    [Infrastructure]
    object IIdentified.Identifier
    {
      [DebuggerStepThrough]
      get { return Key; }
    }

    #endregion

    #region Public members


    /// <summary>
    /// Removes this entity.
    /// </summary>
    /// <seealso cref="IsRemoved"/>
    [Infrastructure]
    public void Remove()
    {
      Remove(true);
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
    [Infrastructure]
    protected virtual void OnRemoving()
    {
    }

    /// <summary>
    /// Called when entity becomes removed.
    /// </summary>
    /// <remarks>
    /// Override this method to perform some actions when entity is removed.
    /// </remarks>
    [Infrastructure]
    protected virtual void OnRemove()
    {
    }

    #endregion

    #region Private \ internal methods

    internal override sealed void EnsureIsFetched(FieldInfo field)
    {
      var state = State;
      if (!(state.PersistenceState==PersistenceState.New ||
        state.Tuple.IsAvailable(field.MappingInfo.Offset)))
        Fetcher.Fetch(Key, field);
    }

    #endregion

    #region System-level members

    internal void Remove(bool notify)
    {
      using (Session.OpenInconsistentRegion()) {

        if (notify)
          OnRemoving();

        if (Session.IsDebugEventLoggingEnabled)
          Log.Debug("Session '{0}'. Removing: Key = '{1}'", Session, Key);
        if (notify)
          Session.NotifyRemovingEntity(this);

        State.EnsureNotRemoved();

        Session.Persist();
        Session.ReferenceManager.BreakAssociations(this, notify);
        Session.Persist();
        State.PersistenceState = PersistenceState.Removed;

        if (notify) {
          OnRemove();
          Session.NotifyRemoveEntity(this);
        }
      }
    }

    #endregion

    #region System-level event-like members

    internal override sealed void OnInitializing(bool notify)
    {
      base.OnInitializing(notify);
      State.Entity = this;
      if (Session.IsDebugEventLoggingEnabled)
        Log.Debug("Session '{0}'. Materializing {1}: Key = '{2}'",
          Session, GetType().GetShortName(), State.Key);
    }

    // This is done just to make it sealed
    internal override sealed void OnInitialize(bool notify)
    {
      base.OnInitialize(notify);
    }

    internal override sealed void OnGettingFieldValue(FieldInfo field, bool notify)
    {
      base.OnGettingFieldValue(field, notify);
      if (Session.IsDebugEventLoggingEnabled)
        Log.Debug("Session '{0}'. Getting value: Key = '{1}', Field = '{2}'", Session, Key, field);
      State.EnsureNotRemoved();
      EnsureIsFetched(field);
    }

    // This is done just to make it sealed
    internal override sealed void OnGetFieldValue(FieldInfo field, object value, bool notify)
    {
      base.OnGetFieldValue(field, value, notify);
    }

    internal override sealed void OnSettingFieldValue(FieldInfo field, object value, bool notify)
    {
      base.OnSettingFieldValue(field, value, notify);
      if (Session.IsDebugEventLoggingEnabled)
        Log.Debug("Session '{0}'. Setting value: Key = '{1}', Field = '{2}'", Session, Key, field);
      if (field.IsPrimaryKey)
        throw new NotSupportedException(string.Format(Strings.ExUnableToSetKeyFieldXExplicitly, field.Name));
        State.EnsureNotRemoved();
    }

    internal override sealed void OnSetFieldValue(FieldInfo field, object oldValue, object newValue, bool notify)
    {
      if (PersistenceState!=PersistenceState.New && PersistenceState!=PersistenceState.Modified)
        State.PersistenceState = PersistenceState.Modified;
      base.OnSetFieldValue(field, oldValue, newValue, notify);
    }

    #endregion

    #region Serialization-related methods

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {     
      SerializationContext.Demand().GetEntityData(this, info, context);
    }

    void IDeserializationCallback.OnDeserialization(object sender)
    {
      DeserializationContext.Demand().OnDeserialization(); 
    }

    #endregion

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected Entity()
    {
      Key key = Key.Create(GetTypeInfo());
      State = Session.CreateEntityState(key);
      OnInitializing(true);
      Session.NotifyCreateEntity(this);
      this.Validate();
    }

    // Is used for EntitySetItem<,> instance construction
    internal Entity(Tuple keyTuple)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyTuple, "keyTuple");
      Key key = Key.Create(GetTypeInfo(), keyTuple, true);
      State = Session.CreateEntityState(key);
      OnInitializing(true);
      // TODO: Add Session.NotifyCreateEntity()?
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
      Key key = Key.Create(GetTypeInfo(), true, values);
      State = Session.CreateEntityState(key);
      OnInitializing(true);
      Session.NotifyCreateEntity(this);
      this.Validate();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate()" copy="true"/>
    /// </summary>
    /// <param name="state">The initial state of this instance fetched from storage.</param>
    /// <param name="notify">If set to <see langword="true"/>, 
    /// initialization related events will be raised.</param>
    protected Entity(EntityState state, bool notify)
    {
      State = state;
      OnInitializing(notify);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/>.</param>
    /// <param name="context">The <see cref="StreamingContext"/>.</param>
    protected Entity(SerializationInfo info, StreamingContext context)
    {
      DeserializationContext.Demand().SetEntityData(this, info, context);
    }    
  }
}