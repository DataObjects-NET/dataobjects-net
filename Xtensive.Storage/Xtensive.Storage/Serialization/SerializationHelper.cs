// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.03.24

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Storage.Model;
using Xtensive.Core.Tuples;
using System.Linq;

namespace Xtensive.Storage.Serialization
{
  [Serializable]
  internal static class SerializationHelper
  {    
    public static void GetEntityValueData(Entity entity, SerializationInfo info, StreamingContext context)
    {
      foreach (FieldInfo field in entity.Type.Fields) {
        if (!IsSerializable(field))
          continue;
          
        object value = entity.GetFieldValue<object>(field);
        info.AddValue(field.Name, value, field.ValueType);
      }
    }

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

      KeyGenerator generator;
      domain.KeyGenerators.TryGetValue(typeInfo.KeyProviderInfo, out generator);

      var keyValue = generator!=null 
        ? generator.DemandNext(session.IsDisconnected) 
        : DeserializeKeyFields(typeInfo, info, context);
      var key = Key.Create(domain, typeInfo, TypeReferenceAccuracy.ExactType, keyValue);

//      if (useGenerator)
//        session.NotifyKeyGenerated(key);
      entity.State = session.CreateEntityState(key);
      entity.SystemBeforeInitialize(false);
    }

    public static Tuple DeserializeKeyFields(TypeInfo entityType, SerializationInfo info, StreamingContext context)
    {
      var keyTuple = Tuple.Create(entityType.Hierarchy.KeyProviderInfo.KeyTupleDescriptor);
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
      foreach (FieldInfo field in entity.Type.Fields) {
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