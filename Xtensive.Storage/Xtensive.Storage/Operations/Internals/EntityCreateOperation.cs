// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.25

using System;
using System.Runtime.Serialization;

namespace Xtensive.Storage.Operations.Internals
{
  /// <summary>
  /// Describes <see cref="Entity"/> creation operation.
  /// </summary>
  [Serializable]
  public class EntityCreateOperation : EntityOperation
  {
    /// <inheritdoc/>
    public override string Title {
      get { return "Create entity"; }
    }

    /// <inheritdoc/>
    public override void Prepare(OperationExecutionContext context)
    {
      // There should be no base method call here!
      context.RegisterKey(context.TryRemapKey(Key), true);
    }

    /// <inheritdoc/>
    public override void Execute(OperationExecutionContext context)
    {
      var session = context.Session;
      var key = context.TryRemapKey(Key);
      var entityType = key.TypeRef.Type;
      var domain = session.Domain;
      session.CreateEntityState(key);
    }

    
    // Constructors

    /// <inheritdoc/>
    public EntityCreateOperation(Key key)
      : base(key)
    {
    }

    /// <inheritdoc/>
    protected EntityCreateOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}