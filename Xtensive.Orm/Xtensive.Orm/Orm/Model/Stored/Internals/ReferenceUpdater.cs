// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.22

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Orm.Model.Stored
{
  internal class ReferenceUpdater
  {
    private Dictionary<string, StoredTypeInfo> types;
    private Dictionary<string, StoredAssociationInfo> associations;
    private Dictionary<string, StoredFieldInfo> fieldMap;
    private Dictionary<StoredTypeInfo, StoredHierarchyInfo> hierarchies;

    public void UpdateReferences(StoredDomainModel model)
    {
      types = new Dictionary<string, StoredTypeInfo>();
      associations = new Dictionary<string, StoredAssociationInfo>();
      hierarchies = new Dictionary<StoredTypeInfo, StoredHierarchyInfo>();

      // building hierarchies map

      var generatedHierarchies = model.Types
        .Where(t => t.IsHierarchyRoot)
        .Select(t => new StoredHierarchyInfo {Root = t});
      
      foreach (var hierarchy in generatedHierarchies)
        hierarchies.Add(hierarchy.Root, hierarchy);

      // building types map

      foreach (var type in model.Types)
        types.Add(type.Name, type);
      
      // building types

      foreach (var type in model.Types)
        UpdateTypeAncestor(type);

      foreach (var type in model.Types) {
        UpdateTypeHierarchy(type);
        UpdateTypeDescendants(type);
        UpdateTypeAllAncestors(type);
      }

      foreach (var type in model.Types)
        UpdateTypeAllDescendants(type);

      // building hierarchies
     
      foreach (var hierarchy in hierarchies.Values) {
        UpdateHierarchySchema(hierarchy);
        UpdateHierarchyTypes(hierarchy);
      }
      
      // building fields / associations

      foreach (var type in model.Types) {
        fieldMap = new Dictionary<string, StoredFieldInfo>();
        if (type.Fields == null)
          type.Fields = ArrayUtils<StoredFieldInfo>.EmptyArray;
        UpdateTypeAllFields(type);
        foreach (var field in type.Fields) {
          fieldMap.Add(field.Name, field);
          UpdateNestedFields(field);
          UpdateFieldDeclaringType(field, type);
        }
        foreach (var association in type.Associations) {
          associations.Add(association.Name, association);
          UpdateAssociationMultiplicity(association);
          UpdateAssociationReferencingField(association);
        }
      }

      foreach (var association in associations.Values) {
        UpdateAssociationReversed(association);
        UpdadeAssociationConnectorType(association);
        UpdateAssociationReferencedType(association);
      }

      model.Hierarchies = hierarchies.Values.ToArray();
      model.Associations = associations.Values.ToArray();

      types = null;
      fieldMap = null;
      associations = null;
      hierarchies = null;
    }

    private void UpdateTypeAncestor(StoredTypeInfo type)
    {
      if (string.IsNullOrEmpty(type.AncestorName))
        return;
      type.Ancestor = types[type.AncestorName];
    }

    private void UpdateTypeHierarchy(StoredTypeInfo type)
    {
      var currentType = type;

      while (currentType != null && !currentType.IsHierarchyRoot)
        currentType = currentType.Ancestor;

      if (currentType != null && currentType.IsHierarchyRoot)
        type.Hierarchy = hierarchies[currentType];
    }

    private void UpdateTypeAllFields(StoredTypeInfo type)
    {
      var fields = new List<StoredFieldInfo>();
      var currentType = type;
      while (currentType != null) {
        fields.AddRange(currentType.Fields);
        currentType = currentType.Ancestor;
      }
      type.AllFields = fields.ToArray();
    }

    private void UpdateTypeDescendants(StoredTypeInfo type)
    {
      type.Descendants = types.Values
        .Where(t => t.Ancestor==type)
        .ToArray();
    }
    
    private void UpdateTypeAllAncestors(StoredTypeInfo type)
    {
      var result = new List<StoredTypeInfo>();
      var currentAncestor = type.Ancestor;
      while (currentAncestor != null) {
        result.Add(currentAncestor);
        currentAncestor = currentAncestor.Ancestor;
      }
      result.Reverse();
      type.AllAncestors = result.ToArray();
    }
    
    private void UpdateTypeAllDescendants(StoredTypeInfo type)
    {
      type.AllDescendants = type.Descendants.Flatten(t => t.Descendants, item => { }, true).ToArray();
    }

    private void UpdateHierarchySchema(StoredHierarchyInfo hierarchy)
    {
      hierarchy.InheritanceSchema = (InheritanceSchema) Enum.Parse(typeof (InheritanceSchema), hierarchy.Root.HierarchyRoot);
    }

    private void UpdateHierarchyTypes(StoredHierarchyInfo hierarchy)
    {
      hierarchy.Types = types.Values.Where(t => t.Hierarchy==hierarchy).ToArray();
    }

    private void UpdateNestedFields(StoredFieldInfo field)
    {
      if (field.Fields == null) {
        field.Fields = ArrayUtils<StoredFieldInfo>.EmptyArray;
        return;
      }

      foreach (var nestedField in field.Fields) {
        nestedField.Parent = field;
        UpdateNestedFields(nestedField);
      }
    }

    private void UpdateFieldDeclaringType(StoredFieldInfo field, StoredTypeInfo type)
    {
      field.DeclaringType = type;
      foreach (var nestedField in field.Fields)
        UpdateFieldDeclaringType(nestedField, type);
    }

    private void UpdateAssociationMultiplicity(StoredAssociationInfo association)
    {
      if (string.IsNullOrEmpty(association.MultiplicityName))
        throw new ArgumentException();
      association.Multiplicity = (Multiplicity) Enum.Parse(typeof (Multiplicity), association.MultiplicityName);
    }

    private void UpdateAssociationReferencingField(StoredAssociationInfo association)
    {
      if (string.IsNullOrEmpty(association.ReferencingFieldName))
        return;
      association.ReferencingField = fieldMap[association.ReferencingFieldName];
    }

    private void UpdateAssociationReversed(StoredAssociationInfo association)
    {
      if (string.IsNullOrEmpty(association.ReversedName))
        return;
      StoredAssociationInfo r;
      if (associations.TryGetValue(association.ReversedName, out r))
        association.Reversed = r;
    }

    private void UpdateAssociationReferencedType(StoredAssociationInfo association)
    {
      if (string.IsNullOrEmpty(association.ReferencedTypeName))
        return;
      association.ReferencedType = types[association.ReferencedTypeName];
    }

    private void UpdadeAssociationConnectorType(StoredAssociationInfo association)
    {
      if (string.IsNullOrEmpty(association.ConnectorTypeName))
        return;
      association.ConnectorType = types[association.ConnectorTypeName];
    }
  }
}
