// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.25

using System;
using System.Runtime.Serialization;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Describes <see cref="Entity"/> creation operation.
  /// </summary>
  [Serializable]
  public class EntitySetClearOperation : EntitySetOperation
  {

    /// <summary>
    /// Gets the title of the operation.
    /// </summary>
    public override string Title {
      get { return "Clear entity set"; }
    }


    /// <summary>
    /// Executes the operation itself.
    /// </summary>
    /// <param name="context">The operation execution context.</param>
    protected override void ExecuteSelf(OperationExecutionContext context)
    {
      GetEntitySet(context).Clear();
    }


    /// <summary>
    /// Clones the operation itself.
    /// </summary>
    /// <param name="clone"></param>
    /// <returns></returns>
    protected override Operation CloneSelf(Operation clone)
    {
      if (clone==null)
        clone = new EntitySetClearOperation(Key, Field);
      return clone;
    }


    // Constructors


    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySetClearOperation"/> class.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="field"></param>
    /// <exception cref="ArgumentOutOfRangeException">Type of provided <paramref name="field"/>
    /// must be a descendant of <see cref="EntitySetBase"/> type.</exception>
    public EntitySetClearOperation(Key key, FieldInfo field)
      : base(key, field)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySetClearOperation"/> class.
    /// </summary>
    /// <param name="info">The info.</param>
    /// <param name="context">The context.</param>
    protected EntitySetClearOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}