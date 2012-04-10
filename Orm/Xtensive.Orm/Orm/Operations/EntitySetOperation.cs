// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.20

using System;
using System.Runtime.Serialization;
using Xtensive.Orm.Model;
using Xtensive.Reflection;


namespace Xtensive.Orm.Operations
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
      var target = session.Query.Single(key);
      return (EntitySetBase) target.GetFieldValue(Field);
    }


    // Constructors

    
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


    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySetOperation"/> class.
    /// </summary>
    /// <param name="info">The info.</param>
    /// <param name="context">The context.</param>
    protected EntitySetOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}