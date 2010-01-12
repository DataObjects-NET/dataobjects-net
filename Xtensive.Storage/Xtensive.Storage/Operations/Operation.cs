// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.19

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Base abstract class for all <see cref="IOperation"/> implementors.
  /// </summary>
  [Serializable]
  public abstract class Operation : IOperation, 
    ISerializable
  {
    /// <summary>
    /// Gets or sets the operation type.
    /// </summary>
    protected OperationType Type { get; private set; }

    /// <inheritdoc/>
    public abstract void Prepare(OperationExecutionContext context);

    /// <inheritdoc/>
    public abstract void Execute(OperationExecutionContext context);

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      GetObjectData(info, context);
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The <see cref="Type"/> property value.</param>
    protected Operation(OperationType type)
    {
      Type = type;
    }

    // Serialization

    /// <summary>
    /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
    /// </summary>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
    /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
    protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("type", Type, typeof(OperationType));
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/>.</param>
    /// <param name="context">The <see cref="StreamingContext"/>.</param>
    protected Operation(SerializationInfo info, StreamingContext context)
    {
      Type = (OperationType)info.GetValue("type", typeof(OperationType));
    }
  }
}