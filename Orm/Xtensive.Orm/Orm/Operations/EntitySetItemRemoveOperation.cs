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
  /// Describes <see cref="EntitySet{TItem}"/> item add operation.
  /// </summary>
  [Serializable]
  public class EntitySetItemRemoveOperation : EntitySetItemOperation
  {

    /// <summary>
    /// Gets the title of the operation.
    /// </summary>
    public override string Title {
      get { return "Remove item from entity set"; }
    }


    /// <summary>
    /// Executes the operation itself.
    /// </summary>
    /// <param name="context">The operation execution context.</param>
    protected override void ExecuteSelf(OperationExecutionContext context)
    {
      var session = context.Session;
      var item = session.Query.Single(context.TryRemapKey(ItemKey));
      GetEntitySet(context).Remove(item);
    }


    /// <summary>
    /// Clones the operation itself.
    /// </summary>
    /// <param name="clone"></param>
    /// <returns></returns>
    protected override Operation CloneSelf(Operation clone)
    {
      if (clone==null)
        clone = new EntitySetItemRemoveOperation(Key, Field, ItemKey);
      return clone;
    }
    
    // Constructors


    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySetItemRemoveOperation"/> class.
    /// </summary>
    /// <param name="key">The key of the entity.</param>
    /// <param name="field">The field involved into the operation.</param>
    /// <param name="itemKey">The item key.</param>
    public EntitySetItemRemoveOperation(Key key, FieldInfo field, Key itemKey)
      : base(key, field, itemKey)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySetItemRemoveOperation"/> class.
    /// </summary>
    /// <param name="info">The info.</param>
    /// <param name="context">The context.</param>
    protected EntitySetItemRemoveOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}