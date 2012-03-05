// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.02

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Building.DependencyGraph;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Reflection;
using Xtensive.Tuples;
using FieldAttributes = Xtensive.Orm.Model.FieldAttributes;
using FieldInfo = Xtensive.Orm.Model.FieldInfo;

namespace Xtensive.Orm.Building.Builders
{
  internal static class TypeBuilder
  {
    /// <summary>
    /// Builds the <see cref="TypeInfo"/> instance, its key fields and <see cref="HierarchyInfo"/> for hierarchy root.
    /// </summary>
    /// <param name="typeDef"><see cref="TypeDef"/> instance.</param>
    internal static TypeInfo BuildType(BuildingContext context, TypeDef typeDef)
    {
      using (Log.InfoRegion(Strings.LogBuildingX, typeDef.UnderlyingType.GetShortName())) {

        var typeInfo = new TypeInfo(context.Model, typeDef.Attributes) {
          UnderlyingType = typeDef.UnderlyingType,
          Name = typeDef.Name,
          MappingName = typeDef.MappingName,
          MappingDatabase = typeDef.MappingDatabase,
          MappingSchema = typeDef.MappingSchema,
          HasVersionRoots = typeDef.UnderlyingType.GetInterfaces().Any(type => type==typeof (IHasVersionRoots))
        };
        context.Model.Types.Add(typeInfo);

        // Registering connections between type & its ancestors
        var node = context.DependencyGraph.TryGetNode(typeDef);
        if (node!=null) {
          foreach (var edge in node.OutgoingEdges.Where(e => e.Kind==EdgeKind.Implementation || e.Kind==EdgeKind.Inheritance)) {
            var baseType = context.Model.Types[edge.Head.Value.UnderlyingType];
            switch (edge.Kind) {
              case EdgeKind.Inheritance:
                context.Model.Types.RegisterInheritance(baseType, typeInfo);
                break;
              case EdgeKind.Implementation:
                context.Model.Types.RegisterImplementation(baseType, typeInfo);
                break;
            }
          }
        }

        if (typeDef.IsEntity) {
          var hierarchyDef = context.ModelDef.FindHierarchy(typeDef);

          // Is type a hierarchy root?
          if (typeInfo.UnderlyingType==hierarchyDef.Root.UnderlyingType) {
            foreach (var keyField in hierarchyDef.KeyFields) {
              var fieldInfo = BuildDeclaredField(context, typeInfo, typeDef.Fields[keyField.Name]);
              fieldInfo.IsPrimaryKey = true;
            }
            typeInfo.Hierarchy = BuildHierarchyInfo(context, typeInfo, hierarchyDef);
          }
          else {
            var root = context.Model.Types[hierarchyDef.Root.UnderlyingType];
            typeInfo.Hierarchy = root.Hierarchy;
            foreach (var fieldInfo in root.Fields.Where(f => f.IsPrimaryKey && f.Parent==null))
              BuildInheritedField(context, typeInfo, fieldInfo);
          }
        }
        else if (typeDef.IsInterface) {
          var hierarchyDef = context.ModelDef.FindHierarchy(typeDef.Implementors[0]);
          foreach (var keyField in hierarchyDef.KeyFields) {
            var fieldInfo = BuildDeclaredField(context, typeInfo, typeDef.Fields[keyField.Name]);
            fieldInfo.IsPrimaryKey = true;
          }
        }

        return typeInfo;
      }
    }

    public static void BuildTypeDiscriminatorMap(BuildingContext context, TypeDef typeDef, TypeInfo typeInfo)
    {
      if (typeDef.TypeDiscriminatorValue!=null) {
        var targetField = typeInfo.Fields.SingleOrDefault(f => f.IsTypeDiscriminator && f.Parent==null);
        if (targetField==null)
          throw new DomainBuilderException(string.Format(Strings.ExTypeDiscriminatorFieldIsNotFoundForXType, typeInfo.Name));
        if (targetField.IsEntity) {
          targetField = targetField.Fields.First();
          targetField.IsTypeDiscriminator = true;
        }
        typeInfo.TypeDiscriminatorValue = ValueTypeBuilder.AdjustValue(
          targetField, targetField.ValueType, typeDef.TypeDiscriminatorValue);
        typeInfo.Hierarchy.TypeDiscriminatorMap.RegisterTypeMapping(typeInfo, typeInfo.TypeDiscriminatorValue);
      }
      if (typeDef.IsDefaultTypeInHierarchy)
        typeInfo.Hierarchy.TypeDiscriminatorMap.RegisterDefaultType(typeInfo);
    }

    public static void BuildFields(BuildingContext context, TypeDef typeDef, TypeInfo typeInfo)
    {
      if (typeInfo.IsInterface) {
        var sourceFields = typeInfo.GetInterfaces()
          .SelectMany(i => i.Fields)
          .Where(f => !f.IsPrimaryKey && f.Parent == null);
        foreach (var srcField in sourceFields) {
          if (!typeInfo.Fields.Contains(srcField.Name))
            BuildInheritedField(context, typeInfo, srcField);
        }
      }
      else {
        var ancestor = typeInfo.GetAncestor();
        if (ancestor != null) {
          foreach (var srcField in ancestor.Fields.Where(f => !f.IsPrimaryKey && f.Parent == null)) {
            FieldDef fieldDef;
            if (typeDef.Fields.TryGetValue(srcField.Name, out fieldDef)) {
              if (fieldDef.UnderlyingProperty == null)
                throw new DomainBuilderException(
                  String.Format(Strings.ExFieldXIsAlreadyDefinedInTypeXOrItsAncestor, fieldDef.Name, typeInfo.Name));
              var getMethod = fieldDef.UnderlyingProperty.GetGetMethod()
                ?? fieldDef.UnderlyingProperty.GetGetMethod(true);
              if ((getMethod.Attributes & MethodAttributes.NewSlot) == MethodAttributes.NewSlot)
                BuildDeclaredField(context, typeInfo, fieldDef);
              else
                BuildInheritedField(context, typeInfo, srcField);
            }
            else
              BuildInheritedField(context, typeInfo, srcField);
          }
          foreach (var pair in ancestor.FieldMap)
            typeInfo.FieldMap.Add(pair.Key, typeInfo.Fields[pair.Value.Name]);
        }
      }

      foreach (var fieldDef in typeDef.Fields) {
        FieldInfo field;
        if (typeInfo.Fields.TryGetValue(fieldDef.Name, out field)) {
          if (field.ValueType!=fieldDef.ValueType)
            throw new DomainBuilderException(
              String.Format(Strings.ExFieldXIsAlreadyDefinedInTypeXOrItsAncestor, fieldDef.Name, typeInfo.Name));
        }
        else
          BuildDeclaredField(context, typeInfo, fieldDef);
      }
      typeInfo.Columns.AddRange(typeInfo.Fields.Where(f => f.Column!=null).Select(f => f.Column));

      if (typeInfo.IsEntity && !IsAuxiliaryType(typeInfo)) {
        foreach (var @interface in typeInfo.GetInterfaces())
          BuildFieldMap(context, @interface, typeInfo);
      }
    }

    #region Private members

    private static void BuildFieldMap(BuildingContext context, TypeInfo @interface, TypeInfo implementor)
    {
      foreach (var field in @interface.Fields.Where(f => f.IsDeclared)) {
        string explicitName = context.NameBuilder.BuildExplicitFieldName(field.DeclaringType, field.Name);
        FieldInfo implField;
        if (implementor.Fields.TryGetValue(explicitName, out implField))
          implField.IsExplicit = true;
        else {
          if (!implementor.Fields.TryGetValue(field.Name, out implField))
            throw new DomainBuilderException(
              String.Format(Strings.TypeXDoesNotImplementYZField, implementor.Name, @interface.Name, field.Name));
        }

        implField.IsInterfaceImplementation = true;

        if (!implementor.FieldMap.ContainsKey(field))
          implementor.FieldMap.Add(field, implField);
        else
          implementor.FieldMap.Override(field, implField);

        var declaringType = implField.DeclaringType;
        var declaringField = implField.DeclaringField;
        if (implField.IsInherited && declaringType.IsEntity) {
          declaringField.IsInterfaceImplementation = true;
          if (!declaringType.FieldMap.ContainsKey(field))
            declaringType.FieldMap.Add(field, declaringField);
        }
      }
    }

    private static FieldInfo BuildDeclaredField(BuildingContext context, TypeInfo type, FieldDef fieldDef)
    {
      Log.Info(Strings.LogBuildingDeclaredFieldXY, type.Name, fieldDef.Name);

      var fieldInfo = new FieldInfo(type, fieldDef.Attributes)
      {
        UnderlyingProperty = fieldDef.UnderlyingProperty,
        Name = fieldDef.Name,
        OriginalName = fieldDef.Name,
        MappingName = fieldDef.MappingName,
        ValueType = fieldDef.ValueType,
        ItemType = fieldDef.ItemType,
        Length = fieldDef.Length,
        Scale = fieldDef.Scale,
        Precision = fieldDef.Precision
      };

      type.Fields.Add(fieldInfo);

      if (fieldInfo.IsEntitySet) {
        AssociationBuilder.BuildAssociation(context, fieldDef, fieldInfo);
        return fieldInfo;
      }

      if (fieldInfo.IsEntity) {
        var fields = context.Model.Types[fieldInfo.ValueType].Fields.Where(f => f.IsPrimaryKey);
        // Adjusting default value if any
        if (fields.Count() == 1 && fieldDef.DefaultValue != null) {
          fieldInfo.DefaultValue = ValueTypeBuilder.AdjustValue(fieldInfo, fields.First().ValueType, fieldDef.DefaultValue);
        }
        BuildNestedFields(context, null, fieldInfo, fields);

        if (!IsAuxiliaryType(type))
          AssociationBuilder.BuildAssociation(context, fieldDef, fieldInfo);

        // Adjusting type discriminator field for references
        if (fieldDef.IsTypeDiscriminator)
          type.Hierarchy.TypeDiscriminatorMap.Field = fieldInfo.Fields.First();
      }

      if (fieldInfo.IsStructure)
        BuildNestedFields(context, null, fieldInfo, context.Model.Types[fieldInfo.ValueType].Fields);

      if (fieldInfo.IsPrimitive) {
        fieldInfo.DefaultValue = fieldDef.DefaultValue;
        fieldInfo.Column = BuildDeclaredColumn(context, fieldInfo);
        if (fieldDef.IsTypeDiscriminator)
          type.Hierarchy.TypeDiscriminatorMap.Field = fieldInfo;
      }
      return fieldInfo;
    }

    private static void BuildInheritedField(BuildingContext context, TypeInfo type, FieldInfo inheritedField)
    {
      Log.Info(Strings.LogBuildingInheritedFieldXY, type.Name, inheritedField.Name);
      var field = inheritedField.Clone();
      type.Fields.Add(field);
      field.ReflectedType = type;
      field.DeclaringType = inheritedField.DeclaringType;
      field.IsInherited = true;

      BuildNestedFields(context, inheritedField, field, inheritedField.Fields);

      if (inheritedField.Column!=null)
        field.Column = BuildInheritedColumn(context, field, inheritedField.Column);
    }

    private static void BuildNestedFields(BuildingContext context, FieldInfo source, FieldInfo target, IEnumerable<FieldInfo> fields)
    {
      var buffer = fields.ToList();

      foreach (var field in buffer) {
        var clone = field.Clone();
        if (target.SkipVersion)
          clone.SkipVersion = true;
        clone.IsSystem = false;
        clone.IsLazyLoad = field.IsLazyLoad || target.IsLazyLoad;
        if (target.IsDeclared) {
          clone.Name = context.NameBuilder.BuildNestedFieldName(target, field);
          clone.OriginalName = field.OriginalName;
          // One-field reference
          if (target.IsEntity && buffer.Count == 1) {
            clone.MappingName = target.MappingName;
            clone.DefaultValue = target.DefaultValue;
          }
          else
            clone.MappingName = context.NameBuilder.BuildMappingName(target, field);
        }
        if (target.Fields.Contains(clone.Name))
          continue;
        clone.Parent = target;
        target.ReflectedType.Fields.Add(clone);

        if (field.IsStructure || field.IsEntity) {
          BuildNestedFields(context, source, clone, field.Fields);
          foreach (FieldInfo clonedFields in clone.Fields)
            target.Fields.Add(clonedFields);
        }
        else {
          if (field.Column!=null)
            clone.Column = BuildInheritedColumn(context, clone, field.Column);
        }
        if (clone.IsEntity && !IsAuxiliaryType(clone.ReflectedType)) {
          var origin = context.Model.Associations
            .Find(context.Model.Types[field.ValueType], true)
            .FirstOrDefault(a => a.OwnerField==field);
          if (origin!=null && !clone.IsInherited) {
            AssociationBuilder.BuildAssociation(context, origin, clone);
            context.DiscardedAssociations.Add(origin);
          }
        }
        if (!clone.IsStructure && !clone.IsEntitySet && 0!=(clone.Attributes & FieldAttributes.Indexed)) {
          var typeDef = context.ModelDef.Types[target.DeclaringType.UnderlyingType];
          var attribute = new IndexAttribute(clone.Name);
          var index = ModelDefBuilder.DefineIndex(context, typeDef, attribute);
          if (typeDef.Indexes.Contains(index.Name))
            throw new DomainBuilderException(
              string.Format(Strings.ExIndexWithNameXIsAlreadyRegistered, index.Name));

          typeDef.Indexes.Add(index);
          Log.Info(Strings.LogIndexX, index.Name);
        }
      }
    }

    private static bool IsAuxiliaryType(TypeInfo type)
    {
      if (!type.IsEntity)
        return false;
      var underlyingBaseType = type.UnderlyingType.BaseType;
      return underlyingBaseType!=null
        && underlyingBaseType.IsGenericType
          && underlyingBaseType.GetGenericTypeDefinition()==typeof (EntitySetItem<,>);
    }

    private static ColumnInfo BuildDeclaredColumn(BuildingContext context, FieldInfo field)
    {
      ColumnInfo column;
      if (field.ValueType==typeof (Key))
        column = new ColumnInfo(field, typeof (string));
      else
        column = new ColumnInfo(field);
      column.Name = context.NameBuilder.BuildColumnName(field, column);
      column.IsNullable = field.IsNullable;

      return column;
    }

    private static ColumnInfo BuildInheritedColumn(BuildingContext context, FieldInfo field, ColumnInfo ancestor)
    {
      var column = ancestor.Clone();
      column.Field = field;
      column.Name = context.NameBuilder.BuildColumnName(field, ancestor);
      column.IsDeclared = field.IsDeclared;
      column.IsPrimaryKey = field.IsPrimaryKey;
      column.IsNullable = field.IsNullable;
      column.IsSystem = field.IsSystem;
      column.DefaultValue = field.DefaultValue;

      return column;
    }

    private static HierarchyInfo BuildHierarchyInfo(BuildingContext context, TypeInfo root, HierarchyDef hierarchyDef)
    {
      var key = BuildKeyInfo(context, root, hierarchyDef);
      var schema = hierarchyDef.Schema;

      // Optimization. It there is the only class in hierarchy then ConcreteTable schema is applied
      if (schema != InheritanceSchema.ConcreteTable) {
        var node = context.DependencyGraph.TryGetNode(hierarchyDef.Root);
        // No dependencies => no descendants
        if (node == null || node.IncomingEdges.Where(e => e.Kind == EdgeKind.Inheritance).Count() == 0)
          schema = InheritanceSchema.ConcreteTable;
      }

      var typeDiscriminatorField = hierarchyDef.Root.Fields.Where(f => f.IsTypeDiscriminator).FirstOrDefault();
      var typeDiscriminatorMap = typeDiscriminatorField!=null ? new TypeDiscriminatorMap() : null;

      var hierarchy = new HierarchyInfo(root, key, schema, typeDiscriminatorMap) {
        Name = root.Name,
      };
      key.Hierarchy = hierarchy; // Setting backreference
      context.Model.Hierarchies.Add(hierarchy);
      return hierarchy;
    }

    private static KeyInfo BuildKeyInfo(BuildingContext context, TypeInfo root, HierarchyDef hierarchyDef)
    {
      var keyFields = new ReadOnlyList<FieldInfo>((
        from field in root.Fields
        where field.IsPrimaryKey
        orderby field.MappingInfo.Offset
        select field
        ).ToList());

      var keyColumns = new ReadOnlyList<ColumnInfo>((
        from field in keyFields
        where field.Column!=null
        select field.Column
        ).ToList());

      var keyTupleDescriptor = TupleDescriptor.Create(keyColumns.Select(c => c.ValueType));

      var typeIdColumnIndex = -1;
      if (hierarchyDef.IncludeTypeId)
        for (int i = 0; i < keyColumns.Count; i++)
          if (keyColumns[i].Field.IsTypeId)
            typeIdColumnIndex = i;

      Type generatorType = null;
      string generatorLocalName = null;
      string generatorName = null;

      var requiresGenerator =
        hierarchyDef.KeyGeneratorType!=null
        && keyFields.All(f => f.Parent==null); // = does not contain foreign key(s)

      if (requiresGenerator) {
        var nameBuilder = context.NameBuilder;
        generatorType = hierarchyDef.KeyGeneratorType;
        generatorLocalName = nameBuilder.BuildKeyGeneratorLocalName(hierarchyDef, keyTupleDescriptor, typeIdColumnIndex);
        generatorName = NameBuilder.BuildKeyGeneratorName(generatorLocalName, hierarchyDef.Root.MappingDatabase);
      }

      var key = new KeyInfo(
        root.Name,
        keyFields,
        keyColumns,
        generatorType,
        generatorLocalName,
        generatorName,
        keyTupleDescriptor,
        typeIdColumnIndex);

      // The most complex part: now we're trying to find out if
      // this KeyInfo is actually quite similar to another one.
      // If so, we clone its EqualityIdentifier and Sequence.
      var keyGenerator = GetKeyGenerator(context, key.GeneratorType, key.GeneratorName);
      KeyInfo existingKey = null;
      if (keyGenerator!=null)
        context.KeyGenerators.TryGetValue(keyGenerator, out existingKey);

      if (existingKey!=null) {
        // There is an existing key like this, with the same key generator
        key.IsFirstAmongSimilarKeys = false;
        key.EqualityIdentifier = existingKey.EqualityIdentifier;
        key.Sequence = existingKey.Sequence;
        return key;
      }

      // No existing key like this was found
      key.IsFirstAmongSimilarKeys = true;
      key.EqualityIdentifier = new object();

      if (keyGenerator!=null) {
        context.KeyGenerators.Add(keyGenerator, key);
        var sequenceIncrement = keyGenerator.SequenceIncrement;
        var mappingName = context.NameBuilder.BuildSequenceName(key);
        if (sequenceIncrement.HasValue)
          key.Sequence = new SequenceInfo(key.GeneratorName) {
            MappingDatabase = hierarchyDef.Root.MappingDatabase,
            MappingSchema = context.Configuration.DefaultSchema,
            MappingName = mappingName,
            Seed = sequenceIncrement.Value,
            Increment = sequenceIncrement.Value
          };
      }

      return key;
    }

    private static KeyGenerator GetKeyGenerator(BuildingContext context, Type generatorType, string generatorName)
    {
      return generatorName==null 
        ? null
        : (KeyGenerator) context.BuilderConfiguration.Services.Get(generatorType, generatorName);
    }

    #endregion
  }
}