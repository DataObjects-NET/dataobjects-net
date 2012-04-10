// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.25

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Xtensive.Core;
using Xtensive.Collections;


namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Describes <see cref="Entity"/> removal operation.
  /// </summary>
  [Serializable]
  public class EntitiesRemoveOperation : KeySetOperation
  {
    /// <inheritdoc/>
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
    public EntitiesRemoveOperation(IEnumerable<Key> keys)
      : base(keys)
    {
    }

    // Serialization

    /// <inheritdoc/>
    public EntitiesRemoveOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}