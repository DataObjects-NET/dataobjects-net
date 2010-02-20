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
using Xtensive.Storage.Services;

namespace Xtensive.Storage.Operations
{
  [Serializable]
  internal sealed class EntityFieldSetOperation : EntityFieldOperation
  {
    private object Value { get; set; }
    private Key ValueKey { get; set; }

    public override void Prepare(OperationExecutionContext context)
    {
      base.Prepare(context);
      context.RegisterKey(context.TryRemapKey(ValueKey), false);
    }

    public override void Execute(OperationExecutionContext context)
    {
      var session = context.Session;
      var key = context.TryRemapKey(Key);
      var valueKey = context.TryRemapKey(ValueKey);
      var entity = Query.Single(session, key);
      var value = ValueKey != null ? Query.Single(session, valueKey) : Value;
      entity.SetFieldValue(Field, value);
    }

    
    // Constructors

    public EntityFieldSetOperation(Key key, FieldInfo fieldInfo, object value)
      : base(key, OperationType.SetEntityField, fieldInfo)
    {
      var entityValue = value as IEntity;
      if (entityValue != null)
        ValueKey = entityValue.Key;
      else
        Value = value;
    }

    public EntityFieldSetOperation(Key key, FieldInfo fieldInfo, Key valueKey)
      : base(key, OperationType.SetEntityField, fieldInfo)
    {
      ValueKey = valueKey;
    }

    
    // Serialization

    /// <inheritdoc/>
    protected override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      var structureValue = Value as Structure;
      if (typeof(IEntity).IsAssignableFrom(Field.ValueType)) {
        // serializing entity value as key
        if (ValueKey != null)
          info.AddValue("value", ValueKey.Format());
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
          ValueKey = Key.Parse(session.Domain, value);
          ValueKey.TypeRef = new TypeReference(ValueKey.TypeRef.Type, TypeReferenceAccuracy.ExactType);
        }
      }
      else if (typeof (Structure).IsAssignableFrom(Field.ValueType)) {
        var serializedTuple = (SerializableTuple) info.GetValue("value", typeof (SerializableTuple));
        var tuple = serializedTuple.Value;
        Value = session.Services.Get<DirectPersistentAccessor>()
          .CreateStructure(Field.ValueType, tuple);
      }
      else
        Value = info.GetValue("value", Field.ValueType);
    }
  }
}