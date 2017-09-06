// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.07

using System;
using System.Diagnostics;
using Xtensive.Caching;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Model;

using Activator=Xtensive.Orm.Internals.Activator;

namespace Xtensive.Orm
{
  /// <summary>
  /// The underlying state of the <see cref="Orm.Entity"/>.
  /// </summary>
  [DebuggerDisplay("Key = {key}, Tuple = {state}, PersistenceState = {persistenceState}")]
  public sealed class EntityState : TransactionalStateContainer<Tuple>, 
    IEquatable<EntityState>,
    IInvalidatable
  {
    private Key key;
    private PersistenceState persistenceState;
    private Entity entity;
    private bool isStale;
    private bool isVersionInfoUpdated;

    /// <summary>
    /// Gets the key.
    /// </summary>
    public Key Key {
      [DebuggerStepThrough]
      get { return key; }
      [DebuggerStepThrough]
      internal set { key = value; }
    }

    /// <summary>
    /// Gets the entity type.
    /// </summary>
    public TypeInfo Type
    {
      [DebuggerStepThrough]
      get { return key.TypeInfo; }
    }

    /// <summary>
    /// Gets the values as <see cref="Tuple"/>.
    /// </summary>
    public Tuple Tuple {
      [DebuggerStepThrough]
      get { return State; }
      [DebuggerStepThrough]
      internal set { State = value; }
    }

    /// <summary>
    /// Gets the values as <see cref="DifferentialTuple"/>.
    /// </summary>
    /// <returns>A <see cref="DifferentialTuple"/> corresponding to the current state.</returns>
    public DifferentialTuple DifferentialTuple {
      get {
        var tuple = Tuple;
        if (tuple==null)
          return null;
        var dTuple = tuple as DifferentialTuple;
        if (dTuple!=null)
          return dTuple;
        dTuple = new DifferentialTuple(tuple);
        Tuple = dTuple;
        return dTuple;
      }
    }

    /// <summary>
    /// Gets a value indicating whether <see cref="Tuple"/> value is already loaded.
    /// </summary>
    public bool IsTupleLoaded {
      get { return IsActual; }
    }

    /// <summary>
    /// Gets the owner of this instance.
    /// </summary>
    /// <exception cref="NotSupportedException">Property value is already set.</exception>
    public Entity Entity {
      get {
        var notAvailable = IsNotAvailable;
        if (entity==null && !notAvailable)
          Activator.CreateEntity(Session, Type.UnderlyingType, this); 
        return notAvailable ? null : entity;
      }
      internal set {
        entity = value;
      }
    }

    /// <summary>
    /// Tries to get entity.
    /// </summary>
    public Entity TryGetEntity()
    {
      return entity;
    }

    /// <summary>
    /// Gets or sets the persistence state.
    /// </summary>
    public PersistenceState PersistenceState {
      get { return persistenceState; }
      set {
        if (value==persistenceState)
          return;
        persistenceState = value;
        if (value==PersistenceState.Synchronized)
          return;
        if (!Session.Configuration.Supports(SessionOptions.NonTransactionalEntityStates))
          Session.DemandTransaction();
        Session.EntityChangeRegistry.Register(this);
        Rebind();
      }
    }

    internal void SetPersistenceState(PersistenceState newState)
    {
      persistenceState = newState;
    }

    /// <summary>
    /// Gets a value indicating whether this entity is available (has a <see cref="Tuple"/>).
    /// Tuple does not exist, if there is no row corresponding to the <see cref="Entity"/>
    /// in the storage.
    /// </summary>
    public bool IsNotAvailable {
      get {
        return Tuple==null;
      }
    }

    /// <summary>
    /// Gets a value indicating whether the state is either <see cref="IsNotAvailable"/>
    /// or is marked as removed (see <see cref="PersistenceState"/>).
    /// </summary>
    public bool IsNotAvailableOrMarkedAsRemoved {
      get { return Tuple==null || persistenceState==PersistenceState.Removed; }
    }

    /// <summary>
    /// Gets a value indicating whether this state is stale (taken from cache).
    /// </summary>
    public bool IsStale {
      get {
        return isStale;
      }
      internal set {
        isStale = value;
      }
    }

    /// <summary>
    /// Gets a value indicating whether <see cref="Orm.Entity.VersionInfo"/> already updated.
    /// </summary>
    public bool IsVersionInfoUpdated {
      get {
        EnsureIsActual();
        return isVersionInfoUpdated;
      }
      internal set {
        isVersionInfoUpdated = value;
      }
    }

    /// <summary>
    /// Reverts the state to the origin by discarding the difference.
    /// </summary>
    public void RollbackDifference()
    {
      var dTuple = DifferentialTuple;
      if (dTuple!=null) {
        dTuple.BackupDifference();
        dTuple.Difference = null;
      }
    }

    /// <summary>
    /// Commits the state difference to the origin.
    /// </summary>
    public void CommitDifference()
    {
      var tuple = Tuple;
      var dTuple = tuple as DifferentialTuple;
      if (dTuple!=null)
        dTuple.Merge();
    }

    /// <summary>
    /// Restore difference of <see cref="DifferentialTuple"/>
    /// </summary>
    public void RestoreDifference()
    {
      var dTuple = DifferentialTuple;
      if (dTuple!=null) {
        dTuple.RestoreDifference();
      }
    }


    /// <summary>
    /// Updates the entity state to the most current one.
    /// </summary>
    /// <param name="update">The state change tuple, or a new state tuple. 
    /// If <see langword="null" />, the entity is considered as removed.</param>
    public void Update(Tuple update)
    {
      EnsureIsActual();
      if (update==null) // Entity is removed
        Tuple = null;
      else {
        var tuple = IsTupleLoaded ? Tuple : null;
        if (tuple==null)
          // Entity was marked as removed before, or it is unfetched at all yet
          Tuple = update.ToRegular();
        else {
          // We must never modify the origin tuple
          var dTuple = tuple as DifferentialTuple;
          if (dTuple!=null) {
            tuple = dTuple.Origin;
            if (!(tuple is RegularTuple))
              tuple = tuple.ToRegular();
            tuple.MergeWith(update, MergeBehavior.PreferDifference);
            dTuple.Origin = tuple;
          }
          else {
            if (!(tuple is RegularTuple))
              tuple = tuple.ToRegular();
            tuple.MergeWith(update, MergeBehavior.PreferDifference);
            Tuple = tuple;
          }
        }
        Session.SystemEvents.NotifyEntityMaterialized(this);
        Session.Events.NotifyEntityMaterialized(this);
      }
    }

    /// <inheritdoc/>
    protected override void Refresh()
    {
      persistenceState = PersistenceState.Synchronized;
      Tuple = null;
      Session.Handler.FetchEntityState(key);
    }

    /// <inheritdoc/>
    void IInvalidatable.Invalidate()
    {
      Invalidate();
    }

    /// <inheritdoc/>
    protected override void Invalidate()
    {
      persistenceState = PersistenceState.Synchronized;
      isVersionInfoUpdated = false;
      base.Invalidate();
    }

    internal void RemapKey(Key newKey)
    {
      key = newKey;
      if (IsActual) {
        var tuple = Tuple;
        if (tuple==null)
          return;
        var dTuple = tuple as DifferentialTuple;
        if (dTuple!=null)
          tuple = dTuple.Origin;
        tuple = Type.InjectPrimaryKey(tuple, key.Value);
        if (dTuple!=null)
          Tuple = new DifferentialTuple(tuple, dTuple.Difference);
        else
          Tuple = tuple;
      }
    }

    internal void DropBackedUpDifference()
    {
      var dTuple = DifferentialTuple;
      if (dTuple!=null)
        dTuple.RestoreDifference();
    }

    #region Equality members

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      return ReferenceEquals(this, obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return Key.GetHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(EntityState other)
    {
      return ReferenceEquals(this, other);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      var tuple = IsTupleLoaded ? (Tuple==null ? Strings.Null : Tuple.ToString()) : Strings.NA;
      return string.Format(Strings.EntityStateFormat, Key, tuple, PersistenceState);
    }


    // Constructors

    internal EntityState(Session session, Key key, Tuple data)
      : base(session)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      this.key = key;
      Tuple = data;
    }

    internal EntityState(Session session, Key key, Tuple data, bool isStale)
      : this(session, key, data)
    {
      IsStale = isStale;
    }
  }
}