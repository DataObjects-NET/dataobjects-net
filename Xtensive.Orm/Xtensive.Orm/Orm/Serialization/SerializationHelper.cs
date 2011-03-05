// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.03.24

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using System.Linq;

namespace Xtensive.Orm.Serialization
{
  [Serializable]
  internal static class SerializationHelper
  {    
    public static void GetEntityValueData(Entity entity, SerializationInfo info, StreamingContext context)
    {
      foreach (FieldInfo field in entity.TypeInfo.Fields) {
        if (!IsSerializable(field))
          continue;
          
        object value = entity.GetFieldValue(field);
        info.AddValue(field.Name, value, field.ValueType);
      }
    }

    #if NET40
    [SecuritySafeCritical]
    #endif
    public static void GetEntityReferenceData(Entity entity, SerializationInfo info, StreamingContext context)
    {
      EntityReference reference = new EntityReference(entity);
      info.SetType(typeof(EntityReference));
      reference.GetObjectData(info, context);
    }

    public static void InitializeEntity(Entity entity, SerializationInfo info, StreamingContext context)
    {
      if (IsInitialized(entity))
        return;

      var session = Session.Demand();
      var domain = session.Domain;
      var typeInfo = domain.Model.Types[entity.GetType()];

      var keyGenerator = domain.KeyGenerators[typeInfo.Key];
      var keyValue = keyGenerator!=null 
        ? keyGenerator.GenerateKey(session.IsDisconnected) 
        : DeserializeKeyFields(typeInfo, info, context);
      var key = Key.Create(domain, typeInfo, TypeReferenceAccuracy.ExactType, keyValue);

      var entityState = session.CreateEntityState(key);
      entity.State = entityState;
      session.InitializeEntity(entity, false);
    }

    public static Tuple DeserializeKeyFields(TypeInfo entityType, SerializationInfo info, StreamingContext context)
    {
      var keyTuple = Tuple.Create(entityType.Hierarchy.Key.TupleDescriptor);
      foreach (FieldInfo keyField in entityType.Fields.Where(f => f.IsPrimaryKey && f.Parent == null)) {
        if (keyField.IsTypeId)
          keyTuple.SetValue(keyField.MappingInfo.Offset, entityType.TypeId);
        else {
          var value = info.GetValue(keyField.Name, keyField.ValueType);
          if (keyField.IsEntity) {
            var referencedEntity = (Entity) value;
            if (!IsInitialized(referencedEntity))
              DeserializationContext.Demand().InitializeEntity(referencedEntity);
            var referencedTuple = referencedEntity.Key.Value;
            referencedTuple.CopyTo(keyTuple, keyField.MappingInfo.Offset);
          }
          else
            keyTuple.SetValue(keyField.MappingInfo.Offset, value);
        }
      }
      return keyTuple;
    }

    public static void DeserializeEntityFields(Entity entity, SerializationInfo info, StreamingContext context)
    {
      foreach (FieldInfo field in entity.TypeInfo.Fields) {
        if (field.IsPrimaryKey)
          continue;

        if (!IsSerializable(field))
          continue;

        object value = info.GetValue(field.Name, field.ValueType);
        entity.SetFieldValue(field, value);
      }
    }

    private static bool IsSerializable(FieldInfo field)
    {
      return field.Parent==null && !field.IsTypeId;
    }

    private static bool IsInitialized(Entity entity)
    {
      return entity.State != null;
    }
  }
}