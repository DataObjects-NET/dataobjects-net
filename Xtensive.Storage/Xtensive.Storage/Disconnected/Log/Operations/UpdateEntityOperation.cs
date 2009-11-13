// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Disconnected.Log.Operations
{
  [Serializable]
  public class UpdateEntityOperation : IUpdateEntityOperation,
    ISerializable
  {
    public Key Key { get; private set; }
    public EntityOperationType Type { get; private set; }
    public FieldInfo Field { get; private set; }
    public object Value { get; private set; }
    private readonly Key entityValueKey;

    public void Prepare(PrefetchContext prefetchContext)
    {
      prefetchContext.Register(Key);
      prefetchContext.Register(entityValueKey);
    }

    public void Execute(Session session)
    {
      var entity = Query.Single(session, Key);
      var setter = DelegateHelper.CreateDelegate<Action<Entity,object>>(
        this, 
        typeof (UpdateEntityOperation), 
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

    public UpdateEntityOperation(Key key, FieldInfo fieldInfo, object value)
    {
      Key = key;
      Type = EntityOperationType.Update;
      Field = fieldInfo;
      var entityValue = value as IEntity;
      if (entityValue != null)
        entityValueKey = entityValue.Key;
      else
        Value = value;
    }

    
    // Serialization

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("key", Key.Format());
      info.AddValue("type", Type, typeof(EntityOperationType));
      info.AddValue("field", new FieldInfoRef(Field), typeof(FieldInfoRef));
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
        var serializedTuple = new SerializedTuple(structureValue.Tuple.ToRegular());
        info.AddValue("value", serializedTuple, typeof(SerializedTuple));
      }
      else
        info.AddValue("value", Value, Field.ValueType);
    }

    protected UpdateEntityOperation(SerializationInfo info, StreamingContext context)
    {
      var session = Session.Demand();
      Key = Key.Parse(session.Domain, info.GetString("key"));
      Key.TypeRef = new TypeReference(Key.TypeRef.Type, TypeReferenceAccuracy.ExactType);
      Type = (EntityOperationType)info.GetInt32("type");
      var fieldRef = (FieldInfoRef)info.GetValue("field", typeof(FieldInfoRef));
      Field = fieldRef.Resolve(session.Domain.Model);
      if (typeof(IEntity).IsAssignableFrom(Field.ValueType)) {
        // deserializing entity
        var value = info.GetString("value");
        if (!value.IsNullOrEmpty()) {
          entityValueKey = Key.Parse(session.Domain, value);
          entityValueKey.TypeRef = new TypeReference(entityValueKey.TypeRef.Type, TypeReferenceAccuracy.ExactType);
        }
      }
      else if (typeof (Structure).IsAssignableFrom(Field.ValueType)) {
        var serializedTuple = (SerializedTuple) info.GetValue("value", typeof (SerializedTuple));
        var tuple = serializedTuple.Value;
        Value = session.CoreServices.PersistentAccessor.CreateStructure(Field.ValueType, tuple);
      }
      else
        Value = info.GetValue("value", Field.ValueType);
    }
  }
}