// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.02

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Building.DependencyGraph;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Orm.Validation;
using Xtensive.Reflection;
using Xtensive.Tuples;
using FieldAttributes = Xtensive.Orm.Model.FieldAttributes;
using FieldInfo = Xtensive.Orm.Model.FieldInfo;

namespace Xtensive.Orm.Building.Builders
{
  internal sealed class TypeBuilder
  {
    private readonly BuildingContext context;
    private readonly HashSet<string> processedKeyGenerators = new HashSet<string>();
    private readonly Dictionary<string, object> keyEqualityIdentifiers = new Dictionary<string, object>();
    private readonly Dictionary<string, SequenceInfo> sequences = new Dictionary<string, SequenceInfo>();
    private readonly Dictionary<string, KeyGeneratorConfiguration> keyGeneratorConfigurations;

    /// <summary>
    /// Builds the <see cref="TypeInfo"/> instance, its key fields and <see cref="HierarchyInfo"/> for hierarchy root.
    /// </summary>
    /// <param name="typeDef"><see cref="TypeDef"/> instance.</param>
    public TypeInfo BuildType(TypeDef typeDef)
    {
      using (BuildLog.InfoRegion(Strings.LogBuildingX, typeDef.UnderlyingType.GetShortName())) {

        var typeInfo = new TypeInfo(context.Model, typeDef.Attributes) {
          UnderlyingType = typeDef.UnderlyingType,
          Name = typeDef.Name,
          MappingName = typeDef.MappingName,
          MappingDatabase = typeDef.MappingDatabase,
          MappingSchema = typeDef.MappingSchema,
          HasVersionRoots = typeDef.UnderlyingType.GetInterfaces().Any(type => type==typeof (IHasVersionRoots)),
          Validators = typeDef.Validators,
        };

        if (typeInfo.IsEntity && DeclaresOnValidate(typeInfo.UnderlyingType))
          typeInfo.Validators.Add(new EntityValidator());

        if (typeDef.StaticTypeId!=null)
          typeInfo.TypeId = typeDef.StaticTypeId.Value;

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
              var fieldInfo = BuildDeclaredField(typeInfo, typeDef.Fields[keyField.Name]);
              fieldInfo.IsPrimaryKey = true;
            }
            typeInfo.Hierarchy = BuildHierarchyInfo(typeInfo, hierarchyDef);
          }
          else {
            var root = context.Model.Types[hierarchyDef.Root.UnderlyingType];
            typeInfo.Hierarchy = root.Hierarchy;
            foreach (var fieldInfo in root.Fields.Where(f => f.IsPrimaryKey && f.Parent==null))
              BuildInheritedField(typeInfo, fieldInfo);
          }
        }
        else if (typeDef.IsInterface) {
          var hierarchyDef = context.ModelDef.FindHierarchy(typeDef.Implementors[0]);
          foreach (var keyField in hierarchyDef.KeyFields) {
            var fieldInfo = BuildDeclaredField(typeInfo, typeDef.Fields[keyField.Name]);
            fieldInfo.IsPrimaryKey = true;
          }
        }

        return typeInfo;
      }
    }

    public void BuildTypeDiscriminatorMap(TypeDef typeDef, TypeInfo typeInfo)
    {
      if (typeDef.TypeDiscriminatorValue!=null) {
        var targetField = typeInfo.Fields.SingleOrDefault(f => f.IsTypeDiscriminator && f.Parent==null);
        if (targetField==null)
          throw new DomainBuilderException(string.Format(Strings.ExTypeDiscriminatorIsNotFoundForXType, typeInfo.Name));
        if (targetField.IsEntity) {
          targetField = targetField.Fields.First();
          targetField.IsTypeDiscriminator = true;
        }
        typeInfo.TypeDiscriminatorValue = ValueTypeBuilder.AdjustValue(targetField, targetField.ValueType, typeDef.TypeDiscriminatorValue);
        typeInfo.Hierarchy.TypeDiscriminatorMap.RegisterTypeMapping(typeInfo, typeInfo.TypeDiscriminatorValue);
      }

      if (typeDef.IsDefaultTypeInHierarchy)
        typeInfo.Hierarchy.TypeDiscriminatorMap.RegisterDefaultType(typeInfo);
    }

    public void BuildFields(TypeDef typeDef, TypeInfo typeInfo)
    {
      if (typeInfo.IsInterface) {
        var sourceFields = typeInfo.GetInterfaces()
          .SelectMany(i => i.Fields)
          .Where(f => !f.IsPrimaryKey && f.Parent == null);
        foreach (var srcField in sourceFields) {
          if (!typeInfo.Fields.Contains(srcField.Name))
            BuildInheritedField(typeInfo, srcField);
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
                BuildDeclaredField(typeInfo, fieldDef);
              else
                BuildInheritedField(typeInfo, srcField);
            }
            else
              BuildInheritedField(typeInfo, srcField);
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
          BuildDeclaredField(typeInfo, fieldDef);
      }
      typeInfo.Columns.AddRange(typeInfo.Fields.Where(f => f.Column!=null).Select(f => f.Column));

      if (typeInfo.IsEntity && !IsAuxiliaryType(typeInfo)) {
        foreach (var @interface in typeInfo.GetInterfaces())
          BuildFieldMap(@interface, typeInfo);
      }
    }

    #region Private/internal methods

    private void BuildFieldMap(TypeInfo @interface, TypeInfo implementor)
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

    private FieldInfo BuildDeclaredField(TypeInfo type, FieldDef fieldDef)
    {
      BuildLog.Info(Strings.LogBuildingDeclaredFieldXY, type.Name, fieldDef.Name);

      var fieldInfo = new FieldInfo(type, fieldDef.Attributes) {
        UnderlyingProperty = fieldDef.UnderlyingProperty,
        Name = fieldDef.Name,
        OriginalName = fieldDef.Name,
        MappingName = fieldDef.MappingName,
        ValueType = fieldDef.ValueType,
        ItemType = fieldDef.ItemType,
        Length = fieldDef.Length,
        Scale = fieldDef.Scale,
        Precision = fieldDef.Precision,
        Validators = fieldDef.Validators,
      };

      if (fieldInfo.IsStructure && DeclaresOnValidate(fieldInfo.ValueType))
        fieldInfo.Validators.Add(new StructureFieldValidator());

      if (fieldInfo.IsEntitySet && DeclaresOnValidate(fieldInfo.ValueType))
        fieldInfo.Validators.Add(new EntitySetFieldValidator());

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
        BuildNestedFields(null, fieldInfo, fields);

        if (!IsAuxiliaryType(type))
          AssociationBuilder.BuildAssociation(context, fieldDef, fieldInfo);

        // Adjusting type discriminator field for references
        if (fieldDef.IsTypeDiscriminator)
          type.Hierarchy.TypeDiscriminatorMap.Field = fieldInfo.Fields.First();
      }

      if (fieldInfo.IsStructure) {
        BuildNestedFields(null, fieldInfo, context.Model.Types[fieldInfo.ValueType].Fields);

        var structureFullTextIndex = context.ModelDef.FullTextIndexes.TryGetValue(fieldInfo.UnderlyingProperty.PropertyType);
        if (structureFullTextIndex!=null) {
          var hierarchyType = fieldInfo.DeclaringType.UnderlyingType;
          var structureType = fieldInfo.UnderlyingProperty.PropertyType;
          TypeInfo hierarchyTypeInfo;
          TypeInfo structureTypeInfo;

          if (!context.Model.Types.TryGetValue(hierarchyType, out hierarchyTypeInfo))
            throw new Exception(string.Format(Strings.ExCouldNotFindTypeXInDomainModel, hierarchyType.Name));
          if (!context.Model.Types.TryGetValue(structureType, out structureTypeInfo))
            throw new Exception(string.Format(Strings.ExCouldNotFindTypeXInDomainModel, structureType.Name));

          var currentIndex = context.ModelDef.FullTextIndexes.TryGetValue(hierarchyTypeInfo.UnderlyingType);
          if (currentIndex==null) {
            currentIndex = new FullTextIndexDef(context.ModelDef.Types.TryGetValue(type.UnderlyingType));
            context.ModelDef.FullTextIndexes.Add(currentIndex);
          }
          foreach (var field in structureFullTextIndex.Fields) {
            var realFieldInfo = structureTypeInfo.Fields[field.Name];
            var realTypeField = fieldInfo.DeclaringType.StructureFieldMapping[new Pair<FieldInfo>(fieldInfo, realFieldInfo)];
            currentIndex.Fields.Add(new FullTextFieldDef(realTypeField.Name, field.IsAnalyzed) {
              Configuration = field.Configuration,
              TypeFieldName = field.TypeFieldName
            });
          }
        }
      }
        

      if (fieldInfo.IsPrimitive) {
        fieldInfo.DefaultValue = fieldDef.DefaultValue;
        fieldInfo.DefaultSqlExpression = fieldDef.DefaultSqlExpression;
        fieldInfo.Column = BuildDeclaredColumn(fieldInfo);
        if (fieldDef.IsTypeDiscriminator)
          type.Hierarchy.TypeDiscriminatorMap.Field = fieldInfo;
      }

      return fieldInfo;
    }

    private void BuildInheritedField(TypeInfo type, FieldInfo inheritedField)
    {
      BuildLog.Info(Strings.LogBuildingInheritedFieldXY, type.Name, inheritedField.Name);
      var field = inheritedField.Clone();
      type.Fields.Add(field);
      field.ReflectedType = type;
      field.DeclaringType = inheritedField.DeclaringType;
      field.IsInherited = true;

      BuildNestedFields(inheritedField, field, inheritedField.Fields);

      if (inheritedField.Column!=null)
        field.Column = BuildInheritedColumn(field, inheritedField.Column);
    }

    private void BuildNestedFields(FieldInfo source, FieldInfo target, IEnumerable<FieldInfo> fields)
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
          BuildNestedFields(source, clone, field.Fields);
          foreach (FieldInfo clonedFields in clone.Fields)
            target.Fields.Add(clonedFields);
        }
        else {
          if (field.Column!=null)
            clone.Column = BuildInheritedColumn(clone, field.Column);
        }
        if (target.IsStructure && clone.IsEntity && !IsAuxiliaryType(clone.ReflectedType)) {
          var origin = context.Model.Associations
            .Find(context.Model.Types[field.ValueType], true)
            .FirstOrDefault(a => a.OwnerField==field);
          if (origin!=null && !clone.IsInherited) {
            AssociationBuilder.BuildAssociation(context, origin, clone);
            context.DiscardedAssociations.Add(origin);
          }
        }
        if (!clone.IsStructure && !clone.IsEntitySet && !target.ReflectedType.IsInterface && 0!=(clone.Attributes & FieldAttributes.Indexed)) {
          var typeDef = context.ModelDef.Types[target.DeclaringType.UnderlyingType];
          var attribute = new IndexAttribute(clone.Name);
          var index = context.ModelDefBuilder.DefineIndex(typeDef, attribute);
          if (typeDef.Indexes.Contains(index.Name))
            throw new DomainBuilderException(
              string.Format(Strings.ExIndexWithNameXIsAlreadyRegistered, index.Name));

          typeDef.Indexes.Add(index);
          BuildLog.Info(Strings.LogIndexX, index.Name);
        }
      }
    }

    private bool IsAuxiliaryType(TypeInfo type)
    {
      if (!type.IsEntity)
        return false;
      var underlyingBaseType = type.UnderlyingType.BaseType;
      return underlyingBaseType!=null
        && underlyingBaseType.IsGenericType
          && underlyingBaseType.GetGenericTypeDefinition()==typeof (EntitySetItem<,>);
    }

    private ColumnInfo BuildDeclaredColumn(FieldInfo field)
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

    private ColumnInfo BuildInheritedColumn(FieldInfo field, ColumnInfo ancestor)
    {
      var column = ancestor.Clone();
      column.Field = field;
      column.Name = context.NameBuilder.BuildColumnName(field, ancestor);
      column.IsDeclared = field.IsDeclared;
      column.IsPrimaryKey = field.IsPrimaryKey;
      column.IsNullable = field.IsNullable;
      column.IsSystem = field.IsSystem;
      column.DefaultValue = field.DefaultValue;
      column.DefaultSqlExpression = field.DefaultSqlExpression;

      return column;
    }

    private HierarchyInfo BuildHierarchyInfo(TypeInfo root, HierarchyDef hierarchyDef)
    {
      var key = BuildKeyInfo(root, hierarchyDef);
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

    private KeyInfo BuildKeyInfo(TypeInfo root, HierarchyDef hierarchyDef)
    {
      var keyFields = root.Fields
        .Where(field => field.IsPrimaryKey)
        .OrderBy(field => field.MappingInfo.Offset)
        .ToList();

      var keyColumns = keyFields
        .Where(field => field.Column!=null)
        .Select(field => field.Column)
        .ToList();

      var keyTupleDescriptor = TupleDescriptor.Create(keyColumns.Select(c => c.ValueType));
      var typeIdColumnIndex = -1;
      if (hierarchyDef.IncludeTypeId)
        for (int i = 0; i < keyColumns.Count; i++)
          if (keyColumns[i].Field.IsTypeId)
            typeIdColumnIndex = i;

      var key = new KeyInfo(root.Name, keyFields, keyColumns, keyTupleDescriptor, typeIdColumnIndex);
      var generatorKind = hierarchyDef.KeyGeneratorKind;

      // Force absence of key generator if key is a reference.
      if (key.ContainsForeignKeys)
        generatorKind = KeyGeneratorKind.None;

      if (generatorKind==KeyGeneratorKind.Default) {
        var canBeHandled = key.SingleColumnType!=null && KeyGeneratorFactory.IsSupported(key.SingleColumnType);
        // Force absence of key generator if key can not be handled by standard keygen.
        if (!canBeHandled)
          generatorKind = KeyGeneratorKind.None;
      }

      if (generatorKind==KeyGeneratorKind.None) {
        // No key generator is attached.
        // Each hierarchy has it's own equality identifier.
        key.IsFirstAmongSimilarKeys = true;
        key.EqualityIdentifier = new object();
        return key;
      }

      // Hierarchy has key generator.

      // Setup key generator name.
      key.GeneratorKind = generatorKind;
      key.GeneratorBaseName = context.NameBuilder.BuildKeyGeneratorBaseName(key, hierarchyDef);
      key.GeneratorName = context.NameBuilder.BuildKeyGeneratorName(key, hierarchyDef);
      var generatorIdentity = context.Configuration.MultidatabaseKeys
        ? key.GeneratorBaseName
        : key.GeneratorName;

      // Equality indentifier is the same if and only if key generator names match.
      key.IsFirstAmongSimilarKeys = !processedKeyGenerators.Contains(key.GeneratorName);
      if (key.IsFirstAmongSimilarKeys)
        processedKeyGenerators.Add(key.GeneratorName);
      object equalityIdentifier;
      if (keyEqualityIdentifiers.TryGetValue(generatorIdentity, out equalityIdentifier))
        key.EqualityIdentifier = equalityIdentifier;
      else {
        key.EqualityIdentifier = new object();
        keyEqualityIdentifiers.Add(generatorIdentity, key.EqualityIdentifier);
      }

      // Don't create sequences for user key generators
      // and for key generators that are not sequence-backed (such as GuidGenerator).
      if (key.GeneratorKind==KeyGeneratorKind.Custom || !IsSequenceBacked(key))
        return key;

      // Generate backing sequence.
      SequenceInfo sequence;
      if (sequences.TryGetValue(key.GeneratorName, out sequence))
        key.Sequence = sequence;
      else {
        var newSequence = BuildSequence(hierarchyDef, key);
        if (context.Configuration.MultidatabaseKeys)
          EnsureSequenceSeedIsUnique(newSequence);
        key.Sequence = newSequence;
        sequences.Add(key.GeneratorName, key.Sequence);
      }

      return key;
    }

    private SequenceInfo BuildSequence(HierarchyDef hierarchyDef, KeyInfo key)
    {
      var seed = 0L;
      var cacheSize = (long) context.Configuration.KeyGeneratorCacheSize;

      var generatorName = key.GeneratorName;
      KeyGeneratorConfiguration configuration;
      if (keyGeneratorConfigurations.TryGetValue(generatorName, out configuration)) {
        seed = configuration.Seed;
        cacheSize = configuration.CacheSize;
      }

      var sequence = new SequenceInfo(generatorName) {
        Seed = seed + cacheSize, // Preallocate keys for the first access
        Increment = cacheSize,
        MappingDatabase = hierarchyDef.Root.MappingDatabase,
        MappingSchema = context.Configuration.DefaultSchema,
        MappingName = context.NameBuilder.BuildSequenceName(key),
      };

      return sequence;
    }

    private bool IsSequenceBacked(KeyInfo key)
    {
      var valueType = key.SingleColumnType;
      return valueType!=null && KeyGeneratorFactory.IsSequenceBacked(valueType);
    }

    private bool DeclaresOnValidate(Type type)
    {
      try {
        var method = type.GetMethod("OnValidate", BindingFlags.Instance | BindingFlags.NonPublic);
        return method!=null && method.DeclaringType!=null && method.DeclaringType.Assembly!=GetType().Assembly;
      }
      catch(AmbiguousMatchException) {
        // Many OnValidate methods, assume OnValidate() is overridden
        return true;
      }
    }

    private void EnsureSequenceSeedIsUnique(SequenceInfo sequenceToCheck)
    {
      var conflictingSequence = sequences.Values
        .FirstOrDefault(sequence =>
          sequence.Seed==sequenceToCheck.Seed
            && sequence.MappingName==sequenceToCheck.MappingName
            && sequence.MappingSchema==sequenceToCheck.MappingSchema);

      if (conflictingSequence!=null)
        throw new DomainBuilderException(string.Format(Strings.ExKeyGeneratorsXAndYHaveTheSameSeedValue,
          conflictingSequence.Name, sequenceToCheck.Name));
    }

    #endregion


    // Constructors

    public TypeBuilder(BuildingContext context)
    {
      this.context = context;

      keyGeneratorConfigurations = context.Configuration.KeyGenerators
        .ToDictionary(configuration => context.NameBuilder.BuildKeyGeneratorName(configuration));
    }
  }
}