// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.07

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Activator=Xtensive.Storage.Internals.Activator;

namespace Xtensive.Storage
{
  /// <summary>
  /// The underlying state of the <see cref="Storage.Entity"/>.
  /// </summary>
  [DebuggerDisplay("Key = {key}, Tuple = {state}, StateIsLoaded = {StateIsLoaded}, PersistenceState = {persistenceState}")]
  public sealed class EntityState : TransactionalStateContainer<Tuple>, 
    IEquatable<EntityState>
  {
    private readonly Key key;
    private PersistenceState persistenceState;
    private Entity entity;

    /// <summary>
    /// Gets the key.
    /// </summary>
    [Infrastructure]
    public Key Key {
      get { return key; }
    }

    /// <summary>
    /// Gets the entity type.
    /// </summary>
    [Infrastructure]
    public TypeInfo Type
    {
      [DebuggerStepThrough]
      get { return key.Type; }
    }

    /// <summary>
    /// Gets the values as <see cref="Tuple"/>.
    /// </summary>
    [Infrastructure]
    public Tuple Tuple {
      [DebuggerStepThrough]
      get { return State; }
      [DebuggerStepThrough]
      private set { State = value; }
    }

    /// <summary>
    /// Gets the values as <see cref="DifferentialTuple"/>.
    /// </summary>
    /// <returns>A <see cref="DifferentialTuple"/> corresponding to the current state.</returns>
    [Infrastructure]
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
    [Infrastructure]
    public bool IsTupleLoaded {
      get { return StateIsLoaded; }
    }

    /// <summary>
    /// Gets the owner of this instance.
    /// </summary>
    /// <exception cref="NotSupportedException">Property value is already set.</exception>
    [Infrastructure]
    public Entity Entity {
      get {
        var notAvailable = IsNotAvailable;
        if (entity==null && !notAvailable)
          Activator.CreateEntity(Type.UnderlyingType, this); 
        return notAvailable ? null : entity;
      }
      internal set {
        if (entity!=null)
          throw Exceptions.AlreadyInitialized("Entity");
        entity = value;
      }
    }

    /// <summary>
    /// Gets or sets the persistence state.
    /// </summary>
    [Infrastructure]
    public PersistenceState PersistenceState {
      get { return persistenceState; }
      set {
        if (value==persistenceState)
          return;
        persistenceState = value;
        switch (persistenceState) {
        case PersistenceState.Synchronized:
          return;
        case PersistenceState.Removed:
          break;
        default:
          break;
        }
        Session.EntityChangeRegistry.Register(this);
        MarkStateAsModified();
      }
    }

    /// <summary>
    /// Gets a value indicating whether this entity is available (has a <see cref="Tuple"/>).
    /// Tuple does not exist, if there is no row corresponding to the <see cref="Entity"/>
    /// in the storage.
    /// </summary>
    [Infrastructure]
    public bool IsNotAvailable {
      get {
        return Tuple==null;
      }
    }

    /// <summary>
    /// Gets a value indicating whether the state is either <see cref="IsNotAvailable"/>
    /// or is marked as removed (see <see cref="PersistenceState"/>).
    /// </summary>
    [Infrastructure]
    public bool IsNotAvailableOrMarkedAsRemoved {
      get { return Tuple==null || persistenceState==PersistenceState.Removed; }
    }

    /// <summary>
    /// Reverts the state to the origin by discarding the difference.
    /// </summary>
    [Infrastructure]
    public void RollbackDifference()
    {
      var dTuple = DifferentialTuple;
      if (dTuple!=null)
        dTuple.Difference = null;
    }

    /// <summary>
    /// Commits the state difference to the origin.
    /// </summary>
    [Infrastructure]
    public void CommitDifference()
    {
      var tuple = Tuple;
      var dTuple = tuple as DifferentialTuple;
      if (dTuple!=null)
        dTuple.Merge();
    }

    /// <summary>
    /// Updates the entity state to the most current one.
    /// </summary>
    /// <param name="update">The state change tuple, or a new state tuple. 
    /// If <see langword="null" />, the entity is considered as removed.</param>
    [Infrastructure]
    public void Update(Tuple update)
    {
      EnsureStateIsActual();
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
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether version already updated.
    /// </summary>
    [Infrastructure]
    internal bool IsVersionUpdated { get; set; }

    /// <summary>
    /// Gets a value indicating whether this state is stale.
    /// </summary>
    [Infrastructure]
    public bool IsStale { get; set; }

    /// <inheritdoc/>
    protected override Tuple LoadState()
    {
      persistenceState = PersistenceState.Synchronized;
      Tuple = null;
      Session.Handler.FetchInstance(key);
      return Tuple;
    }

    /// <inheritdoc/>
    protected override void ResetState()
    {
      persistenceState = PersistenceState.Synchronized;
      base.ResetState();
    }

    #region Equality members

    /// <inheritdoc/>
    [Infrastructure]
    public override bool Equals(object obj)
    {
      return ReferenceEquals(this, obj);
    }

    /// <inheritdoc/>
    [Infrastructure]
    public override int GetHashCode()
    {
      return Key.GetHashCode();
    }

    /// <inheritdoc/>
    [Infrastructure]
    public bool Equals(EntityState other)
    {
      return ReferenceEquals(this, other);
    }

    #endregion

    /// <inheritdoc/>
    [Infrastructure]
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