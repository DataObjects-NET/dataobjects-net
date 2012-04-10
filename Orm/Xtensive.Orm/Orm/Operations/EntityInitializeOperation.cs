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

    /// <summary>
    /// Gets the title of the operation.
    /// </summary>
    public override string Title {
      get { return "Initialize entity"; }
    }


    /// <summary>
    /// Executes the operation itself.
    /// </summary>
    /// <param name="context">The operation execution context.</param>
    protected override void ExecuteSelf(OperationExecutionContext context)
    {
      // Does nothing.
    }


    /// <summary>
    /// Clones the operation itself.
    /// </summary>
    /// <param name="clone"></param>
    /// <returns></returns>
    protected override Operation CloneSelf(Operation clone)
    {
      if (clone==null)
        clone = new EntityInitializeOperation(Key);
      return clone;
    }

    
    // Constructors


    /// <summary>
    /// Initializes a new instance of the <see cref="EntityInitializeOperation"/> class.
    /// </summary>
    /// <param name="key">The key of the entity.</param>
    public EntityInitializeOperation(Key key)
      : base(key)
    {
    }

    // Serialization


    /// <summary>
    /// Initializes a new instance of the <see cref="EntityInitializeOperation"/> class.
    /// </summary>
    /// <param name="info">The info.</param>
    /// <param name="context">The context.</param>
    protected EntityInitializeOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}