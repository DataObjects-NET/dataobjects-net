// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Services;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Describes <see cref="Entity"/> field set operation.
  /// </summary>
  [Serializable]
  public sealed class EntityFieldSetOperation : EntityFieldOperation
  {
    /// <summary>
    /// Gets the new field value, if field is NOT a reference field 
    /// (i.e. not a field of <see cref="IEntity"/> type).
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    /// Gets the new field value key, if field is a reference field 
    /// (i.e. field of <see cref="IEntity"/> type).
    /// </summary>
    public Key ValueKey { get; set; }

    /// <inheritdoc/>
    public override string Title {
      get { return "Set field"; }
    }

    /// <inheritdoc/>
    public override string Description {
      get {
        return "{0}, Value = {1}".FormatWith(base.Description, Value ?? ValueKey);
      }
    }

    /// <inheritdoc/>
    public override void Prepare(OperationExecutionContext context)
    {
      base.Prepare(context);
      // Next line works properly when ValueKey==null
      context.RegisterKey(context.TryRemapKey(ValueKey), false);
    }

    /// <inheritdoc/>
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

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>.
    /// </summary>
    /// <param name="key">The key of the changed entity.</param>
    /// <param name="field">The field involved into the operation.</param>
    /// <param name="value">The new field value.</param>
    public EntityFieldSetOperation(Key key, FieldInfo field, object value)
      : base(key, field)
    {
      var entityValue = value as IEntity;
      if (entityValue != null)
        ValueKey = entityValue.Key;
      else
        Value = value;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>.
    /// </summary>
    /// <param name="key">The key of the changed entity.</param>
    /// <param name="field">The field involved into the operation.</param>
    /// <param name="valueKey">The new field value key.</param>
    public EntityFieldSetOperation(Key key, FieldInfo field, Key valueKey)
      : base(key, field)
    {
      ValueKey = valueKey;
    }

    
    // Serialization

    /// <inheritdoc/>
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
  }
}