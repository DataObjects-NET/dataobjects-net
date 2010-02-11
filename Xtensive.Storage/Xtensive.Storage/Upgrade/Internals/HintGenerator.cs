// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.04

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Model;
using Xtensive.Storage.Model.Stored;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Building;

namespace Xtensive.Storage.Upgrade
{
  internal sealed class HintGenerator
  {
    private readonly StoredDomainModel storedModel;
    private readonly StoredDomainModel currentModel;
    private readonly StorageInfo extractedModel;
    
    private readonly Dictionary<StoredTypeInfo, StoredTypeInfo> typeMapping
      = new Dictionary<StoredTypeInfo, StoredTypeInfo>();
    private readonly Dictionary<StoredTypeInfo, StoredTypeInfo> backwardTypeMapping
      = new Dictionary<StoredTypeInfo, StoredTypeInfo>();
    private readonly Dictionary<StoredFieldInfo, StoredFieldInfo> fieldMapping
      = new Dictionary<StoredFieldInfo, StoredFieldInfo>();
    private readonly Dictionary<StoredFieldInfo, StoredFieldInfo> backwardFieldMapping
      = new Dictionary<StoredFieldInfo, StoredFieldInfo>();
    
    private readonly List<Hint> schemaHints = new List<Hint>();

    public HintGenerationResult GenerateHints(IEnumerable<UpgradeHint> upgradeHints)
    {
      var proccessedHints = TranslateGenericTypeHints(upgradeHints).ToList();
      
      var typeRenames = proccessedHints.OfType<RenameTypeHint>().ToArray();
      ValidateRenameTypeHints(typeRenames);
      BuildTypeMapping(typeRenames);
      
      var fieldRenames = proccessedHints.OfType<RenameFieldHint>().ToArray();
      ValidateRenameFieldHints(fieldRenames);
      BuildFieldMapping(fieldRenames);
      BuildConnectorTypeMapping();
      
      var removeTypeHints = proccessedHints.OfType<RemoveTypeHint>().ToArray();
      ValidateRemoveTypeHints(removeTypeHints);
      proccessedHints.AddRange(UpdateRemoveTypeHints(removeTypeHints));

      var moveFieldHints = proccessedHints.OfType<MoveFieldHint>().ToArray();
      proccessedHints.AddRange(TransalateMoveFieldHints(moveFieldHints));
      proccessedHints.AddRange(GenerateTypeIdRemovalHints());
      
      var removeFieldHints = proccessedHints.OfType<RemoveFieldHint>().ToArray();
      ValidateRemoveFieldHints(removeFieldHints);
      UpdateRemoveFieldHints(removeFieldHints);

      GenerateRenameTableHints();
      GenerateRenameColumnHints();

      var fieldCopyHints = proccessedHints.OfType<CopyFieldHint>().ToArray();
      ValidateCopyFieldHints(fieldCopyHints);
      GenerateCopyColumnHints(fieldCopyHints);

      GenerateClearDataHints();

      var changeTypeHints = proccessedHints.OfType<ChangeFieldTypeHint>().ToArray();
      UpdateChangeFieldTypeHints(changeTypeHints);

      return new HintGenerationResult(proccessedHints, schemaHints);
    }

    #region Validation

    private void ValidateRenameTypeHints(IEnumerable<RenameTypeHint> hints)
    {
      var sourceTypes = new Dictionary<string, RenameTypeHint>();
      var targetTypes = new Dictionary<Type, RenameTypeHint>();
      foreach (var hint in hints) {
        var newTypeName = hint.NewType.GetFullName();
        var oldTypeName = hint.OldType;
        // checking that types exists in models
        if (!currentModel.Types.Any(type => type.UnderlyingType==newTypeName))
          throw TypeIsNotFound(hint.NewType.GetFullName());
        if (!storedModel.Types.Any(type => type.UnderlyingType==oldTypeName))
          throw TypeIsNotFound(hint.OldType);
        // each original type should be used only once
        // each result type should be used only once
        RenameTypeHint evilHint;
        if (sourceTypes.TryGetValue(hint.OldType, out evilHint))
          throw HintConflict(hint, evilHint);
        if (targetTypes.TryGetValue(hint.NewType, out evilHint))
          throw HintConflict(hint, evilHint);
        sourceTypes.Add(hint.OldType, hint);
        targetTypes.Add(hint.NewType, hint);
      }
    }
    
    private void ValidateRenameFieldHints(IEnumerable<RenameFieldHint> hints)
    {
      var sourceFields = new Dictionary<Pair<Type, string>, RenameFieldHint>();
      var targetFields = new Dictionary<Pair<Type, string>, RenameFieldHint>();
      foreach (var hint in hints) {
        // checking that both target and source fields exists in models
        var targetTypeName = hint.TargetType.GetFullName();
        var targetType = currentModel.Types.SingleOrDefault(type => type.UnderlyingType==targetTypeName);
        if (targetType==null)
          throw TypeIsNotFound(targetTypeName);
        var sourceType = backwardTypeMapping[targetType];
        var sourceTypeName = sourceType.UnderlyingType;
        if (!sourceType.Fields.Any(field => field.Name==hint.OldFieldName))
          throw FieldIsNotFound(sourceTypeName, hint.OldFieldName);
        if (!targetType.Fields.Any(field => field.Name==hint.NewFieldName))
          throw FieldIsNotFound(targetTypeName, hint.NewFieldName);
        // each source field should be used only once
        // each destination field should be used only once
        RenameFieldHint evilHint;
        var sourceField = new Pair<Type, string>(hint.TargetType, hint.OldFieldName);
        var targetField = new Pair<Type, string>(hint.TargetType, hint.NewFieldName);
        if (sourceFields.TryGetValue(sourceField, out evilHint))
          throw HintConflict(hint, evilHint);
        if (targetFields.TryGetValue(targetField, out evilHint))
          throw HintConflict(hint, evilHint);
        sourceFields.Add(sourceField, hint);
        targetFields.Add(targetField, hint);
      }
    }

    private void ValidateCopyFieldHints(IEnumerable<CopyFieldHint> hints)
    {
      foreach (var hint in hints) {
        // checking source type/field
        var sourceTypeName = hint.SourceType;
        var sourceType = storedModel.Types.SingleOrDefault(type => type.UnderlyingType==sourceTypeName);
        if (sourceType==null)
          throw TypeIsNotFound(sourceTypeName);
        if (!sourceType.AllFields.Any(field => field.Name==hint.SourceField))
          throw FieldIsNotFound(sourceTypeName, hint.SourceField);
        // checking destination type/field
        var targetTypeName = hint.TargetType.GetFullName();
        var targetType = currentModel.Types.SingleOrDefault(type => type.UnderlyingType==targetTypeName);
        if (targetType==null)
          throw TypeIsNotFound(targetTypeName);
        if (!targetType.AllFields.Any(field => field.Name==hint.TargetField))
          throw FieldIsNotFound(targetTypeName, hint.TargetField);
      }
    }

    private void ValidateRemoveFieldHints(IEnumerable<RemoveFieldHint> hints)
    {
      foreach (var hint in hints) {
        // checking source type/field
        var sourceTypeName = hint.Type;
        var sourceType = storedModel.Types.SingleOrDefault(type => type.UnderlyingType==sourceTypeName);
        if (sourceType==null)
          throw TypeIsNotFound(sourceTypeName);
        if (!sourceType.AllFields.Any(field => field.Name==hint.Field))
          throw FieldIsNotFound(sourceTypeName, hint.Field);
      }
    }

    private void ValidateRemoveTypeHints(IEnumerable<RemoveTypeHint> hints)
    {
      foreach (var hint in hints) {
        // checking source type
        var sourceTypeName = hint.Type;
        var sourceType = storedModel.Types.SingleOrDefault(type => type.UnderlyingType==sourceTypeName);
        if (sourceType==null)
          throw TypeIsNotFound(sourceTypeName);
      }
    }

    #endregion

    #region Map

    private void MapType(StoredTypeInfo oldType, StoredTypeInfo newType)
    {
      if (typeMapping.ContainsKey(oldType))
        throw new InvalidOperationException(String.Format(Strings.ExTypeMappingDoesNotContainXType, oldType));
      typeMapping[oldType] = newType;
      backwardTypeMapping[newType] = oldType;
    }

    private void MapField(StoredFieldInfo oldField, StoredFieldInfo newField)
    {
      if (fieldMapping.ContainsKey(oldField))
        throw new InvalidOperationException(String.Format(Strings.ExFieldMappingDoesNotContainField, oldField));
      fieldMapping[oldField] = newField;
      backwardFieldMapping[newField] = oldField;
    }

    private void MapNestedFields(StoredFieldInfo oldField, StoredFieldInfo newField)
    {
      var oldNestedFields = ((IEnumerable<StoredFieldInfo>) oldField.Fields).ToArray();
      if (oldNestedFields.Length==0)
        return;
      var oldValueType = storedModel.Types
        .Single(type => type.UnderlyingType==oldField.ValueType);
      foreach (var oldNestedField in oldNestedFields) {
        var oldNestedFieldOriginalName = oldNestedField.OriginalName;
        var oldNestedFieldOrigin = oldValueType.AllFields
          .Single(field => field.Name==oldNestedFieldOriginalName);
        var newNestedFieldOrigin = fieldMapping[oldNestedFieldOrigin];
        var newNestedField = newField.Fields
          .Single(field => field.OriginalName==newNestedFieldOrigin.Name);
        MapFieldRecursively(oldNestedField, newNestedField);
      }
    }

    private void MapFieldRecursively(StoredFieldInfo oldField, StoredFieldInfo newField)
    {
      MapField(oldField, newField);
      MapNestedFields(oldField, newField);
    }
    
    private void BuildTypeMapping(IEnumerable<RenameTypeHint> renames)
    {
      var oldConnectorTypes = storedModel.Associations
        .Select(association => association.ConnectorType)
        .Where(type => type!=null)
        .ToHashSet();
      var newConnectorTypes = currentModel.Associations
        .Select(association => association.ConnectorType)
        .Where(type => type!=null)
        .ToHashSet();

      var oldModelTypes = storedModel.Types
        .Where(type => !oldConnectorTypes.Contains(type));
      var newModelTypes = currentModel.Types
        .Where(type => !newConnectorTypes.Contains(type))
        .ToDictionary(type => type.UnderlyingType);

      foreach (var oldType in oldModelTypes) {
        var maybeHint = renames
          .FirstOrDefault(hint => hint.OldType==oldType.UnderlyingType);
        var newTypeName = maybeHint!=null
          ? maybeHint.NewType.GetFullName()
          : oldType.UnderlyingType;
        StoredTypeInfo newType;
        if (newModelTypes.TryGetValue(newTypeName, out newType))
          MapType(oldType, newType);
      }
    }

    private void BuildFieldMapping(IEnumerable<RenameFieldHint> renames)
    {
      foreach (var pair in typeMapping)
        BuildFieldMapping(renames, pair.Key, pair.Value);

      foreach (var pair in fieldMapping.ToList())
        MapNestedFields(pair.Key, pair.Value);
    }

    private void BuildFieldMapping(IEnumerable<RenameFieldHint> renames, StoredTypeInfo oldType, StoredTypeInfo newType)
    {
      var newFields = newType.Fields.ToDictionary(field => field.Name);
      foreach (var oldField in oldType.Fields) {
        var maybeHint = renames
          .FirstOrDefault(hint => hint.OldFieldName==oldField.Name && hint.TargetType.GetFullName()==newType.UnderlyingType);
        var newFieldName = maybeHint!=null
          ? maybeHint.NewFieldName
          : oldField.Name;
        StoredFieldInfo newField;
        if (newFields.TryGetValue(newFieldName, out newField))
          MapField(oldField, newField);
      }
    }

    private void BuildConnectorTypeMapping()
    {
      var oldAssociations = storedModel.Associations
        .Where(association => association.ConnectorType!=null);
      foreach (var oldAssociation in oldAssociations) {
        if (typeMapping.ContainsKey(oldAssociation.ConnectorType))
          continue;
        var oldReferencingField = oldAssociation.ReferencingField;
        var oldReferencingType = oldReferencingField.DeclaringType;
        StoredTypeInfo newReferencingType;
        if (!typeMapping.TryGetValue(oldReferencingType, out newReferencingType))
          continue;
        StoredFieldInfo newReferencingField;
        if (!fieldMapping.TryGetValue(oldReferencingField, out newReferencingField))
          newReferencingField = newReferencingType.Fields
            .SingleOrDefault(field => field.Name==oldReferencingField.Name);
        if (newReferencingField==null)
          continue;
        var newAssociation = currentModel.Associations
          .Single(association => association.ReferencingField==newReferencingField);
          
        MapType(oldAssociation.ConnectorType, newAssociation.ConnectorType);

        var oldMaster = oldAssociation.ConnectorType.AllFields
          .Single(field => field.Name==WellKnown.MasterFieldName);
        var newMaster = newAssociation.ConnectorType.AllFields
          .Single(field => field.Name==WellKnown.MasterFieldName);
        var oldSlave = oldAssociation.ConnectorType.AllFields
          .Single(field => field.Name==WellKnown.SlaveFieldName);
        var newSlave = newAssociation.ConnectorType.AllFields
          .Single(field => field.Name==WellKnown.SlaveFieldName);

        MapFieldRecursively(oldMaster, newMaster);
        MapFieldRecursively(oldSlave, newSlave);
      }
    }

    #endregion

    #region Hint translation

    private IEnumerable<UpgradeHint> TranslateGenericTypeHints(IEnumerable<UpgradeHint> hints)
    {
      var renameTypeHints = hints.OfType<RenameTypeHint>();
      var renameGenericTypeHints = renameTypeHints.Where(hint => hint.NewType.IsGenericTypeDefinition);
      var renameFieldHints = hints.OfType<RenameFieldHint>().Where(hint => hint.TargetType.IsGenericTypeDefinition);

      // Build generic types mapping
      var genericTypesMapping = new Dictionary<Pair<string, Type>, List<Pair<string, Type>>>();
      var oldGenericTypes = GetGenericTypes(storedModel);
      var newGenericTypes = GetGenericTypes(BuildingContext.Demand().Model);
      foreach (var oldGenericDefName in oldGenericTypes.Keys) {
        var newGenericDefType = GetNewType(oldGenericDefName, newGenericTypes.Keys, renameTypeHints);
        if (newGenericDefType==null)
          continue;
        var genericArgumentsMapping = new List<Pair<string, Type>>();
        foreach (var oldGenericArgumentName in oldGenericTypes[oldGenericDefName]) {
          var newGenericArgumentType = GetNewType(oldGenericArgumentName, newGenericTypes[newGenericDefType], renameTypeHints);
          if (newGenericArgumentType!=null)
            genericArgumentsMapping.Add(new Pair<string, Type>(oldGenericArgumentName, newGenericArgumentType));
        }
        if (genericArgumentsMapping.Count > 0)
          genericTypesMapping.Add(new Pair<string, Type>(oldGenericDefName, newGenericDefType), genericArgumentsMapping);
      }

      // Build rename generic type hints
      var newRenameHints = new List<UpgradeHint>();
      foreach (var genericTypePair in genericTypesMapping)
        foreach (var genericArgumentPair in genericTypePair.Value) {
          var oldTypeFullName = GetGenericTypeFullName(genericTypePair.Key.First, genericArgumentPair.First);
          var newType = genericTypePair.Key.Second.MakeGenericType(genericArgumentPair.Second);
          if (oldTypeFullName != newType.GetFullName())
            newRenameHints.Add(new RenameTypeHint(oldTypeFullName, newType));
        }
      
      // Build rename generic type field hints
      foreach (var hint in renameFieldHints) {
        var newGenericDefType = hint.TargetType;
        var genericTypePair = genericTypesMapping.Keys.FirstOrDefault(pair => pair.Second==newGenericDefType);
        var oldGenericTypeName = genericTypePair.First;
        if (oldGenericTypeName==null)
          continue;
        var genericArgumentMapping = genericTypesMapping[genericTypePair];
        foreach (var pair in genericArgumentMapping)
          newRenameHints.Add(new RenameFieldHint(newGenericDefType.MakeGenericType(pair.Second), 
            hint.OldFieldName, hint.NewFieldName));
      }

      // Return new hint set
      return hints
        .Except(renameGenericTypeHints.Cast<UpgradeHint>())
        .Except(renameFieldHints.Cast<UpgradeHint>())
        .Concat(newRenameHints);
    }

    private IEnumerable<UpgradeHint> TransalateMoveFieldHints(IEnumerable<MoveFieldHint> moveFieldHints)
    {
      foreach (var hint in moveFieldHints) {
        yield return new CopyFieldHint(hint.SourceType, hint.SourceField, hint.TargetType, hint.TargetField);
        yield return new RemoveFieldHint(hint.SourceType, hint.SourceField);
      }
    }

    private IEnumerable<UpgradeHint> UpdateRemoveTypeHints(IEnumerable<RemoveTypeHint> removeTypeHints)
    {
      var resultSet = new List<UpgradeHint>();
      foreach (var removeTypeHint in removeTypeHints)
        resultSet.AddRange(UpdateRemoveTypeHint(removeTypeHint));
      return resultSet;
    }

    private void UpdateChangeFieldTypeHints(IEnumerable<ChangeFieldTypeHint> changeFieldTypeHints)
    {
      foreach (var hint in changeFieldTypeHints)
        UpdateChangeFieldTypeHint(hint);
    }

    private void UpdateRemoveFieldHints(IEnumerable<RemoveFieldHint> removeFieldHints)
    {
      foreach (var hint in removeFieldHints)
        UpdateRemoveFieldHint(hint);
    }

    private IEnumerable<UpgradeHint> UpdateRemoveTypeHint(RemoveTypeHint hint)
    {
      var affectedTables = new List<string>();
      var typeName = hint.Type;
      var storedType = storedModel.Types.SingleOrDefault(type =>
        type.UnderlyingType==typeName);
      if (storedType==null)
        throw TypeIsNotFound(typeName);
      var inheritanceSchema = storedType.Hierarchy.Schema;
      var removeFieldHints = new List<UpgradeHint>();

      switch (inheritanceSchema) {
        case InheritanceSchema.ClassTable:
          foreach (var fieldInfo in storedType.Fields.Where(field => field.Fields.Length==0))
            removeFieldHints.Add(new RemoveFieldHint(storedType.UnderlyingType, fieldInfo.Name));
          affectedTables.Add(GetTablePath(storedType.MappingName));
          break;
        case InheritanceSchema.SingleTable:
          foreach (var fieldInfo in storedType.Fields.Where(field => field.Fields.Length==0))
            removeFieldHints.Add(new RemoveFieldHint(storedType.UnderlyingType, fieldInfo.Name));
          affectedTables.Add(GetTablePath(storedType.Hierarchy.Root.MappingName));
          break;
        case InheritanceSchema.ConcreteTable:
          var typeToProcess = GetAffectedMappedTypes(storedType,
            storedType.Hierarchy.Schema==InheritanceSchema.ConcreteTable);
          foreach (var typeInfo in typeToProcess) {
            foreach (var fieldInfo in storedType.Fields.Where(field => field.Fields.Length==0))
              removeFieldHints.Add(new RemoveFieldHint(typeInfo.UnderlyingType, fieldInfo.Name));
          }
          affectedTables.AddRange(
            typeToProcess.Select(type => GetTablePath(type.MappingName)));
          break;
        default:
          throw Exceptions.InternalError(String.Format(
            Strings.ExInheritanceSchemaIsInvalid, inheritanceSchema), Log.Instance);
      }
      hint.AffectedTables = new ReadOnlyList<string>(affectedTables);
      return removeFieldHints;
    }

    private void UpdateChangeFieldTypeHint(ChangeFieldTypeHint hint)
    {
      var affectedColumns = new List<string>();
      var currentTypeName = hint.Type.GetFullName();
      var currentType = currentModel.Types.SingleOrDefault(type => 
        type.UnderlyingType==currentTypeName);
      if (currentType==null)
        throw TypeIsNotFound(currentTypeName);
      var currentField = currentType.AllFields
        .SingleOrDefault(field => field.Name==hint.FieldName);
      if (currentField==null)
        throw FieldIsNotFound(currentTypeName, hint.FieldName);
      var inheritanceSchema = currentType.Hierarchy.Schema;
      
      switch (inheritanceSchema) {
      case InheritanceSchema.ClassTable:
        affectedColumns.Add(GetColumnPath(currentField.DeclaringType.MappingName, currentField.MappingName));
        break;
      case InheritanceSchema.SingleTable:
        affectedColumns.Add(GetColumnPath(currentType.Hierarchy.Root.MappingName, currentField.MappingName));
        break;
      case InheritanceSchema.ConcreteTable:
        var typeToProcess = GetAffectedMappedTypes(currentType,
          currentType.Hierarchy.Schema==InheritanceSchema.ConcreteTable);
        affectedColumns.AddRange(
          typeToProcess.Select(type => GetColumnPath(type.MappingName, currentField.MappingName)));
        break;
      default:
          throw Exceptions.InternalError(String.Format(
            Strings.ExInheritanceSchemaIsInvalid, inheritanceSchema), Log.Instance);
      }
      hint.AffectedColumns = new ReadOnlyList<string>(affectedColumns);
    }

    private void UpdateRemoveFieldHint(RemoveFieldHint hint)
    {
      var affectedColumns = new List<string>();
      var typeName = hint.Type;
      var storedType = storedModel.Types.SingleOrDefault(type => type.UnderlyingType==typeName);
      if (storedType==null)
        throw TypeIsNotFound(typeName);
      var storedField = storedType.AllFields
        .SingleOrDefault(field => field.Name==hint.Field);
      if (storedField==null)
        throw FieldIsNotFound(typeName, hint.Field);
      foreach (var primitiveField in storedField.PrimitiveFields) {
        var inheritanceSchema = storedType.Hierarchy.Schema;
        switch (inheritanceSchema) {
          case InheritanceSchema.ClassTable:
            affectedColumns.Add(
              GetColumnPath(primitiveField.DeclaringType.MappingName, primitiveField.MappingName));
            break;
          case InheritanceSchema.SingleTable:
            affectedColumns.Add(
              GetColumnPath(storedType.Hierarchy.Root.MappingName, primitiveField.MappingName));
            break;
          case InheritanceSchema.ConcreteTable:
            var typeToProcess = GetAffectedMappedTypes(
              storedType,
              storedType.Hierarchy.Schema == InheritanceSchema.ConcreteTable);
            affectedColumns.AddRange(
              typeToProcess.Select(type => GetColumnPath(type.MappingName, primitiveField.MappingName)));
            break;
          default:
            throw Exceptions.InternalError(String.Format(Strings.ExInheritanceSchemaIsInvalid, inheritanceSchema), Log.Instance);
        }
      }
      hint.AffectedColumns = new ReadOnlyList<string>(affectedColumns);
    }
    
    #endregion
    
    #region Hint generation

    private IEnumerable<UpgradeHint> GenerateTypeIdRemovalHints()
    {
      var result = new List<UpgradeHint>();
      var types = 
        from p in typeMapping
        let sourceHierarchy = p.Key.Hierarchy
        let targetHierarchy = p.Value.Hierarchy
        where
          targetHierarchy != null && sourceHierarchy != null &&
          targetHierarchy.Schema == InheritanceSchema.ConcreteTable &&
          targetHierarchy.Types.Length == 1
        select p.Key;
      foreach (var type in types) {
        var typeIdField = type.Fields.Single(f => f.IsTypeId);
        var targetType = typeMapping[type];
        var targetTypeIdField = targetType.Fields.Single(f => f.IsTypeId);
        if (targetTypeIdField.IsPrimaryKey)
          continue;
        if (!extractedModel.Tables[type.MappingName].Columns.Contains(typeIdField.MappingName)) 
          continue;
        var hint = new RemoveFieldHint(type.UnderlyingType, typeIdField.Name);
        result.Add(hint);
      }
      return result;
    }

    private void GenerateRenameTableHints()
    {
      var mappingsToProcess = typeMapping
        .Where(type => type.Key.IsEntity)
        .Where(type => type.Key.Hierarchy.Schema!=InheritanceSchema.SingleTable || type.Key.Hierarchy.Root==type.Key);
      foreach (var mapping in mappingsToProcess) {
        var oldTable = mapping.Key.MappingName;
        var newTable = mapping.Value.MappingName;
        if (oldTable != newTable)
          RegisterRenameTableHint(oldTable, newTable);
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
        switch (newType.Hierarchy.Schema) {
        case InheritanceSchema.ClassTable:
          // rename column in inheritors only when field is a key
          GenerateRenameFieldHint(oldField, newField, newType, newField.IsPrimaryKey);
          break;
        case InheritanceSchema.SingleTable:
          // always rename only one column
          GenerateRenameFieldHint(oldField, newField, newType, false);
          break;
        case InheritanceSchema.ConcreteTable:
          // rename column in all inheritors and type itself
          GenerateRenameFieldHint(oldField, newField, newType, true);
          break;
        default:
          throw Exceptions.InternalError(String.Format(Strings.ExInheritanceSchemaIsInvalid, newType.Hierarchy.Schema), Log.Instance);
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
        if (!backwardTypeMapping.TryGetValue(newTargetType, out oldTargetType))
          continue;
        RegisterRenameFieldHint(oldTargetType.MappingName, newTargetType.MappingName,
          oldField.MappingName, newField.MappingName);
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
      var targetType = currentModel.Types.Single(type => type.UnderlyingType==targetTypeName);
      var targetField = targetType.AllFields.Single(field => field.Name==hint.TargetField);
      var targetHierarchy = targetType.Hierarchy;

      var sourceTypeName = hint.SourceType;
      var sourceType = storedModel.Types.Single(type => type.UnderlyingType==sourceTypeName);
      var sourceField = sourceType.AllFields.Single(field => field.Name==hint.SourceField);
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
      var sourceTable = sourceType.MappingName;
      var targetTables = GetAffectedMappedTypes(targetType, targetHierarchy.Schema==InheritanceSchema.ConcreteTable)
        .Select(type => type.MappingName);

      // generating result hints
      foreach (var targetTable in targetTables) {
        var sourceTablePath = GetTablePath(sourceTable);
        var identities = new List<IdentityPair>();
        var copiedColumns = new List<Pair<string>>();
        foreach (var keyColumnPair in pairedKeyColumns)
          identities.Add(new IdentityPair(
            GetColumnPath(sourceTable, keyColumnPair.First),
            GetColumnPath(targetTable, keyColumnPair.Second),
            false));
        foreach (var columnPair in pairedColumns)
          copiedColumns.Add(new Pair<string>(
            GetColumnPath(sourceTable, columnPair.First),
            GetColumnPath(targetTable, columnPair.Second)));
        schemaHints.Add(new CopyDataHint(sourceTablePath, identities, copiedColumns));
      }
    }
    
    private void GenerateClearDataHints()
    {
      var removedTypes = storedModel.Types
        .Where(type => type.IsEntity && IsRemoved(type))
        .ToArray();

      removedTypes.Apply(GenerateClearHierarchyHints);
      removedTypes.Apply(GenerateClearReferencesHints);
    }

    private void GenerateClearHierarchyHints(StoredTypeInfo removedType)
    {
      var typesToProcess = new List<StoredTypeInfo>();
      switch (removedType.Hierarchy.Schema) {
      case InheritanceSchema.ClassTable:
        typesToProcess.AddRange(removedType.AllAncestors.Where(type => !IsRemoved(type)));
        break;
      case InheritanceSchema.SingleTable:
        if (!IsRemoved(removedType.Hierarchy.Root))
          typesToProcess.Add(removedType.Hierarchy.Root);
        break;
      case InheritanceSchema.ConcreteTable:
        break;
      default:
          throw Exceptions.InternalError(String.Format(Strings.ExInheritanceSchemaIsInvalid, removedType.Hierarchy.Schema), Log.Instance);
      }
      foreach (var type in typesToProcess) {
        var tableName = type.MappingName;
        var sourceTablePath = GetTablePath(tableName);
        var identities = new List<IdentityPair> {
          new IdentityPair(
            GetColumnPath(tableName, GetTypeIdMappingName(type)),
            removedType.TypeId.ToString(),
            true)
        };
        schemaHints.Add(new DeleteDataHint(sourceTablePath, identities));
      }
    }
    
    private void GenerateClearReferencesHints(StoredTypeInfo removedType)
    {
      var affectedAssociations = storedModel.Associations
        .Where(association => association.ReferencedType==removedType)
        .Concat(removedType.AllAncestors.SelectMany(ancestor =>
          storedModel.Associations
            .Where(association => association.ReferencedType==ancestor)));
      foreach (var association in affectedAssociations) {
        var typesToProcess = new List<StoredTypeInfo>();
        if (IsRemoved(association.ReferencingField))
          continue;
        if (association.ConnectorType==null) {
          var rootType = association.ReferencingField.DeclaringType;
          typesToProcess.AddRange(GetAffectedMappedTypes(rootType,
            rootType.Hierarchy.Schema==InheritanceSchema.ConcreteTable));
        }
        else
          typesToProcess.Add(association.ConnectorType);
        foreach (var currentType in typesToProcess.Where(type=>!IsRemoved(type)))
          GenerateClearReferenceHint(removedType, currentType, association);
      }
    }

    private void GenerateClearReferenceHint(StoredTypeInfo removedType, StoredTypeInfo updatedType,
      StoredAssociationInfo association)
    {
      var identityFieldsOfRemovedType = removedType.AllFields.Where(f => f.IsPrimaryKey).ToList();
      var identityFieldsOfUpdatedType = association.ConnectorType!=null
        ? association.ConnectorType.Fields
          .Single(field => field.Name==(association.IsMaster ? WellKnown.SlaveFieldName : WellKnown.MasterFieldName))
          .Fields
        : association.ReferencingField.Fields;
      var pairedIdentityFields = JoinFieldsByOriginalName(identityFieldsOfRemovedType, identityFieldsOfUpdatedType);
      var pairedIdentityColumns = AssociateMappedFields(pairedIdentityFields);
      if (pairedIdentityColumns==null)
        throw new InvalidOperationException(String.Format(Strings.ExPairedIdentityColumnsForTypesXAndXNotFound, removedType, updatedType));

      var sourceTablePath = GetTablePath(updatedType.MappingName);
      var identities = pairedIdentityColumns.Select(pair =>
        new IdentityPair(
          GetColumnPath(updatedType.MappingName, pair.Second),
          GetColumnPath(removedType.MappingName, pair.First), false))
        .ToList();
      if (removedType.Hierarchy.Schema!=InheritanceSchema.ConcreteTable)
        identities.Add(new IdentityPair(
          GetColumnPath(removedType.MappingName, GetTypeIdMappingName(removedType)),
          removedType.TypeId.ToString(), true));
      var updatedColumns = pairedIdentityColumns.Select(pair =>
        new Pair<string, object>(GetColumnPath(updatedType.MappingName, pair.Second), null))
        .ToList();

      if (association.ConnectorType==null)
        schemaHints.Add(new UpdateDataHint(sourceTablePath, identities, updatedColumns));
      else
        schemaHints.Add(new DeleteDataHint(sourceTablePath, identities));
    }
    
    private bool IsRemoved(StoredTypeInfo type)
    {
      return !typeMapping.ContainsKey(type);
    }

    private bool IsRemoved(StoredFieldInfo field)
    {
      return !fieldMapping.ContainsKey(field);
    }

    private void RegisterRenameTableHint(string oldTableName, string newTableName)
    {
      if (EnsureTableExist(oldTableName))
        schemaHints.Add(new RenameHint(GetTablePath(oldTableName), GetTablePath(newTableName)));
    }

    private void RegisterRenameFieldHint(string oldTableName, string newTableName, string oldColumnName, string newColumnName)
    {
      if (EnsureTableExist(oldTableName) && EnsureFieldExist(oldTableName, oldColumnName))
        schemaHints.Add(new RenameHint(
          GetColumnPath(oldTableName, oldColumnName),
          GetColumnPath(newTableName, newColumnName)));
        
    }

    private bool EnsureTableExist(string tableName)
    {
      if (!extractedModel.Tables.Contains(tableName)) {
        Log.Warning(Strings.ExTableXIsNotFound);
        return false;
      }
      return true;
    }

    private bool EnsureFieldExist(string tableName, string fieldName)
    {
      if (!EnsureTableExist(tableName))
        return false;
      if (!extractedModel.Tables[tableName].Columns.Contains(fieldName)) {
        Log.Warning(Strings.ExColumnXIsNotFoundInTableY, fieldName, tableName);
        return false;
      }
      return true;
    }

    #endregion
    
    #region Static helpers

    private static IEnumerable<StoredTypeInfo> GetAffectedMappedTypes(StoredTypeInfo type, bool includeInheritors)
    {
      var result = EnumerableUtils.One(type);
      if (includeInheritors)
        result = result.Concat(type.AllDescendants);
      if (type.Hierarchy.Schema==InheritanceSchema.ConcreteTable)
        result = result.Where(t => !t.IsAbstract);
      return result;
    }

    private static Type GetNewType(string oldTypeName, IEnumerable<Type> newTypes, IEnumerable<RenameTypeHint> hints)
    {
      var renameTypeHint = hints.FirstOrDefault(hint => hint.OldType==oldTypeName);
      return renameTypeHint != null 
        ? renameTypeHint.NewType 
        : newTypes.FirstOrDefault(type => type.GetFullName() == oldTypeName);
    }

    private static Dictionary<string, List<string>> GetGenericTypes(StoredDomainModel model)
    {
      var genericTypes = new Dictionary<string, List<string>>();
      foreach (var typeInfo in model.Types.Where(type => type.IsGeneric)) {
        List<string> argumentTypes;
        if (!genericTypes.TryGetValue(typeInfo.GenericDefinitionTypeName, out argumentTypes)) {
          argumentTypes = new List<string>();
          genericTypes.Add(typeInfo.GenericDefinitionTypeName, argumentTypes);
        }
        argumentTypes.Add(typeInfo.GenericArgumentTypeName);
      }
      return genericTypes;
    }

    private static Dictionary<Type, List<Type>> GetGenericTypes(DomainModel model)
    {
      var genericTypes = new Dictionary<Type, List<Type>>();
      foreach (var typeInfo in model.Types.Where(type => type.UnderlyingType.IsGenericType)) {
        var generifDefType = typeInfo.UnderlyingType.GetGenericTypeDefinition();
        List<Type> argumentTypes;
        if (!genericTypes.TryGetValue(generifDefType, out argumentTypes)) {
          argumentTypes = new List<Type>();
          genericTypes.Add(generifDefType, argumentTypes);
        }
        argumentTypes.Add(typeInfo.UnderlyingType.GetGenericArguments()[0]);
      }
      return genericTypes;
    }

    private static string GetGenericTypeFullName(string genericDefinitionTypeName, string genericArgumentTypeName)
    {
      return string.Format("{0}<{1}>", genericDefinitionTypeName.Replace("<>", string.Empty), genericArgumentTypeName);
    }

    private static string GetTablePath(string name)
    {
      return string.Format("Tables/{0}", name);
    }

    private static string GetColumnPath(string tableName, string columnName)
    {
      return string.Format("Tables/{0}/Columns/{1}", tableName, columnName);
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

    private static InvalidOperationException TypeIsNotFound(string name)
    {
      return new InvalidOperationException(string.Format(
        Strings.ExTypeXIsNotFound, name));
    }

    private static InvalidOperationException FieldIsNotFound(string typeName, string fieldName)
    {
      return new InvalidOperationException(string.Format(
        Strings.ExFieldXYIsNotFound, typeName, fieldName));
    }

    private static InvalidOperationException HintConflict(UpgradeHint hintOne, UpgradeHint hintTwo)
    {
      return new InvalidOperationException(string.Format(
        Strings.ExHintXIsConflictingWithHintY, hintOne, hintTwo));
    }

    private static InvalidOperationException KeysDoNotMatch(string typeOne, string typeTwo)
    {
      return new InvalidOperationException(string.Format(
        Strings.ExKeyOfXDoesNotMatchKeyOfY, typeOne, typeTwo));
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
      return new InvalidOperationException(string.Format(
        Strings.ExTypeXMustBelongToHierarchy, type));
    }

    #endregion
    
    
    // Constructors

    public HintGenerator(StoredDomainModel storedModel, DomainModel currentModel, StorageInfo extractedModel)
    {
      this.extractedModel = extractedModel;
      this.storedModel = storedModel;
      this.currentModel = currentModel.ToStoredModel();
      this.currentModel.UpdateReferences();
    }
  }
}
