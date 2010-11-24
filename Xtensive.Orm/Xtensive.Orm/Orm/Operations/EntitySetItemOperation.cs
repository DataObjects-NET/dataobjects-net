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
using Xtensive.Internals.DocTemplates;
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

    /// <inheritdoc/>
    public override string Description {
      get {
        return "{0}, Item Key = {1}".FormatWith(base.Description, ItemKey);
      }
    }

    /// <inheritdoc/>
    protected override void PrepareSelf(OperationExecutionContext context)
    {
      base.PrepareSelf(context);
      context.RegisterKey(context.TryRemapKey(ItemKey), false);
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>.
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

    /// <inheritdoc/>
    protected override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("ItemKey", ItemKey.Format());
    }

    /// <inheritdoc/>
    protected EntitySetItemOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      ItemKey = Key.Parse(Domain.Demand(), info.GetString("ItemKey"));
      ItemKey.TypeReference = new TypeReference(ItemKey.TypeReference.Type, TypeReferenceAccuracy.ExactType);
    }
  }
}