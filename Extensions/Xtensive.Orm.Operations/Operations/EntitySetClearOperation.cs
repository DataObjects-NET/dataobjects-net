// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.25

using System;
using System.Text.Json.Serialization;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Describes <see cref="Entity"/> creation operation.
  /// </summary>
  public sealed class EntitySetClearOperation : EntitySetOperation
  {
    /// <inheritdoc/>
    [JsonIgnore]
    public override string Title {
      get { return "Clear entity set"; }
    }

    /// <inheritdoc/>
    protected override void ExecuteSelf(OperationExecutionContext context)
    {
      GetEntitySet(context).Clear();
    }

    /// <inheritdoc/>
    protected override Operation CloneSelf(Operation clone)
    {
      if (clone==null)
        clone = new EntitySetClearOperation(Key, Field);
      return clone;
    }


    // Constructors

    /// <inheritdoc/>
    public EntitySetClearOperation(Key key, FieldInfo field)
      : base(key, field)
    {
    }
  }
}