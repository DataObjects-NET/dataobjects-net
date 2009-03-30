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
        if (field.IsPrimaryKey)
          continue;
        object value = entity.GetFieldValue<object>(field, false);
        info.AddValue(field.Name, value, field.ValueType);
      }
    }

    public static void DeserializeKey(Entity entity, SerializationInfo info)
    {
      if (IsInitialized(entity))
        return;

      Session session = Session.Current;      
      TypeInfo entityType = session.Domain.Model.Types[entity.GetType()];
      Tuple keyValue;

      KeyGenerator generator = session.Domain.KeyGenerators[entityType.Hierarchy.GeneratorInfo];
      if (generator!=null) // Generated key
        keyValue = generator.Next();             
      else { 
        // Not generated key
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
        keyValue = Tuple.Create(keyValues);        
      }
      Key key = Key.Create(entityType, keyValue, true);
      entity.State = session.CreateEntityState(key);
      entity.OnInitializing(false);
    }

    public static void DeserializeFieldValues(Entity entity, SerializationInfo info)
    {
      foreach (FieldInfo field in entity.Type.Fields) {
        if (field.IsPrimaryKey)
          continue;

        object value = info.GetValue(field.Name, field.ValueType);
        entity.SetFieldValue(field, value, false);
      }
    }

    private static bool IsInitialized(Entity entity)
    {
      return entity.State != null;
    }
  }
}