// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
using DataDeletionInfo = Xtensive.Modelling.Comparison.Hints.DeleteDataHint.DeleteDataHintState;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class HintGenerator
  {
    [Flags]
    private enum CleanupInfo : byte
    {
      None = 0,
      TypeMovedToAnotherHierarchy = 1,
      ConflictByTable = 2,
      RootOfConflict = 4
    }

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
    private readonly HashSet<StoredTypeInfo> suspiciousTypes;
    private readonly IReadOnlyList<StoredTypeInfo> currentNonConnectorTypes;
    private readonly IReadOnlyList<StoredTypeInfo> extractedNonConnectorTypes;

    private readonly List<Hint> schemaHints = new List<Hint>();

    public HintGenerationResult Run()
    {
      // Generating schema hints

      GenerateRenameTableHints();
      GenerateRenameColumnHints();

      var copyFieldHints = hints.GetItems<CopyFieldHint>();
      GenerateCopyColumnHints(copyFieldHints);

      var removedTypes = GetRemovedTypes();
      var conflictsByTable = GetConflictsByTable(removedTypes);
      GenerateRecordCleanupHints(removedTypes, conflictsByTable, false);

      var movedTypes = GetMovedTypes();
      GenerateRecordCleanupHints(movedTypes, conflictsByTable, true);

      // Adding useful info
      CalculateAffectedTablesAndColumns(hints);

      // Hints validation
      new UpgradeHintValidator(currentModel, extractedModel, typeMapping, reverseTypeMapping).Validate(hints);

      var upgradedTypesMapping = reverseTypeMapping
        .ToDictionary(item => item.Key.UnderlyingType, item => item.Value.UnderlyingType);

      return new HintGenerationResult(hints.ToList(hints.Count), schemaHints, upgradedTypesMapping);
    }

    #region Hint generation

    private void GenerateRenameTableHints()
    {
      var mappingsToProcess = typeMapping
        .Where(type => type.Key.IsEntity)
        .Where(type => type.Key.Hierarchy.InheritanceSchema != InheritanceSchema.SingleTable
          || type.Key.Hierarchy.Root == type.Key);
      foreach (var mapping in mappingsToProcess) {
        var oldType = mapping.Key;
        var newType = mapping.Value;
        if (!GetTableName(oldType).Equals(GetTableName(newType), StringComparison.Ordinal)) {
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
            throw Exceptions.InternalError(string.Format(
              Strings.ExInheritanceSchemaIsInvalid, newType.Hierarchy.InheritanceSchema), UpgradeLog.Instance);
        }
      }
    }

    private void GenerateRenameFieldHint(StoredFieldInfo oldField, StoredFieldInfo newField,
      StoredTypeInfo newType, bool includeInheritors)
    {
      if (oldField.MappingName.Equals(newField.MappingName, StringComparison.Ordinal)) {
        return;
      }

      foreach (var newTargetType in GetAffectedMappedTypes(newType, includeInheritors)) {
        if (!reverseTypeMapping.TryGetValue(newTargetType, out var oldTargetType)) {
          continue;
        }

        RegisterRenameFieldHint(oldTargetType, newTargetType, oldField.MappingName, newField.MappingName);
      }
    }

    private void GenerateCopyColumnHints(IEnumerable<CopyFieldHint> hints)
    {
      foreach (var hint in hints) {
        GenerateCopyColumnHint(hint);
      }
    }

    private void GenerateCopyColumnHint(CopyFieldHint hint)
    {
      // searching for all required objects
      var targetTypeName = hint.TargetType.GetFullName();
      var targetType = currentModel.Types
        .SingleOrDefault(type => type.UnderlyingType.Equals(targetTypeName, StringComparison.Ordinal));
      if (targetType == null) {
        throw new InvalidOperationException(string.Format(Strings.ExUpgradeHintTargetTypeNotFound, targetTypeName));
      }

      var targetField = targetType.AllFields
        .SingleOrDefault(field => field.Name.Equals(hint.TargetField, StringComparison.Ordinal));
      if (targetField == null) {
        throw new InvalidOperationException(string.Format(Strings.ExUpgradeHintTargetFieldNotFound, hint.TargetField));
      }

      var targetHierarchy = targetType.Hierarchy;

      var sourceTypeName = hint.SourceType;
      var sourceType = extractedModel.Types
        .SingleOrDefault(type => type.UnderlyingType.Equals(sourceTypeName, StringComparison.Ordinal));
      if (sourceType == null) {
        throw new InvalidOperationException(string.Format(Strings.ExUpgradeHintSourceTypeNotFound, sourceTypeName));
      }

      var sourceField = sourceType.AllFields
        .SingleOrDefault(field => field.Name.Equals(hint.SourceField, StringComparison.Ordinal));
      if (sourceField == null) {
        throw new InvalidOperationException(string.Format(Strings.ExUpgradeHintSourceFieldNotFound, hint.SourceField));
      }

      var sourceHierarchy = sourceType.Hierarchy;

      // checking that types have hierarchies
      if (sourceHierarchy == null) {
        throw TypeIsNotInHierarchy(hint.SourceType);
      }

      if (targetHierarchy == null) {
        throw TypeIsNotInHierarchy(targetTypeName);
      }

      // building set of key columns
      var pairedKeyColumns = AssociateMappedKeyFields(sourceHierarchy, targetHierarchy);
      if (pairedKeyColumns == null) {
        throw KeysDoNotMatch(sourceTypeName, targetTypeName);
      }

      // building set of copied columns
      var pairedColumns = AssociateMappedFields(new Pair<StoredFieldInfo>(sourceField, targetField));
      if (pairedColumns == null) {
        throw FieldsDoNotMatch(sourceField, targetField);
      }

      // building source/destination table/column names
      var targetTypes = GetAffectedMappedTypes(
        targetType, targetHierarchy.InheritanceSchema == InheritanceSchema.ConcreteTable);

      // generating result hints
      foreach (var target in targetTypes) {
        var sourceTablePath = GetTablePath(sourceType);
        var identities = new List<IdentityPair>(pairedKeyColumns.Length);
        var copiedColumns = new List<Pair<string>>(pairedColumns.Length);

        foreach (var keyColumnPair in pairedKeyColumns) {
          identities.Add(CreateIdentityPair(target, sourceType, keyColumnPair));
        }

        foreach (var columnPair in pairedColumns) {
          copiedColumns.Add(new Pair<string>(
            GetColumnPath(sourceType, columnPair.First),
            GetColumnPath(target, columnPair.Second)));
        }

        schemaHints.Add(new CopyDataHint(sourceTablePath, identities, copiedColumns));
      }
    }

    private void GenerateRecordCleanupHints(List<StoredTypeInfo> removedTypes,
      HashSet<StoredTypeInfo> conflictsByTable, bool isMovedToAnotherHierarchy)
    {
      if (!isMovedToAnotherHierarchy) {
        removedTypes.ForEach((rType) =>
          GenerateCleanupByForeignKeyHints(rType, GetCleanupInfo(rType, conflictsByTable, isMovedToAnotherHierarchy)));
      }
      removedTypes.ForEach(type =>
          GenerateCleanupByPrimaryKeyHints(type, GetCleanupInfo(type, conflictsByTable, isMovedToAnotherHierarchy)));
    }

    private void GenerateCleanupByPrimaryKeyHints(StoredTypeInfo removedType, CleanupInfo cleanupInfo)
    {
      IEnumerable<(StoredTypeInfo, IdentityPair)> typesToProcess;
      var hierarchy = removedType.Hierarchy;
      switch (hierarchy.InheritanceSchema) {
        case InheritanceSchema.ClassTable:
          typesToProcess = GetTypesToCleanForClassTable(removedType, cleanupInfo);
          break;
        case InheritanceSchema.SingleTable:
          typesToProcess = GetTypesToCleanForSingleTable(removedType, cleanupInfo);
          break;
        case InheritanceSchema.ConcreteTable:
          typesToProcess = GetTypesToCleanForConcreteTable(removedType, cleanupInfo);
          break;
        default:
          throw Exceptions.InternalError(string.Format(Strings.ExInheritanceSchemaIsInvalid, hierarchy.InheritanceSchema), UpgradeLog.Instance);
      }

      var deleteInfo = DataDeletionInfo.None;
      if ((cleanupInfo & CleanupInfo.TypeMovedToAnotherHierarchy) != 0)
        deleteInfo |= DataDeletionInfo.PostCopy;
      if ((cleanupInfo & CleanupInfo.ConflictByTable) != 0)
        deleteInfo |= DataDeletionInfo.TableMovement;

      foreach (var info in typesToProcess) {
        var sourceTablePath = GetTablePath(info.Item1);
        var identities = info.Item2 != null
          ? new IdentityPair[] { info.Item2 }
          : Array.Empty<IdentityPair>();
        schemaHints.Add(
          new DeleteDataHint(sourceTablePath, identities, deleteInfo));
      }
    }

    private IEnumerable<(StoredTypeInfo, IdentityPair)> GetTypesToCleanForClassTable(
      StoredTypeInfo removedType, CleanupInfo cleanupInfo)
    {
      if ((cleanupInfo & CleanupInfo.TypeMovedToAnotherHierarchy) == 0) {
        if ((cleanupInfo & CleanupInfo.ConflictByTable) == 0) {
          return removedType.AllAncestors.Append(removedType)
            .Select(t => (t, CreateIdentityPair(removedType, t)));
        }
        else {
          if ((cleanupInfo & CleanupInfo.RootOfConflict) == 0)
            return Array.Empty<(StoredTypeInfo, IdentityPair)>();

          var capacity = (2 * removedType.AllAncestors.Length) + removedType.AllDescendants.Length + 1;
          var typesToProcess = new List<(StoredTypeInfo, IdentityPair)>(capacity);
          typesToProcess.Add((removedType, CreateIdentityPair(removedType, removedType)));
          foreach(var dType in removedType.AllDescendants) {
            typesToProcess.Add((removedType, CreateIdentityPair(removedType, removedType, dType.TypeId)));
            typesToProcess.Add((dType, null));
          }

          foreach(var aType in removedType.AllAncestors) {
            typesToProcess.Add((aType, CreateIdentityPair(removedType, aType, removedType.TypeId)));
            foreach (var dType in removedType.AllDescendants) {
              typesToProcess.Add((aType, CreateIdentityPair(removedType, aType, dType.TypeId)));
            }
          }
          return typesToProcess;
        }
      }
      else {
        if ((cleanupInfo & CleanupInfo.ConflictByTable) == 0) {
          return removedType.AllAncestors
            .Select(aType =>(aType, CreateIdentityPair(removedType, aType)));
        }
      }
      return Array.Empty<(StoredTypeInfo, IdentityPair)>();
    }

    private IEnumerable<(StoredTypeInfo, IdentityPair)> GetTypesToCleanForSingleTable(
      StoredTypeInfo removedType, CleanupInfo cleanupInfo)
    {
      var rootType = removedType.Hierarchy.Root;
      if ((cleanupInfo & CleanupInfo.TypeMovedToAnotherHierarchy) == 0) {
        if ((cleanupInfo & CleanupInfo.ConflictByTable) == 0)
          return new (StoredTypeInfo, IdentityPair)[] { (rootType, CreateIdentityPair(removedType, rootType)) };
        else {
          return removedType.AllDescendants.Append(rootType)
            .Select(t => (rootType, CreateIdentityPair(t, rootType)));
        }
      }
      else {
        if ((cleanupInfo & CleanupInfo.ConflictByTable) == 0)
          return new (StoredTypeInfo, IdentityPair)[] { (rootType, CreateIdentityPair(removedType, rootType)) };
      }
      return Array.Empty<(StoredTypeInfo, IdentityPair)>();
    }

    private IEnumerable<(StoredTypeInfo, IdentityPair)> GetTypesToCleanForConcreteTable(
      StoredTypeInfo removedType, CleanupInfo cleanupInfo)
    {
      if ((cleanupInfo & CleanupInfo.TypeMovedToAnotherHierarchy) == 0) {
        if ((cleanupInfo & CleanupInfo.ConflictByTable) == 0)
          return new (StoredTypeInfo, IdentityPair)[] { (removedType, null) };
        else {
          if ((cleanupInfo & CleanupInfo.RootOfConflict) == 0)
            return Array.Empty<(StoredTypeInfo, IdentityPair)>();
          return removedType.AllDescendants.Append(removedType)
            .Select(t => (t, (IdentityPair)null));
        }
      }
      else {
        if ((cleanupInfo & CleanupInfo.ConflictByTable) == 0)
          return new (StoredTypeInfo, IdentityPair)[] { (removedType, null) };
      }
      return Array.Empty<(StoredTypeInfo, IdentityPair)>();
    }

    private void GenerateCleanupByForeignKeyHints(StoredTypeInfo removedType, CleanupInfo cleanupInfo)
    {
      var removedTypeAndAncestors = new HashSet<StoredTypeInfo>(removedType.AllAncestors.Length + 1);
      removedType.AllAncestors.Append(removedType).ForEach(t => removedTypeAndAncestors.Add(t));


      var descendantsToHash = (cleanupInfo & CleanupInfo.ConflictByTable) != 0
        ? removedType.AllDescendants
        : Array.Empty<StoredTypeInfo>();
      var descendants = new HashSet<StoredTypeInfo>(descendantsToHash.Length);
      descendants.UnionWith(descendantsToHash);

      var affectedAssociations = (
        from association in extractedModel.Associations
        let requiresInverseCleanup =
          association.IsMaster &&
          association.ConnectorType != null &&
          (removedTypeAndAncestors.Contains(association.ReferencingField.DeclaringType) ||
            descendants.Contains(association.ReferencingField.DeclaringType))
        where
          // Regular association X.Y, Y must be cleaned up
          removedTypeAndAncestors.Contains(association.ReferencedType) ||
            descendants.Contains(association.ReferencedType) ||
          // X.EntitySet<Y>, where X is in removedTypeAndAncestors,
          // connectorType.X must be cleaned up as well
          requiresInverseCleanup
        select (association, requiresInverseCleanup)
        ).ToList();
      foreach (var pair in affectedAssociations) {
        var association = pair.association;
        var requiresInverseCleanup = pair.requiresInverseCleanup;
        if (association.ConnectorType==null) {
          // This is regular reference
          var field = association.ReferencingField;
          var declaringType = field.DeclaringType;
          if (declaringType.IsInterface) {
            ClearAssociationForInterface(removedType, association, requiresInverseCleanup);
          }
          else {
            var removedOrAncestor = removedTypeAndAncestors.Contains(association.ReferencedType);
            ClearDirectAssociation(removedType, declaringType,
              association, requiresInverseCleanup, removedOrAncestor, cleanupInfo);
          }
        }
        else {
          // This is EntitySet
          var removedOrAncestor = removedTypeAndAncestors.Contains(association.ReferencedType);
          ClearIndirectAssociation(removedType,
            association, requiresInverseCleanup, removedOrAncestor, cleanupInfo);
        }
      }
    }

    private void ClearAssociationForInterface(
      StoredTypeInfo removedType,
      StoredAssociationInfo association,
      bool requiresInverseCleanup)
    {
      var field = association.ReferencingField;
              var candidates = extractedModel.Types
                .Where(t => !t.IsInterface
                  && t.Fields.Any(f => f.IsInterfaceImplementation
                    && f.Name.Equals(field.Name, StringComparison.Ordinal)
                    && f.ValueType.Equals(field.ValueType, StringComparison.Ordinal)))
                .ToList();
              foreach (var candidate in candidates) {
                  var inheritanceSchema = candidate.Hierarchy.InheritanceSchema;
                  GenerateClearReferenceHints(
                    removedType,
                    GetAffectedMappedTypesAsArray(candidate, inheritanceSchema==InheritanceSchema.ConcreteTable),
                    association,
                    requiresInverseCleanup);
              }
          }

    private void ClearDirectAssociation(StoredTypeInfo removedType,
      StoredTypeInfo declaringType,
      StoredAssociationInfo association,
      bool requiresInverseCleanup,
      bool useRemovedType,
      CleanupInfo cleanupInfo)
    {
            var inheritanceSchema = declaringType.Hierarchy.InheritanceSchema;
      if ((cleanupInfo & CleanupInfo.ConflictByTable) == 0) {
        var includeInheritors = inheritanceSchema == InheritanceSchema.ConcreteTable;
            GenerateClearReferenceHints(
              removedType,
          GetAffectedMappedTypesAsArray(declaringType, includeInheritors),
              association,
              requiresInverseCleanup);
          }
        else {
        if ((cleanupInfo & CleanupInfo.ConflictByTable) != 0 && (cleanupInfo & CleanupInfo.RootOfConflict) == 0)
          return;
        var type = useRemovedType
          ? removedType
          : association.ReferencedType;
        var includeInheritors = inheritanceSchema == InheritanceSchema.ConcreteTable;
        GenerateClearReferenceHints(
          type,
          GetAffectedMappedTypesAsArray(declaringType, includeInheritors),
          association,
          requiresInverseCleanup);

        foreach(var dType in type.AllDescendants) {
          GenerateClearReferenceHints(
            dType,
            GetAffectedMappedTypesAsArray(declaringType, includeInheritors),
            association,
            requiresInverseCleanup);
        }
      }
    }

    private void ClearIndirectAssociation(StoredTypeInfo removedType,
      StoredAssociationInfo association,
      bool requiresInverseCleanup,
      bool useRemovedType,
      CleanupInfo cleanupInfo)
    {
      var deleteInfo = DataDeletionInfo.None;
      if ((cleanupInfo & CleanupInfo.ConflictByTable) != 0)
        deleteInfo |= DataDeletionInfo.TableMovement;

      if ((deleteInfo & DataDeletionInfo.TableMovement) == 0) {
          GenerateClearReferenceHints(
            removedType,
            new [] {association.ConnectorType},
            association,
        requiresInverseCleanup,
        deleteInfo);
      }
      else {
        if ((cleanupInfo & CleanupInfo.ConflictByTable) != 0 && (cleanupInfo & CleanupInfo.RootOfConflict) == 0)
          return;
        var type = useRemovedType
          ? removedType
          : association.ReferencedType;
        GenerateClearReferenceHints(
            type,
            new[] { association.ConnectorType },
            association,
            requiresInverseCleanup,
            deleteInfo);
        foreach (var dType in type.AllDescendants) {
          GenerateClearReferenceHints(
            dType,
            new[] { association.ConnectorType },
            association,
            requiresInverseCleanup,
            deleteInfo);
        }
      }
    }

    private void GenerateClearReferenceHints(
      StoredTypeInfo removedType,
      StoredTypeInfo[] updatedTypes,
      StoredAssociationInfo association,
      bool inverse,
      DataDeletionInfo? dataDeletionInfo = null)
    {
      foreach (var updatedType in updatedTypes) {
        GenerateClearReferenceHint(removedType, updatedType, association, inverse, dataDeletionInfo);
      }
    }

    private void GenerateClearReferenceHint(
      StoredTypeInfo removedType,
      StoredTypeInfo updatedType,
      StoredAssociationInfo association,
      bool inverse,
      DataDeletionInfo? dataDeletionInfo)
    {
      if (association.ReferencingField.IsEntitySet && association.ConnectorType==null) {
        // There is nothing to cleanup in class containing EntitySet<T> property,
        // when T is removed, and EntitySet<T> was paired to a property of T.
        return;
      }

      var identityFieldsOfRemovedType = removedType.AllFields.Where(f => f.IsPrimaryKey).ToList();
      var identityFieldsOfUpdatedType = association.ConnectorType != null
        ? association.ConnectorType.Fields
          .Single(field => field.Name == ((association.IsMaster ^ inverse) ? WellKnown.SlaveFieldName : WellKnown.MasterFieldName))
          .Fields
        : association.ReferencingField.Fields;
      var pairedIdentityFields = JoinFieldsByOriginalName(identityFieldsOfRemovedType, identityFieldsOfUpdatedType);
      var pairedIdentityColumns = AssociateMappedFields(pairedIdentityFields);
      if (pairedIdentityColumns == null) {
        throw new InvalidOperationException(
          string.Format(Strings.ExPairedIdentityColumnsForTypesXAndXNotFound, removedType, updatedType));
      }

      var sourceTablePath = GetTablePath(updatedType);
      var identities = pairedIdentityColumns.SelectToList(pair =>
        CreateIdentityPair(removedType, updatedType, pair));
      if (removedType.Hierarchy.InheritanceSchema != InheritanceSchema.ConcreteTable) {
        identities.Add(CreateIdentityPair(removedType, removedType));
      }

      var updatedColumns = pairedIdentityColumns
        .SelectToList(pair =>
          new Pair<string, object>(GetColumnPath(updatedType, pair.Second), null));

      if (association.ConnectorType == null) {
        schemaHints.Add(new UpdateDataHint(sourceTablePath, identities, updatedColumns));
      }
      else {
        if (!dataDeletionInfo.HasValue)
          throw new InvalidOperationException("DeleteDataHint require DataDeletionInfo");
        schemaHints.Add(new DeleteDataHint(sourceTablePath, identities, dataDeletionInfo.Value));
      }
    }

    #endregion

    #region Generate additional info

    private void CalculateAffectedTablesAndColumns(NativeTypeClassifier<UpgradeHint> hints)
    {
      if (hints.GetItemCount<RemoveTypeHint>() > 0) {
        hints.GetItems<RemoveTypeHint>().ForEach(UpdateAffectedTables);
      }
      if (hints.GetItemCount<RemoveFieldHint>() > 0) {
        hints.GetItems<RemoveFieldHint>().ForEach(UpdateAffectedColumns);
      }
      if (hints.GetItemCount<ChangeFieldTypeHint>() > 0) {
        hints.GetItems<ChangeFieldTypeHint>().ForEach(UpdateAffectedColumns);
      }
    }

    private void UpdateAffectedTables(RemoveTypeHint hint)
    {
      var affectedTables = new List<string>();
      var typeName = hint.Type;
      var storedType = extractedModel.Types
        .SingleOrDefault(type => type.UnderlyingType.Equals(typeName, StringComparison.Ordinal));
      if (storedType == null) {
        throw TypeNotFound(typeName);
      }

      var inheritanceSchema = storedType.Hierarchy.InheritanceSchema;

      switch (inheritanceSchema) {
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
          throw Exceptions.InternalError(string.Format(
            Strings.ExInheritanceSchemaIsInvalid, inheritanceSchema), UpgradeLog.Instance);
      }
      hint.AffectedTables = affectedTables.AsReadOnly();
    }

    private void UpdateAffectedColumns(ChangeFieldTypeHint hint)
    {
      var currentTypeName = hint.Type.GetFullName();
      var currentType = currentModel.Types
        .SingleOrDefault(type => type.UnderlyingType.Equals(currentTypeName, StringComparison.Ordinal));
      if (currentType == null) {
        throw TypeNotFound(currentTypeName);
      }

      var currentField = currentType.AllFields
        .SingleOrDefault(field => field.Name.Equals(hint.FieldName, StringComparison.Ordinal));
      if (currentField==null) {
        throw FieldNotFound(currentTypeName, hint.FieldName);
      }

      var affectedColumns = GetAffectedColumns(currentType, currentField);
      hint.AffectedColumns = affectedColumns.AsReadOnly();
    }

    private void UpdateAffectedColumns(RemoveFieldHint hint)
    {
      if (hint.IsExplicit) {
        return;
      }

      var typeName = hint.Type;
      var storedType = extractedModel.Types
        .SingleOrDefault(type => type.UnderlyingType.Equals(typeName, StringComparison.Ordinal));
      if (storedType == null) {
        throw TypeNotFound(typeName);
      }

      StoredFieldInfo storedField = null;
      // Nested field, looks like a field of a structure
      if (hint.Field.Contains(".", StringComparison.Ordinal)) {
        var path = hint.Field.Split('.');
        var fields = storedType.AllFields;
        var fieldName = string.Empty;
        for (var i = 0; i < path.Length; i++) {
          fieldName += string.IsNullOrEmpty(fieldName) ? path[i] : "." + path[i];
          var parameter = fieldName;
          storedField = fields.SingleOrDefault(field => field.Name.Equals(parameter, StringComparison.Ordinal));
          if (storedField == null) {
            throw FieldNotFound(typeName, hint.Field);
          }

          fields = storedField.Fields;
        }
      }
      else {
        storedField = storedType.AllFields
          .SingleOrDefault(field => field.Name.Equals(hint.Field, StringComparison.Ordinal));
      }
      if (storedField == null) {
        throw FieldNotFound(typeName, hint.Field);
      }

      var affectedColumns = GetAffectedColumns(storedType, storedField);
      hint.AffectedColumns = affectedColumns.AsReadOnly();
    }

    #endregion

    #region Helpers

    private void RegisterRenameTableHint(StoredTypeInfo oldType, StoredTypeInfo newType)
    {
      if (EnsureTableExist(oldType)) {
        schemaHints.Add(new RenameHint(GetTablePath(oldType), GetTablePath(newType)));
      }
    }

    private void RegisterRenameFieldHint(
      StoredTypeInfo oldType, StoredTypeInfo newType, string oldColumnName, string newColumnName)
    {
      if (EnsureTableExist(oldType) && EnsureFieldExist(oldType, oldColumnName)) {
        schemaHints.Add(
          new RenameHint(GetColumnPath(oldType, oldColumnName), GetColumnPath(newType, newColumnName)));
      }
    }

    private bool EnsureTableExist(StoredTypeInfo type)
    {
      var nodeName = GetTableName(type);

      if (extractedStorageModel.Tables.Contains(nodeName)) {
        return true;
      }

      UpgradeLog.Warning(Strings.ExTableXIsNotFound, nodeName);
      return false;
    }

    private bool EnsureFieldExist(StoredTypeInfo type, string fieldName)
    {
      if (!EnsureTableExist(type)) {
        return false;
      }
      var nodeName = GetTableName(type);
      var actualFieldName = nameBuilder.ApplyNamingRules(fieldName);
      if (extractedStorageModel.Tables[nodeName].Columns.Contains(actualFieldName)) {
        return true;
      }

      UpgradeLog.Warning(Strings.ExColumnXIsNotFoundInTableY, actualFieldName, nodeName);
      return false;
    }

    private List<string> GetAffectedColumns(StoredTypeInfo type, StoredFieldInfo field)
    {
      var affectedColumns = new List<string>();

      if (type.IsStructure) {
        var structureFields = extractedModel.Types
          .Where(t => t.IsEntity)
          .SelectMany(t => extractedModelFields[t].Where(f => f.IsStructure && f.ValueType.Equals(type.UnderlyingType, StringComparison.Ordinal)));

        foreach (var structureField in structureFields) {
          var nestedField = structureField.Fields.FirstOrDefault(f => f.OriginalName.Equals(field.Name,StringComparison.Ordinal));
          if (nestedField != null) {
            affectedColumns.AddRange(GetAffectedColumns(structureField.DeclaringType, nestedField));
          }
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
            throw Exceptions.InternalError(string.Format(Strings.ExInheritanceSchemaIsInvalid, inheritanceSchema), UpgradeLog.Instance);
        }
      }
      return affectedColumns;
    }

    private HashSet<StoredTypeInfo> GetConflictsByTable(IReadOnlyList<StoredTypeInfo> removedTypes)
    {
      // It gets new types and removed types and looks if they use
      // table with the same path (db/schema/table path is unique).
      // If there is such pair of new and removed types then table
      // will be reused on schema comparison but data will be cleared
      // because it no longer represents type connected to the table.

      // IMPORTANT NOTE! SingleTable hierarchies use the same table
      // so any new type from such hierarhcy will conflict with removed
      // by table. Knowing that we basically cannot register table conflicts
      // on table basis, except for the case when root is conflict.

      var capacity = currentModel.Types.Length - typeMapping.Count;
      var currentTables = new HashSet<string>(capacity, StringComparer.Ordinal);
      foreach (var newType in currentNonConnectorTypes.Where(t => !reverseTypeMapping.ContainsKey(t))) {
        if (newType.Hierarchy == null
          || (newType.Hierarchy.InheritanceSchema == InheritanceSchema.SingleTable && !newType.IsHierarchyRoot))
          continue;
        var key = $"{newType.MappingDatabase}.{newType.MappingSchema}.{newType.MappingName}";
        currentTables.Add(key);
      }

      var conflictsByTable = new HashSet<StoredTypeInfo>();

      foreach (var rType in removedTypes) {
        var rTypeIdentifier = $"{rType.MappingDatabase}.{rType.MappingSchema}.{rType.MappingName}";
        if (suspiciousTypes.Contains(rType) && currentTables.Contains(rTypeIdentifier)) {
          _ = conflictsByTable.Add(rType);
        }
      }
      return conflictsByTable;
    }

    private List<StoredTypeInfo> GetRemovedTypes()
    {
      return (
        from type in extractedNonConnectorTypes
        where type.IsEntity && (!type.IsAbstract) && (!type.IsGeneric) && (!type.IsInterface)
        where IsRemoved(type)
        select type
        ).ToList();

      bool IsRemoved(StoredTypeInfo type)
      {
        return !typeMapping.ContainsKey(type);
      }
    }

    private List<StoredTypeInfo> GetMovedTypes()
    {
      return (
        from type in extractedNonConnectorTypes
        where type.IsEntity && (!type.IsAbstract) && (!type.IsGeneric) && (!type.IsInterface)
        where IsMovedToAnotherHierarchy(type)
        select type
        ).ToList();

      bool IsMovedToAnotherHierarchy(StoredTypeInfo oldType)
      {
        var newType = typeMapping.GetValueOrDefault(oldType);
        if (newType == null) {
          return false; // Type is removed
        }
        var oldRoot = oldType.Hierarchy.Root;
        if (oldRoot == null) {
          return false; // Just to be sure
        }
        var newRoot = newType.Hierarchy.Root;
        if (newRoot == null) {
          return false; // Just to be sure
        }
        return newRoot != typeMapping.GetValueOrDefault(oldRoot);
      }
    }

    private static IEnumerable<StoredTypeInfo> GetAffectedMappedTypes(StoredTypeInfo type, bool includeInheritors)
    {
      var result = EnumerableUtils.One(type);
      if (includeInheritors) {
        result = result.Concat(type.AllDescendants);
      }
      if (type.Hierarchy.InheritanceSchema == InheritanceSchema.ConcreteTable) {
        result = result.Where(t => !t.IsAbstract);
      }
      return result;
    }

    private static StoredTypeInfo[] GetAffectedMappedTypesAsArray(StoredTypeInfo type, bool includeInheritors)
    {
      var count = 1;
      var result = EnumerableUtils.One(type);
      if (includeInheritors) {
        result = result.Concat(type.AllDescendants);
        count += type.AllDescendants.Length;
      }
      return type.Hierarchy.InheritanceSchema == InheritanceSchema.ConcreteTable
        ? result.Where(t => !t.IsAbstract).ToArray()
        : result.ToArray(count);

    }

    private IdentityPair CreateIdentityPair(StoredTypeInfo removedType, StoredTypeInfo updatedType, int? typeIdOverride = null)
    {
      return new IdentityPair(GetColumnPath(updatedType, GetTypeIdMappingName(removedType)),
        (typeIdOverride ?? removedType.TypeId).ToString(),
        true);
    }

    private IdentityPair CreateIdentityPair(StoredTypeInfo removedType, StoredTypeInfo updatedType, Pair<string> columnPair)
    {
      return new IdentityPair(
          GetColumnPath(updatedType, columnPair.Second),
          GetColumnPath(removedType, columnPair.First), false);
    }

    private string GetTableName(StoredTypeInfo type)
    {
      return resolver.GetNodeName(
        type.MappingDatabase, type.MappingSchema, type.MappingName);
    }

    private string GetTablePath(StoredTypeInfo type) => $"Tables/{GetTableName(type)}";

    private string GetColumnPath(StoredTypeInfo type, string columnName)
    {
      var nodeName = GetTableName(type);
      // Due to current implementation of domain model FieldInfo.MappingName is not correct,
      // it has naming rules unapplied, corresponding ColumnInfo.Name however is correct.
      // StoredFieldInfo.MappingName is taken directly from FieldInfo.MappingName and thus is incorrect too.
      // We need to apply naming rules here to make it work.
      var actualColumnName = nameBuilder.ApplyNamingRules(columnName);
      return $"Tables/{nodeName}/Columns/{actualColumnName}";
    }

    private CleanupInfo GetCleanupInfo(StoredTypeInfo removedType,
      HashSet<StoredTypeInfo> conflictsByTable,
      bool isMovedToAnotherHierarchy)
    {
      var info = CleanupInfo.None;
      if (isMovedToAnotherHierarchy)
        info |= CleanupInfo.TypeMovedToAnotherHierarchy;
      var typeItselfConflicts = conflictsByTable.Contains(removedType);
      var anyAncestorConflicts = removedType.AllAncestors.Any(aType => conflictsByTable.Contains(aType));
      if (typeItselfConflicts || anyAncestorConflicts)
        info |= CleanupInfo.ConflictByTable;
      if (typeItselfConflicts && !anyAncestorConflicts)
        info |= CleanupInfo.RootOfConflict;
      return info;
    }

    private static string GetTypeIdMappingName(StoredTypeInfo type)
    {
      return type.AllFields
        .Single(field => field.Name.Equals(WellKnown.TypeIdFieldName, StringComparison.Ordinal))
        .MappingName;
    }

    private static Pair<StoredFieldInfo>[] JoinFieldsByOriginalName(
      ICollection<StoredFieldInfo> sources, ICollection<StoredFieldInfo> targets)
    {
      var arrayLength = sources.Count > targets.Count ? targets.Count : sources.Count;

      var result =
        from source in sources
        join target in targets
          on source.OriginalName equals target.OriginalName
        select new Pair<StoredFieldInfo>(source, target);
      return result.ToArray(arrayLength);
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
      if (sourceKeyFields.Length != targetKeyFields.Length) {
        return null;
      }
      var pairedKeyFields = JoinFieldsByOriginalName(sourceKeyFields, targetKeyFields);
      return pairedKeyFields.Length == sourceKeyFields.Length
        ? AssociateMappedFields(pairedKeyFields)
        : null;
    }

    private static Pair<string>[] AssociateMappedFields(params Pair<StoredFieldInfo>[] fieldsToProcess)
    {
      var result = new ChainedBuffer<Pair<StoredFieldInfo>>();
      var tasks = new Queue<Pair<StoredFieldInfo>>();
      foreach (var task in fieldsToProcess) {
        tasks.Enqueue(task);
      }

      while (tasks.TryDequeue(out var task)) {
        var source = task.First;
        var target = task.Second;
        // both fields are primitive -> put to result is types match
        if (source.IsPrimitive && target.IsPrimitive) {
          if (source.ValueType!=target.ValueType) {
            return null;
          }
          result.Add(task);
          continue;
        }
        // exactly one of the fields is primitive -> failure
        if (source.IsPrimitive || target.IsPrimitive) {
          return null;
        }
        // both fields are not primitive -> recursively process nested fields
        if (source.Fields.Length != target.Fields.Length) {
          return null;
        }
        var pairedNestedFields = JoinFieldsByOriginalName(source.Fields, target.Fields);
        if (pairedNestedFields.Length != source.Fields.Length) {
          return null;
        }

        foreach (var newTask in pairedNestedFields) {
          tasks.Enqueue(newTask);
        }
      }
      return result
        .SelectToArray(mapping => new Pair<string>(mapping.First.MappingName, mapping.Second.MappingName));
    }

    #endregion

    #region Exception helpers

    private static InvalidOperationException TypeNotFound(string name)
      => new InvalidOperationException(string.Format(Strings.ExTypeXIsNotFound, name));

    private static InvalidOperationException FieldNotFound(string typeName, string fieldName)
      => new InvalidOperationException(string.Format(Strings.ExFieldXYIsNotFound, typeName, fieldName));

    private static InvalidOperationException HintConflict(UpgradeHint hintOne, UpgradeHint hintTwo)
      => new InvalidOperationException(string.Format(Strings.ExHintXIsConflictingWithHintY, hintOne, hintTwo));

    private static InvalidOperationException KeysDoNotMatch(string typeOne, string typeTwo)
      => new InvalidOperationException(string.Format(Strings.ExKeyOfXDoesNotMatchKeyOfY, typeOne, typeTwo));

    private static InvalidOperationException FieldsDoNotMatch(StoredFieldInfo fieldOne, StoredFieldInfo fieldTwo)
    {
      var nameOne = fieldOne.DeclaringType.UnderlyingType + "." + fieldOne.Name;
      var nameTwo = fieldTwo.DeclaringType.UnderlyingType + "." + fieldTwo.Name;
      return new InvalidOperationException(string.Format(
        Strings.ExStructureOfFieldXDoesNotMatchStructureOfFieldY, nameOne, nameTwo));
    }

    private static InvalidOperationException TypeIsNotInHierarchy(string type)
      => new InvalidOperationException(string.Format(Strings.ExTypeXMustBelongToHierarchy, type));

    #endregion


    // Constructors

    public HintGenerator(UpgradeHintsProcessingResult hintsProcessingResult,
      HandlerAccessor handlers,
      MappingResolver resolver,
      StorageModel extractedStorageModel,
      StoredDomainModel currentDomainModel,
      StoredDomainModel extractedDomainModel)
    {
      ArgumentValidator.EnsureArgumentNotNull(hintsProcessingResult, nameof(hintsProcessingResult));
      ArgumentValidator.EnsureArgumentNotNull(handlers, nameof(handlers));
      ArgumentValidator.EnsureArgumentNotNull(resolver, nameof(resolver));
      ArgumentValidator.EnsureArgumentNotNull(extractedStorageModel, nameof(extractedStorageModel));
      ArgumentValidator.EnsureArgumentNotNull(currentDomainModel, nameof(currentDomainModel));
      ArgumentValidator.EnsureArgumentNotNull(extractedDomainModel, nameof(extractedDomainModel));

      typeMapping = hintsProcessingResult.TypeMapping;
      reverseTypeMapping = hintsProcessingResult.ReverseTypeMapping;
      fieldMapping = hintsProcessingResult.FieldMapping;
      hints = hintsProcessingResult.Hints;
      suspiciousTypes = hintsProcessingResult.SuspiciousTypes;
      currentNonConnectorTypes = hintsProcessingResult.CurrentNonConnectorTypes;
      extractedNonConnectorTypes = hintsProcessingResult.ExtractedNonConnectorTypes;


      this.extractedStorageModel = extractedStorageModel;
      this.resolver = resolver;
      nameBuilder = handlers.NameBuilder;

      currentModel = currentDomainModel;
      currentModel.UpdateReferences();

      extractedModelFields = new Dictionary<StoredTypeInfo, StoredFieldInfo[]>();
      extractedModel = extractedDomainModel;

      foreach (var type in extractedModel.Types) {
        extractedModelFields.Add(type, type.Fields.Flatten(f => f.Fields, null, true).ToArray());
      }
    }
  }
}
