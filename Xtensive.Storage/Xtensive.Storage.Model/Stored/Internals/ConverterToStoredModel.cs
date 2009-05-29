// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.24

using System;
using System.Linq;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Model.Stored
{
  internal class ConverterToStoredModel
  {
    public StoredDomainModel Convert(DomainModel model)
    {
      return new StoredDomainModel {Types = model.Types.Select(t => ConvertType(t)).ToArray()};
    }

    private static StoredTypeInfo ConvertType(TypeInfo source)
    {
      var declaredFields = source.Fields
        .Where(f => f.IsDeclared && !f.IsNested)
        .ToArray();
      var sourceAncestor = source.GetAncestor();
      string hierarchyRoot = null;
      if (source.Hierarchy != null && source.Hierarchy.Root == source)
        hierarchyRoot = source.Hierarchy.Schema.ToString();
      var associations = source.GetOutgoingAssociations()
        .Where(a => declaredFields.Contains(a.ReferencingField))
        .Select(a => ConvertAssociation(a))
        .ToArray();
      // hack: for SingleTable hierarchies mapping name is not set correctly
      // and always should be taken from hierarchy root
      var mappingNameSource =
        source.Hierarchy!=null && source.Hierarchy.Schema==InheritanceSchema.SingleTable
          ? source.Hierarchy.Root
          : source;
      var result = new StoredTypeInfo
        {
          Name = source.Name,
          UnderlyingType = GetTypeFullName(source.UnderlyingType),
          TypeId = source.TypeId,
          MappingName = mappingNameSource.MappingName,
          IsEntity = source.IsEntity,
          IsAbstract = source.IsAbstract,
          IsInterface = source.IsInterface,
          IsStructure = source.IsStructure,
          IsSystem = source.IsSystem,
          AncestorName = sourceAncestor != null ? sourceAncestor.Name : null,
          Associations = associations,
          HierarchyRoot = hierarchyRoot,
          Fields = declaredFields.Select(f => ConvertField(f)).ToArray(),
        };
      return result;
    }

    private static StoredAssociationInfo ConvertAssociation(AssociationInfo source)
    {
      var result = new StoredAssociationInfo
        {
          Name = source.Name,
          MappingName = source.UnderlyingType != null ? source.UnderlyingType.MappingName : null,
          IsMaster = source.IsMaster,
          MultiplicityName = source.Multiplicity.ToString(),
          ReferencedTypeName = source.ReferencedType.Name,
          ReferencingFieldName = source.ReferencingField.Name,
          ReversedName = source.Reversed != null ? source.Reversed.Name : null,
        };
      return result;
    }

    private static StoredFieldInfo ConvertField(FieldInfo source)
    {
      var result = new StoredFieldInfo
        {
          Name = source.Name,
          MappingName = source.MappingName,
          PropertyName = source.UnderlyingProperty != null ? source.UnderlyingProperty.Name : null,
          ValueType = source.ValueType.GetFullName(),
          ItemType = GetTypeFullName(source.ItemType),
          Fields = source.Fields.Select(f => ConvertField(f)).ToArray(),
          Length = source.Length.HasValue ? source.Length.Value : 0,
          IsCollatable = source.IsCollatable,
          IsEntity = source.IsEntity,
          IsEntitySet = source.IsEntitySet,
          IsEnum = source.IsEnum,
          IsExplicit = source.IsExplicit,
          IsInterfaceImplementation = source.IsInterfaceImplementation,
          IsLazyLoad = source.IsLazyLoad,
          IsNullable = source.IsNullable,
          IsPrimaryKey = source.IsPrimaryKey,
          IsPrimitive = source.IsPrimitive,
          IsStructure = source.IsStructure,
          IsSystem = source.IsSystem,
          IsTranslatable = source.IsTranslatable,
          IsTypeId = source.IsTypeId,
        };
      return result;
    }

    private static string GetTypeFullName(Type type)
    {
      return type!=null ? type.GetFullName() : null;
    }
  }
}