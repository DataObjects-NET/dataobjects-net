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
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />.
    /// </summary>
    /// <param name="key">The key of the entity.</param>
    protected KeyOperation(Key key)
    {
      Key = key;
    }

    // Serialization

    /// <summary>
    /// <see cref="SerializableDocTemplate.GetObjectData" copy="true" />
    /// </summary>
    #if NET40
    [SecurityCritical]
    #else
    [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
    #endif
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      GetObjectData(info, context);
    }

    /// <summary>
    /// <see cref="SerializableDocTemplate.GetObjectData" copy="true" />
    /// </summary>
    protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Key", Key.Format());
    }

    /// <inheritdoc/>
    protected KeyOperation(SerializationInfo info, StreamingContext context)
    {
      Key = Key.Parse(info.GetString("Key"));
      Key.TypeReference = new TypeReference(Key.TypeReference.Type, TypeReferenceAccuracy.ExactType);
    }
  }
}