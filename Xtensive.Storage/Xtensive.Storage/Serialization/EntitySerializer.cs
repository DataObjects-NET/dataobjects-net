// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.03.24

using System;
using System.Runtime.Serialization;
using Xtensive.Storage.Model;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Serialization
{
  [Serializable]
  internal static class EntitySerializer
  {
    public static void GetObjectData(Entity entity, SerializationInfo info, StreamingContext context)
    {
      var serializationContext = SerializationContext.DemandCurrent();
      var serializationKind = serializationContext.GetSerializationKind(entity);

      if (serializationKind==SerializationKind.ByReference) {
        EntityReference reference = new EntityReference(entity);
        info.SetType(typeof(EntityReference));
        reference.GetObjectData(info, context);
        return;
      }      

      foreach (FieldInfo field in entity.Type.Fields) {
        if (!IsSerializable(field))
          continue;
          
        object value = entity.GetFieldValue<object>(field, false);
        info.AddValue(field.Name, value, field.ValueType);
      }
    }

    public static void InitializeEntity(Entity entity, SerializationInfo info)
    {
      if (IsInitialized(entity))
        return;

      Session session = Session.Current;      
      TypeInfo entityType = session.Domain.Model.Types[entity.GetType()];

      KeyGenerator generator = session.Domain.KeyGenerators[entityType.Hierarchy.GeneratorInfo];
      
      Tuple keyValue = generator!=null ? 
        generator.Next() : DeserializeKeyFields(entityType, info);

      Key key = Key.Create(entityType, keyValue, true);
      entity.State = session.CreateEntityState(key);
      entity.OnInitializing(false);
    }

    private static Tuple DeserializeKeyFields(TypeInfo entityType, SerializationInfo info)
    {
      var keyFields = entityType.Hierarchy.KeyInfo.Fields;
      object[] keyValues = new object[keyFields.Count];
      int index = 0;
      foreach (FieldInfo keyField in keyFields) {
        object value = info.GetValue(keyField.Name, keyField.ValueType);
        keyValues[index] = value;
        index++;
        if (keyField.IsEntity) {
          Entity referencedEntity = (Entity) value;
          if (!IsInitialized(referencedEntity))
            DeserializationContext.DemandCurrent().InitializeEntity(referencedEntity);
        }
      }
      return Tuple.Create(keyValues);
    }

    public static void DeserializeEntityFields(Entity entity, SerializationInfo info)
    {
      foreach (FieldInfo field in entity.Type.Fields) {
        if (field.IsPrimaryKey)
          continue;

        if (!IsSerializable(field))
          continue;

        object value = info.GetValue(field.Name, field.ValueType);
        entity.SetFieldValue(field, value, false);
      }
    }

    private static bool IsSerializable(FieldInfo field)
    {
      return !field.IsTypeId && field.Parent==null;
    }

    private static bool IsInitialized(Entity entity)
    {
      return entity.State != null;
    }
  }
}