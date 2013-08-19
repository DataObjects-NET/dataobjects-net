// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.03.18

using System;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;


namespace Xtensive.Orm.Serialization
{
  /// <summary>
  /// Object to be serialized instead of <see cref="Entity"/> when serialization <see cref="SerializationKind.ByReference"/> is used.
  /// </summary>
  [Serializable]
  internal sealed class EntityReference : IObjectReference, 
    ISerializable
  {
    private const string KeyValueName = WellKnown.KeyFieldName;
    private readonly Entity entity;

    [SecurityCritical]
    public object GetRealObject(StreamingContext context)
    {
      return entity;
    }
      
    [SecurityCritical]
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue(KeyValueName, entity.Key.Format());
    }

    public EntityReference(Entity entity)
    {
      this.entity = entity;
    }

    private EntityReference(SerializationInfo info, StreamingContext context)
    {
      var session = Session.Demand();
      Key key = Key.Parse(session.Domain, info.GetString(KeyValueName));
      entity = session.Query.SingleOrDefault(key);

      if (entity==null)
        throw new InvalidOperationException(
          string.Format(Strings.ExCannotResolveEntityWithKeyX, key));
    }
  }
}