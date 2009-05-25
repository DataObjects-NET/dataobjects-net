// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.24

using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Storage.Model.Stored
{
  internal class ConverterToStoredModel
  {
    private readonly Dictionary<TypeInfo, StoredTypeInfo> types
      = new Dictionary<TypeInfo, StoredTypeInfo>();
    private readonly Dictionary<HierarchyInfo, StoredHierarchyInfo> hierarchies
      = new Dictionary<HierarchyInfo, StoredHierarchyInfo>();
    private readonly Dictionary<FieldInfo, StoredFieldInfo> fields
      = new Dictionary<FieldInfo, StoredFieldInfo>();
    private readonly Dictionary<AssociationInfo, StoredAssociationInfo> associations
      = new Dictionary<AssociationInfo, StoredAssociationInfo>();

    public StoredDomainModel Convert(DomainModel model)
    {
      foreach (var type in model.Types)
        ConvertType(type);

      foreach (var hierarchy in model.Hierarchies)
        ConvertHierarchy(hierarchy);
    
      foreach (var association in model.Associations)
        ConvertAssociation(association);

      foreach (var field in model.Types.SelectMany(t => t.Fields.Where(f => f.IsDeclared)))
        ConvertField(field);

      return new StoredDomainModel
        {
          Associations = associations.Values.ToArray(),
          Fields = fields.Values.ToArray(),
          Hierarchies = hierarchies.Values.ToArray(),
          Types = types.Values.ToArray()
        };
    }

    private void ConvertType(TypeInfo source)
    {
      var sourceAncestor = source.GetAncestor();
      var result = new StoredTypeInfo
        {
          Name = NamingHelper.GetFullTypeName(source),
          TypeId = source.TypeId,
          MappingName = source.MappingName,
          HierarchyName = source.Hierarchy != null ? source.Hierarchy.Name : null,
          IsEntity = source.IsEntity,
          IsAbstract = source.IsAbstract,
          IsInterface = source.IsInterface,
          IsStructure = source.IsStructure,
          IsSystem = source.IsSystem,
          FieldNames = ExtractFieldNames(source.Fields),
          AncestorName = sourceAncestor != null ? NamingHelper.GetFullTypeName(sourceAncestor) : null,
        };
      types.Add(source, result);
    }

    private void ConvertHierarchy(HierarchyInfo source)
    {
      var result = new StoredHierarchyInfo
        {
          Name = source.Name,
          RootName = NamingHelper.GetFullTypeName(source.Root),
          SchemaName = source.Schema.ToString(),
        };
      hierarchies.Add(source, result);
    }

    private void ConvertAssociation(AssociationInfo source)
    {
      var result = new StoredAssociationInfo
        {
          Name = source.Name,
          IsMaster = source.IsMaster,
          MultiplicityName = source.Multiplicity.ToString(),
          ReferencedTypeName = NamingHelper.GetFullTypeName(source.ReferencedType),
          ReferencingFieldName = NamingHelper.GetFullFieldName(source.ReferencingField),
          UnderlyingTypeName = source.UnderlyingType != null ? NamingHelper.GetFullTypeName(source.UnderlyingType) : null,
          ReversedName = source.Reversed != null ? source.Reversed.Name : null,
        };
      associations.Add(source, result);
    }

    private void ConvertField(FieldInfo source)
    {
      var result = new StoredFieldInfo
        {
          Name = NamingHelper.GetFullFieldName(source),
          MappingName = source.MappingName,
          IsCollatable = source.IsCollatable,
          IsEntity = source.IsEntity,
          IsEntitySet = source.IsEntitySet,
          IsEnum = source.IsEnum,
          IsExplicit = source.IsExplicit,
          IsInterfaceImplementation = source.IsInterfaceImplementation,
          IsLazyLoad = source.IsLazyLoad,
          IsNested = source.IsNested,
          IsNullable = source.IsNullable,
          IsPrimaryKey = source.IsPrimaryKey,
          IsPrimitive = source.IsPrimitive,
          IsStructure = source.IsStructure,
          IsSystem = source.IsSystem,
          IsTranslatable = source.IsTranslatable,
          IsTypeId = source.IsTypeId,
          FieldNames = ExtractFieldNames(source.Fields),
          UnderlyingPropertyName = source.UnderlyingProperty != null ? source.UnderlyingProperty.Name : null,
          ValueTypeName = NamingHelper.GetFullTypeName(source.ValueType),
          ItemTypeName = source.ItemType != null ? NamingHelper.GetFullTypeName(source.ItemType) : null,
          Length = source.Length,
        };
      fields.Add(source, result);
    }

    private static string[] ExtractFieldNames(IEnumerable<FieldInfo> fieldSequence)
    {
      return fieldSequence.Where(f => f.IsDeclared).Select(f => f.Name).ToArray();
    }
  }
}