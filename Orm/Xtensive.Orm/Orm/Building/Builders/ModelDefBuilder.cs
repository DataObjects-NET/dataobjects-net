// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2009.05.20

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Validation;
using Xtensive.Reflection;

namespace Xtensive.Orm.Building.Builders
{
  internal sealed class ModelDefBuilder
  {
    private readonly BuildingContext context;
    private readonly AttributeProcessor attributeProcessor;
    private readonly Queue<Type> types;

    public void ProcessTypes()
    {
      var closedGenericTypes = new List<Type>();
      var openGenericTypes = new List<Type>();

      while (types.TryDequeue(out var type)) {
        if (!IsTypeAvailable(type)) {
          continue;
        }

        if (type.IsGenericType) {
          if (type.IsGenericTypeDefinition) {
            openGenericTypes.Add(type);
          }
          else {
            closedGenericTypes.Add(type);
          }
        }
        else {
          ProcessType(type);
        }
      }

      closedGenericTypes = closedGenericTypes.Where(t => !t.FullName.IsNullOrEmpty()).ToList();
      closedGenericTypes = closedGenericTypes.SortTopologically(
        (first, second) => second.GetGenericArguments().Any(argument => argument.IsAssignableFrom(first)));
      if (closedGenericTypes == null) {
        throw Exceptions.InternalError("Cyclic dependency in closed generics graph", BuildLog.Instance);
      }

      foreach (var type in closedGenericTypes) {
        ProcessType(type);
      }

      foreach (var type in openGenericTypes) {
        ProcessType(type);
      }
    }

    #region Automatic processing members

    public TypeDef ProcessType(Type type)
    {
      var modelDef = context.ModelDef;

      var typeDef = modelDef.Types.TryGetValue(type);
      if (typeDef != null) {
        return typeDef;
      }

      using (BuildLog.InfoRegion(Strings.LogDefiningX, type.GetFullName())) {
        typeDef = DefineType(type);
        if (modelDef.Types.Contains(typeDef.Name)) {
          throw new DomainBuilderException(string.Format(Strings.ExTypeWithNameXIsAlreadyDefined, typeDef.Name));
        }

        HierarchyDef hierarchyDef = null;
        if (typeDef.IsEntity) {
          // HierarchyRootAttribute is required for hierarchy root
          var hra = type.GetAttribute<HierarchyRootAttribute>(AttributeSearchOptions.Default);
          if (hra != null) {
            hierarchyDef = DefineHierarchy(typeDef, hra);
          }
        }

        ProcessProperties(typeDef, hierarchyDef);

        if (typeDef.IsEntity || typeDef.IsInterface) {
          ProcessIndexes(typeDef);
        }

        if (hierarchyDef != null) {
          BuildLog.Info(Strings.LogHierarchyX, typeDef.Name);
          modelDef.Hierarchies.Add(hierarchyDef);
        }

        modelDef.Types.Add(typeDef);

        ProcessFullTextIndexes(typeDef);

        typeDef.Validators.AddRange(type.GetCustomAttributes(WellKnownOrmInterfaces.ObjectValidator, false).Cast<IObjectValidator>());
        return typeDef;
      }
    }

    private void ProcessFullTextIndexes(TypeDef typeDef)
    {
      if (ShouldSkipFulltextIndex(typeDef)) {
        return;
      }

      var fullTextIndexDef = new FullTextIndexDef(typeDef);
      foreach (var fieldDef in typeDef.Fields.Where(f => f.UnderlyingProperty != null)) {
        var fullTextAttribute = fieldDef.UnderlyingProperty
          .GetAttribute<FullTextAttribute>(AttributeSearchOptions.InheritAll);
        if (fullTextAttribute == null) {
          continue;
        }

        var fullTextField = new FullTextFieldDef(fieldDef.Name, fullTextAttribute.Analyzed) {
          Configuration = fullTextAttribute.Configuration,
          TypeFieldName = fullTextAttribute.DataTypeField
        };
        fullTextIndexDef.Fields.Add(fullTextField);
      }

      if (fullTextIndexDef.Fields.Count > 0) {
        context.ModelDef.FullTextIndexes.Add(fullTextIndexDef);
      }
    }

    public void ProcessProperties(TypeDef typeDef, HierarchyDef hierarchyDef)
    {
      var properties = TypeHelper.GetProperties(typeDef.UnderlyingType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

      foreach (var propertyInfo in properties
          .Where(IsFieldAvailable)) {   // Domain builder stage-related filter

        // FieldAttribute presence is required
        var reversedFieldAttributes = GetReversedFieldAttributes<FieldAttribute>(propertyInfo);
        if (reversedFieldAttributes.Count == 0) {
          continue;
        }

        var field = DefineField(propertyInfo, reversedFieldAttributes);

        // Declared & inherited fields must be processed for hierarchy root
        if (hierarchyDef != null) {
          typeDef.Fields.Add(field);
          BuildLog.Info(Strings.LogFieldX, field.Name);

          var keyAttributes = propertyInfo.GetAttribute<KeyAttribute>(AttributeSearchOptions.InheritAll);
          if (keyAttributes != null) {
            attributeProcessor.Process(hierarchyDef, field, keyAttributes);
          }
        }
        // Only declared properties must be processed in other cases
        else if (propertyInfo.DeclaringType == propertyInfo.ReflectedType) {
          typeDef.Fields.Add(field);
          BuildLog.Info(Strings.LogFieldX, field.Name);
        }

        // Checking whether property type is registered in model
        var propertyType = field.UnderlyingProperty.PropertyType;
        if (propertyType.IsGenericParameter) {
          continue;
        }

        if (propertyType.IsSubclassOf(WellKnownOrmTypes.Persistent) && !context.ModelDef.Types.Contains(propertyType)) {
          types.Enqueue(propertyType);
        }
      }
    }

    private void ProcessIndexes(TypeDef typeDef)
    {
      // process indexes which defined directly for type
      var ownIndexesOfType = typeDef.Fields
        .Where(f => f.IsIndexed)
        .Select(f => new IndexAttribute(f.Name))
        .Concat(typeDef.UnderlyingType.GetAttributes<IndexAttribute>(AttributeSearchOptions.InheritNone) ??
          Enumerable.Empty<IndexAttribute>());

      var ownIndexesCollection = new HashSet<IndexAttribute>();
      foreach (var attribute in ownIndexesOfType) {
        var index = DefineIndex(typeDef, attribute);
        ownIndexesCollection.Add(attribute);
        if (typeDef.Indexes.Contains(index.Name)) {
          throw new DomainBuilderException(
            string.Format(Strings.ExIndexWithNameXIsAlreadyRegistered, index.Name));
        }

        typeDef.Indexes.Add(index);
        BuildLog.Info(Strings.LogIndexX, index.Name);
      }

      //process indexes which inherited from base classes
      //GetAttribute<T>(AttributeSearchOptions.InheritFromAllBase) extension returns all attributes of T - own indexes of type and inherited.
      var allIndexes = typeDef.UnderlyingType
          .GetAttributes<IndexAttribute>(AttributeSearchOptions.InheritFromAllBase) ??
        Enumerable.Empty<IndexAttribute>();

      //We need only inherited attributes. We checks all IndexAttributes in allIndexes and skip indexes
      //which own of type.
      foreach (var attribute in allIndexes.Where(index => !ownIndexesCollection.Contains(index))) {
        var index = DefineIndex(typeDef, attribute);
        if (typeDef.Indexes.Contains(index.Name)) {
          throw new DomainBuilderException(
            string.Format(Strings.ExIndexWithNameXIsAlreadyRegistered, index.Name));
        }

        index.IsInherited = true;
        typeDef.Indexes.Add(index);
        BuildLog.Info(Strings.LogIndexX, index.Name);
      }
    }

    #endregion

    #region Definition-related members

    public TypeDef DefineType(Type type)
    {
      var typeDef = new TypeDef(this, type, context.Validator);
      typeDef.Name = context.NameBuilder.BuildTypeName(context, typeDef);

      if (!(type.UnderlyingSystemType.IsInterface || type.IsAbstract)) {
        var sta = type.GetAttribute<SystemTypeAttribute>(AttributeSearchOptions.Default);
        if (sta != null) {
          attributeProcessor.Process(typeDef, sta);
        }
      }

      if (typeDef.IsEntity) {
        var tdva = type.GetAttribute<TypeDiscriminatorValueAttribute>(AttributeSearchOptions.Default);
        if (tdva != null) {
          attributeProcessor.Process(typeDef, tdva);
        }
      }

      if (typeDef.IsInterface) {
        var mva = type.GetAttribute<MaterializedViewAttribute>(AttributeSearchOptions.Default);
        if (mva != null) {
          attributeProcessor.Process(typeDef, mva);
        }
      }

      var ma = type.GetAttribute<TableMappingAttribute>(AttributeSearchOptions.Default);
      if (ma != null) {
        attributeProcessor.Process(typeDef, ma);
      }

      return typeDef;
    }

    public HierarchyDef DefineHierarchy(TypeDef typeDef, HierarchyRootAttribute attribute)
    {
      context.Validator.ValidateHierarchyRoot(context.ModelDef, typeDef);

      var hierarchyDef = new HierarchyDef(typeDef);
      attributeProcessor.Process(hierarchyDef, attribute);

      // KeyGeneratorAttribute is optional
      var kga = typeDef.UnderlyingType.GetAttribute<KeyGeneratorAttribute>(AttributeSearchOptions.InheritAll);
      if (kga != null) {
        attributeProcessor.Process(hierarchyDef, kga);
      }

      return hierarchyDef;
    }

    public FieldDef DefineField(PropertyInfo propertyInfo) =>
      DefineField(propertyInfo, GetReversedFieldAttributes<FieldAttribute>(propertyInfo));

    public FieldDef DefineField(PropertyInfo propertyInfo, IReadOnlyList<FieldAttribute> reversedFieldAttributes)
    {
      // Persistent indexers are not supported
      var indexParameters = propertyInfo.GetIndexParameters();
      if (indexParameters.Length > 0) {
        throw new DomainBuilderException(Strings.ExIndexedPropertiesAreNotSupported);
      }

      var fieldDef = new FieldDef(propertyInfo, context.Validator);
      fieldDef.Name = context.NameBuilder.BuildFieldName(fieldDef);

      if (reversedFieldAttributes.Count > 0) {
        for (int i = reversedFieldAttributes.Count; i-- > 0;) {
          attributeProcessor.Process(fieldDef, reversedFieldAttributes[i]);
        }

        // Association
        var reversedAssociationAttributes = GetReversedFieldAttributes<AssociationAttribute>(propertyInfo);
        for (int i = reversedAssociationAttributes.Count; i-- > 0;) {
          attributeProcessor.Process(fieldDef, reversedAssociationAttributes[i]);
        }

        // Mapping name
        var reversedMappingAttributes = GetReversedFieldAttributes<FieldMappingAttribute>(propertyInfo);
        for (int i = reversedMappingAttributes.Count; i-- > 0;) {
          attributeProcessor.Process(fieldDef, reversedMappingAttributes[i]);
        }

        // Type discriminator
        var typeDiscriminatorAttribute =
          propertyInfo.GetAttribute<TypeDiscriminatorAttribute>(AttributeSearchOptions.InheritAll);
        if (typeDiscriminatorAttribute != null) {
          attributeProcessor.Process(fieldDef, typeDiscriminatorAttribute);
        }

        // Version
        var versionAttribute = propertyInfo.GetAttribute<VersionAttribute>(AttributeSearchOptions.InheritAll);
        if (versionAttribute != null) {
          attributeProcessor.Process(fieldDef, versionAttribute);
        }

        // Validators
        fieldDef.Validators.AddRange(propertyInfo.GetCustomAttributes(WellKnownOrmInterfaces.PropertyValidator, false).Cast<IPropertyValidator>());
      }

      return fieldDef;
    }

    public FieldDef DefineField(Type declaringType, string name, Type valueType)
    {
      // Prefer standard field definition flow if corresponding property exists
      var propertyInfo = declaringType.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic);
      if (propertyInfo != null) {
        return DefineField(propertyInfo);
      }

      return new FieldDef(valueType, context.Validator) {Name = name};
    }

    public IndexDef DefineIndex(TypeDef typeDef, IndexAttribute attribute)
    {
      var index = new IndexDef(typeDef, context.Validator) {IsSecondary = true};
      attributeProcessor.Process(index, attribute);

      if (string.IsNullOrEmpty(index.Name) && index.KeyFields.Count > 0) {
        index.Name = context.NameBuilder.BuildIndexName(typeDef, index);
      }

      return index;
    }

    #endregion

    #region Helper members (to reduce cohesion)

    private bool ShouldSkipFulltextIndex(TypeDef typeDef)
    {
      var hierarchy = context.ModelDef.FindHierarchy(typeDef);
      return hierarchy == null && !typeDef.IsStructure;
    }

    // Attributes will contain attributes from all inheritance chain
    // with the most specific type first.
    // Should be enumerated in reversed order for correct processing (i.e. descendants override settings from base).
    private static IReadOnlyList<T> GetReversedFieldAttributes<T>(PropertyInfo property) where T : Attribute =>
      property.GetAttributes<T>(AttributeSearchOptions.InheritAll);

    private bool IsTypeAvailable(Type type) =>
      context.BuilderConfiguration.ModelFilter.IsTypeAvailable(type)
      && type != WellKnownOrmTypes.EntitySetItemOfT1T2;

    private bool IsFieldAvailable(PropertyInfo property) =>
      context.BuilderConfiguration.ModelFilter.IsFieldAvailable(property);

    #endregion

    // Constructors

    public ModelDefBuilder(BuildingContext context)
    {
      this.context = context;
      attributeProcessor = new AttributeProcessor(context);
      types = new Queue<Type>(context.Configuration.Types);
    }
  }
}