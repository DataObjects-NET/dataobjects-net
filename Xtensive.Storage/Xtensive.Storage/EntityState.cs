// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.07

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Integrity.Transactions;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Activator=Xtensive.Storage.Internals.Activator;

namespace Xtensive.Storage
{
  /// <summary>
  /// The underlying data of the <see cref="Storage.Entity"/>.
  /// </summary>
  public sealed class EntityState : Tuple,
    ITransactionalState
  {
    private Entity entity;

    /// <summary>
    /// Gets the key.
    /// </summary>
    public Key Key { get; internal set; }

    /// <summary>
    /// Gets the entity type.
    /// </summary>
    public TypeInfo Type
    {
      [DebuggerStepThrough]
      get { return Key.Type; }
    }

    /// <summary>
    /// Gets the values as <see cref="DifferentialTuple"/>.
    /// </summary>
    public DifferentialTuple Data { get; private set; }

    /// <summary>
    /// Gets the transaction the state belongs to.
    /// </summary>
    public Transaction Transaction { get; private set; }
    
    /// <summary>
    /// Gets the the persistence state.
    /// </summary>
    public PersistenceState PersistenceState { get; internal set; }

    /// <summary>
    /// Gets the owner of this instance.
    /// </summary>
    public Entity Entity
    {
      get
      {
        EnsureHasEntity();
        return entity;
      }
      internal set
      {
        entity = value;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this entity is removed.
    /// </summary>
    public bool IsRemoved
    {
      get { return Data==null; }
    }

    /// <summary>
    /// Determines whether the field with the specified offset is fetched.
    /// </summary>
    /// <param name="offset">The offset of the field.</param>
    public bool IsFetched(int offset)
    {
      return PersistenceState==PersistenceState.New || IsAvailable(offset);
    }

    /// <summary>
    /// Updates the entity state to the most current one.
    /// </summary>
    /// <param name="tuple">The state change tuple, or a new state tuple. 
    /// If <see langword="null" />, the entity is considered as removed.</param>
    /// <param name="transaction">The transaction.</param>
    public void Update(Tuple tuple, Transaction transaction)
    {
      ArgumentValidator.EnsureArgumentNotNull(transaction, "transaction");
      EnsureConsistency(transaction);
      if (Data == null)
        Data = new DifferentialTuple(tuple.ToRegular());
      else
        Data.Origin.MergeWith(tuple);
    }

    /// <summary>
    /// Marks this instance as removed.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    public void Remove(Transaction transaction)
    {
      ArgumentValidator.EnsureArgumentNotNull(transaction, "transaction");
      EnsureConsistency(transaction);
      Data = null;
    }

    #region EnsureXxx methods

    /// <inheritdoc/>
    public void EnsureConsistency(Transaction transaction)
    {
      if (!Transaction.State.IsActive()) {
        Reset(transaction);
        Fetcher.Fetch(Key);
      }
    }

    /// <inheritdoc/>
    public void Reset(Transaction transaction)
    {
      Transaction = transaction;
      Data = null;
    }

    /// <summary>
    /// Ensures the <see cref="Entity"/> property has been set,
    /// i.e. <see cref="Xtensive.Storage.Entity"/> is associated
    /// with this state.
    /// </summary>
    public void EnsureHasEntity()
    {
      if (entity!=null)
        return;
      var result = Activator.CreateEntity(Type.UnderlyingType, this);
      result.Initialize();
      Entity = result;
    }

    /// <summary>
    /// Ensures the entity is not removed and its data is actual.
    /// Call this method before getting or setting values.
    /// </summary>
    public void EnsureNotRemoved(Transaction transaction)
    {
      EnsureConsistency(transaction);
      if (IsRemoved)
        throw new InvalidOperationException(Strings.ExEntityIsRemoved);
    }

    #endregion

    #region Tuple implementation

    /// <inheritdoc/>
    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      return Data.GetFieldState(fieldIndex);
    }

    /// <inheritdoc/>
    public override object GetValueOrDefault(int fieldIndex)
    {
      return Data.GetValueOrDefault(fieldIndex);
    }

    /// <inheritdoc/>
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      Data.SetValue(fieldIndex, fieldValue);
    }

    /// <inheritdoc/>
    public override TupleDescriptor Descriptor
    {
      get { return Data.Descriptor; }
    }

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

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("Key = '{0}', Values = {1}, State = {2}", Key, Data.ToRegular(), PersistenceState);
    }


    // Constructors

    internal EntityState(Key key, DifferentialTuple tuple, Transaction transaction, Entity entity)
      : this(key, tuple, transaction)
    {
      this.entity = entity;
    }

    internal EntityState(Key key, DifferentialTuple tuple, Transaction transaction)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      ArgumentValidator.EnsureArgumentNotNull(transaction, "transaction");
      Key = key;
      Data = tuple;
      Transaction = transaction;
    }
  }
}