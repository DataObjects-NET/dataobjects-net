// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.30

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Storage.Model.Stored;
using Xtensive.Storage.Model;
using Xtensive.Storage.Upgrade.Hints;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Generates <see cref="DataHint"/>s for clear data if some model types are removed.
  /// </summary>
  internal sealed class DataUpgrader
  {
    private StoredDomainModel storedModel;
    private DomainModel currentModel;
    private IEnumerable<UpgradeHint> hints;
    private readonly Dictionary<StoredTypeInfo, TypeInfo> typeMapping = 
      new Dictionary<StoredTypeInfo, TypeInfo>();
    private readonly Dictionary<StoredFieldInfo, FieldInfo> fieldMapping = 
      new Dictionary<StoredFieldInfo, FieldInfo>();
    private readonly Dictionary<StoredTypeInfo, List<StoredAssociationInfo>> associationMapping = 
      new Dictionary<StoredTypeInfo, List<StoredAssociationInfo>>();
    private readonly List<DataHint> updateHints = new List<DataHint>();
    private readonly List<DataHint> deleteHints = new List<DataHint>();

    /// <summary>
    /// Gets the data upgrade hints.
    /// </summary>
    /// <param name="storedModel">The stored model.</param>
    /// <param name="currentModel">The current domain model.</param>
    /// <param name="hints">The hints.</param>
    /// <returns>Update data hints.</returns>
    internal ReadOnlyList<DataHint> GetDataUpgradeHints(StoredDomainModel storedModel, 
      DomainModel currentModel, IEnumerable<UpgradeHint> hints)
    {
      this.storedModel = storedModel;
      this.currentModel = currentModel;
      this.hints = hints;
      updateHints.Clear();
      deleteHints.Clear();
      BuildTypeMapping();
      BuildAssociationMapping();
      
      var removedTypes = storedModel.Types.Where(type => type.IsEntity && IsRemoved(type)).ToArray();
      removedTypes.Apply(ProcessHierarchy);
      removedTypes.Apply(ProcessReferences);

      var dataHints = new List<DataHint>();
      dataHints.AddRange(updateHints);
      dataHints.AddRange(deleteHints);
      return new ReadOnlyList<DataHint>(dataHints);
    }

    private void ProcessHierarchy(StoredTypeInfo removedType)
    {
      var parents = GetAncestors(removedType);
      foreach (var parent in parents) {
        if (!removedType.Hierarchy.Types.Contains(parent) || IsRemoved(parent))
          continue;
        var deleteHint = new DeleteDataHint();
        deleteHint.SourceTablePath = GetTablePath(parent.MappingName);
        deleteHint.Identities.Add(new IdentityPair(
          GetColumnPath(parent.MappingName, WellKnown.TypeIdField),
          removedType.TypeId.ToString(), true));
        RegisterHint(deleteHint);
      }
    }

    private void ProcessReferences(StoredTypeInfo removedType)
    {
      List<StoredAssociationInfo> associations;
      if (associationMapping.TryGetValue(removedType, out associations))
        foreach (var association in associations) {
          switch (association.Multiplicity) {
          case Multiplicity.ZeroToOne:
          case Multiplicity.OneToOne:
          case Multiplicity.OneToMany:
          case Multiplicity.ManyToOne:
            ProcessAssociation(removedType, association);
            break;
          case Multiplicity.ManyToMany:
          case Multiplicity.ZeroToMany:
            ProcessConnectorAssociation(removedType, association);
            break;
          }
        }
    }

    private void ProcessAssociation(StoredTypeInfo removedType, StoredAssociationInfo association)
    {
      var updatedType = association.ReferencingField.DeclaringType;
      var removedPrimaryKeys = removedType.Hierarchy.Root.Fields.Where(field=>field.IsPrimaryKey).ToList();
      var updatedFields = association.ReferencingField.Fields.ToList();

      if (updatedFields.Count == 0)
        throw new InvalidOperationException();

      if (IsRemoved(updatedType) || IsRemoved(updatedFields[0]))
        return;

      if (removedPrimaryKeys.Count != updatedFields.Count)
        throw new InvalidOperationException();
      var keyPairs = removedPrimaryKeys.Zip(updatedFields);
      keyPairs.Apply(pair => {
        if (pair.First.ValueType != pair.Second.ValueType)
          throw new InvalidOperationException();
      });
      
      var updateHint = new UpdateDataHint();
      updateHint.SourceTablePath = GetTablePath(updatedType.MappingName);
      keyPairs.Apply(pair => {
        updateHint.UpdateParameter.Add(new Pair<string, object>(
          GetColumnPath(updatedType.MappingName, pair.Second.MappingName), null));
        updateHint.Identities.Add(new IdentityPair(
          GetColumnPath(updatedType.MappingName, pair.Second.MappingName),
          GetColumnPath(removedType.MappingName, pair.First.MappingName), false));
      });
      updateHint.Identities.Add(new IdentityPair(
        GetColumnPath(removedType.MappingName, WellKnown.TypeIdField),
        removedType.TypeId.ToString(), true));
      
      RegisterHint(updateHint);
    }

    private void ProcessConnectorAssociation(StoredTypeInfo removedType, StoredAssociationInfo association)
    {
      var connectorType = association.ConnectorType;
      var removedPrimaryKeys = removedType.Hierarchy.Root.Fields.Where(field=>field.IsPrimaryKey).ToList();
      var updatedFields = ResolveOldField(association.ConnectorType,
        association.IsMaster ? WellKnown.MasterField : WellKnown.SlaveField, true).Fields.ToList();

      if (updatedFields.Count == 0)
        throw new InvalidOperationException();

      if (IsRemoved(connectorType) || IsRemoved(updatedFields[0]))
        return;
      
      if (removedPrimaryKeys.Count != updatedFields.Count)
        throw new InvalidOperationException();
      var keyPairs = removedPrimaryKeys.Zip(updatedFields);
      keyPairs.Apply(pair => {
        if (pair.First.ValueType != pair.Second.ValueType)
          throw new InvalidOperationException();
      });
      
      var deleteHint = new DeleteDataHint();
      deleteHint.SourceTablePath = GetTablePath(connectorType.MappingName);
      keyPairs.Apply(pair => deleteHint.Identities.Add(new IdentityPair(
        GetColumnPath(connectorType.MappingName, pair.Second.MappingName),
        GetColumnPath(removedType.MappingName, pair.First.MappingName), false)));
      deleteHint.Identities.Add(new IdentityPair(
        GetColumnPath(removedType.MappingName, WellKnown.TypeIdField),
        removedType.TypeId.ToString(), true));

      RegisterHint(deleteHint);
    }
    
    private static IEnumerable<StoredTypeInfo> GetAncestors(StoredTypeInfo type)
    {
      var current = type;
      while (current.Ancestor!=null) {
        var result = current.Ancestor;
        current = result;
        yield return result;
      }
    }

    private IEnumerable<StoredAssociationInfo> GetAssociations(StoredTypeInfo type)
    {
      return storedModel.Associations
        .Where(association => association.ReferencedTypeName==type.Name);
    }

    private void RegisterHint(DataHint hint)
    {
      // TODO: Implement merge hints

      if (hints is UpdateDataHint)
        updateHints.Add(hint);
      else
        deleteHints.Add(hint);
    }

    private static string GetTablePath(string name)
    {
      return string.Format("Tables/{0}", name);
    }

    private static string GetColumnPath(string tableName, string columnName)
    {
      return string.Format("Tables/{0}/Columns/{1}", tableName, columnName);
    }

    private static StoredFieldInfo ResolveOldField(StoredTypeInfo type, string name, bool includeInheritedFields)
    {
      StoredFieldInfo result;
      StoredTypeInfo currentType = type;
      do {
        result = currentType.Fields.SingleOrDefault(f => f.Name==name);
        currentType = currentType.Ancestor;
      } while (includeInheritedFields && result==null && currentType!=null);
      if (result == null)
        throw new InvalidOperationException();
      return result;
    }

    private void BuildTypeMapping()
    {
      typeMapping.Clear();
      fieldMapping.Clear();
      foreach (var storedTypeInfo in storedModel.Types.Where(type=>type.IsEntity)) {
        var currentType = currentModel.Types.Entities
        .FirstOrDefault(typeInfo =>
          typeInfo.Name==storedTypeInfo.Name
            || hints.OfType<RenameTypeHint>()
              .Any(hint => hint.NewType==typeInfo.UnderlyingType
                && hint.OldType==storedTypeInfo.UnderlyingType));
        if (currentType != null) {
          typeMapping.Add(storedTypeInfo, currentType);
          BuildFieldMapping(storedTypeInfo, currentType);
        }
      }
    }
    
    private void BuildFieldMapping(StoredTypeInfo storedType, TypeInfo type)
    {
      foreach (var storedFieldInfo in storedType.Fields) {
        var currentField = type.Fields
        .FirstOrDefault(fieldInfo =>
          fieldInfo.Name==storedFieldInfo.Name
            || hints.OfType<RenameFieldHint>()
              .Any(hint => hint.TargetType == type.UnderlyingType 
                && hint.OldFieldName == storedFieldInfo.Name 
                && hint.NewFieldName == fieldInfo.Name));
        if (currentField != null) {
          if (storedFieldInfo.Fields.Count() == 0)
            fieldMapping.Add(storedFieldInfo, currentField);
          else
            storedFieldInfo.Fields.Apply(field => fieldMapping.Add(field, currentField));
        }
      }
    }

    private void BuildAssociationMapping()
    {
      associationMapping.Clear();
      foreach (var storedTypeInfo in storedModel.Types.Where(type=>type.IsEntity)) {
        var associations = new List<StoredAssociationInfo>();
        GetAssociations(storedTypeInfo).Apply(associations.Add);
        GetAncestors(storedTypeInfo).Apply(parent => GetAssociations(parent).Apply(associations.Add));
        if (associations.Count > 0)
          associationMapping.Add(storedTypeInfo, associations);
      }
    }
    
    private bool IsRemoved(StoredTypeInfo type)
    {
      return !typeMapping.ContainsKey(type);
    }

    private bool IsRemoved(StoredFieldInfo field)
    {
      return !fieldMapping.ContainsKey(field);
    }

  }
}