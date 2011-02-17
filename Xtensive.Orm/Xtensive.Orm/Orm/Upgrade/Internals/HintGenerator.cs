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
using Xtensive.Reflection;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Model;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Resources;
using Xtensive.Storage.Model;
using Xtensive.Orm.Building;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class HintGenerator
  {
    private readonly StoredDomainModel storedModel;
    private readonly StoredDomainModel currentModel;
    private readonly StorageInfo extractedModel;

    private readonly Dictionary<string, StoredTypeInfo> currentTypes;
    private readonly Dictionary<string, StoredTypeInfo> storedTypes;
    private readonly Dictionary<StoredTypeInfo, StoredTypeInfo> typeMapping;
    private readonly Dictionary<StoredTypeInfo, StoredTypeInfo> reverseTypeMapping;
    private readonly Dictionary<StoredFieldInfo, StoredFieldInfo> fieldMapping;
    private readonly Dictionary<StoredFieldInfo, StoredFieldInfo> reverseFieldMapping;
    
    private readonly List<Hint> schemaHints = new List<Hint>();

    public HintGenerationResult GenerateHints(IEnumerable<UpgradeHint> upgradeHints)
    {
      // Starting from generics
      var hints = new NativeTypeClassifier<UpgradeHint>(true);
      hints.AddRange(RewriteGenericTypeHints(upgradeHints));
      
      // Type-level processing
      
      // Processing type renames
      var renameTypeHints = hints.GetItems<RenameTypeHint>().ToList();
      var removeTypeHints = hints.GetItems<RemoveTypeHint>().ToList();
      BuildTypeMapping(renameTypeHints, removeTypeHints);

      // Field-level processing
      
      // Building field mapping
      var renameFieldHints = hints.GetItems<RenameFieldHint>().ToList();
      var changeFieldTypeHints = hints.GetItems<ChangeFieldTypeHint>().ToList();
      BuildFieldMapping(renameFieldHints, changeFieldTypeHints);

      // Updating mappings for connector types
      BuildConnectorTypeMapping();
      
      // Processing field movements
      var moveFieldHints = hints.GetItems<MoveFieldHint>().ToList();
      hints.AddRange(RewriteMoveFieldHints(moveFieldHints));
      hints.AddRange(GenerateTypeIdFieldRemoveHintsForConcreteTable());
      
      // Generating schema hints

      GenerateRenameTableHints();
      GenerateRenameColumnHints();

      var copyFieldHints = hints.GetItems<CopyFieldHint>().ToList();
      GenerateCopyColumnHints(copyFieldHints);

      var removedTypes = GetRemovedTypes(storedModel);
      GenerateRecordCleanupHints(removedTypes, false);
      
      var movedTypes = GetMovedTypes(storedModel);
      GenerateRecordCleanupHints(movedTypes, true);
      
      // Adding useful info

      CalculateAffectedTablesAndColumns(hints);

      // Hints validation
      ValidateHints(hints);

      return new HintGenerationResult(hints.ToList(), schemaHints);
    }

    #region Validation

    private void ValidateHints(NativeTypeClassifier<UpgradeHint> hints)
    {
      ValidateRenameTypeHints(hints.GetItems<RenameTypeHint>());
      ValidateRemoveTypeHints(hints.GetItems<RemoveTypeHint>());
      ValidateRenameFieldHints(hints.GetItems<RenameFieldHint>());
      ValidateRemoveFieldHints(hints.GetItems<RemoveFieldHint>());
      ValidateCopyFieldHints(hints.GetItems<CopyFieldHint>());
    }

    private void ValidateRenameTypeHints(IEnumerable<RenameTypeHint> hints)
    {
      var sourceTypes = new Dictionary<string, RenameTypeHint>();
      var targetTypes = new Dictionary<Type, RenameTypeHint>();
      foreach (var hint in hints) {
        var newTypeName = hint.NewType.GetFullName();
        var oldTypeName = hint.OldType;
        // Checking that types exists in models
        if (!currentModel.Types.Any(type => type.UnderlyingType==newTypeName))
          throw TypeNotFound(hint.NewType.GetFullName());
        if (!storedModel.Types.Any(type => type.UnderlyingType==oldTypeName))
          throw TypeNotFound(hint.OldType);
        // Each original type should be used only once
        // Each result type should be used only once
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
        // Both target and source fields should exist
        var targetTypeName = hint.TargetType.GetFullName();
        var targetType = currentModel.Types.SingleOrDefault(type => type.UnderlyingType==targetTypeName);
        if (targetType==null)
          throw TypeNotFound(targetTypeName);
        var sourceType = reverseTypeMapping[targetType];
        var sourceTypeName = sourceType.UnderlyingType;
        if (sourceType.GetField(hint.OldFieldName)==null)
          throw FieldNotFound(sourceTypeName, hint.OldFieldName);
        if (targetType.GetField(hint.NewFieldName)==null)
          throw FieldNotFound(targetTypeName, hint.NewFieldName);
        // Each source field should be used only once
        // Each destination field should be used only once
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
        // Checking source type and field
        var sourceTypeName = hint.SourceType;
        var sourceType = storedModel.Types.SingleOrDefault(type => type.UnderlyingType==sourceTypeName);
        if (sourceType==null)
          throw TypeNotFound(sourceTypeName);
        if (!sourceType.AllFields.Any(field => field.Name==hint.SourceField))
          throw FieldNotFound(sourceTypeName, hint.SourceField);
        // Checking destination type and field
        var targetTypeName = hint.TargetType.GetFullName();
        var targetType = currentModel.Types.SingleOrDefault(type => type.UnderlyingType==targetTypeName);
        if (targetType==null)
          throw TypeNotFound(targetTypeName);
        if (!targetType.AllFields.Any(field => field.Name==hint.TargetField))
          throw FieldNotFound(targetTypeName, hint.TargetField);
      }
    }

    private void ValidateRemoveFieldHints(IEnumerable<RemoveFieldHint> hints)
    {
      foreach (var hint in hints) {
        // Checking source type and field
        var sourceTypeName = hint.Type;
        var sourceType = storedModel.Types.SingleOrDefault(type => type.UnderlyingType==sourceTypeName);
        if (sourceType==null)
          throw TypeNotFound(sourceTypeName);
        if (!sourceType.AllFields.Any(field => field.Name==hint.Field))
          throw FieldNotFound(sourceTypeName, hint.Field);
      }
    }

    private void ValidateRemoveTypeHints(IEnumerable<RemoveTypeHint> hints)
    {
      foreach (var hint in hints) {
        // Checking source type
        var sourceTypeName = hint.Type;
        var sourceType = storedModel.Types.SingleOrDefault(type => type.UnderlyingType==sourceTypeName);
        if (sourceType==null)
          throw TypeNotFound(sourceTypeName);
      }
    }

    #endregion

    #region Map

    private void MapType(StoredTypeInfo oldType, StoredTypeInfo newType)
    {
      if (typeMapping.ContainsKey(oldType))
        throw new InvalidOperationException(String.Format(Strings.ExTypeMappingDoesNotContainXType, oldType));
      typeMapping[oldType] = newType;
      reverseTypeMapping[newType] = oldType;
      reverseTypeMapping[newType] = oldType;
    }

    private void MapField(StoredFieldInfo oldField, StoredFieldInfo newField)
    {
      if (fieldMapping.ContainsKey(oldField))
        throw new InvalidOperationException(String.Format(Strings.ExFieldMappingDoesNotContainField, oldField));
      fieldMapping[oldField] = newField;
      reverseFieldMapping[newField] = oldField;
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
        if (!fieldMapping.ContainsKey(oldNestedFieldOrigin))
          continue;
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

    private void BuildTypeMapping(IEnumerable<RenameTypeHint> renames, IEnumerable<RemoveTypeHint> removes)
    {
      // Excluding EntitySetItem<TL,TR> descendants.
      // They're not interesting at all for us, since
      // these types aren't ever referenced.
      IEnumerable<StoredTypeInfo> oldModelTypes = GetNonConnectorTypes(storedModel);

      var newConnectorTypes = currentModel.Associations
        .Select(association => association.ConnectorType)
        .Where(type => type!=null)
        .ToHashSet();

      var newModelTypes = currentModel.Types
        .Where(type => !newConnectorTypes.Contains(type))
        .ToDictionary(type => type.UnderlyingType);

      var renameLookup = renames.ToDictionary(hint => hint.OldType);
      var removeLookup = removes.ToDictionary(hint => hint.Type);

      // Mapping types
      foreach (var oldType in oldModelTypes) {
        var removeTypeHint = removeLookup.GetValueOrDefault(oldType.UnderlyingType);
        if (removeTypeHint!=null)
          continue;
        var renameTypeHint = renameLookup.GetValueOrDefault(oldType.UnderlyingType);
        var newTypeName = renameTypeHint!=null
          ? renameTypeHint.NewType.GetFullName()
          : oldType.UnderlyingType;
        var newType = newModelTypes.GetValueOrDefault(newTypeName);
        if (newType != null)
          MapType(oldType, newType);
      }
    }

    private void BuildFieldMapping(IEnumerable<RenameFieldHint> renames, IEnumerable<ChangeFieldTypeHint> typeChanges)
    {
      foreach (var pair in typeMapping)
        BuildFieldMapping(renames, typeChanges, pair.Key, pair.Value);
      foreach (var pair in fieldMapping.ToList()) // Will be modified, so .ToList is necessary
        MapNestedFields(pair.Key, pair.Value);
    }

    private void BuildFieldMapping(IEnumerable<RenameFieldHint> renames, IEnumerable<ChangeFieldTypeHint> typeChanges, 
      StoredTypeInfo oldType, StoredTypeInfo newType)
    {
      var newFields = newType.Fields.ToDictionary(field => field.Name);
      foreach (var oldField in oldType.Fields) {
        var renameHint = renames
          .FirstOrDefault(hint => hint.OldFieldName==oldField.Name && hint.TargetType.GetFullName()==newType.UnderlyingType);
        var newFieldName = renameHint!=null
          ? renameHint.NewFieldName
          : oldField.Name;
        var newField = newFields.GetValueOrDefault(newFieldName);
        if (newField==null)
          continue;
        if (oldField.IsStructure) {
          // If it is structure, we map it immediately
          MapField(oldField, newField);
          continue;
        }
        var typeChangeHint = typeChanges
          .FirstOrDefault(hint => hint.Type.GetFullName()==newType.UnderlyingType && hint.FieldName==newField.Name);
        if (typeChangeHint==null) {
          // Check & skip field if type is changed
          var newValueTypeName = newField.IsEntitySet
            ? newField.ItemType
            : newField.ValueType;
          var oldValueTypeName = oldField.IsEntitySet
            ? oldField.ItemType
            : oldField.ValueType;
          var newValueType = currentTypes.GetValueOrDefault(newValueTypeName);
          var oldValueType = storedTypes.GetValueOrDefault(oldValueTypeName);
          if (newValueType!=null && oldValueType!=null) {
            // We deal with reference field
            var mappedOldValueType = typeMapping.GetValueOrDefault(oldValueType);
            if (mappedOldValueType==null)
              // Mapped to nothing = removed
              continue;
            if (mappedOldValueType!=newValueType && !newValueType.AllDescendants.Contains(mappedOldValueType))
              // This isn't a Dog -> Animal type mapping
              continue;
          }
          else
            // We deal with regular field
            if (oldValueTypeName!=newValueTypeName)
              continue;
        }
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

        var newReferencingType = typeMapping.GetValueOrDefault(oldReferencingType);
        if (newReferencingType==null)
          continue;

        var newReferencingField = fieldMapping.GetValueOrDefault(oldReferencingField);
        if (newReferencingField==null)
          newReferencingField = newReferencingType.Fields
            .SingleOrDefault(field => field.Name==oldReferencingField.Name);
        if (newReferencingField==null)
          continue;

        var newAssociation = currentModel.Associations
          .SingleOrDefault(association => association.ReferencingField==newReferencingField);
        if (newAssociation==null || newAssociation.ConnectorType==null)
          continue;
          
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

    #region Hint rewriting

    private IEnumerable<UpgradeHint> RewriteGenericTypeHints(IEnumerable<UpgradeHint> hints)
    {
      var renameTypeHints = hints.OfType<RenameTypeHint>().ToList();
      var renameGenericTypeHints = renameTypeHints.Where(hint => hint.NewType.IsGenericTypeDefinition).ToList();
      var renameFieldHints = hints.OfType<RenameFieldHint>().Where(hint => hint.TargetType.IsGenericTypeDefinition).ToList();

      // Build generic types mapping
      var genericTypeMapping = new List<Triplet<string, Type, List<Pair<string, Type>>>>();
      var oldGenericTypes = GetGenericTypes(storedModel);
      var newGenericTypes = GetGenericTypes(BuildingContext.Demand().Model);
      var renamedTypesLookup = renameTypeHints.ToDictionary(h => h.OldType);
      var newTypesLookup     = newGenericTypes.GetClasses().ToDictionary(t => t.GetFullName());
      foreach (var oldGenericDefName in oldGenericTypes.GetClasses()) {
        var newGenericDefType = GetNewType(oldGenericDefName, newTypesLookup, renamedTypesLookup);
        if (newGenericDefType==null)
          continue;
        foreach (var pair in oldGenericTypes.GetItems(oldGenericDefName)) {
          var genericArgumentsMapping = new List<Pair<string, Type>>();
          foreach (string oldGenericArgumentType in pair.Second) {
            var newGenericArgumentType = GetNewType(oldGenericArgumentType, newTypesLookup, renamedTypesLookup);
            if (newGenericArgumentType==null)
              break;
            genericArgumentsMapping.Add(new Pair<string, Type>(oldGenericArgumentType, newGenericArgumentType));
          }
          if (genericArgumentsMapping.Count == pair.Second.Length)
            genericTypeMapping.Add(new Triplet<string, Type, List<Pair<string, Type>>>(
              oldGenericDefName, newGenericDefType, genericArgumentsMapping));
        }
      }

      // Build rename generic type hints
      var rewrittenHints = new List<UpgradeHint>();
      foreach (var triplet in genericTypeMapping) {
        var oldGenericArguments = triplet.Third.Select(pair => pair.First).ToArray();
        var newGenericArguments = triplet.Third.Select(pair => pair.Second).ToArray();
        var oldTypeFullName = GetGenericTypeFullName(triplet.First, oldGenericArguments);
        var newType = triplet.Second.MakeGenericType(newGenericArguments);
        if (oldTypeFullName != newType.GetFullName())
          rewrittenHints.Add(new RenameTypeHint(oldTypeFullName, newType));
      }

      var genericTypeDefLookup = (
        from triplet in genericTypeMapping
        group triplet by triplet.Second.GetGenericTypeDefinition()
        into g
        select new {Definition = g.Key, Instances = g.ToArray()}
        ).ToDictionary(g => g.Definition);
      
      // Build rename generic type field hints
      foreach (var hint in renameFieldHints) {
        var newGenericDefType = hint.TargetType;
        var instanceGroup = genericTypeDefLookup.GetValueOrDefault(newGenericDefType);
        if (instanceGroup==null)
          continue;
        foreach (var triplet in instanceGroup.Instances) {
          var newGenericArguments = triplet.Third.Select(pair => pair.Second).ToArray();
          rewrittenHints.Add(new RenameFieldHint(newGenericDefType.MakeGenericType(newGenericArguments), 
            hint.OldFieldName, hint.NewFieldName));
        }
      }

      // Return new hint set
      return hints
        .Except(renameGenericTypeHints.Cast<UpgradeHint>())
        .Except(renameFieldHints.Cast<UpgradeHint>())
        .Concat(rewrittenHints);
    }

    private IEnumerable<UpgradeHint> RewriteMoveFieldHints(IEnumerable<MoveFieldHint> moveFieldHints)
    {
      foreach (var hint in moveFieldHints) {
        yield return new CopyFieldHint(hint.SourceType, hint.SourceField, hint.TargetType, hint.TargetField);
        yield return new RemoveFieldHint(hint.SourceType, hint.SourceField);
      }
    }

    #endregion

    #region Hint generation

    private IEnumerable<UpgradeHint> GenerateTypeIdFieldRemoveHintsForConcreteTable()
    {
      // Removes TypeId field ( = column) from hierarchies with ConcreteTable inheritance mapping
      var result = new List<UpgradeHint>();
      var types =
        from pair in typeMapping
        let sourceHierarchy = pair.Key.Hierarchy
        let targetHierarchy = pair.Value.Hierarchy
        where
          targetHierarchy != null && sourceHierarchy != null &&
          targetHierarchy.InheritanceSchema == InheritanceSchema.ConcreteTable
        select pair.Key;
      foreach (var type in types) {
        var typeIdField = type.Fields.SingleOrDefault(f => f.IsTypeId);
        if (typeIdField==null) // Table of old type may not contain TypeId
          continue;
        var targetType = typeMapping[type];
        var targetTypeIdField = targetType.Fields.Single(f => f.IsTypeId);
        if (targetTypeIdField.IsPrimaryKey)
          continue;
        if (!extractedModel.Tables.Contains(type.MappingName))
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
        .Where(type => type.Key.Hierarchy.InheritanceSchema!=InheritanceSchema.SingleTable || type.Key.Hierarchy.Root==type.Key);
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
          throw Exceptions.InternalError(String.Format(Strings.ExInheritanceSchemaIsInvalid, newType.Hierarchy.InheritanceSchema), Log.Instance);
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
      var targetType = currentModel.Types.SingleOrDefault(type => type.UnderlyingType==targetTypeName);
      if (targetType==null)
        throw new InvalidOperationException(String.Format(Strings.ExUpgradeHintTargetTypeNotFound, targetTypeName));
      var targetField = targetType.AllFields.SingleOrDefault(field => field.Name==hint.TargetField);
      if (targetField==null)
        throw new InvalidOperationException(String.Format(Strings.ExUpgradeHintTargetFieldNotFound, hint.TargetField));
      var targetHierarchy = targetType.Hierarchy;

      var sourceTypeName = hint.SourceType;
      var sourceType = storedModel.Types.SingleOrDefault(type => type.UnderlyingType==sourceTypeName);
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
      var sourceTable = sourceType.MappingName;
      var targetTables = GetAffectedMappedTypes(targetType, targetHierarchy.InheritanceSchema==InheritanceSchema.ConcreteTable)
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
        throw Exceptions.InternalError(String.Format(Strings.ExInheritanceSchemaIsInvalid, hierarchy.InheritanceSchema), Log.Instance);
      }
      foreach (var type in typesToProcess) {
        var tableName = type.MappingName;
        var sourceTablePath = GetTablePath(tableName);
        var identities = new List<IdentityPair>();
        // ConcreteTable schema doesn't include TypeId
        if (hierarchy.InheritanceSchema != InheritanceSchema.ConcreteTable)
          identities.Add(new IdentityPair(
            GetColumnPath(tableName, GetTypeIdMappingName(type)),
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
        from association in storedModel.Associations
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
          var declaringType = association.ReferencingField.DeclaringType;
          var inheritanceSchema = declaringType.Hierarchy.InheritanceSchema;
          GenerateClearReferenceHints(
            removedType, 
            GetAffectedMappedTypes(declaringType, inheritanceSchema==InheritanceSchema.ConcreteTable).ToArray(),
            association,
            requiresInverseCleanup);
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

      var sourceTablePath = GetTablePath(updatedType.MappingName);
      var identities = pairedIdentityColumns.Select(pair =>
        new IdentityPair(
          GetColumnPath(updatedType.MappingName, pair.Second),
          GetColumnPath(removedType.MappingName, pair.First), false))
        .ToList();
      if (removedType.Hierarchy.InheritanceSchema!=InheritanceSchema.ConcreteTable)
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
      var storedType = storedModel.Types.SingleOrDefault(type =>
        type.UnderlyingType == typeName);
      if (storedType == null)
        throw TypeNotFound(typeName);
      var inheritanceSchema = storedType.Hierarchy.InheritanceSchema;

      switch (inheritanceSchema)
      {
        case InheritanceSchema.ClassTable:
          affectedTables.Add(GetTablePath(storedType.MappingName));
          break;
        case InheritanceSchema.SingleTable:
          affectedTables.Add(GetTablePath(storedType.Hierarchy.Root.MappingName));
          break;
        case InheritanceSchema.ConcreteTable:
          var typeToProcess = GetAffectedMappedTypes(storedType,
            storedType.Hierarchy.InheritanceSchema == InheritanceSchema.ConcreteTable);
          affectedTables.AddRange(
            typeToProcess.Select(type => GetTablePath(type.MappingName)));
          break;
        default:
          throw Exceptions.InternalError(String.Format(
            Strings.ExInheritanceSchemaIsInvalid, inheritanceSchema), Log.Instance);
      }
      hint.AffectedTables = new ReadOnlyList<string>(affectedTables);
    }

    private void UpdateAffectedColumns(ChangeFieldTypeHint hint)
    {
      var affectedColumns = new List<string>();
      var currentTypeName = hint.Type.GetFullName();
      var currentType = currentModel.Types.SingleOrDefault(type =>
        type.UnderlyingType == currentTypeName);
      if (currentType == null)
        throw TypeNotFound(currentTypeName);
      var currentField = currentType.AllFields
        .SingleOrDefault(field => field.Name == hint.FieldName);
      if (currentField == null)
        throw FieldNotFound(currentTypeName, hint.FieldName);
      var inheritanceSchema = currentType.Hierarchy.InheritanceSchema;

      switch (inheritanceSchema)
      {
        case InheritanceSchema.ClassTable:
          affectedColumns.Add(GetColumnPath(currentField.DeclaringType.MappingName, currentField.MappingName));
          break;
        case InheritanceSchema.SingleTable:
          affectedColumns.Add(GetColumnPath(currentType.Hierarchy.Root.MappingName, currentField.MappingName));
          break;
        case InheritanceSchema.ConcreteTable:
          var typeToProcess = GetAffectedMappedTypes(currentType,
            currentType.Hierarchy.InheritanceSchema == InheritanceSchema.ConcreteTable);
          affectedColumns.AddRange(
            typeToProcess.Select(type => GetColumnPath(type.MappingName, currentField.MappingName)));
          break;
        default:
          throw Exceptions.InternalError(String.Format(
            Strings.ExInheritanceSchemaIsInvalid, inheritanceSchema), Log.Instance);
      }
      hint.AffectedColumns = new ReadOnlyList<string>(affectedColumns);
    }

    private void UpdateAffectedColumns(RemoveFieldHint hint)
    {
      var affectedColumns = new List<string>();
      var typeName = hint.Type;
      var storedType = storedModel.Types.SingleOrDefault(type => type.UnderlyingType == typeName);
      if (storedType == null)
        throw TypeNotFound(typeName);
      var storedField = storedType.AllFields
        .SingleOrDefault(field => field.Name == hint.Field);
      if (storedField == null)
        throw FieldNotFound(typeName, hint.Field);
      foreach (var primitiveField in storedField.PrimitiveFields)
      {
        var inheritanceSchema = storedType.Hierarchy.InheritanceSchema;
        switch (inheritanceSchema)
        {
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
              storedType.Hierarchy.InheritanceSchema == InheritanceSchema.ConcreteTable);
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
        Log.Warning(Strings.ExTableXIsNotFound, tableName);
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

    private static Type GetNewType(string oldTypeName, Dictionary<string,Type> newTypes, Dictionary<string,RenameTypeHint> hints)
    {
      RenameTypeHint hint;
      Type newType;
      return hints.TryGetValue(oldTypeName, out hint)
        ? hint.NewType
        : (newTypes.TryGetValue(oldTypeName, out newType) ? newType : null);
    }

    private static ClassifiedCollection<string,  Pair<string, string[]>> GetGenericTypes(StoredDomainModel model)
    {
      var genericTypes = new ClassifiedCollection<string,  Pair<string, string[]>>(pair => new [] {pair.First});
      foreach (var typeInfo in model.Types.Where(type => type.IsGeneric)) {
        var typeDefinitionName = typeInfo.GenericTypeDefinition;
        genericTypes.Add(new Pair<string, string[]>(typeDefinitionName, typeInfo.GenericArguments));
      }
      return genericTypes;
    }

    private static ClassifiedCollection<Type,  Pair<Type, Type[]>> GetGenericTypes(DomainModel model)
    {
      var genericTypes = new ClassifiedCollection<Type,  Pair<Type, Type[]>>(pair => new [] {pair.First});
      foreach (var typeInfo in model.Types.Where(type => type.UnderlyingType.IsGenericType)) {
        var typeDefinition = typeInfo.UnderlyingType.GetGenericTypeDefinition();
        genericTypes.Add(new Pair<Type, Type[]>(typeDefinition, typeInfo.UnderlyingType.GetGenericArguments()));
      }
      return genericTypes;
    }

    private static string GetGenericTypeFullName(string genericDefinitionTypeName, string[] genericArgumentNames)
    {
      return string.Format("{0}<{1}>", genericDefinitionTypeName.Replace("<>", string.Empty), 
        genericArgumentNames.ToCommaDelimitedString());
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

    private static InvalidOperationException TypeNotFound(string name)
    {
      return new InvalidOperationException(string.Format(
        Strings.ExTypeXIsNotFound, name));
    }

    private static InvalidOperationException FieldNotFound(string typeName, string fieldName)
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
      reverseFieldMapping = new Dictionary<StoredFieldInfo, StoredFieldInfo>();
      fieldMapping = new Dictionary<StoredFieldInfo, StoredFieldInfo>();
      reverseTypeMapping = new Dictionary<StoredTypeInfo, StoredTypeInfo>();
      typeMapping = new Dictionary<StoredTypeInfo, StoredTypeInfo>();
      this.extractedModel = extractedModel;
      this.storedModel = storedModel;
      this.currentModel = currentModel.ToStoredModel();
      this.currentModel.UpdateReferences();
      currentTypes = this.currentModel.Types.ToDictionary(t => t.UnderlyingType);
      storedTypes = this.storedModel.Types.ToDictionary(t => t.UnderlyingType);
    }
  }
}
