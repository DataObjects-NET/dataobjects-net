// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.01

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.ReferentialIntegrity;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// Principal data objects about which information has to be managed. 
  /// It has a unique identity, independent existence, and forms the operational unit of consistency.
  /// Instance of <see cref="Entity"/> type can be referenced via <see cref="Key"/>.
  /// </summary>
  public abstract class Entity : Persistent,
    IEntity
  {
    #region Internal properties

    [Infrastructure]
    internal EntityState State
    {
      [DebuggerStepThrough]
      get;
      [DebuggerStepThrough]
      set;
    }

    /// <exception cref="Exception">Property is already initialized.</exception>
    [Field]
    internal int TypeId
    {
      [DebuggerStepThrough]
      get { return GetField<int>(Session.Domain.NameBuilder.TypeIdFieldName); }
    }

    #endregion

    #region Properties: Key, Type, Data, PersistenceState

    /// <exception cref="Exception">Property is already initialized.</exception>
    [Infrastructure]
    public Key Key
    {
      [DebuggerStepThrough]
      get { return State.Key; }
    }

    /// <summary>
    /// Gets a value indicating whether this entity is removed.
    /// </summary>
    [Infrastructure]
    public bool IsRemoved {
      get {
        return State.IsRemoved;
      }
    }

    /// <inheritdoc/>
    public override sealed TypeInfo Type
    {
      [DebuggerStepThrough]
      get { return State.Type; }
    }

    /// <inheritdoc/>
    protected internal override sealed Tuple Data
    {
      [DebuggerStepThrough]
      get { return State.Data; }
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

    #region Remove method

    /// <inheritdoc/>
    [Infrastructure]
    public void Remove()
    {
      Remove(true);
    }

    #endregion

    #region Protected event-like methods

    /// <inheritdoc/>
    protected internal override bool SkipValidation
    {
      get { return IsRemoved; }
    }

    /// <summary>
    /// Called when entity is about to be removed.
    /// </summary>
    [Infrastructure]
    protected virtual void OnRemoving()
    {
    }

    /// <summary>
    /// Called when become removed.
    /// </summary>
    [Infrastructure]
    protected virtual void OnRemoved()
    {
    }

    #endregion

    #region Private \ internal methods

    internal override sealed void EnsureIsFetched(FieldInfo field)
    {
      var state = State;
      if (!(state.PersistenceState==PersistenceState.New || 
            state.Data.IsAvailable(field.MappingInfo.Offset)))
        Fetcher.Fetch(Key, field);
    }

    #endregion

    #region System-level members

    internal void Remove(bool notify)
    {
      if (notify)
        OnRemoving();

      if (Session.IsDebugEventLoggingEnabled)
        LogTemplate<Log>.Debug("Session '{0}'. Removing: Key = '{1}'", Session, Key);

      State.EnsureNotRemoved();

      Session.Persist();
      ReferenceManager.ClearReferencesTo(this);
      State.PersistenceState = PersistenceState.Removed;

      if (notify)
        OnRemoved();
    }

    #endregion

    #region System-level event-like members

    protected internal sealed override void OnBeforeInitialize()
    {
      base.OnBeforeInitialize();
      if (Session.IsDebugEventLoggingEnabled)
        Log.Debug("Session '{0}'. Creating entity: Key = '{1}'", Session, State.Key);
      State.Bind(this);
    }

    // This is done just to make it sealed
    protected internal sealed override void OnAfterInitialize()
    {
      base.OnAfterInitialize();
    }

    protected internal sealed override void OnBeforeGetField(FieldInfo field)
    {
      base.OnBeforeGetField(field);
      if (Session.IsDebugEventLoggingEnabled)
        LogTemplate<Log>.Debug("Session '{0}'. Getting value: Key = '{1}', Field = '{2}'", Session, Key, field);
      State.EnsureNotRemoved();
      EnsureIsFetched(field);
    }

    // This is done just to make it sealed
    protected sealed internal override void OnAfterGetField(FieldInfo field)
    {
      base.OnAfterGetField(field);
    }

    protected sealed internal override void OnBeforeSetField(FieldInfo field)
    {
      base.OnBeforeSetField(field);
      if (Session.IsDebugEventLoggingEnabled)
        LogTemplate<Log>.Debug("Session '{0}'. Setting value: Key = '{1}', Field = '{2}'", Session, Key, field);
      if (field.IsPrimaryKey)
        throw new NotSupportedException(string.Format(Strings.ExUnableToSetKeyFieldXExplicitly, field.Name));
      State.EnsureNotRemoved();
    }

    protected internal sealed override void OnAfterSetField(FieldInfo field)
    {
      base.OnAfterSetField(field);
      if (PersistenceState!=PersistenceState.New && PersistenceState!=PersistenceState.Modified)
        State.PersistenceState = PersistenceState.Modified;
    }

    #endregion

    // Constructors


    private Entity(bool nullEntity)
    {
    }
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected Entity()
    {
      Key key = Key.Create(Session.Domain.Model.Types[GetType()]);
      State = Session.Cache.Add(key);
      Initialize(true);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="tuple">The <see cref="Data"/> that will be used for key building.</param>
    /// <remarks>Use this kind of constructor when you need to explicitly set key for this instance.</remarks>
    protected Entity(Tuple tuple)
    {
      ArgumentValidator.EnsureArgumentNotNull(tuple, "tuple");
      Key key = Key.Create(Session.Domain.Model.Types[GetType()], tuple, true);
      State = Session.Cache.Add(key);
      Initialize(true);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate()" copy="true"/>
    /// </summary>
    /// <param name="state">The initial state of this instance fetched from storage.</param>
    protected Entity(EntityState state)
    {
      State = state;
      Initialize(false);
    }
  }
}