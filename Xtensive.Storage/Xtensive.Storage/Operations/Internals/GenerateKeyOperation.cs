// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.02.15

using System;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Operations
{
  [Serializable]
  internal sealed class GenerateKeyOperation : Operation
  {
    private const string keyName = "key";
    private readonly KeyGenerator keyGenerator;

    public Key Key { get; private set; }

    public override void Prepare(OperationExecutionContext context)
    {
      if (context.KeyMapping.ContainsKey(Key))
        return;
      var domain = context.Session.Domain;
      var hierarchy = Key.TypeRef.Type.Hierarchy;
      if (keyGenerator!=null)
        GenerateNewKey(context, domain);
      else
        MapCompositeKey(context, domain, hierarchy);
    }

    public override void Execute(OperationExecutionContext context)
    {}

    private void GenerateNewKey(OperationExecutionContext context, Domain domain)
    {
      var result = keyGenerator.IsTemporaryKey(Key.Value)
        ? KeyFactory.Generate(domain, Key.Type)
        : Key;
      context.AddKeyMapping(Key, result);
    }

    private void MapCompositeKey(OperationExecutionContext context, Domain domain, HierarchyInfo hierarchy)
    {
      if (hierarchy.KeyFields.Count==1 && !hierarchy.KeyFields[0].IsEntity)
        return;
      var columnIndex = 0;
      var sourceTuple = Key.Value;
      var resultTuple = Tuple.Create(sourceTuple.Descriptor);
      var hasTemporaryComponentBeenFound = false;
      foreach (var keyField in hierarchy.KeyFields) {
        if (keyField.IsPrimitive) {
          resultTuple.SetValue(columnIndex, sourceTuple.GetValue(columnIndex));
          columnIndex++;
        }
        else {
          var componentKeyValue = Tuple
            .Create(keyField.Association.TargetType.KeyProviderInfo.KeyTupleDescriptor);
          sourceTuple.CopyTo(componentKeyValue, columnIndex, keyField.MappingInfo.Length);
          var componentKey = Key.Create(domain, keyField.Association.TargetType.UnderlyingType,
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
        var result = Key.Create(domain, Key.TypeRef.Type.UnderlyingType, resultTuple);
        context.AddKeyMapping(Key, result);
      }
    }


    // Constructors

    public GenerateKeyOperation(Key key)
      : base(OperationType.GenerateKey)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");

      Key = key;
    }

    public GenerateKeyOperation(Key key, KeyGenerator keyGenerator)
      : this(key)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyGenerator, "keyGenerator");

      this.keyGenerator = keyGenerator;
    }
    
    // Serialization

    protected override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("key", (SerializableKey) Key, typeof (SerializableKey));
    }

    protected GenerateKeyOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Key = ((SerializableKey) info.GetValue(keyName, typeof (SerializableKey))).Key;
    }
  }
}