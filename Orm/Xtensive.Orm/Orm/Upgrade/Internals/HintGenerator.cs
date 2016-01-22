// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.04

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Model;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade.Internals;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Reflection;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class HintGenerator
  {
    private readonly NameBuilder nameBuilder;
    private readonly MappingResolver resolver;

    private readonly StorageModel extractedStorageModel;
    private readonly StoredDomainModel extractedModel;
    private readonly StoredDomainModel currentModel;

    private readonly Dictionary<StoredTypeInfo, StoredTypeInfo> typeMapping;
    private readonly Dictionary<StoredTypeInfo, StoredTypeInfo> reverseTypeMapping;
    private readonly Dictionary<StoredFieldInfo, StoredFieldInfo> fieldMapping;
    private readonly Dictionary<StoredTypeInfo, StoredFieldInfo[]> extractedModelFields;

    private readonly NativeTypeClassifier<UpgradeHint> hints; 

    private readonly List<Hint> schemaHints = new List<Hint>();

    public HintGenerationResult Run()
    {
      // Generating schema hints

      GenerateRenameTableHints();
      GenerateRenameColumnHints();

      var copyFieldHints = hints.GetItems<CopyFieldHint>().ToList();
      GenerateCopyColumnHints(copyFieldHints);

      var removedTypes = GetRemovedTypes(extractedModel);
      GenerateRecordCleanupHints(removedTypes, false);
      
      var movedTypes = GetMovedTypes(extractedModel);
      GenerateRecordCleanupHints(movedTypes, true);
      
      // Adding useful info

      CalculateAffectedTablesAndColumns(hints);

      // Hints validation
      new UpgradeHintValidator(currentModel, extractedModel, typeMapping, reverseTypeMapping).Validate(hints);

      var upgradedTypesMapping = reverseTypeMapping
        .ToDictionary(item => item.Key.UnderlyingType, item => item.Value.UnderlyingType);

      return new HintGenerationResult(hints.ToList(), schemaHints, upgradedTypesMapping);
    }

    #region Hint generation

    private void GenerateRenameTableHints()
    {
      var mappingsToProcess = typeMapping
        .Where(type => type.Key.IsEntity)
        .Where(type => type.Key.Hierarchy.InheritanceSchema!=InheritanceSchema.SingleTable
          || type.Key.Hierarchy.Root==type.Key);
      foreach (var mapping in mappingsToProcess) {
        var oldType = mapping.Key;
        var newType = mapping.Value;
        if (GetTableName(oldType)!=GetTableName(newType)) {
          RegisterRenameTableHint(oldType, newType);
        }
      }
    }

    private void GenerateRenameColumnHints()
    {
      var fieldsToProcess = fieldMapping
        .Where(m => m.Key.IsPrimitive && m.Key.DeclaringType.IsEntity);
      foreach (var mapping in fieldsToProcess) {
        var oldField = mapping.Key;
        var newField = mapping.Value;
        var newType = newField.DeclaringType;
        switch (newType.Hierarchy.InheritanceSchema) {
        case InheritanceSchema.ClassTable:
          // Rename column in inheritors only when field is a key
          GenerateRenameFieldHint(oldField, newField, newType, newField.IsPrimaryKey);
          break;
        case InheritanceSchema.SingleTable:
          // Always rename only one column
          GenerateRenameFieldHint(oldField, newField, newType, false);
          break;
        case InheritanceSchema.ConcreteTable:
          // Rename column in all inheritors and type itself
          GenerateRenameFieldHint(oldField, newField, newType, true);
          break;
        default:
          throw Exceptions.InternalError(String.Format(
            Strings.ExInheritanceSchemaIsInvalid, newType.Hierarchy.InheritanceSchema), UpgradeLog.Instance);
        }
      }
    }

    private void GenerateRenameFieldHint(StoredFieldInfo oldField, StoredFieldInfo newField,
      StoredTypeInfo newType, bool includeInheritors)
    {
      if (oldField.MappingName==newField.MappingName)
        return;
      foreach (var newTargetType in GetAffectedMappedTypes(newType, includeInheritors)) {
        StoredTypeInfo oldTargetType;
        if (!reverseTypeMapping.TryGetValue(newTargetType, out oldTargetType))
          continue;
        RegisterRenameFieldHint(oldTargetType, newTargetType, oldField.MappingName, newField.MappingName);
      }
    }
    
    private void GenerateCopyColumnHints(IEnumerable<CopyFieldHint> hints)
    {
      foreach (var hint in hints)
        GenerateCopyColumnHint(hint);
    }

    private void GenerateCopyColumnHint(CopyFieldHint hint)
    {
      // searching for all required objects
      var targetTypeName = hint.TargetType.GetFullName();
      var targetType = currentModel.Types.SingleOrDefault(type => type.UnderlyingType==targetTypeName);
      if (targetType==null)
        throw new InvalidOperationException(String.Format(Strings.ExUpgradeHintTargetTypeNotFound, targetTypeName));
      var targetField = targetType.AllFields.SingleOrDefault(field => field.Name==hint.TargetField);
      if (targetField==null)
        throw new InvalidOperationException(String.Format(Strings.ExUpgradeHintTargetFieldNotFound, hint.TargetField));
      var targetHierarchy = targetType.Hierarchy;

      var sourceTypeName = hint.SourceType;
      var sourceType = extractedModel.Types.SingleOrDefault(type => type.UnderlyingType==sourceTypeName);
      if (sourceType==null)
        throw new InvalidOperationException(String.Format(Strings.ExUpgradeHintSourceTypeNotFound, sourceTypeName));
      var sourceField = sourceType.AllFields.SingleOrDefault(field => field.Name==hint.SourceField);
      if (sourceField==null)
        throw new InvalidOperationException(String.Format(Strings.ExUpgradeHintSourceFieldNotFound, hint.SourceField));
      var sourceHierarchy = sourceType.Hierarchy;
      
      // checking that types have hierarchies
      if (sourceHierarchy==null)
        throw TypeIsNotInHierarchy(hint.SourceType);
      if (targetHierarchy == null)
        throw TypeIsNotInHierarchy(targetTypeName);
      
      // building set of key columns
      var pairedKeyColumns = AssociateMappedKeyFields(sourceHierarchy, targetHierarchy);
      if (pairedKeyColumns==null)
        throw KeysDoNotMatch(sourceTypeName, targetTypeName);

      // building set of copied columns
      var pairedColumns = AssociateMappedFields(new Pair<StoredFieldInfo>(sourceField, targetField));
      if (pairedColumns==null)
        throw FieldsDoNotMatch(sourceField, targetField);

      // building source/destination table/column names
      var targetTypes = GetAffectedMappedTypes(
        targetType, targetHierarchy.InheritanceSchema==InheritanceSchema.ConcreteTable);

      // generating result hints
      foreach (var target in targetTypes) {
        var sourceTablePath = GetTablePath(sourceType);
        var identities = new List<IdentityPair>();
        var copiedColumns = new List<Pair<string>>();
        foreach (var keyColumnPair in pairedKeyColumns)
          identities.Add(new IdentityPair(
            GetColumnPath(sourceType, keyColumnPair.First),
            GetColumnPath(target, keyColumnPair.Second),
            false));
        foreach (var columnPair in pairedColumns)
          copiedColumns.Add(new Pair<string>(
            GetColumnPath(sourceType, columnPair.First),
            GetColumnPath(target, columnPair.Second)));
        schemaHints.Add(new CopyDataHint(sourceTablePath, identities, copiedColumns));
      }
    }

    private void GenerateRecordCleanupHints(List<StoredTypeInfo> removedTypes, bool isMovedToAnotherHierarchy)
    {
      if (!isMovedToAnotherHierarchy)
        removedTypes.ForEach(GenerateCleanupByForegnKeyHints);
      removedTypes.ForEach(type => GenerateCleanupByPrimaryKeyHints(type, isMovedToAnotherHierarchy));
    }

    private void GenerateCleanupByPrimaryKeyHints(StoredTypeInfo removedType, bool isMovedToAnotherHierarchy)
    {
      var typesToProcess = new List<StoredTypeInfo>();
      var hierarchy = removedType.Hierarchy;
      switch (hierarchy.InheritanceSchema) {
      case InheritanceSchema.ClassTable:
        if (!isMovedToAnotherHierarchy)
          typesToProcess.Add(removedType);
        typesToProcess.AddRange(removedType.AllAncestors);
        break;
      case InheritanceSchema.SingleTable:
        typesToProcess.Add(hierarchy.Root);
        break;
      case InheritanceSchema.ConcreteTable:
        typesToProcess.Add(removedType);
        break;
      default:
        throw Exceptions.InternalError(String.Format(Strings.ExInheritanceSchemaIsInvalid, hierarchy.InheritanceSchema), UpgradeLog.Instance);
      }
      foreach (var type in typesToProcess) {
        var sourceTablePath = GetTablePath(type);
        var identities = new List<IdentityPair>();
        // ConcreteTable schema doesn't include TypeId
        if (hierarchy.InheritanceSchema != InheritanceSchema.ConcreteTable)
          identities.Add(new IdentityPair(
            GetColumnPath(type, GetTypeIdMappingName(type)),
            removedType.TypeId.ToString(),
            true));
        schemaHints.Add(
          new DeleteDataHint(sourceTablePath, identities, isMovedToAnotherHierarchy));
      }
    }
    
    private void GenerateCleanupByForegnKeyHints(StoredTypeInfo removedType)
    {
      var removedTypeAndAncestors = removedType.AllAncestors.AddOne(removedType).ToHashSet();
      var affectedAssociations = (
        from association in extractedModel.Associations
        let requiresInverseCleanup = 
          association.IsMaster &&
          association.ConnectorType!=null &&
          removedTypeAndAncestors.Contains(association.ReferencingField.DeclaringType)
        where
          // Regular association X.Y, Y must be cleaned up
          removedTypeAndAncestors.Contains(association.ReferencedType) ||
          // X.EntitySet<Y>, where X is in removedTypeAndAncestors,
          // connectorType.X must be cleaned up as well
          requiresInverseCleanup
        select new {association, requiresInverseCleanup}
        ).ToList();
      foreach (var pair in affectedAssociations) {
        var association = pair.association;
        bool requiresInverseCleanup = pair.requiresInverseCleanup;
        if (association.ConnectorType==null) {
          // This is regular reference
          var field = association.ReferencingField;
          var declaringType = field.DeclaringType;
          if (declaringType.IsInterface) {
              var candidates = (extractedModel.Types.Where(
                t => !t.IsInterface && t.Fields.Any(
                  f => f.IsInterfaceImplementation && f.Name == field.Name && f.ValueType == field.ValueType)))
                  .ToList();
              foreach (var candidate in candidates) {
                  var inheritanceSchema = candidate.Hierarchy.InheritanceSchema;
                  GenerateClearReferenceHints(
                    removedType, 
                    GetAffectedMappedTypes(candidate, inheritanceSchema==InheritanceSchema.ConcreteTable).ToArray(),
                    association,
                    requiresInverseCleanup);
              }
          }
          else {
            var inheritanceSchema = declaringType.Hierarchy.InheritanceSchema;
            GenerateClearReferenceHints(
              removedType, 
              GetAffectedMappedTypes(declaringType, inheritanceSchema==InheritanceSchema.ConcreteTable).ToArray(),
              association,
              requiresInverseCleanup);
          }
        }
        else
          // This is EntitySet
          GenerateClearReferenceHints(
            removedType, 
            new [] {association.ConnectorType},
            association,
            requiresInverseCleanup);
      }
    }

    private void GenerateClearReferenceHints(
      StoredTypeInfo removedType, 
      StoredTypeInfo[] updatedTypes,
      StoredAssociationInfo association,
      bool inverse)
    {
      foreach (var updatedType in updatedTypes)
        GenerateClearReferenceHint(
          removedType,
          updatedType,
          association,
          inverse);
    }

    private void GenerateClearReferenceHint(
      StoredTypeInfo removedType, 
      StoredTypeInfo updatedType,
      StoredAssociationInfo association,
      bool inverse)
    {
      if (association.ReferencingField.IsEntitySet && association.ConnectorType==null)
        // There is nothing to cleanup in class containing EntitySet<T> property,
        // when T is removed, and EntitySet<T> was paired to a property of T.
        return;

      var identityFieldsOfRemovedType = removedType.AllFields.Where(f => f.IsPrimaryKey).ToList();
      var identityFieldsOfUpdatedType = association.ConnectorType!=null
        ? association.ConnectorType.Fields
          .Single(field => field.Name==((association.IsMaster ^ inverse) ? WellKnown.SlaveFieldName : WellKnown.MasterFieldName))
          .Fields
        : association.ReferencingField.Fields;
      var pairedIdentityFields = JoinFieldsByOriginalName(identityFieldsOfRemovedType, identityFieldsOfUpdatedType);
      var pairedIdentityColumns = AssociateMappedFields(pairedIdentityFields);
      if (pairedIdentityColumns==null)
        throw new InvalidOperationException(
          String.Format(Strings.ExPairedIdentityColumnsForTypesXAndXNotFound, removedType, updatedType));

      var sourceTablePath = GetTablePath(updatedType);
      var identities = pairedIdentityColumns.Select(pair =>
        new IdentityPair(
          GetColumnPath(updatedType, pair.Second),
          GetColumnPath(removedType, pair.First), false))
        .ToList();
      if (removedType.Hierarchy.InheritanceSchema!=InheritanceSchema.ConcreteTable)
        identities.Add(new IdentityPair(
          GetColumnPath(removedType, GetTypeIdMappingName(removedType)),
          removedType.TypeId.ToString(), true));
      var updatedColumns = pairedIdentityColumns.Select(pair =>
        new Pair<string, object>(GetColumnPath(updatedType, pair.Second), null))
        .ToList();

      if (association.ConnectorType==null)
        schemaHints.Add(new UpdateDataHint(sourceTablePath, identities, updatedColumns));
      else
        schemaHints.Add(new DeleteDataHint(sourceTablePath, identities));
    }

    #endregion

    #region Generate additional info

    private void CalculateAffectedTablesAndColumns(IEnumerable<UpgradeHint> hints)
    {
      foreach (var hint in hints) {
        if (hint is RemoveTypeHint)
          UpdateAffectedTables((RemoveTypeHint) hint);
        if (hint is RemoveFieldHint)
          UpdateAffectedColumns((RemoveFieldHint)hint);
        if (hint is ChangeFieldTypeHint)
          UpdateAffectedColumns((ChangeFieldTypeHint)hint);
      }
    }

    private void UpdateAffectedTables(RemoveTypeHint hint)
    {
      var affectedTables = new List<string>();
      var typeName = hint.Type;
      var storedType = extractedModel.Types.SingleOrDefault(type =>
        type.UnderlyingType == typeName);
      if (storedType == null)
        throw TypeNotFound(typeName);
      var inheritanceSchema = storedType.Hierarchy.InheritanceSchema;

      switch (inheritanceSchema)
      {
        case InheritanceSchema.ClassTable:
          affectedTables.Add(GetTablePath(storedType));
          break;
        case InheritanceSchema.SingleTable:
          affectedTables.Add(GetTablePath(storedType.Hierarchy.Root));
          break;
        case InheritanceSchema.ConcreteTable:
          var typeToProcess = GetAffectedMappedTypes(storedType,
            storedType.Hierarchy.InheritanceSchema == InheritanceSchema.ConcreteTable);
          affectedTables.AddRange(typeToProcess.Select(GetTablePath));
          break;
        default:
          throw Exceptions.InternalError(String.Format(
            Strings.ExInheritanceSchemaIsInvalid, inheritanceSchema), UpgradeLog.Instance);
      }
      hint.AffectedTables = new ReadOnlyList<string>(affectedTables);
    }

    private void UpdateAffectedColumns(ChangeFieldTypeHint hint)
    {
      var currentTypeName = hint.Type.GetFullName();
      var currentType = currentModel.Types.SingleOrDefault(type =>
        type.UnderlyingType==currentTypeName);
      if (currentType==null)
        throw TypeNotFound(currentTypeName);
      var currentField = currentType.AllFields
        .SingleOrDefault(field => field.Name==hint.FieldName);
      if (currentField==null)
        throw FieldNotFound(currentTypeName, hint.FieldName);
      var affectedColumns = GetAffectedColumns(currentType, currentField);
      hint.AffectedColumns = new ReadOnlyList<string>(affectedColumns);
    }

    private void UpdateAffectedColumns(RemoveFieldHint hint)
    {
      if (hint.IsExplicit)
        return;

      var typeName = hint.Type;
      var storedType = extractedModel.Types.SingleOrDefault(type => type.UnderlyingType==typeName);
      if (storedType==null)
        throw TypeNotFound(typeName);

      StoredFieldInfo storedField = null;
      // Nested field, looks like a field of a structure
      if (hint.Field.Contains(".")) {
        string[] path = hint.Field.Split('.');
        var fields = storedType.AllFields;
        string fieldName = string.Empty;
        for (int i = 0; i < path.Length; i++) {
          fieldName += string.IsNullOrEmpty(fieldName) ? path[i] : "." + path[i];
          string parameter = fieldName;
          storedField = fields.SingleOrDefault(field => field.Name==parameter);
          if (storedField==null)
            throw FieldNotFound(typeName, hint.Field);
          fields = storedField.Fields;
        }
      }
      else
        storedField = storedType.AllFields
          .SingleOrDefault(field => field.Name==hint.Field);

      if (storedField==null)
        throw FieldNotFound(typeName, hint.Field);
      var affectedColumns = GetAffectedColumns(storedType, storedField);
      hint.AffectedColumns = new ReadOnlyList<string>(affectedColumns);
    }

    #endregion

    #region Helpers

    private bool IsRemoved(StoredTypeInfo type)
    {
      return !typeMapping.ContainsKey(type);
    }

    private bool IsRemoved(StoredFieldInfo field)
    {
      return !fieldMapping.ContainsKey(field);
    }

    private bool IsMovedToAnotherHierarchy(StoredTypeInfo oldType)
    {
      var newType = typeMapping.GetValueOrDefault(oldType);
      if (newType==null)
        return false; // Type is removed
      var oldRoot = oldType.Hierarchy.Root;
      if (oldRoot==null)
        return false; // Just to be sure
      var newRoot = newType.Hierarchy.Root;
      if (newRoot==null)
        return false; // Just to be sure
      return newRoot!=typeMapping.GetValueOrDefault(oldRoot);
    }

    private void RegisterRenameTableHint(StoredTypeInfo oldType, StoredTypeInfo newType)
    {
      if (EnsureTableExist(oldType))
        schemaHints.Add(new RenameHint(GetTablePath(oldType), GetTablePath(newType)));
    }

    private void RegisterRenameFieldHint(
      StoredTypeInfo oldType, StoredTypeInfo newType, string oldColumnName, string newColumnName)
    {
      if (EnsureTableExist(oldType) && EnsureFieldExist(oldType, oldColumnName))
        schemaHints.Add(new RenameHint(
          GetColumnPath(oldType, oldColumnName),
          GetColumnPath(newType, newColumnName)));
    }

    private bool EnsureTableExist(StoredTypeInfo type)
    {
      var nodeName = GetTableName(type);

      if (extractedStorageModel.Tables.Contains(nodeName))
        return true;

      UpgradeLog.Warning(Strings.ExTableXIsNotFound, nodeName);
      return false;
    }

    private bool EnsureFieldExist(StoredTypeInfo type, string fieldName)
    {
      if (!EnsureTableExist(type))
        return false;
      var nodeName = GetTableName(type);
      var actualFieldName = nameBuilder.ApplyNamingRules(fieldName);
      if (extractedStorageModel.Tables[nodeName].Columns.Contains(actualFieldName))
        return true;
      UpgradeLog.Warning(Strings.ExColumnXIsNotFoundInTableY, actualFieldName, nodeName);
      return false;
    }

    private List<string> GetAffectedColumns(StoredTypeInfo type, StoredFieldInfo field)
    {
      var affectedColumns = new List<string>();

      if (type.IsStructure) {
        var structureFields = extractedModel.Types
          .Where(t => t.IsEntity)
          .SelectMany(t => extractedModelFields[t].Where(f => f.IsStructure && f.ValueType==type.UnderlyingType));

        foreach (var structureField in structureFields) {
          var nestedField = structureField.Fields.FirstOrDefault(f => f.OriginalName==field.Name);
          if (nestedField!=null)
            affectedColumns.AddRange(GetAffectedColumns(structureField.DeclaringType, nestedField));
        }

        return affectedColumns;
      }

      foreach (var primitiveField in field.PrimitiveFields) {
        var inheritanceSchema = type.Hierarchy.InheritanceSchema;
        switch (inheritanceSchema) {
          case InheritanceSchema.ClassTable:
            affectedColumns.Add(GetColumnPath(primitiveField.DeclaringType, primitiveField.MappingName));
            break;
          case InheritanceSchema.SingleTable:
            affectedColumns.Add(GetColumnPath(type.Hierarchy.Root, primitiveField.MappingName));
            break;
          case InheritanceSchema.ConcreteTable:
            var columns = GetAffectedMappedTypes(type, true)
              .Select(t => GetColumnPath(t, primitiveField.MappingName));
            affectedColumns.AddRange(columns);
            break;
          default:
            throw Exceptions.InternalError(String.Format(Strings.ExInheritanceSchemaIsInvalid, inheritanceSchema), UpgradeLog.Instance);
        }
      }
      return affectedColumns;
    }

    private static IEnumerable<StoredTypeInfo> GetNonConnectorTypes(StoredDomainModel model)
    {
      var connectorTypes = (
        from association in model.Associations
        let type = association.ConnectorType
        where type != null
        select type
        ).ToHashSet();
      return model.Types.Where(type => !connectorTypes.Contains(type));
    }

    private List<StoredTypeInfo> GetRemovedTypes(StoredDomainModel model)
    {
      return (
        from type in GetNonConnectorTypes(model)
        where type.IsEntity && (!type.IsAbstract) && (!type.IsGeneric) && (!type.IsInterface)
        where IsRemoved(type)
        select type
        ).ToList();
    }

    private List<StoredTypeInfo> GetMovedTypes(StoredDomainModel model)
    {
      return (
        from type in GetNonConnectorTypes(model)
        where type.IsEntity && (!type.IsAbstract) && (!type.IsGeneric) && (!type.IsInterface)
        where IsMovedToAnotherHierarchy(type)
        select type
        ).ToList();
    }

    private static IEnumerable<StoredTypeInfo> GetAffectedMappedTypes(StoredTypeInfo type, bool includeInheritors)
    {
      var result = EnumerableUtils.One(type);
      if (includeInheritors)
        result = result.Concat(type.AllDescendants);
      if (type.Hierarchy.InheritanceSchema==InheritanceSchema.ConcreteTable)
        result = result.Where(t => !t.IsAbstract);
      return result;
    }


    private string GetTableName(StoredTypeInfo type)
    {
      return resolver.GetNodeName(
        type.MappingDatabase, type.MappingSchema, type.MappingName);
    }

    private string GetTablePath(StoredTypeInfo type)
    {
      var nodeName = GetTableName(type);
      return string.Format("Tables/{0}", nodeName);
    }

    private string GetColumnPath(StoredTypeInfo type, string columnName)
    {
      var nodeName = GetTableName(type);
      // Due to current implementation of domain model FieldInfo.MappingName is not correct,
      // it has naming rules unapplied, corresponding ColumnInfo.Name however is correct.
      // StoredFieldInfo.MappingName is taken directly from FieldInfo.MappingName and thus is incorrect too.
      // We need to apply naming rules here to make it work.
      var actualColumnName = nameBuilder.ApplyNamingRules(columnName);
      return string.Format("Tables/{0}/Columns/{1}", nodeName, actualColumnName);
    }

    private static string GetTypeIdMappingName(StoredTypeInfo type)
    {
      var typeIdField = type.AllFields.Single(field => field.Name==WellKnown.TypeIdFieldName);
      return typeIdField.MappingName;
    }

    private static Pair<StoredFieldInfo>[] JoinFieldsByOriginalName(
      IEnumerable<StoredFieldInfo> sources, IEnumerable<StoredFieldInfo> targets)
    {
      var result =
        from source in sources
        join target in targets
          on source.OriginalName equals target.OriginalName
        select new Pair<StoredFieldInfo>(source, target);
      return result.ToArray();
    }

    private static Pair<string>[] AssociateMappedKeyFields(
      StoredHierarchyInfo sourceHierarchy,
      StoredHierarchyInfo targetHierarchy)
    {
      var sourceKeyFields = sourceHierarchy.Root.Fields
        .Where(field => field.IsPrimaryKey)
        .ToArray();
      var targetKeyFields = targetHierarchy.Root.Fields
        .Where(field => field.IsPrimaryKey)
        .ToArray();
      if (sourceKeyFields.Length != targetKeyFields.Length)
        return null;
      var pairedKeyFields = JoinFieldsByOriginalName(sourceKeyFields, targetKeyFields);
      if (pairedKeyFields.Length != sourceKeyFields.Length)
        return null;
      return AssociateMappedFields(pairedKeyFields);
    }

    private static Pair<string>[] AssociateMappedFields(params Pair<StoredFieldInfo>[] fieldsToProcess)
    {
      var result = new List<Pair<StoredFieldInfo>>();
      var tasks = new Queue<Pair<StoredFieldInfo>>();
      foreach (var task in fieldsToProcess)
        tasks.Enqueue(task);
      while (tasks.Count > 0) {
        var task = tasks.Dequeue();
        var source = task.First;
        var target = task.Second;
        // both fields are primitive -> put to result is types match
        if (source.IsPrimitive && target.IsPrimitive) {
          if (source.ValueType!=target.ValueType)
            return null;
          result.Add(task);
          continue;
        }
        // exactly one of the fields is primitive -> failure
        if (source.IsPrimitive || target.IsPrimitive)
          return null;
        // both fields are not primitive -> recursively process nested fields
        if (source.Fields.Length != target.Fields.Length)
          return null;
        var pairedNestedFields = JoinFieldsByOriginalName(source.Fields, target.Fields);
        if (pairedNestedFields.Length != source.Fields.Length)
          return null;
        foreach (var newTask in pairedNestedFields)
          tasks.Enqueue(newTask);
      }
      return result
        .Select(mapping => new Pair<string>(mapping.First.MappingName, mapping.Second.MappingName))
        .ToArray();
    }

    #endregion

    #region Exception helpers

    private static InvalidOperationException TypeNotFound(string name)
    {
      return new InvalidOperationException(string.Format(Strings.ExTypeXIsNotFound, name));
    }

    private static InvalidOperationException FieldNotFound(string typeName, string fieldName)
    {
      return new InvalidOperationException(string.Format(Strings.ExFieldXYIsNotFound, typeName, fieldName));
    }

    private static InvalidOperationException HintConflict(UpgradeHint hintOne, UpgradeHint hintTwo)
    {
      return new InvalidOperationException(string.Format(Strings.ExHintXIsConflictingWithHintY, hintOne, hintTwo));
    }

    private static InvalidOperationException KeysDoNotMatch(string typeOne, string typeTwo)
    {
      return new InvalidOperationException(string.Format(Strings.ExKeyOfXDoesNotMatchKeyOfY, typeOne, typeTwo));
    }

    private static InvalidOperationException FieldsDoNotMatch(StoredFieldInfo fieldOne, StoredFieldInfo fieldTwo)
    {
      var nameOne = fieldOne.DeclaringType.UnderlyingType + "." + fieldOne.Name;
      var nameTwo = fieldTwo.DeclaringType.UnderlyingType + "." + fieldTwo.Name;
      return new InvalidOperationException(string.Format(
        Strings.ExStructureOfFieldXDoesNotMatchStructureOfFieldY, nameOne, nameTwo));
    }

    private static InvalidOperationException TypeIsNotInHierarchy(string type)
    {
      return new InvalidOperationException(string.Format(Strings.ExTypeXMustBelongToHierarchy, type));
    }

    #endregion


    // Constructors

    public HintGenerator(Dictionary<StoredTypeInfo, StoredTypeInfo> typeMapping,
      Dictionary<StoredTypeInfo, StoredTypeInfo> reverseTypeMapping,
      Dictionary<StoredFieldInfo, StoredFieldInfo> fieldMapping,
      NativeTypeClassifier<UpgradeHint> hints,
      HandlerAccessor handlers,
      MappingResolver resolver,
      StorageModel extractedStorageModel,
      StoredDomainModel currentDomainModel,
      StoredDomainModel extractedDomainModel)
    {
      ArgumentValidator.EnsureArgumentNotNull(typeMapping, "typeMapping");
      ArgumentValidator.EnsureArgumentNotNull(reverseTypeMapping, "reverseTypeMapping");
      ArgumentValidator.EnsureArgumentNotNull(fieldMapping, "fieldMapping");
      ArgumentValidator.EnsureArgumentNotNull(hints, "hints");
      ArgumentValidator.EnsureArgumentNotNull(handlers, "handlers");
      ArgumentValidator.EnsureArgumentNotNull(resolver, "resolver");
      ArgumentValidator.EnsureArgumentNotNull(extractedStorageModel, "extractedStorageModel");
      ArgumentValidator.EnsureArgumentNotNull(currentDomainModel, "currentDomainModel");
      ArgumentValidator.EnsureArgumentNotNull(extractedDomainModel, "extractedDomainModel");

      this.typeMapping = typeMapping;
      this.reverseTypeMapping = reverseTypeMapping;
      this.fieldMapping = fieldMapping;
      this.hints = hints;

      extractedModelFields = new Dictionary<StoredTypeInfo, StoredFieldInfo[]>();

      this.extractedStorageModel = extractedStorageModel;
      this.resolver = resolver;

      nameBuilder = handlers.NameBuilder;

      currentModel = currentDomainModel;
      currentModel.UpdateReferences();

      extractedModel = extractedDomainModel;

      foreach (var type in extractedModel.Types)
        extractedModelFields.Add(type, type.Fields.Flatten(f => f.Fields, null, true).ToArray());
    }
  }
}
