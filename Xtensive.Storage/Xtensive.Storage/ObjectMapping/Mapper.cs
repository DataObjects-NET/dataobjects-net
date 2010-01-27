// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.16

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.ObjectMapping;
using Xtensive.Core.ObjectMapping.Model;
using Xtensive.Storage.Operations;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.ObjectMapping
{
  /// <summary>
  /// The O2O-mapper.
  /// </summary>
  public sealed class Mapper : MapperBase<GraphComparisonResult>
  {
    private OperationSet comparisonResult;
    private Session session;
    private Dictionary<object, Key> keyMapping;

    /// <inheritdoc/>
    protected override void OnObjectModified(OperationInfo operationInfo)
    {
      IOperation operation;
      switch (operationInfo.Type) {
      case Core.ObjectMapping.OperationType.AddItem:
        operation = CreateEntitySetItemOperation(operationInfo, Operations.OperationType.AddEntitySetItem);
        break;
      case Core.ObjectMapping.OperationType.RemoveItem:
        operation = CreateEntitySetItemOperation(operationInfo, Operations.OperationType.RemoveEntitySetItem);
        break;
      case Core.ObjectMapping.OperationType.CreateObject:
        operation = CreateEntityCreationOperation(operationInfo);
        break;
      case Core.ObjectMapping.OperationType.RemoveObject:
        operation = new EntityOperation(ExtractKey(operationInfo.Object),
          Operations.OperationType.RemoveEntity);
        break;
      case Core.ObjectMapping.OperationType.SetProperty:
        operation = CreatePropertySettingOperation(operationInfo);
        break;
      default:
        throw new ArgumentOutOfRangeException("operationInfo.Type");
      }
      comparisonResult.Register(operation);
    }

    private IOperation CreatePropertySettingOperation(OperationInfo operationInfo)
    {
      IOperation operation;
      var fieldInfo = ExtractFieldInfo(operationInfo);
      var lastProperty = operationInfo.PropertyPath[operationInfo.PropertyPath.Count - 1];
      if (operationInfo.Value==null || lastProperty.ValueType.ObjectKind==ObjectKind.Primitive
        || lastProperty.ValueType.ObjectKind == ObjectKind.UserStructure) {
        operation = new EntityFieldSetOperation(ExtractKey(operationInfo.Object), fieldInfo,
          operationInfo.Value);
      }
      else {
        var newDtoKey = MappingDescription.ExtractTargetKey(operationInfo.Value);
        operation = new EntityFieldSetOperation(ExtractKey(operationInfo.Object), fieldInfo,
          keyMapping[newDtoKey]);
      }
      return operation;
    }

    private IOperation CreateEntityCreationOperation(OperationInfo operationInfo)
    {
      IOperation operation;
      if (keyMapping==null)
        keyMapping = new Dictionary<object, Key>();
      var newKey = CreateKey(operationInfo.Object, operationInfo.Object.GetType());
      var dtoKey = MappingDescription.ExtractTargetKey(operationInfo.Object);
      keyMapping[dtoKey] = newKey;
      operation = new EntityOperation(newKey, Operations.OperationType.CreateEntity);
      return operation;
    }

    /// <inheritdoc/>
    protected override void InitializeComparison(object originalTarget, object modifiedTarget)
    {
      comparisonResult = new OperationSet();
      if (keyMapping!=null)
        keyMapping.Clear();
      session = Session.Demand();
    }

    /// <inheritdoc/>
    protected override GraphComparisonResult GetComparisonResult(Dictionary<object, object> originalObjects,
      Dictionary<object, object> modifiedObjects)
    {
      Dictionary<object, object> formattedKeyMapping = null;
      if (keyMapping!=null) {
        formattedKeyMapping = keyMapping.Select(pair => new {pair.Key, Value = pair.Value.Format()})
          .ToDictionary(pair => pair.Key, pair => (object) pair.Value);
        keyMapping.Clear();
      }
      var result = new GraphComparisonResult(modifiedObjects, comparisonResult,
        formattedKeyMapping!=null ? new ReadOnlyDictionary<object, object>(formattedKeyMapping, false) : null);
      session = null;
      comparisonResult = null;
      return result;
    }

    #region Private \ internal methods

    private Key ExtractKey(object obj)
    {
      Key result;
      var dtoKey = (string) MappingDescription.ExtractTargetKey(obj);
      if (keyMapping != null && keyMapping.TryGetValue(dtoKey, out result))
        return result;
      return Key.Parse(session.Domain, dtoKey);
    }

    private Key CreateKey(object target, Type type)
    {
      var sourceType = MappingDescription.GetMappedSourceType(type);
      if (sourceType.TargetType.GeneratorArgumentsProvider!=null)
        return Key.Create(session.Domain, session.Domain.Model.Types[sourceType.SystemType],
          TypeReferenceAccuracy.ExactType, sourceType.TargetType.GeneratorArgumentsProvider.Invoke(target));
      return Key.Create(session.Domain, sourceType.SystemType);
    }

    private EntitySetItemOperation CreateEntitySetItemOperation(OperationInfo operationInfo,
      Operations.OperationType type)
    {
      var key = ExtractKey(operationInfo.Object);
      var itemKey = ExtractKey(operationInfo.Value);
      var fieldInfo = ExtractFieldInfo(operationInfo);
      return new EntitySetItemOperation(key, fieldInfo, type, itemKey);
    }

    private FieldInfo ExtractFieldInfo(OperationInfo operationInfo)
    {
      var sourceType = MappingDescription
        .GetMappedSourceType(operationInfo.PropertyPath[0].SystemProperty.ReflectedType);
      var sourceTypeInfo = session.Domain.Model.Types[sourceType.SystemType];
      var lastIndex = operationInfo.PropertyPath.Count - 1;
      var fieldName = MakeFieldName(operationInfo.PropertyPath);
      return session.Domain.Model.Types[sourceType.SystemType].Fields[fieldName];
    }

    private static string MakeFieldName(System.Collections.ObjectModel.ReadOnlyCollection<TargetPropertyDescription> propertyPath)
    {
      var stringBuidler = new StringBuilder();
      for (var i = 0; i < propertyPath.Count; i++) {
        if (i != 0)
          stringBuidler.Append(".");
        stringBuidler.Append(propertyPath[i].SystemProperty.Name);
      }
      return stringBuidler.ToString();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="mappingDescription">The mapping description.</param>
    public Mapper(MappingDescription mappingDescription)
      : base(mappingDescription, new MapperSettings())
    {}

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="mappingDescription">The mapping description.</param>
    /// <param name="settings">The mapper settings.</param>
    public Mapper(MappingDescription mappingDescription, MapperSettings settings)
      : base(mappingDescription, settings)
    {}
  }
}