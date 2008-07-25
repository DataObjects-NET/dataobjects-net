// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.01

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
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

    #region Internal properties

    [DebuggerHidden]
    internal EntityData Data
    {
      get { return data; }
    }

    /// <exception cref="Exception">Property is already initialized.</exception>
    [Field]
    [DebuggerHidden]
    internal int TypeId
    {
      get { return GetValue<int>(Session.Domain.NameProvider.TypeId); }
      private set
      {
        FieldInfo field = Type.Fields[Session.Domain.NameProvider.TypeId];
        field.GetAccessor<int>().SetValue(this, field, value);
      }
    }

    #endregion

    #region Properties: Key, Type, Tuple, PersistenceState

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
        Session.DirtyData.Register(Data);
      }
    }

    #endregion

    #region IIdentifier members

    /// <inheritdoc/>
    Key IIdentified<Key>.Identifier
    {
      get { return Key; }
    }

    /// <inheritdoc/>
    object IIdentified.Identifier
    {
      get { return Key; }
    }

    #endregion

    #region Remove method =)

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

    #endregion

    #region Protected event-like methods

    /// <inheritdoc/>
    protected internal override sealed void OnCreating()
    {
      Session.IdentityMap.Add(Data);
      Session.DirtyData.Register(Data);
      TypeId = Type.TypeId;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Entity is removed.</exception>
    protected internal override sealed void OnGettingValue(FieldInfo field)
    {
      EnsureIsNotRemoved();
      EnsureIsFetched(field);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Entity is removed.</exception>
    protected internal override sealed void OnSettingValue(FieldInfo field)
    {
      EnsureIsNotRemoved();
    }

    /// <inheritdoc/>
    protected internal override sealed void OnSetValue(FieldInfo field)
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

    #region Private \ internal methods

    internal static Entity Activate(Type type, EntityData data)
    {
      if (!activators.ContainsKey(type)) {
        var constructorInvocationDelegate =
          (Func<EntityData, Entity>) DelegateHelper.CreateConstructorDelegate(
                                       type,
                                       typeof (Func<EntityData, Entity>));
        activators.Add(type, constructorInvocationDelegate);
        return constructorInvocationDelegate(data);
      }
      return activators[type](data);
    }

    private void EnsureIsFetched(FieldInfo field)
    {
      if (Session.DirtyData.GetItems(PersistenceState.New).Contains(Data))
        return;
      if (Data.Tuple.IsAvailable(field.MappingInfo.Offset))
        return;
      Tuple result = Fetcher.Fetch(Key, field);
      Data.Tuple.Origin.MergeWith(result, field.MappingInfo.Offset, field.MappingInfo.Length, MergeConflictBehavior.PreferTarget);
    }

    private void EnsureIsNotRemoved()
    {
      if (PersistenceState==PersistenceState.Removed)
        throw new InvalidOperationException(Strings.ExEntityIsRemoved);
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
      Tuple origin = Tuple.Create(type.TupleDescriptor);
      key.Tuple.CopyTo(origin, 0);

      data = new EntityData(key, new DifferentialTuple(origin), this);
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
      Tuple origin = Tuple.Create(type.TupleDescriptor);
      key.Tuple.CopyTo(origin, 0);

      data = new EntityData(key, new DifferentialTuple(origin), this);
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
      OnCreating();
    }
  }
}