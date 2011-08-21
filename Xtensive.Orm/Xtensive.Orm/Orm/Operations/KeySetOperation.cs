// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.10

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Xtensive.Core;
using Xtensive.Collections;
using Xtensive.Internals.DocTemplates;
using System.Linq;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Describes operation over key set.
  /// </summary>
  [Serializable]
  public abstract class KeySetOperation : Operation,
    ISerializable
  {
    /// <inheritdoc/>
    public override string Description {
      get {
        return "{0}:\r\n{1}".FormatWith(Title, EnumerableExtensions.ToDelimitedString<Key>(Keys, "\r\n").Indent(2));
      }
    }

    /// <summary>
    /// Gets the key set.
    /// </summary>
    public ReadOnlySet<Key> Keys { get; private set; }

    /// <inheritdoc/>
    protected override void PrepareSelf(OperationExecutionContext context)
    {
      foreach (var key in Keys)
        context.RegisterKey(context.TryRemapKey(key), false);
    }


    // Constructors

    /// <inheritdoc/>
    public KeySetOperation(Key key)
      : this(EnumerableUtils.One(key))
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="keys">The sequence of keys.</param>
    public KeySetOperation(IEnumerable<Key> keys)
    {
      Keys = new ReadOnlySet<Key>(new Set<Key>(keys));
    }

    // Serialization

    /// <inheritdoc/>
    protected KeySetOperation(SerializationInfo info, StreamingContext context)
    {
      var formattedKeys = info.GetString("Keys");
      var keys = new Set<Key>();
      foreach (var formattedKey in formattedKeys.RevertibleSplit('\\', ';')) {
        var key = Key.Parse(Domain.Demand(), formattedKey);
//        key.TypeReference = new TypeReference(key.TypeReference.Type, TypeReferenceAccuracy.ExactType);
        keys.Add(key);
      }
      Keys = new ReadOnlySet<Key>(keys);
    }

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
      var formattedKeys = Keys.Select(key => key.Format()).RevertibleJoin('\\', ';');
      info.AddValue("Keys", formattedKeys);
    }
  }

}