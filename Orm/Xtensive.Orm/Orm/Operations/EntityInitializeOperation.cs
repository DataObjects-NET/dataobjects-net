// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.11

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Describes <see cref="Entity"/> initialization operation.
  /// Actually, does nothing - it is used to suppress nested
  /// system operations.
  /// </summary>
  [Serializable]
  public class EntityInitializeOperation : EntityOperation
  {
    /// <inheritdoc/>
    public override string Title {
      get { return "Initialize entity"; }
    }

    /// <inheritdoc/>
    protected override void ExecuteSelf(OperationExecutionContext context)
    {
      // Does nothing.
    }

    /// <inheritdoc/>
    protected override Operation CloneSelf(Operation clone)
    {
      if (clone==null)
        clone = new EntityInitializeOperation(Key);
      return clone;
    }

    
    // Constructors

    /// <inheritdoc/>
    public EntityInitializeOperation(Key key)
      : base(key)
    {
    }

    // Serialization

    /// <inheritdoc/>
    protected EntityInitializeOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}