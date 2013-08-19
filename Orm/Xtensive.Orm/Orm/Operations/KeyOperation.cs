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


namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Describes an operation involving the <see cref="Key"/>.
  /// </summary>
  [Serializable]
  public abstract class KeyOperation : Operation, 
    ISerializable
  {
    /// <summary>
    /// Gets the key of the entity.
    /// </summary>
    public Key Key { get; private set; }

    /// <inheritdoc/>
    public override string Description {
      get {
        return "{0}, Key = {1}".FormatWith(Title, Key);
      }
    }

    /// <inheritdoc/>
    protected override void PrepareSelf(OperationExecutionContext context)
    {
      context.RegisterKey(context.TryRemapKey(Key), false);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class..
    /// </summary>
    /// <param name="key">The key of the entity.</param>
    protected KeyOperation(Key key)
    {
      Key = key;
    }

    // Serialization

    [SecurityCritical]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      GetObjectData(info, context);
    }

    /// <summary>
    /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
    /// </summary>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
    /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
    protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Key", Key.Format());
    }

    /// <inheritdoc/>
    protected KeyOperation(SerializationInfo info, StreamingContext context)
    {
      Key = Key.Parse(Domain.Demand(), info.GetString("Key"));
//      Key.TypeReference = new TypeReference(Key.TypeReference.Type, TypeReferenceAccuracy.ExactType);
    }
  }
}