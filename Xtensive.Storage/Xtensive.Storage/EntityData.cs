// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.07

using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  /// <summary>
  /// The underlying data of the <see cref="Storage.Entity"/>.
  /// </summary>
  public sealed class EntityData
  {
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
    public DifferentialTuple Tuple { get; internal set; }

    /// <summary>
    /// Gets the the persistence state.
    /// </summary>
    public PersistenceState PersistenceState { get; internal set; }

    /// <summary>
    /// Gets the owner of this instance.
    /// </summary>
    public Entity Entity { get; internal set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("Key = '{0}', Tuple = {1}, State = {2}", Key, Tuple.ToRegular(), PersistenceState);
    }


    // Constructors

    internal EntityData(Key key, DifferentialTuple tuple, PersistenceState state)
    {
      Key = key;
      Tuple = tuple;
      PersistenceState = state;
    }
  }
}