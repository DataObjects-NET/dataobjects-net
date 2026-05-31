// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Xtensive.Orm.Model;
using Xtensive.Orm.Operations.Interfaces;
using Xtensive.Orm.Operations.Serialization;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Describes <see cref="Entity"/> field set operation.
  /// </summary>
  [Serializable]
  public sealed class EntityFieldSetOperation : EntityFieldOperation
  {
    [JsonIgnore]
    private IReadOnlyList<IOperation> nestedOperations = Array.Empty<IOperation>();

    /// <summary>
    /// Gets the new field value, if field is NOT a reference field 
    /// (i.e. not a field of <see cref="IEntity"/> type).
    /// </summary>
    [JsonInclude]
    public object Value { get; set; }

    /// <summary>
    /// Gets the new field value key, if field is a reference field 
    /// (i.e. field of <see cref="IEntity"/> type).
    /// </summary>
    [JsonInclude]
    public Key ValueKey { get; set; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override string Title {
      get { return "Set field"; }
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public override string Description {
      get
      {
        if (!IsStructure)
          return $"{base.Description}, Value = {Value ?? ValueKey}";
        else {
          return $"{base.Description}, Values = [{string.Join("," + Environment.NewLine, NestedOperations.Select(n => n.Description))}]";
        }
      }
    }

    /// <summary>
    /// Describes whether changed field is <see cref="IEntity"/> reference field.
    /// If <see langword="true"/>, ValueKey property defines referenced entity key.
    /// </summary>
    [JsonIgnore]
    public bool IsReference => Field.IsEntity;

    /// <summary>
    /// Describes whether changed field is <see cref="Structure"/> field.
    /// If <see langword="true"/>, both Value and ValueKey property will be <see langword="null"/> and NestedOperations will contain actual operations.
    /// </summary>
    [JsonIgnore]
    public bool IsStructure => Field.IsStructure;

    /// <summary>
    /// In case of setting a field of persistent structure type, declared in Entity, it contains actual operations with Entity fields.
    /// </summary>
    [JsonInclude]
    [JsonConverter(typeof(CollectionOfOperationsConverter))]
    public IReadOnlyList<IOperation> NestedOperations
    {
      get { return nestedOperations; }
      private set {
        nestedOperations = (value.Count == 0) ? Array.Empty<IOperation>() : value;
      }
    }

    /// <inheritdoc/>
    protected override void PrepareSelf(OperationExecutionContext context)
    {
      base.PrepareSelf(context);
      // Next line works properly when ValueKey==null
      context.RegisterKey(context.TryRemapKey(ValueKey), false);
      foreach(var nested in NestedOperations.OfType<IExecutableOperation>()) {
        nested.Prepare(context);
      }
    }

    /// <inheritdoc/>
    protected override void ExecuteSelf(OperationExecutionContext context)
    {
      if (IsStructure) {
        foreach (var nested in NestedOperations.OfType<IExecutableOperation>()) {
          nested.Execute(context);
        }
      }
      else {
        var session = context.Session;
        var key = context.TryRemapKey(Key);
        var valueKey = context.TryRemapKey(ValueKey);
        var entity = session.Query.Single(key);
        var value = IsReference ? session.Query.Single(valueKey) : Value;
        context.EntityAccessor.SetFieldValue(entity, Field, value);
        //entity.SetFieldValue(Field, value);
      }
    }

    /// <inheritdoc/>
    protected override Operation CloneSelf(Operation clone)
    {
      if (clone == null) {
        if (this.IsReference) {
          clone = new EntityFieldSetOperation(Key, Field, ValueKey);
        }
        else if (this.IsStructure) {
          
          var clonedNestedOperations = new IOperation[this.NestedOperations.Count];
          var i = 0;
          foreach (var nested in this.nestedOperations) {
            clonedNestedOperations[i++] = nested.Clone(false);
          }

          clone = new EntityFieldSetOperation(Key, Field, clonedNestedOperations);
        }
        else
          clone = new EntityFieldSetOperation(Key, Field, Value);
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
      if (value is IEntity entityValue) {
        ValueKey = entityValue.Key;
      }
      else if (value is Structure structure) {

        // when update  Entity.SomeStructure field we treat the update as series of single-field updates
        var structureType = structure.Session.Domain.Model.Types[value.GetType()];
        var ownerType = key.TypeInfo;

        var nOperations = new IOperation[structureType.Fields.Count];
        var i = 0;
        foreach (var strField in structureType.Fields) {
          var mappedEntityField = ownerType.StructureFieldMapping[new Core.Pair<FieldInfo>(field, strField)];
          var structFieldValue = structure[strField.Name];
          nOperations[i++] = new EntityFieldSetOperation(key, mappedEntityField, structFieldValue);
        }
        nestedOperations = nOperations.AsReadOnly();

        // temporary;
        //Value = value;
      }
      else {
        Value = value;
      }
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


    /// <summary>
    /// Cloning only.
    /// </summary>
    private EntityFieldSetOperation(Key key, FieldInfo field, IReadOnlyList<IOperation> nestedOperations)
      : base(key, field)
    {
      if (!field.IsStructure)
        throw new InvalidOperationException();
      this.nestedOperations = nestedOperations;
    }

    [JsonConstructor]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Style", "IDE0051")]
    private EntityFieldSetOperation(Key key, FieldInfo field, object value, Key valueKey)
      : base(key, field)
    {
      if (field.IsEntity)
        ValueKey = valueKey;
      else
        Value = value;
    }
  }
}