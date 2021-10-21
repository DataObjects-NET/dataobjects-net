// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Model;
using Xtensive.Orm.Services;

namespace Xtensive.Orm.Operations
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
      get
      {
        return $"{base.Description}, Value = {Value ?? ValueKey}";
      }
    }

    /// <inheritdoc/>
    protected override void PrepareSelf(OperationExecutionContext context)
    {
      base.PrepareSelf(context);
      // Next line works properly when ValueKey==null
      context.RegisterKey(context.TryRemapKey(ValueKey), false);
    }

    /// <inheritdoc/>
    protected override void ExecuteSelf(OperationExecutionContext context)
    {
      var session = context.Session;
      var key = context.TryRemapKey(Key);
      var valueKey = context.TryRemapKey(ValueKey);
      var entity = session.Query.Single(key);
      var value = ValueKey != null ? session.Query.Single(valueKey) : Value;
      entity.SetFieldValue(Field, value);
    }

    /// <inheritdoc/>
    protected override Operation CloneSelf(Operation clone)
    {
      if (clone == null) {
        if (ValueKey==null)
          clone = new EntityFieldSetOperation(Key, Field, Value);
        else
          clone = new EntityFieldSetOperation(Key, Field, ValueKey);
      }
      return clone;
    }

    
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class..
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
    /// Initializes a new instance of this class..
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
    private EntityFieldSetOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      var session = Session.Demand();
      if (WellKnownOrmInterfaces.Entity.IsAssignableFrom(Field.ValueType)) {
        // deserializing entity
        var value = info.GetString("value");
        if (!value.IsNullOrEmpty()) {
          ValueKey = Key.Parse(session.Domain, value);
//          ValueKey.TypeReference = new TypeReference(ValueKey.TypeReference.Type, TypeReferenceAccuracy.ExactType);
        }
      }
      else if (WellKnownOrmTypes.Structure.IsAssignableFrom(Field.ValueType)) {
        var tuple = (Tuple) info.GetValue("value", WellKnownOrmTypes.Tuple);
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
      if (WellKnownOrmInterfaces.Entity.IsAssignableFrom(Field.ValueType)) {
        // serializing entity value as key
        if (ValueKey != null)
          info.AddValue("value", ValueKey.Format());
        else
          info.AddValue("value", string.Empty);
      }
      else if (structureValue != null) {
        // serializing structure value as tuple
        info.AddValue("value", structureValue.Tuple.ToRegular(), WellKnownOrmTypes.Tuple);
      }
      else
        info.AddValue("value", Value, Field.ValueType);
    }
  }
}