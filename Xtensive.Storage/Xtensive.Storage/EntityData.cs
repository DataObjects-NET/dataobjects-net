// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.07

using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  /// <summary>
  /// Inner representation of <see cref="Storage.Entity"/>.
  /// </summary>
  public sealed class EntityData : IIdentified<Key>
  {
    private PersistenceState persistenceState;

    /// <summary>
    /// Gets the key.
    /// </summary>
    public Key Key { get; internal set; }

    /// <summary>
    /// Gets the type.
    /// </summary>
    public TypeInfo Type
    {
      get { return Key.Type; }
    }

    /// <summary>
    /// Gets the tuple.
    /// </summary>
    public DifferentialTuple Tuple { get; internal set; }

    /// <summary>
    /// Gets the the persistence state .
    /// </summary>
    public PersistenceState PersistenceState
    {
      get { return persistenceState; }
      internal set
      {
        if (persistenceState == value)
          return;
        persistenceState = value;
        if (Entity != null)
          Entity.OnPersistentStateChanged();
      }
    }

    /// <summary>
    /// Gets the owner of this instance.
    /// </summary>
    public Entity Entity { get; internal set; }

    /// <inheritdoc/>
    object IIdentified.Identifier
    {
      get { return Identifier; }
    }

    /// <inheritdoc/>
    public Key Identifier
    {
      get { return Key; }
    }


    // Constructors

    internal EntityData(Key key, DifferentialTuple tuple)
    {
      Key = key;
      Tuple = tuple;
      PersistenceState = PersistenceState.Persisted;
    }

    internal EntityData(Key key, DifferentialTuple tuple, Entity entity)
    {
      Key = key;
      Tuple = tuple;
      Entity = entity;
      PersistenceState = PersistenceState.New;
    }
  }
}