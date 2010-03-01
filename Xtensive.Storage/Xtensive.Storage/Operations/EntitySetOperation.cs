// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.20

using System;
using System.Runtime.Serialization;
using Xtensive.Storage.Model;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Describes an operation with <see cref="Entity"/> field of <see cref="EntitySet{TItem}"/> type.
  /// </summary>
  [Serializable]
  public abstract class EntitySetOperation : EntityFieldOperation
  {
    private readonly static Type entitySetBaseType = typeof (EntitySetBase);

    /// <summary>
    /// Gets the entity set involved into this operation.
    /// </summary>
    /// <param name="context">The operation context.</param>
    /// <returns>Entity set involved into this operation.</returns>
    public EntitySetBase GetEntitySet(OperationExecutionContext context)
    {
      var session = context.Session;
      var key = context.TryRemapKey(Key);
      var target = Query.Single(session, key);
      return (EntitySetBase) target.GetFieldValue(Field);
    }


    // Constructors

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException">Type of provided <paramref name="field"/>
    /// must be a descendant of <see cref="EntitySetBase"/> type.</exception>
    protected EntitySetOperation(Key key, FieldInfo field)
      : base(key, field)
    {
      if (!entitySetBaseType.IsAssignableFrom(field.UnderlyingProperty.PropertyType))
        throw new ArgumentOutOfRangeException(
          Strings.ExTypeOfXMustBeADescendantOfYType,
            field.UnderlyingProperty.GetShortName(true),
            entitySetBaseType.GetShortName());
    }

    /// <inheritdoc/>
    protected EntitySetOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}