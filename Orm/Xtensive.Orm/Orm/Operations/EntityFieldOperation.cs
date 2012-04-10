// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.19

using System;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Xtensive.Core;

using Xtensive.Orm.Model;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Describes an operation with <see cref="Entity"/> field.
  /// </summary>
  [Serializable]
  public abstract class EntityFieldOperation : EntityOperation
  {
    /// <summary>
    /// Gets the field involved into the operation.
    /// </summary>
    public FieldInfo Field { get; private set; }


    /// <summary>
    /// Gets the description.
    /// </summary>
    public override string Description {
      get {
        return "{0}, Field = {1}".FormatWith(base.Description, Field.Name);
      }
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class..
    /// </summary>
    /// <param name="key">The key of the entity.</param>
    /// <param name="field">The field involved into the operation.</param>
    protected EntityFieldOperation(Key key, FieldInfo field)
      : base(key)
    {
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      Field = field;
    }

    // Serialization


    /// <summary>
    /// Initializes a new instance of the <see cref="EntityFieldOperation"/> class.
    /// </summary>
    /// <param name="info">The info.</param>
    /// <param name="context">The context.</param>
    protected EntityFieldOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      var session = Session.Demand();
      var fieldRef = (FieldInfoRef)info.GetValue("field", typeof(FieldInfoRef));
      Field = fieldRef.Resolve(session.Domain.Model);
    }


    /// <summary>
    /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
    /// </summary>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
    /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
    protected override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("field", new FieldInfoRef(Field), typeof(FieldInfoRef));
    }
  }
}