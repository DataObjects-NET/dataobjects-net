// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Xtensive.Core.Reflection;
using System.Linq;

namespace Xtensive.Storage.Building.Builders
{
  internal static class HierarchyBuilder
  {
    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    public static HierarchyInfo BuildHierarchy(TypeInfo root, HierarchyDef hierarchyDef)
    {
      var keyInfo = new KeyInfo();

      foreach (var keyField in hierarchyDef.KeyFields) {
        FieldInfo field;
        if (!root.Fields.TryGetValue(keyField.Name, out field))
          throw new DomainBuilderException(
            string.Format(Strings.ExKeyFieldXWasNotFoundInTypeY, keyField.Name, root.Name));

        keyInfo.Fields.Add(field, keyField.Direction);
      }

      var context = BuildingContext.Current;
      GeneratorInfo gi = null;
      if (hierarchyDef.KeyGenerator != null) {
        gi = context.Model.Generators.Find(
          hierarchyDef.KeyGenerator, 
          keyInfo.Fields.Select(c => c.Key.ValueType).ToArray());
        if (gi==null) {
          gi = new GeneratorInfo(hierarchyDef.KeyGenerator, keyInfo) {
            Name = root.Name
          };
          if (gi.KeyGeneratorType==typeof (KeyGenerator))
            gi.MappingName = BuildingContext.Current.NameBuilder.BuildGeneratorName(gi);
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
      }

      var hierarchy = new HierarchyInfo(root, hierarchyDef.Schema, keyInfo, gi) {
        Name = root.Name
      };
      context.Model.Hierarchies.Add(hierarchy);
      return hierarchy;
    }

    public static void BuildHierarchyColumns(HierarchyInfo hierarchy)
    {
      var columnsCollection = hierarchy.Root.Indexes.PrimaryIndex.KeyColumns;

      for (int i = 0; i < columnsCollection.Count; i++)
        hierarchy.KeyInfo.Columns.Add(columnsCollection[i].Key);

      hierarchy.KeyInfo.UpdateState();
    }
  }
}