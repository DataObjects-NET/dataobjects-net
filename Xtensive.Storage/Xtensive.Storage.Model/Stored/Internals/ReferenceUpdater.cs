// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.22

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Model.Stored
{
  internal class ReferenceUpdater
  {
    private readonly Dictionary<object, Dictionary<string, StoredNode>> nodeLookups
      = new Dictionary<object, Dictionary<string, StoredNode>>();

    private T FindNode<T>(IEnumerable<T> nodes, string name) where T : StoredNode
    {
      Dictionary<string, StoredNode> lookup;

      if (!nodeLookups.TryGetValue(nodes, out lookup)) {
        lookup = new Dictionary<string, StoredNode>();
        foreach (var item in nodes)
          lookup.Add(item.Name, item);
        nodeLookups.Add(nodes, lookup);
      }

      return (T) lookup[name];
    }

    public void UpdateReferences(StoredDomainModel model)
    {
      // fixing null arrays

      if (model.Associations == null)
        model.Associations = ArrayUtils<StoredAssociationInfo>.EmptyArray;

      if (model.Hierarchies == null)
        model.Hierarchies = ArrayUtils<StoredHierarchyInfo>.EmptyArray;

      if (model.Types == null)
        model.Types = ArrayUtils<StoredTypeInfo>.EmptyArray;

      if (model.Fields == null)
        model.Fields = ArrayUtils<StoredFieldInfo>.EmptyArray;
      
      // fixing enums

      foreach (var association in model.Associations)
        association.Multiplicity = (Multiplicity) Enum.Parse(
          typeof (Multiplicity),
          association.MultiplicityName);

      foreach (var hierarchy in model.Hierarchies)
        hierarchy.Schema = (InheritanceSchema) Enum.Parse(
          typeof (InheritanceSchema),
          hierarchy.SchemaName);

      // fixing references

      foreach (var type in model.Types) {
        if (!string.IsNullOrEmpty(type.HierarchyName))
          type.Hierarchy = FindNode(model.Hierarchies, type.HierarchyName);
        type.Fields = type.FieldNames
          .Select(f => FindNode(model.Fields, NamingHelper.GetFullFieldName(type.Name, f)))
          .ToArray();
        if (!string.IsNullOrEmpty(type.AncestorName))
          type.Ancestor = FindNode(model.Types, type.AncestorName);
      }

      foreach (var association in model.Associations) {
        association.ReferencedType = FindNode(model.Types, association.ReferencedTypeName);
        association.ReferencingField = FindNode(model.Fields, association.ReferencingFieldName);
        if (!string.IsNullOrEmpty(association.ReversedName))
          association.Reversed = FindNode(model.Associations, association.ReversedName);
      }

      foreach (var hierarchy in model.Hierarchies) {
        hierarchy.Root = FindNode(model.Types, hierarchy.RootName);
        hierarchy.Types = model.Types.Where(t => t.Hierarchy==hierarchy).ToArray();
      }

      foreach (var field in model.Fields) {
        if (!field.Fields.IsNullOrEmpty()) {
          field.Fields = field.FieldNames
            .Select(f => FindNode(model.Fields, NamingHelper.ChangeFieldName(field, f)))
            .ToArray();
          foreach (var nestedField in field.Fields)
            nestedField.Parent = field;
        }
      }
    }
  }
}