// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Runtime.Serialization;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Operations
{
  [Serializable]
  internal sealed class EntityFieldSetOperation : EntityFieldOperation
  {
    private object Value { get; set; }
    private Key entityValueKey;

    public override void Prepare(OperationExecutionContext context)
    {
      base.Prepare(context);
      if (context.KeysForRemap.Contains(entityValueKey))
        entityValueKey = context.KeyMapping[entityValueKey];
      context.Register(entityValueKey);
    }

    public override void Execute(OperationExecutionContext context)
    {
      var session = context.Session;
      var entity = Query.Single(session, Key);
      var setter = DelegateHelper.CreateDelegate<Action<Entity,object>>(
        this, 
        typeof (EntityFieldSetOperation), 
        "ExecuteSetValue", 
        Field.ValueType);
      var value = entityValueKey != null 
                    ? Query.Single(session, entityValueKey) 
                    : Value;
      setter.Invoke(entity, value);
    }

    private void ExecuteSetValue<T>(Entity entity, object value)
    {
      entity.SetFieldValue(Field, (T)value);
    }

    
    // Constructors

    public EntityFieldSetOperation(Key key, FieldInfo fieldInfo, object value)
      : base(key, OperationType.SetEntityField, fieldInfo)
    {
      var entityValue = value as IEntity;
      if (entityValue != null)
        entityValueKey = entityValue.Key;
      else
        Value = value;
    }

    
    // Serialization

    /// <inheritdoc/>
    protected override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      var structureValue = Value as Structure;
      if (typeof(IEntity).IsAssignableFrom(Field.ValueType)) {
        // serializing entity value as key
        if (entityValueKey != null)
          info.AddValue("value", entityValueKey.Format());
        else
          info.AddValue("value", string.Empty);
      }
      else if (structureValue != null) {
        // serializing structure value as tuple
        var serializedTuple = new SerializableTuple(structureValue.Tuple.ToRegular());
        info.AddValue("value", serializedTuple, typeof (SerializableTuple));
      }
      else
        info.AddValue("value", Value, Field.ValueType);
    }

    protected EntityFieldSetOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      var session = Session.Demand();
      if (typeof(IEntity).IsAssignableFrom(Field.ValueType)) {
        // deserializing entity
        var value = info.GetString("value");
        if (!value.IsNullOrEmpty()) {
          entityValueKey = Key.Parse(session.Domain, value);
          entityValueKey.TypeRef = new TypeReference(entityValueKey.TypeRef.Type, TypeReferenceAccuracy.ExactType);
        }
      }
      else if (typeof (Structure).IsAssignableFrom(Field.ValueType)) {
        var serializedTuple = (SerializableTuple) info.GetValue("value", typeof (SerializableTuple));
        var tuple = serializedTuple.Value;
        Value = session.CoreServices.PersistentAccessor.CreateStructure(Field.ValueType, tuple);
      }
      else
        Value = info.GetValue("value", Field.ValueType);
    }
  }
}