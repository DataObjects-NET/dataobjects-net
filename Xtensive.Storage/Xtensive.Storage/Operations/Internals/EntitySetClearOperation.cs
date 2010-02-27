// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.25

using System;
using System.Runtime.Serialization;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Operations.Internals
{
  /// <summary>
  /// Describes <see cref="Entity"/> creation operation.
  /// </summary>
  [Serializable]
  public class EntitySetClearOperation : EntitySetOperation
  {
    /// <inheritdoc/>
    public override string Title {
      get { return "Clear entity set"; }
    }

    /// <inheritdoc/>
    public override void Execute(OperationExecutionContext context)
    {
      GetEntitySet(context).Clear();
    }

    
    // Constructors

    /// <inheritdoc/>
    public EntitySetClearOperation(Key key, FieldInfo field)
      : base(key, field)
    {
    }

    /// <inheritdoc/>
    protected EntitySetClearOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}