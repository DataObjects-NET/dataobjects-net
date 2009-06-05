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
using Xtensive.Storage.Upgrade.Hints;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Upgrade
{
  internal sealed class HintGenerator
  {
    private readonly StoredDomainModel storedModel;
    private readonly DomainModel currentModel;
 
    private readonly Dictionary<StoredTypeInfo, TypeInfo> typeMapping
      = new Dictionary<StoredTypeInfo, TypeInfo>();
    private readonly Dictionary<TypeInfo, StoredTypeInfo> backwardTypeMapping
      = new Dictionary<TypeInfo, StoredTypeInfo>();
    private readonly Dictionary<StoredFieldInfo, FieldInfo> fieldMapping
      = new Dictionary<StoredFieldInfo, FieldInfo>();
    private readonly List<Hint> resultHints = new List<Hint>();

    public IEnumerable<Hint> GenerateHints(IEnumerable<UpgradeHint> upgradeHints)
    {
      var typeRenames = upgradeHints.OfType<RenameTypeHint>().ToArray();
      var fieldRenames = upgradeHints.OfType<RenameFieldHint>().ToArray();
      var fieldCopyHints = upgradeHints.OfType<CopyFieldHint>().ToArray();

      ValidateRenameTypeHints(typeRenames);
      BuildTypeMapping(typeRenames);

      ValidateRenameFieldHints(fieldRenames);
      BuildFieldMapping(fieldRenames);
      BuildAssociationMapping();

      ValidateCopyFieldHints(fieldCopyHints);
      GenerateRenameTableHints();
      GenerateRenameColumnHints();
      GenerateCopyColumnHints(fieldCopyHints);
      return resultHints;
    }

    #region Validation

    private void ValidateRenameTypeHints(IEnumerable<RenameTypeHint> hints)
    {
      var sourceTypes = new Dictionary<string, RenameTypeHint>();
      var targetTypes = new Dictionary<Type, RenameTypeHint>();
      foreach (var hint in hints) {
        // checking that types exists in models
        if (!currentModel.Types.Contains(hint.NewType))
          throw TypeIsNotFound(hint.NewType.GetFullName());
        if (!storedModel.Types.Any(type => type.UnderlyingType==hint.OldType))
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
        if (!currentModel.Types.Contains(hint.TargetType))
          throw TypeIsNotFound(hint.TargetType.GetFullName());
        var targetType = currentModel.Types[hint.TargetType];
        var sourceType = backwardTypeMapping[targetType];
        if (!sourceType.Fields.Any(field => field.Name==hint.OldFieldName))
          throw FieldIsNotFound(sourceType.UnderlyingType, hint.OldFieldName);
        if (!targetType.Fields.Contains(hint.NewFieldName))
          throw FieldIsNotFound(hint.TargetType.GetFullName(), hint.NewFieldName);
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
        if (!storedModel.Types.Any(type => type.UnderlyingType==hint.SourceType))
          throw TypeIsNotFound(hint.SourceType);
        var sourceType = storedModel.Types.Single(type => type.UnderlyingType==hint.SourceType);
        if (!sourceType.AllFields.Any(field => field.Name==hint.SourceField))
          throw FieldIsNotFound(sourceType.UnderlyingType, hint.SourceField);
        // checking destination type/field
        if (!currentModel.Types.Contains(hint.DestinationType))
          throw TypeIsNotFound(hint.DestinationType.GetFullName());
        var destinationType = currentModel.Types[hint.DestinationType];
        if (!destinationType.Fields.Contains(hint.DestinationField))
          throw FieldIsNotFound(hint.DestinationType.GetFullName(), hint.DestinationField);
      }
    }

    #endregion

    #region Mapping

    private void MapType(StoredTypeInfo oldType, TypeInfo newType)
    {
      if (typeMapping.ContainsKey(oldType))
        return;
      typeMapping[oldType] = newType;
      backwardTypeMapping[newType] = oldType;
    }

    private void MapField(StoredFieldInfo oldField, FieldInfo newField)
    {
      if (fieldMapping.ContainsKey(oldField))
        return;
      fieldMapping[oldField] = newField;
    }

    private void BuildTypeMapping(IEnumerable<RenameTypeHint> renames)
    {
      var oldConnectorTypes = storedModel.Associations
        .Select(association => association.ConnectorType)
        .Where(type => type!=null)
        .ToHashSet();
      var newConnectorTypes = currentModel.Associations
        .Select(association => association.UnderlyingType)
        .Where(type => type!=null)
        .ToHashSet();

      var oldModelTypes = storedModel.Types
        .Where(type => !oldConnectorTypes.Contains(type));
      var newModelTypes = currentModel.Types
        .Where(type => !newConnectorTypes.Contains(type))
        .ToDictionary(type => type.UnderlyingType.GetFullName());

      foreach (var oldType in oldModelTypes) {
        var maybeHint = renames
          .FirstOrDefault(hint => hint.OldType==oldType.UnderlyingType);
        var newTypeName = maybeHint!=null
          ? maybeHint.NewType.GetFullName()
          : oldType.UnderlyingType;
        TypeInfo newType;
        if (newModelTypes.TryGetValue(newTypeName, out newType))
          MapType(oldType, newType);
      }
    }

 
    private void BuildFieldMapping(IEnumerable<RenameFieldHint> renames)
    {
      foreach (var pair in typeMapping)
        BuildFieldMapping(renames, pair.Key, pair.Value);

      foreach (var pair in fieldMapping.ToList())
        ProcessFieldMapping(pair.Key, pair.Value);
    }

    private void BuildFieldMapping(IEnumerable<RenameFieldHint> renames, StoredTypeInfo oldType, TypeInfo newType)
    {
      var newFields = newType.Fields
        .Where(field => field.IsDeclared && !field.IsNested)
        .ToDictionary(field => field.Name);
      foreach (var oldField in oldType.Fields) {
        var maybeHint = renames
          .FirstOrDefault(hint => hint.OldFieldName==oldField.Name && hint.TargetType==newType.UnderlyingType);
        var newFieldName = maybeHint!=null
          ? maybeHint.NewFieldName
          : oldField.Name;
        FieldInfo newField;
        if (newFields.TryGetValue(newFieldName, out newField))
          MapField(oldField, newField);
      }
    }

    private void ProcessFieldMapping(StoredFieldInfo oldField, FieldInfo newField)
    {
      if (oldField.Fields.Length==0)
        return;
      var oldValueType = storedModel.Types
        .Single(type => type.UnderlyingType==oldField.ValueType);
      foreach (var oldNestedField in oldField.Fields) {
        var oldNestedFieldOriginalName = oldNestedField.OriginalName;
        var oldNestedFieldOrigin = oldValueType.AllFields
          .Single(field => field.Name==oldNestedFieldOriginalName);
        var newNestedFieldOrigin = fieldMapping[oldNestedFieldOrigin];
        var newNestedField = newField.Fields
          .Single(field => field.OriginalName==newNestedFieldOrigin.Name
               && field.Parent==newField);
        MapField(oldNestedField, newNestedField);
        ProcessFieldMapping(oldNestedField, newNestedField);
      }
    }

    private void BuildAssociationMapping()
    {
      var oldAssociations = storedModel.Associations
        .Where(association => association.ConnectorType!=null);
      foreach (var oldAssociation in oldAssociations) {
        if (typeMapping.ContainsKey(oldAssociation.ConnectorType))
          continue;
        var oldReferencingField = oldAssociation.ReferencingField;
        var oldReferencingType = oldReferencingField.DeclaringType;
        TypeInfo newReferencingType;
        if (!typeMapping.TryGetValue(oldReferencingType, out newReferencingType))
          continue;
        FieldInfo newReferencingField;
        if (!fieldMapping.TryGetValue(oldReferencingField, out newReferencingField))
          newReferencingField = newReferencingType.Fields
            .SingleOrDefault(field => field.Name==oldReferencingField.Name);
        if (newReferencingField==null)
          continue;
        var newAssociation = newReferencingField.Association;
        MapType(oldAssociation.ConnectorType, newAssociation.UnderlyingType);

        var oldMaster = oldAssociation.ConnectorType.Fields.Single(field => field.Name==WellKnown.MasterField);
        var newMaster = newAssociation.UnderlyingType.Fields.Single(field => field.Name==WellKnown.MasterField);
        var oldSlave = oldAssociation.ConnectorType.Fields.Single(field => field.Name==WellKnown.SlaveField);
        var newSlave = newAssociation.UnderlyingType.Fields.Single(field => field.Name==WellKnown.SlaveField);

        MapField(oldMaster, newMaster);
        ProcessFieldMapping(oldMaster, newMaster);
        MapField(oldSlave, newSlave);
        ProcessFieldMapping(oldSlave, newSlave);
      }
    }

    #endregion

    #region Hint generation

    private void GenerateRenameTableHints()
    {
      var mappingsToProcess = typeMapping
        .Where(type => type.Key.IsEntity)
        .Where(type => type.Key.Hierarchy.Schema!=InheritanceSchema.SingleTable || type.Key.Hierarchy.Root==type.Key);
      foreach (var mapping in mappingsToProcess) {
        var oldTable = mapping.Key.MappingName;
        var newTable = mapping.Value.MappingName;
        if (oldTable != newTable)
          resultHints.Add(new RenameHint(GetTablePath(oldTable), GetTablePath(newTable)));
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
          throw new ArgumentOutOfRangeException();
        }
      }
    }

    private void GenerateRenameFieldHint(StoredFieldInfo oldField, FieldInfo newField, TypeInfo newType, bool recursive)
    {
      if (oldField.MappingName==newField.MappingName)
        return;

      if (!recursive) {
        resultHints.Add(new RenameHint(
          GetColumnPath(backwardTypeMapping[newType].MappingName, oldField.MappingName),
          GetColumnPath(newType.MappingName, newField.MappingName)));
        return;
      }

      var typesToProcess = EnumerableUtils.One(newType)
        .Concat(currentModel.Types.FindDescendants(newType, true));
      foreach (var newInheritor in typesToProcess) {
        StoredTypeInfo oldInheritor;
        if (!backwardTypeMapping.TryGetValue(newInheritor, out oldInheritor))
          continue;
        resultHints.Add(new RenameHint(
          GetColumnPath(oldInheritor.MappingName, oldField.MappingName),
          GetColumnPath(newInheritor.MappingName, newField.MappingName)));
      }
    }
    
    private void GenerateCopyColumnHints(IEnumerable<CopyFieldHint> hints)
    {
      foreach (var hint in hints)
        TranslateCopyColumnHint(hint);
    }

    private void TranslateCopyColumnHint(CopyFieldHint hint)
    {
      // searching for all required objects
      var destinationTypeName = hint.DestinationType.GetFullName();
      var destinationType = currentModel.Types[hint.DestinationType];
      var destinationField = destinationType.Fields.Single(field => field.Name==hint.DestinationField);
      var sourceType = storedModel.Types.Single(type => type.UnderlyingType==hint.SourceType);
      var sourceField = sourceType.AllFields.Single(field => field.Name==hint.SourceField);
      
      var sourceHierarchy = sourceType.Hierarchy;
      var destinationHierarchy = destinationType.Hierarchy;

      if (!sourceField.IsPrimitive || !destinationField.IsPrimitive)
        throw new NotImplementedException();

      // checking that types have hierarchies
      if (sourceHierarchy==null)
        throw TypeIsNotInHierarchy(hint.SourceType);
      if (destinationHierarchy == null)
        throw TypeIsNotInHierarchy(destinationTypeName);
      
      // building sets of key columns
      var sourceKeys = sourceHierarchy.Root.Fields
        .Where(f => f.IsPrimaryKey)
        .ToDictionary(
          f => f.Name,
          f => new Pair<string>(f.MappingName, f.ValueType));
      var destinationKeys = destinationHierarchy.Root.Fields
        .Where(f => f.IsPrimaryKey)
        .ToDictionary(
          f => f.Name,
          f => new Pair<string>(f.MappingName, f.ValueType.GetFullName()));

      // checking that keys match: all key columns must have the same name and the same type.
      if (sourceKeys.Count != destinationKeys.Count)
        throw KeysDoNotMatch(sourceType.Name, destinationTypeName);
      var keyPairs = new List<Pair<string>>();
      foreach (var sourceKey in sourceKeys) {
        Pair<string> destinationKey;
        var mismatch =
          !destinationKeys.TryGetValue(sourceKey.Key, out destinationKey) ||
            destinationKey.Second!=sourceKey.Value.Second; // comparing types
        if (mismatch)
          throw KeysDoNotMatch(sourceType.Name, destinationTypeName);
        keyPairs.Add(new Pair<string>(sourceKey.Value.First, destinationKey.First));
      }

      // building destination table names
      var destinationTables = EnumerableUtils.One(destinationType.MappingName);
      if (destinationHierarchy.Schema==InheritanceSchema.ConcreteTable)
        destinationTables = destinationTables
          .Concat(destinationType.GetDescendants(true).Select(type => type.MappingName));

      // generating result hints
      var sourceTable = sourceType.MappingName;
      foreach (var destinationTable in destinationTables) {
        var identityParameters = keyPairs.Select(pair => new IdentityPair(
            GetColumnPath(sourceTable, pair.First),
            GetColumnPath(destinationTable, pair.Second), false));
        var copyDataHint = new CopyDataHint {SourceTablePath = GetTablePath(sourceTable)};
        copyDataHint.CopiedColumns.Add(new Pair<string>(
          GetColumnPath(sourceTable, sourceField.MappingName),
          GetColumnPath(destinationTable, destinationField.MappingName)));
        copyDataHint.Identities.AddRange(identityParameters);
        resultHints.Add(copyDataHint);
      }
    }

    #endregion


    // Constructors

    public HintGenerator(StoredDomainModel storedModel, DomainModel currentModel)
    {
      this.storedModel = storedModel;
      this.currentModel = currentModel;
    }
    
    #region Static helpers

    private static string GetTablePath(string name)
    {
      return string.Format("Tables/{0}", name);
    }

    private static string GetColumnPath(string tableName, string columnName)
    {
      return string.Format("Tables/{0}/Columns/{1}", tableName, columnName);
    }

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

    private static InvalidOperationException TypeIsNotInHierarchy(string type)
    {
      return new InvalidOperationException(string.Format(
        Strings.ExTypeXMustBelongToHierarchy, type));
    }

    #endregion
  }
}