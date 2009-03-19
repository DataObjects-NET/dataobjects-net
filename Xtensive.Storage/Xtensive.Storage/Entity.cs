// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.01

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Integrity.Validation;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
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
    internal EntityState State { get; set; }

    /// <exception cref="Exception">Property is already initialized.</exception>
    [Field]
    internal int TypeId {
      [DebuggerStepThrough]
      get { return GetFieldValue<int>(Session.Domain.NameBuilder.TypeIdFieldName); }
    }

    #endregion

    #region Properties: Key, Type, Tuple, PersistenceState

    /// <exception cref="Exception">Property is already initialized.</exception>
    [Infrastructure]
    public Key Key {
      [DebuggerStepThrough]
      get { return State.Key; }
    }

    /// <summary>
    /// Gets a value indicating whether this entity is removed.
    /// </summary>
    [Infrastructure]
    public bool IsRemoved {
      get { return State.IsRemoved; }
    }

    /// <inheritdoc/>
    internal override sealed TypeInfo Type {
      [DebuggerStepThrough]
      get { return State.Type; }
    }

    /// <inheritdoc/>
    protected internal override sealed Tuple Tuple {
      [DebuggerStepThrough]
      get { return State.Tuple; }
    }

    /// <summary>
    /// Gets persistence state of the entity.
    /// </summary>
    [Infrastructure]
    public PersistenceState PersistenceState {
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

    /// <inheritdoc/>
    [Infrastructure]
    public void Remove()
    {
      Remove(true);
    }

    /// <summary>
    /// Finds the objects that reference this instance.
    /// </summary>
    /// <returns>The set of objects that reference this instance</returns>
    [Infrastructure]
    public IEnumerable<Entity> FindReferencingObjects()
    {
      foreach (AssociationInfo association in Type.GetAssociations())
        foreach (Entity item in association.FindReferencingObjects(this))
          yield return item;
    }

    /// <summary>
    /// Finds the objects that reference this instance within specified <paramref name="association"/>.
    /// </summary>
    /// <returns>The set of objects that reference this instance within specified <paramref name="association"/>.</returns>
    /// <exception cref="InvalidOperationException">Type doesn't participate in the specified association.</exception>
    [Infrastructure]
    public IEnumerable<Entity> FindReferencingObjects(AssociationInfo association)
    {
      if (!association.ReferencedType.UnderlyingType.IsAssignableFrom(Type.UnderlyingType))
        throw new InvalidOperationException(string.Format("Type '{0}' doesn't participate in the specified association.", Type.Name));
      return association.FindReferencingObjects(this);
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
    /// Called when entity becomes removed.
    /// </summary>
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
      if (notify)
        OnRemoving();

      if (Session.IsDebugEventLoggingEnabled)
        LogTemplate<Log>.Debug("Session '{0}'. Removing: Key = '{1}'", Session, Key);

      State.EnsureNotRemoved();

      if (Type.GetAssociations().Count>0) 
        Session.Persist();
      Session.ReferenceManager.ClearReferencesTo(this, notify);
      State.PersistenceState = PersistenceState.Removed;

      if (notify)
        OnRemove();
    }

    #endregion

    #region System-level event-like members

    internal sealed override void OnInitializing(bool notify)
    {
      base.OnInitializing(notify);
      State.Entity = this;
      if (Session.IsDebugEventLoggingEnabled)
        Log.Debug("Session '{0}'. Materializing {1}: Key = '{2}'",
          Session, GetType().GetShortName(), State.Key);
    }

    // This is done just to make it sealed
    internal sealed override void OnInitialize(bool notify)
    {
      base.OnInitialize(notify);
    }

    internal sealed override void OnGettingFieldValue(FieldInfo field, bool notify)
    {
      base.OnGettingFieldValue(field, notify);
      if (Session.IsDebugEventLoggingEnabled)
        LogTemplate<Log>.Debug("Session '{0}'. Getting value: Key = '{1}', Field = '{2}'", Session, Key, field);
      State.EnsureNotRemoved();
      EnsureIsFetched(field);
    }

    // This is done just to make it sealed
    sealed internal override void OnGetFieldValue(FieldInfo field, object value, bool notify)
    {
      base.OnGetFieldValue(field, value, notify);
    }

    sealed internal override void OnSettingFieldValue(FieldInfo field, object value, bool notify)
    {
      base.OnSettingFieldValue(field, value, notify);
      if (Session.IsDebugEventLoggingEnabled)
        LogTemplate<Log>.Debug("Session '{0}'. Setting value: Key = '{1}', Field = '{2}'", Session, Key, field);
      if (field.IsPrimaryKey)
        throw new NotSupportedException(string.Format(Strings.ExUnableToSetKeyFieldXExplicitly, field.Name));
      State.EnsureNotRemoved();
    }

    internal sealed override void OnSetFieldValue(FieldInfo field, object oldValue, object newValue, bool notify)
    {
      if (PersistenceState!=PersistenceState.New && PersistenceState!=PersistenceState.Modified)
        State.PersistenceState = PersistenceState.Modified;
      base.OnSetFieldValue(field, oldValue, newValue, notify);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected Entity()
    {
      Key key = Key.Create(Session.Domain.Model.Types[GetType()]);
      State = Session.CreateEntityState(key);
      OnInitializing(true);
      this.Validate();
    }
      
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="tuple">The <see cref="Tuple"/> that will be used for key building.</param>
    /// <remarks>Use this kind of constructor when you need to explicitly set key for this instance.</remarks>
    protected Entity(Tuple tuple)
    {
      ArgumentValidator.EnsureArgumentNotNull(tuple, "tuple");
      Key key = Key.Create(Session.Domain.Model.Types[GetType()], tuple, true);
      State = Session.CreateEntityState(key);
      OnInitializing(true);
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
  }
}