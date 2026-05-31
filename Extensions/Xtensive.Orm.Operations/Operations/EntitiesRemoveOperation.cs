// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.25

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Xtensive.Core;


namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Describes <see cref="Entity"/> removal operation.
  /// </summary>
  [Serializable]
  public class EntitiesRemoveOperation : KeySetOperation
  {
    /// <inheritdoc/>
    [JsonIgnore]
    public override string Title {
      get { return "Remove entities"; }
    }

    /// <inheritdoc/>
    protected override void ExecuteSelf(OperationExecutionContext context)
    {
      var session = context.Session;
      var entities =
        from key in Keys
        let remappedKey = context.TryRemapKey(key)
        let entity = session.Query.Single(remappedKey)
        select entity;
      session.Remove(entities);
    }

    /// <inheritdoc/>
    protected override Operation CloneSelf(Operation clone)
    {
      if (clone==null)
        clone = new EntitiesRemoveOperation(Keys);
      return clone;
    }


    // Constructors

    /// <inheritdoc/>
    public EntitiesRemoveOperation(Key key)
      : base(key)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="keys">The keys of entities to remove.</param>
    [JsonConstructor]
    public EntitiesRemoveOperation(IReadOnlyList<Key> keys)
      : base(keys)
    {
    }

    //[JsonConstructor]
    //private EntitiesRemoveOperation()
    //  : base(Array.Empty<Key>())
    //{

    //}

    
    //private EntitiesRemoveOperation(
    //  IReadOnlyList<Key> keys,
    //  OperationType type,
    //  IReadOnlyList<IOperation> precedingOperations,
    //  IReadOnlyList<IOperation> followingOperations,
    //  IReadOnlyList<IOperation> undoOperations,
    //  IReadOnlyDictionary<string, Key> identifiedEntities)
    //  : base(keys)
    //{
    //}
  }
}