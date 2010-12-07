// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.02.15

using System;
using System.Linq;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Describes key generation operation.
  /// </summary>
  [Serializable]
  public sealed class KeyGenerateOperation : KeyOperation
  {
    /// <inheritdoc/>
    public override string Title {
      get { return "Generate Key"; }
    }

    /// <inheritdoc/>
    protected override void PrepareSelf(OperationExecutionContext context)
    {
      if (context.KeyMapping.ContainsKey(Key))
        return;
      var keyInfo = Key.TypeReference.Type.Key;
      if (!keyInfo.ContainsForeignKeys)
        MapNewKey(context);
      else
        MapCompositeKey(context);
    }

    private void MapNewKey(OperationExecutionContext context)
    {
      var domain = context.Session.Domain;
      if (Key.IsTemporary(domain)) {
        var mappedKey = Key.Create(context.Session, Key.TypeInfo.UnderlyingType);
        context.AddKeyMapping(Key, mappedKey);
      }
    }

    private void MapCompositeKey(OperationExecutionContext context)
    {
      // TODO: AY: Review this later
      var domain = context.Session.Domain;
      var keyInfo = Key.TypeReference.Type.Key;
      var hierarchy = keyInfo.Hierarchy;
      if (hierarchy.Key.Fields.Count==1 && !hierarchy.Key.Fields[0].IsEntity)
        return;
      var columnIndex = 0;
      var sourceTuple = Key.Value;
      var resultTuple = Tuple.Create(sourceTuple.Descriptor);
      var hasTemporaryComponentBeenFound = false;
      foreach (var keyField in hierarchy.Key.Fields) {
        if (keyField.Parent!=null)
          continue;
        if (keyField.IsPrimitive) {
          resultTuple.SetValue(columnIndex, sourceTuple.GetValue(columnIndex));
          columnIndex++;
        }
        else {
          var association = keyField.Associations.Last();
          var componentKeyValue = Tuple.Create(association.TargetType.Key.TupleDescriptor);
          sourceTuple.CopyTo(componentKeyValue, columnIndex, keyField.MappingInfo.Length);
          var componentKey = Key.Create(domain, association.TargetType.UnderlyingType,
            componentKeyValue);
          var componentKeyLength = componentKey.Value.Count;
          Key mappedKey;
          if (context.KeyMapping.TryGetValue(componentKey, out mappedKey)) {
            mappedKey.Value.CopyTo(resultTuple, 0, columnIndex, componentKeyLength);
            hasTemporaryComponentBeenFound = true;
          }
          else
            componentKeyValue.CopyTo(resultTuple, 0, columnIndex, componentKeyLength);
          columnIndex += componentKeyLength;
        }
      }
      if (hasTemporaryComponentBeenFound) {
        var result = Key.Create(domain, Key.TypeReference.Type.UnderlyingType, resultTuple);
        context.AddKeyMapping(Key, result);
      }
    }

    /// <inheritdoc/>
    protected override void ExecuteSelf(OperationExecutionContext context)
    {
    }

    /// <inheritdoc/>
    protected override Operation CloneSelf(Operation clone)
    {
      if (clone==null)
        clone = new KeyGenerateOperation(Key);
      return clone;
    }


    // Constructors

    /// <inheritdoc/>
    public KeyGenerateOperation(Key key)
      : base(key)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
    }

    /// <inheritdoc/>
    protected KeyGenerateOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}