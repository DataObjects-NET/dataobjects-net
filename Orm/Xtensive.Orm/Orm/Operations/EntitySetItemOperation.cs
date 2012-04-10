// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Xtensive.Core;

using Xtensive.Orm.Model;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Describes an operation with <see cref="EntitySet{TItem}"/> item.
  /// </summary>
  [Serializable]
  public abstract class EntitySetItemOperation : EntitySetOperation
  {
    /// <summary>
    /// Gets the key of the involved item.
    /// </summary>
    public Key ItemKey { get; set; }


    /// <summary>
    /// Gets the description.
    /// </summary>
    public override string Description {
      get {
        return "{0}, Item Key = {1}".FormatWith(base.Description, ItemKey);
      }
    }


    /// <summary>
    /// Prepares the self.
    /// </summary>
    /// <param name="context">The context.</param>
    protected override void PrepareSelf(OperationExecutionContext context)
    {
      base.PrepareSelf(context);
      context.RegisterKey(context.TryRemapKey(ItemKey), false);
    }

    
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class..
    /// </summary>
    /// <param name="key">The key of the entity.</param>
    /// <param name="field">The field involved into the operation.</param>
    /// <param name="itemKey">The item key.</param>
    protected EntitySetItemOperation(Key key, FieldInfo field, Key itemKey)
      : base(key, field)
    {
      ArgumentValidator.EnsureArgumentNotNull(itemKey, "itemKey");
      ItemKey = itemKey;
    }

    // Serialization


    /// <summary>
    /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
    /// </summary>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
    /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
    protected override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("ItemKey", ItemKey.Format());
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySetItemOperation"/> class.
    /// </summary>
    /// <param name="info">The info.</param>
    /// <param name="context">The context.</param>
    protected EntitySetItemOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      ItemKey = Key.Parse(Domain.Demand(), info.GetString("ItemKey"));
//      ItemKey.TypeReference = new TypeReference(ItemKey.TypeReference.Type, TypeReferenceAccuracy.ExactType);
    }
  }
}