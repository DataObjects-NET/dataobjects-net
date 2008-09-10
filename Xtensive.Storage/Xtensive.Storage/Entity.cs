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

namespace Xtensive.Storage
{
  /// <summary>
  /// Principal data objects about which information has to be managed. 
  /// It has a unique identity, independent existence, and forms the operational unit of consistency.
  /// Instance of <see cref="Entity"/> type can be referenced via <see cref="Key"/>.
  /// </summary>
  public abstract class Entity : Persistent,
    IEntity,
    IValidationAware
  {
    private static readonly ThreadSafeDictionary<Type, Func<EntityData, Entity>> activators = 
      ThreadSafeDictionary<Type, Func<EntityData, Entity>>.Create(new object());
    private readonly EntityData data;

    #region Internal properties

    [Infrastructure]
    internal EntityData Data {
      [DebuggerStepThrough]
      get { return data; }
    }

    /// <exception cref="Exception">Property is already initialized.</exception>
    [Field]
    internal int TypeId {
      [DebuggerStepThrough]
      get { return GetValue<int>(NameBuilder.TypeIdFieldName); }
    }

    #endregion

    #region Properties: Key, Type, Tuple, PersistenceState

    /// <exception cref="Exception">Property is already initialized.</exception>
    [Infrastructure]
    public Key Key {
      [DebuggerStepThrough]
      get { return Data.Key; }
    }

    /// <summary>
    /// Gets a value indicating whether this entity is removed.
    /// </summary>
    [Infrastructure]
    public bool IsRemoved
    {
      get {
        return Data.IsRemoved;
      }
    }

    /// <inheritdoc/>
    public override sealed TypeInfo Type {
      [DebuggerStepThrough]
      get { return Data.Type; }
    }

    /// <inheritdoc/>
    protected internal sealed override Tuple Tuple {
      [DebuggerStepThrough]
      get { return Data; }
    }

    /// <summary>
    /// Gets persistence state of the entity.
    /// </summary>
    [Infrastructure]
    public PersistenceState PersistenceState
    {
      [DebuggerStepThrough]
      get { return Data.PersistenceState; }
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
    public void Remove()
    {      
      if (Log.IsLogged(LogEventTypes.Debug))
        Log.Debug("Session '{0}'. Removing: Key = '{1}'", Session, Key);

      data.EnsureCanOperate();
      OnRemoving();

      Session.Persist();        
      ReferenceManager.ClearReferencesTo(this);
      Data.IsRemoved = true;
      // We expect entity's PersistentState to be Synchronized here.
      Session.removedEntities.Add(Data);
      Data.PersistenceState = PersistenceState.Removed;
      
      OnRemoved();        
    }

    #endregion

    #region Protected event-like methods

    /// <inheritdoc/>
    protected internal override sealed void OnCreating()
    {
      Data.Entity = this;
      if (PersistenceState!=PersistenceState.New) {
        Session.newEntities.Add(Data);
        Data.PersistenceState = PersistenceState.New;
      }
      this.Validate();
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Entity is removed.</exception>
    protected internal override sealed void OnGettingValue(FieldInfo field)
    {   
      if (Log.IsLogged(LogEventTypes.Debug))
        Log.Debug("Session '{0}'. Getting value: Key = '{1}', Field = '{2}'", Session, Key, field);
      data.EnsureCanOperate();
      EnsureIsFetched(field);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Entity is removed.</exception>
    protected internal override sealed void OnSettingValue(FieldInfo field)
    { 
      if (Log.IsLogged(LogEventTypes.Debug))
        Log.Debug("Session '{0}'. Setting value: Key = '{1}', Field = '{2}'", Session, Key, field);
      data.EnsureCanOperate();
    }

    /// <inheritdoc/>
    protected internal override sealed void OnSetValue(FieldInfo field)
    {
      if (PersistenceState!=PersistenceState.New && PersistenceState!=PersistenceState.Modified) {
        Session.modifiedEntities.Add(Data);
        Data.PersistenceState = PersistenceState.Modified;
      }
      if (Session.Domain.Configuration.AutoValidation)
        this.Validate();
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

    internal static Entity Activate(Type type, EntityData data)
    {
      return activators.GetValue(type, 
        DelegateHelper.CreateConstructorDelegate<Func<EntityData, Entity>>)
        .Invoke(data);
    }

    internal override sealed void EnsureIsFetched(FieldInfo field)
    {
      if (!Data.IsFetched(field.MappingInfo.Offset))
        Fetcher.Fetch(Key, field);
    }    

    #endregion

    #region IValidationAware members

    /// <summary>
    /// Called when entity should be validated.
    /// Override this method to perform custom validation.
    /// </summary>
    [Infrastructure]
    public virtual void OnValidate()
    {
    }

    /// <inheritdoc/>
    [Infrastructure]
    void IValidationAware.OnValidate()
    {
      if (IsRemoved)
        return;

      this.CheckConstraints();
      this.OnValidate();
    }

    /// <inheritdoc/>
    public ValidationContextBase Context
    {
      get
      {
        return Session.ValidationContext;
      }
    }

    /// <inheritdoc/>
    [Infrastructure]
    bool IValidationAware.IsCompatibleWith(ValidationContextBase context)
    {
      return context==Session.ValidationContext;
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

      data = Session.DataCache.Create(key, true, Session.Transaction);
      OnCreating();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="tuple">The <see cref="Tuple"/> that will be used for key building.</param>
    /// <remarks>Use this kind of constructor when you need to explicitly build key for this instance.</remarks>
    protected Entity(Tuple tuple)
    {
      TypeInfo type = Session.Domain.Model.Types[GetType()];
      Key key = Session.Domain.KeyManager.Get(type, tuple);

      if (Log.IsLogged(LogEventTypes.Debug))
        Log.Debug("Session '{0}'. Creating entity: Key = '{1}'", Session, key);

      data = Session.DataCache.Create(key, true, Session.Transaction);
      OnCreating();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate()" copy="true"/>
    /// </summary>
    /// <param name="data">The initial data of this instance fetched from storage.</param>
    protected Entity(EntityData data)
    {
      this.data = data;

      if (Log.IsLogged(LogEventTypes.Debug))
        Log.Debug("Session '{0}'. Creating entity: Key = '{1}'", Session, Key);
    }
  }
}