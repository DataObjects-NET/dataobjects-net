// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.24

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Reflection;

namespace Xtensive.Orm.Model.Stored
{
  internal sealed class ConverterToStoredModel
  {
    private HashSet<string> processedAssociations;

    public StoredDomainModel Convert(DomainModel model, Func<TypeInfo, bool> filter)
    {
      processedAssociations = new HashSet<string>();
      var typesToProcess = model.Types.AsEnumerable();
      if (filter!=null)
        typesToProcess = typesToProcess.Where(filter);
      var processedTypes = typesToProcess.Select(ConvertType).ToArray();
      return new StoredDomainModel {Types = processedTypes};
    }

    private StoredTypeInfo ConvertType(TypeInfo source)
    {
      var inheritedAssociatedFields = new HashSet<string>();
      var declaredFields = source.Fields
        .Where(field => field.IsDeclared && !field.IsNested)
        .ToArray();
      var sourceAncestor = source.GetAncestor();
      string hierarchyRoot = null;
      if (source.Hierarchy!=null && source.Hierarchy.Root==source)
        hierarchyRoot = source.Hierarchy.InheritanceSchema.ToString();
      var associations = source.GetOwnerAssociations()
        .Where(a => {
          if (processedAssociations.Contains(a.Name))
            return false;
          if (declaredFields.Contains(a.OwnerField))
            return true;
          if (a.IsPaired) {
            inheritedAssociatedFields.Add(a.OwnerField.Name);
            return true;
          }
          return false;
        })
        .Select(ConvertAssociation)
        .ToArray();
      foreach (var association in associations)
        processedAssociations.Add(association.Name);

      var fields = source.Fields
        .Where(field => (field.IsDeclared && !field.IsNested) || inheritedAssociatedFields.Contains(field.Name))
        .ToArray();

      // hack: for SingleTable hierarchies mapping name is not set correctly
      // and always should be taken from hierarchy root
      var mappingNameSource =
        source.Hierarchy!=null && source.Hierarchy.InheritanceSchema==InheritanceSchema.SingleTable
          ? source.Hierarchy.Root
          : source;
      var result = new StoredTypeInfo {
        Name = source.Name,
        UnderlyingType = GetTypeFullName(source.UnderlyingType),
        TypeId = source.TypeId,
        MappingName = mappingNameSource.MappingName,
        MappingSchema = source.MappingSchema ?? string.Empty,
        MappingDatabase = source.MappingDatabase ?? string.Empty,
        IsEntity = source.IsEntity,
        IsAbstract = source.IsAbstract,
        IsInterface = source.IsInterface,
        IsStructure = source.IsStructure,
        IsSystem = source.IsSystem,
        AncestorName = sourceAncestor!=null ? sourceAncestor.Name : null,
        Associations = associations,
        HierarchyRoot = hierarchyRoot,
        Fields = fields.Select(ConvertField).ToArray(),
      };
      return result;
    }

    private static StoredAssociationInfo ConvertAssociation(AssociationInfo source)
    {
      var result = new StoredAssociationInfo {
        Name = source.Name,
        MappingName = source.AuxiliaryType!=null ? source.AuxiliaryType.MappingName : null,
        ConnectorTypeName = source.AuxiliaryType!=null ? source.AuxiliaryType.Name : null,
        IsMaster = source.IsMaster,
        MultiplicityName = source.Multiplicity.ToString(),
        ReferencedTypeName = source.TargetType.Name,
        ReferencingFieldName = source.OwnerField.Name,
        ReversedName = source.Reversed!=null ? source.Reversed.Name : null,
      };
      return result;
    }

    private static StoredFieldInfo ConvertField(FieldInfo source)
    {
      var nestedFields = source.Fields
        .Where(field => field.Parent==source);
      var result = new StoredFieldInfo {
        Name = source.Name,
        MappingName = source.MappingName,
        PropertyName = source.UnderlyingProperty!=null ? source.UnderlyingProperty.Name : null,
        OriginalName = source.OriginalName,
        ValueType = source.ValueType.GetFullName(),
        ItemType = GetTypeFullName(source.ItemType),
        Fields = nestedFields.Select(ConvertField).ToArray(),
        Length = source.Length.HasValue ? source.Length.Value : 0,
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