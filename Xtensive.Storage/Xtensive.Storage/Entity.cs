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
using Xtensive.Core.Reflection;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples;
using Xtensive.Integrity.Validation;
using Xtensive.Integrity.Validation.Interfaces;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
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
    private static readonly ThreadSafeDictionary<Type, Func<EntityState, Entity>> activators = 
      ThreadSafeDictionary<Type, Func<EntityState, Entity>>.Create(new object());
    private readonly EntityState entityState;

    #region Internal properties

    [Infrastructure]
    internal EntityState EntityState {
      [DebuggerStepThrough]
      get { return entityState; }
    }

    /// <exception cref="Exception">Property is already initialized.</exception>
    [Field]
    internal int TypeId {
      [DebuggerStepThrough]
      get { return GetValue<int>(Session.Domain.NameBuilder.TypeIdFieldName); }
    }

    #endregion

    #region Properties: Key, Type, Data, PersistenceState

    /// <exception cref="Exception">Property is already initialized.</exception>
    [Infrastructure]
    public Key Key {
      [DebuggerStepThrough]
      get { return EntityState.Key; }
    }

    /// <summary>
    /// Gets a value indicating whether this entity is removed.
    /// </summary>
    [Infrastructure]
    public bool IsRemoved
    {
      get {
        EntityState.EnsureIsActual();
        return EntityState.IsRemoved;
      }
    }

    /// <inheritdoc/>
    public override sealed TypeInfo Type {
      [DebuggerStepThrough]
      get { return EntityState.Type; }
    }

    /// <inheritdoc/>
    protected internal sealed override Tuple Data {
      [DebuggerStepThrough]
      get { return EntityState; }
    }

    /// <summary>
    /// Gets persistence state of the entity.
    /// </summary>
    [Infrastructure]
    public PersistenceState PersistenceState
    {
      [DebuggerStepThrough]
      get { return EntityState.PersistenceState; }
    }

    #endregion

    #region IIdentifier members

    /// <inheritdoc/>
    [Infrastructure]
    Key IIdentified<Key>.Identifier {
      [DebuggerStepThrough]
      get { return Key; }
    }

    /// <inheritdoc/>
    [Infrastructure]
    object IIdentified.Identifier {
      [DebuggerStepThrough]
      get { return Key; }
    }

    #endregion

    #region Remove method

    /// <inheritdoc/>
    [Infrastructure]
    public void Remove()
    {      
      if (Log.IsLogged(LogEventTypes.Debug))
        Log.Debug("Session '{0}'. Removing: Key = '{1}'", Session, Key);

      entityState.EnsureIsActual();
      entityState.EnsureIsNotRemoved();
      OnRemoving();

      Session.Persist();
      ReferenceManager.ClearReferencesTo(this);      
      Session.removedEntities.Add(EntityState);
      Session.Cache.Remove(EntityState);
      EntityState.PersistenceState = PersistenceState.Removed;
      
      OnRemoved();
    }

    #endregion

    #region Protected event-like methods

    /// <inheritdoc/>
    protected internal override bool SkipValidation
    {
      get { return IsRemoved; }
    }

    /// <inheritdoc/>
    protected internal override sealed void OnCreating()
    {
      EntityState.Entity = this;
      if (PersistenceState!=PersistenceState.New) {
        Session.newEntities.Add(EntityState);
        EntityState.PersistenceState = PersistenceState.New;
      }
      this.Validate();
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Entity is removed.</exception>
    protected internal override sealed void OnGettingValue(FieldInfo field)
    {   
      if (Log.IsLogged(LogEventTypes.Debug))
        Log.Debug("Session '{0}'. Getting value: Key = '{1}', Field = '{2}'", Session, Key, field);
      entityState.EnsureIsActual();
      entityState.EnsureIsNotRemoved();
      EnsureIsFetched(field);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Entity is removed.</exception>
    protected internal override sealed void OnSettingValue(FieldInfo field)
    { 
      if (Log.IsLogged(LogEventTypes.Debug))
        Log.Debug("Session '{0}'. Setting value: Key = '{1}', Field = '{2}'", Session, Key, field);
      if (field.IsPrimaryKey)
        throw new NotSupportedException(string.Format(Strings.ExUnableToSetKeyFieldXExplicitly, field.Name));
      entityState.EnsureIsActual();
      entityState.EnsureIsNotRemoved();
    }

    /// <inheritdoc/>
    protected internal override sealed void OnSetValue(FieldInfo field)
    {
      if (PersistenceState!=PersistenceState.New && PersistenceState!=PersistenceState.Modified) {
        Session.modifiedEntities.Add(EntityState);
        EntityState.PersistenceState = PersistenceState.Modified;
      }
      base.OnSetValue(field);      
    }    

    /// <summary>
    /// Called when entity is to be removed.
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

    internal static Entity Activate(Type type, EntityState state)
    {
      return activators.GetValue(type,
        DelegateHelper.CreateConstructorDelegate<Func<EntityState, Entity>>)
        .Invoke(state);
    }

    internal override sealed void EnsureIsFetched(FieldInfo field)
    {
      if (!EntityState.IsFetched(field.MappingInfo.Offset))
        Fetcher.Fetch(Key, field);
    }

    #endregion

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected Entity()
    {
      TypeInfo type = Session.Domain.Model.Types[GetType()];
      Key key = Session.Domain.KeyManager.Next(type);

      if (Log.IsLogged(LogEventTypes.Debug))
        Log.Debug("Session '{0}'. Creating entity: Key = '{1}'", Session, key);

      entityState = Session.Cache.Create(key, Session.Transaction);
      OnCreating();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="tuple">The <see cref="Data"/> that will be used for key building.</param>
    /// <remarks>Use this kind of constructor when you need to explicitly build key for this instance.</remarks>
    protected Entity(Tuple tuple)
    {
      TypeInfo type = Session.Domain.Model.Types[GetType()];
      Key key = Session.Domain.KeyManager.Get(type, tuple);

      if (Log.IsLogged(LogEventTypes.Debug))
        Log.Debug("Session '{0}'. Creating entity: Key = '{1}'", Session, key);

      entityState = Session.Cache.Create(key, Session.Transaction);
      OnCreating();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate()" copy="true"/>
    /// </summary>
    /// <param name="state">The initial data of this instance fetched from storage.</param>
    protected Entity(EntityState state)
    {
      this.entityState = state;

      if (Log.IsLogged(LogEventTypes.Debug))
        Log.Debug("Session '{0}'. Creating entity: Key = '{1}'", Session, Key);
    }
  }
}