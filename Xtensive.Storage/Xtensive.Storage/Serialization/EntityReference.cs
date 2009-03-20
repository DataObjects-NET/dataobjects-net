// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.03.18

using System;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Serialization
{
  /// <summary>
  /// Entity reference (for serialization).
  /// </summary>
  [Serializable]
  internal sealed class EntityReference : IObjectReference, ISerializable
  {
    private const string KeyValueName = "Key";
    private readonly Key entityKey;

    public object GetRealObject(StreamingContext context)
    {
      Entity entity = entityKey.Resolve();
      if (entity==null)
        throw new InvalidOperationException(
          string.Format(Strings.ExCanNotToResolveEntityWithKeyX, entityKey));
      return entity;
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue(KeyValueName, entityKey.Format());
    }

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="entity">The entity to serialize.</param>
    public EntityReference(Entity entity)
    {
      entityKey = entity.Key;
    }

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="info">The serialization info to deserialize.</param>
    /// <param name="context">The streaming context.</param>
    private EntityReference(SerializationInfo info, StreamingContext context)
    {
      entityKey = Key.Parse(info.GetString(KeyValueName));
    }
  }
}