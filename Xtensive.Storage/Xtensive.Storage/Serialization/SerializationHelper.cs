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

      Session session = Session.Demand();
      TypeInfo entityType = session.Domain.Model.Types[entity.GetType()];
      KeyGenerator generator = null;
      if (entityType.Hierarchy.GeneratorInfo != null)
        generator = session.Domain.KeyGenerators[entityType.Hierarchy.GeneratorInfo];
      
      Tuple keyValue = generator!=null ? 
        generator.Next() : DeserializeKeyFields(entityType, info, context);

      Key key = Key.Create(entityType, keyValue, true);
      entity.State = session.CreateEntityState(key);
      entity.NotifyInitializing();
    }

    public static Tuple DeserializeKeyFields(TypeInfo entityType, SerializationInfo info, StreamingContext context)
    {
      var keyFields = entityType.Hierarchy.KeyInfo.Fields;
        Tuple keyTuple = Tuple.Create(entityType.Hierarchy.KeyInfo.TupleDescriptor);
      foreach (FieldInfo keyField in keyFields.Keys) {
        if (keyField.IsTypeId)
          keyTuple.SetValue(keyField.MappingInfo.Offset, entityType.TypeId);
        else {
          object value = info.GetValue(keyField.Name, keyField.ValueType);
          if (keyField.IsEntity) {
            Entity referencedEntity = (Entity) value;
            if (!IsInitialized(referencedEntity))
              DeserializationContext.Demand().InitializeEntity(referencedEntity);
            Tuple referencedTuple = referencedEntity.Key.Value;
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