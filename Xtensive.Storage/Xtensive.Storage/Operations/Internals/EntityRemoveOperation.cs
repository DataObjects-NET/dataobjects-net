// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.25

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Xtensive.Storage.Operations.Internals
{
  /// <summary>
  /// Describes <see cref="Entity"/> removal operation.
  /// </summary>
  [Serializable]
  public class EntityRemoveOperation : EntityOperation
  {
    /// <inheritdoc/>
    public override string Title {
      get { return "Remove entity"; }
    }

    /// <inheritdoc/>
    public override void Execute(OperationExecutionContext context)
    {
      var session = context.Session;
      var key = context.TryRemapKey(Key);
      var entity = Query.Single(session, key);
      entity.Remove();
    }


    // Constructors

    /// <inheritdoc/>
    public EntityRemoveOperation(Key key)
      : base(key)
    {
    }

    /// <inheritdoc/>
    protected EntityRemoveOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}