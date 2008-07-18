// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.01

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Collections;
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
  public abstract class Entity
    : Persistent,
      IEntity
  {
    private static readonly Dictionary<Type, Func<EntityData, Entity>> activators = new Dictionary<Type, Func<EntityData, Entity>>();
    private readonly EntityData data;

    [DebuggerHidden]
    internal EntityData Data
    {
      get { return data; }
    }

    /// <exception cref="Exception">Property is already initialized.</exception>
    [DebuggerHidden]
    public Key Key
    {
      get { return Data.Key; }
    }

    /// <inheritdoc/>
    [DebuggerHidden]
    public override sealed TypeInfo Type
    {
      get { return Data.Type; }
    }

    /// <inheritdoc/>
    [DebuggerHidden]
    protected internal sealed override Tuple Tuple
    {
      get { return Data.Tuple; }
    }

    /// <exception cref="Exception">Property is already initialized.</exception>
    [Field]
    [DebuggerHidden]
    internal int TypeId
    {
      get { return GetValue<int>(Session.Domain.NameProvider.TypeId); }
      set
      {
        if (TypeId > 0)
          throw Exceptions.AlreadyInitialized(Session.Domain.NameProvider.TypeId);
        FieldInfo field = Type.Fields[Session.Domain.NameProvider.TypeId];
        field.GetAccessor<int>().SetValue(this, field, value);
      }
    }

    /// <summary>
    /// Removes the instance.
    /// </summary>
    public void Remove()
    {
      EnsureIsNotRemoved();
      Session.Persist();

      OnRemoving();
      ReferenceManager.ClearReferencesTo(this);
      PersistenceState = PersistenceState.Removed;
      OnRemoved();
    }

    #region Inner events

    /// <inheritdoc/>
    protected internal override sealed void OnCreating()
    {
      Session.IdentityMap.Add(Data);
      Session.DirtyItems.Register(Data);
      if (TypeId == 0)
        TypeId = Type.TypeId;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Entity is removed.</exception>
    protected internal override sealed void OnGetting(FieldInfo fieldInfo)
    {
      EnsureIsNotRemoved();
      EnsureIsFetched(fieldInfo);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Entity is removed.</exception>
    protected internal override sealed void OnSetting(FieldInfo fieldInfo)
    {
      EnsureIsNotRemoved();
    }

    /// <inheritdoc/>
    protected internal override sealed void OnSet(FieldInfo fieldInfo)
    {
      PersistenceState = PersistenceState.Modified;
    }

    protected virtual void OnRemoving()
    {
    }

    protected virtual void OnRemoved()
    {
    }

    #endregion

    #region IEntity Members

    /// <summary>
    /// Gets persistence state of the entity.
    /// </summary>
    public PersistenceState PersistenceState
    {
      get { return Data.PersistenceState; }
      internal set
      {
        if (Data.PersistenceState == value)
          return;
        Data.PersistenceState = value;
        Session.DirtyItems.Register(Data);
        OnPersistentStateChanged();
      }
    }

    /// <summary>
    /// Raised when <see cref="PersistenceState"/> of persistent object is changed.
    /// </summary>
    public event EventHandler PersistenceStateChanged;

    /// <summary>
    /// Gets object identifier.
    /// </summary>
    public Key Identifier
    {
      get { return Key; }
    }

    /// <summary>
    /// Gets object identifier.
    /// </summary>
    object IIdentified.Identifier
    {
      get { return Identifier; }
    }

    #endregion

    #region Private

    // TODO: Refactor
    private void EnsureIsFetched(FieldInfo field)
    {
      if (Session.DirtyItems.GetItems(PersistenceState.New).Contains(Data))
        return;
      if (Data.Tuple.IsAvailable(field.MappingInfo.Offset))
        return;
      Fetcher.Fetch(Key, field);
    }

    private void EnsureIsNotRemoved()
    {
      if (PersistenceState==PersistenceState.Removed)
        throw new InvalidOperationException(Strings.ExEntityIsRemoved);
    }

    internal void OnPersistentStateChanged()
    {
      if (PersistenceStateChanged!=null)
        PersistenceStateChanged(this, EventArgs.Empty);
    }

    #endregion

    protected static void RegisterActivator(Type type, Func<EntityData, Entity> activator)
    {
      if (!activators.ContainsKey(type))
        activators.Add(type, activator);
    }

    internal static Entity Activate(Type type, EntityData data)
    {
      if (!activators.ContainsKey(type))
        throw new ArgumentException(String.Format("Type '{0}' was not registered for activation", type));
      return activators[type](data);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected Entity()
      : this(ArrayUtils<object>.EmptyArray)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="keyData">The values that will be used for key building.</param>
    /// <remarks>Use this type of constructor when you need to explicitly build key for this instance.</remarks>
    protected Entity(params object[] keyData)
    {
      TypeInfo type = Session.Domain.Model.Types[GetType()];
      Key key = Session.Domain.KeyManager.BuildPrimaryKey(type, keyData);
      DifferentialTuple tuple = new DifferentialTuple(Tuple.Create(type.TupleDescriptor));
      key.Tuple.Copy(tuple, 0);

      data = new EntityData(key, tuple, this);
      OnCreating();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate()" copy="true"/>
    /// </summary>
    /// <param name="data">The initial data of this instance fetched from storage.</param>
    protected Entity(EntityData data)
      : base(data)
    {
      this.data = data;
      data.Entity = this;
    }
  }
}