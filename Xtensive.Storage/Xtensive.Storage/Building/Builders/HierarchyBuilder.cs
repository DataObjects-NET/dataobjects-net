// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Building.Builders
{
  internal static class HierarchyBuilder
  {
    public static HierarchyDef TryDefineHierarchy(TypeDef type)
    {
      if (!type.IsEntity)
        return null;
      Log.Info("Detecting hierarchy.");

      var attributes = type.UnderlyingType.GetAttributes<HierarchyRootAttribute>(AttributeSearchOptions.InheritFromAllBase);
      if (attributes==null || attributes.Length == 0)
        return null;

      if (attributes.Length!=1)
        throw new DomainBuilderException("Multiple hierarchy attributes are not allowed.");

      TypeDef root = BuildingContext.Current.Definition.FindRoot(type);
      if (root!=null)
        return null;

      HierarchyDef hierarchy = new HierarchyDef(type);
      AttributeProcessor.Process(hierarchy, type, attributes[0]);
      return hierarchy;
    }

    public static HierarchyDef DefineHierarchy(TypeDef typeDef, InheritanceSchema inheritanceSchema)
    {
      BuildingContext context = BuildingContext.Current;

      Log.Info("Defining hierarchy for type '{0}'", typeDef.UnderlyingType.FullName);

      TypeDef root = context.Definition.FindRoot(typeDef);
      if (root!=null)
        throw new DomainBuilderException(
          string.Format(Resources.Strings.ExTypeDefXIsAlreadyBelongsToHierarchyWithTheRootY,
          typeDef.UnderlyingType.FullName,  root.UnderlyingType.FullName));

      foreach (HierarchyDef hierarchy in context.Definition.Hierarchies)
        if (hierarchy.Root.UnderlyingType.IsSubclassOf(typeDef.UnderlyingType)) 
          throw new DomainBuilderException(
            string.Format(Resources.Strings.ExXDescendantIsAlreadyARootOfAnotherHierarchy, hierarchy.Root.UnderlyingType));

      return 
        new HierarchyDef(typeDef) {Schema = inheritanceSchema};
    }

    public static HierarchyInfo BuildHierarchy(TypeInfo root, HierarchyDef hierarchyDef)
    {
      KeyInfo ki = new KeyInfo();

      foreach (KeyValuePair<KeyField, Direction> pair in hierarchyDef.KeyFields) {
        FieldInfo field;
        if (!root.Fields.TryGetValue(pair.Key.Name, out field))
          throw new DomainBuilderException(
            string.Format(Resources.Strings.ExKeyFieldXWasNotFoundInTypeY, pair.Key.Name, root.Name));

        ki.Fields.Add(field, pair.Value);
      }

      var context = BuildingContext.Current;
      GeneratorInfo gi = context.Model.Generators[hierarchyDef.KeyGenerator, ki];
      if (gi == null) {
        gi = new GeneratorInfo(hierarchyDef.KeyGenerator, ki);
        gi.Name = root.Name;
        if (hierarchyDef.KeyGeneratorCacheSize.HasValue && hierarchyDef.KeyGeneratorCacheSize > 0)
          gi.CacheSize = hierarchyDef.KeyGeneratorCacheSize.Value;
        else
          gi.CacheSize = context.Configuration.KeyGeneratorCacheSize;
        context.Model.Generators.Add(gi);
      }
      else {
        if (hierarchyDef.KeyGeneratorCacheSize.HasValue && hierarchyDef.KeyGeneratorCacheSize.Value < gi.CacheSize)
          gi.CacheSize = hierarchyDef.KeyGeneratorCacheSize.Value;
      }

      if (hierarchyDef.KeyGenerator == typeof(KeyGenerator)) {
        if (ki.Fields.Count > 2)
          throw new DomainBuilderException(Resources.Strings.ExDefaultGeneratorCanServeHierarchyWithExactlyOneKeyField);
        if (ki.Fields.Count==2 && !ki.Fields[1].Key.IsSystem)
          throw new DomainBuilderException(Resources.Strings.ExDefaultGeneratorCanServeHierarchyWithExactlyOneKeyField);
      }

      HierarchyInfo hierarchy = new HierarchyInfo(root, hierarchyDef.Schema, ki, gi);
      hierarchy.Name = root.Name;
      context.Model.Hierarchies.Add(hierarchy);
      return hierarchy;
    }

    public static void BuildHierarchyColumns(HierarchyInfo hierarchy)
    {
      DirectionCollection<ColumnInfo> columnsCollection = hierarchy.Root.Indexes.PrimaryIndex.KeyColumns;

      for (int i = 0; i < columnsCollection.Count; i++)
        hierarchy.KeyInfo.Columns.Add(columnsCollection[i].Key);

      hierarchy.KeyInfo.Lock();
      if (hierarchy.GeneratorInfo.Type == typeof(KeyGenerator))
        hierarchy.GeneratorInfo.MappingName = BuildingContext.Current.NameBuilder.Build(hierarchy.GeneratorInfo);
    }
  }
}