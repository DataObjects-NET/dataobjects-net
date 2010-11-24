// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.16

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Core;
using Xtensive.Collections;
using Xtensive.Internals.DocTemplates;
using Xtensive.ObjectMapping;
using Xtensive.ObjectMapping.Model;
using Xtensive.Orm.Operations;
using FieldInfo=Xtensive.Orm.Model.FieldInfo;
using MappingOperation=Xtensive.ObjectMapping.Operation;
using OperationType=Xtensive.ObjectMapping.OperationType;

namespace Xtensive.Orm.ObjectMapping
{
  /// <summary>
  /// The O2O-mapper for persistent types.
  /// </summary>
  public sealed class Mapper : MapperBase<GraphComparisonResult>
  {
    private OperationLog comparisonResult;
    private Session session;
    private Dictionary<object, Key> newObjectKeys;
    private Dictionary<object, Key> existingObjectKeys;

    /// <inheritdoc/>
    protected override void OnObjectModified(MappingOperation mappingOperation)
    {
      IOperation operation;
      switch (mappingOperation.Type) {
      case OperationType.AddItem:
      case OperationType.RemoveItem:
        operation = CreateEntitySetItemOperation(mappingOperation);
        break;
      case OperationType.CreateObject:
        operation = CreateEntityCreateOperation(mappingOperation);
        break;
      case OperationType.RemoveObject:
        operation = new EntitiesRemoveOperation(ExtractKey(mappingOperation.Object));
        break;
      case OperationType.SetProperty:
        operation = CreateEntityFieldSetOperation(mappingOperation);
        break;
      default:
        throw new ArgumentOutOfRangeException("mappingOperation.Type");
      }
      comparisonResult.Log(operation);
    }

    /// <inheritdoc/>
    protected override void InitializeComparison(object originalTarget, object modifiedTarget)
    {
      comparisonResult = new OperationLog(OperationLogType.SystemOperationLog);
      if (newObjectKeys!=null)
        newObjectKeys.Clear();
      if (existingObjectKeys!=null)
        existingObjectKeys.Clear();
      session = Session.Demand();
    }

    /// <inheritdoc/>
    protected override GraphComparisonResult GetComparisonResult(Dictionary<object, object> originalObjects,
      Dictionary<object, object> modifiedObjects)
    {
      Dictionary<object, object> formattedKeyMapping = null;
      if (newObjectKeys!=null && newObjectKeys.Count > 0) {
        formattedKeyMapping = newObjectKeys.Select(pair => new {pair.Key, Value = pair.Value.Format()})
          .ToDictionary(pair => pair.Key, pair => (object) pair.Value);
        newObjectKeys.Clear();
      }
      var result = new GraphComparisonResult(originalObjects, modifiedObjects, comparisonResult,
        formattedKeyMapping!=null ? new ReadOnlyDictionary<object, object>(formattedKeyMapping, false) : null);
      session = null;
      comparisonResult = null;
      if (existingObjectKeys!=null)
        existingObjectKeys.Clear();
      return result;
    }

    #region Private \ internal methods

    private IOperation CreateEntityFieldSetOperation(MappingOperation mappingOperation)
    {
      IOperation operation;
      var fieldInfo = ExtractFieldInfo(mappingOperation);
      var lastProperty = mappingOperation.PropertyPath[mappingOperation.PropertyPath.Count - 1];
      if (mappingOperation.Value==null || lastProperty.ValueType.ObjectKind==ObjectKind.Primitive
        || lastProperty.ValueType.ObjectKind == ObjectKind.UserStructure) {
        operation = new EntityFieldSetOperation(ExtractKey(mappingOperation.Object), fieldInfo,
          mappingOperation.Value);
      }
      else {
        var key = ExtractKey(mappingOperation.Value);
        operation = new EntityFieldSetOperation(ExtractKey(mappingOperation.Object), fieldInfo, key);
      }
      return operation;
    }

    private IOperation CreateEntityCreateOperation(MappingOperation mappingOperation)
    {
      if (newObjectKeys==null)
        newObjectKeys = new Dictionary<object, Key>();
      var newKey = CreateKey(mappingOperation.Object);
      return new EntityCreateOperation(newKey);
    }

    private Key ExtractKey(object obj)
    {
      Key result;
      var dtoKey = (string) MappingDescription.ExtractTargetKey(obj);
      if (newObjectKeys!=null && newObjectKeys.TryGetValue(dtoKey, out result))
        return result;
      if (existingObjectKeys==null)
        existingObjectKeys = new Dictionary<object, Key>();
      if (!existingObjectKeys.TryGetValue(dtoKey, out result)) {
        result = Key.Parse(session.Domain, dtoKey);
        existingObjectKeys.Add(dtoKey, result);
      }
      return result;
    }

    private Key CreateKey(object target)
    {
      Key result;
      var dtoKey = MappingDescription.ExtractTargetKey(target);
      if (newObjectKeys.TryGetValue(dtoKey, out result))
        return result;
      var sourceType = MappingDescription.GetMappedSourceType(target.GetType());
      if (sourceType.TargetType.GeneratorArgumentsProvider!=null) {
        var customKeyFieldValues = GetCustomKeyFields(target, sourceType.TargetType);
        result = Key.Create(session.Domain, session.Domain.Model.Types[sourceType.SystemType],
          TypeReferenceAccuracy.ExactType, customKeyFieldValues);
      }
      else
        result = Key.Create(session, sourceType.SystemType);
      newObjectKeys[dtoKey] = result;
      return result;
    }

    /// <exception cref="ArgumentException"><paramref name="targetType"/> state is invalid.</exception>
    private object[] GetCustomKeyFields(object target, TargetTypeDescription targetType)
    {
      var arguments = targetType.GeneratorArgumentsProvider.Invoke(target);
      if (arguments==null)
        throw new ArgumentException("targetType");
      var result = new object[arguments.Length];
      for (var i = 0; i < arguments.Length; i++) {
        var argument = arguments[i];
        var argumentTargetType = MappingDescription.GetTargetType(argument.GetType());
        result[i] = argumentTargetType.ObjectKind==ObjectKind.Entity
          ? CreateKey(argument)
          : argument;
      }
      return result;
    }

    /// <exception cref="ArgumentOutOfRangeException"><c>operationInfo.Type</c> is wrong.</exception>
    private EntitySetItemOperation CreateEntitySetItemOperation(MappingOperation mappingOperation)
    {
      var key = ExtractKey(mappingOperation.Object);
      var itemKey = ExtractKey(mappingOperation.Value);
      var fieldInfo = ExtractFieldInfo(mappingOperation);
      switch (mappingOperation.Type) {
      case OperationType.AddItem:
        return new EntitySetItemAddOperation(key, fieldInfo, itemKey);
      case OperationType.RemoveItem:
        return new EntitySetItemRemoveOperation(key, fieldInfo, itemKey);
      default:
        throw new ArgumentOutOfRangeException("mappingOperation.Type");
      }
    }

    private FieldInfo ExtractFieldInfo(MappingOperation mappingOperation)
    {
      var sourceType = MappingDescription
        .GetMappedSourceType(mappingOperation.PropertyPath[0].SystemProperty.ReflectedType);
      var sourceTypeInfo = session.Domain.Model.Types[sourceType.SystemType];
      var lastIndex = mappingOperation.PropertyPath.Count - 1;
      var fieldName = MakeFieldName(mappingOperation.PropertyPath);
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