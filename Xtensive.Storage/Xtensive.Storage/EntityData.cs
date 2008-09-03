// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.07

using System;
using System.Diagnostics;
using Xtensive.Core.Tuples;
using Xtensive.Integrity.Transactions;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// The underlying data of the <see cref="Storage.Entity"/>.
  /// </summary>
  public sealed class EntityData : Tuple
  {
    private Transaction transaction;

    /// <summary>
    /// Gets the key.
    /// </summary>
    public Key Key { get; internal set; }

    /// <summary>
    /// Gets the type.
    /// </summary>
    public TypeInfo Type
    {
      [DebuggerStepThrough]
      get { return Key.Type; }
    }

    /// <summary>
    /// Gets the tuple.
    /// </summary>
    public DifferentialTuple DifferentialData { get; set; }

    /// <summary>
    /// Gets the the persistence state.
    /// </summary>
    public PersistenceState PersistenceState { get; internal set; }

    /// <summary>
    /// Gets the owner of this instance.
    /// </summary>
    public Entity Entity { get; internal set; }

    /// <summary>
    /// Ensures the data belongs to the current <see cref="Transaction"/> and resents the data if not.
    /// </summary>
    public void EnsureDataIsActual()
    {
      if ((transaction.State & TransactionState.Completed)!=0) {
        Reset();
        transaction = transaction.Session.Transaction;
        if (transaction==null)
          throw new InvalidOperationException(Strings.ExTransactionRequired);
      }
    }

    /// <summary>
    /// Resets this entity data.
    /// </summary>
    public void Reset()
    {
      Tuple origin = Create(Type.TupleDescriptor);
      Key.Tuple.CopyTo(origin);
      DifferentialData = new DifferentialTuple(origin);
    }

    #region Tuple implementation

    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      return DifferentialData.GetFieldState(fieldIndex);
    }

    /// <inheritdoc/>
    public override T GetValueOrDefault<T>(int fieldIndex)
    {
      return DifferentialData.GetValueOrDefault<T>(fieldIndex);
    }

    /// <inheritdoc/>
    public override object GetValueOrDefault(int fieldIndex)
    {
      return DifferentialData.GetValueOrDefault(fieldIndex);
    }

    /// <inheritdoc/>
    public override void SetValue<T>(int fieldIndex, T fieldValue)
    {
      DifferentialData.SetValue(fieldIndex, fieldValue);
    }

    /// <inheritdoc/>
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      DifferentialData.SetValue(fieldIndex, fieldValue);
    }

    /// <inheritdoc/>
    public override TupleDescriptor Descriptor
    {
       get { return DifferentialData.Descriptor; }
    }

    public override bool Equals(object obj)
    {
      return ReferenceEquals(this, obj);
    }

    public override int GetHashCode()
    {
      return Key.GetHashCode();
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("Key = '{0}', Tuple = {1}, State = {2}", Key, DifferentialData.ToRegular(), PersistenceState);
    }


    // Constructors

    internal EntityData(Key key, DifferentialTuple tuple, PersistenceState state, Transaction transaction)
    {
      Key = key;
      DifferentialData = tuple;
      PersistenceState = state;
      this.transaction = transaction;
    }
  }
}