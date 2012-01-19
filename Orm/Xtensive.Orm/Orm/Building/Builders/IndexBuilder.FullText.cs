// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.16

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Model;
using Xtensive.Orm.Resources;

namespace Xtensive.Orm.Building.Builders
{
  internal static partial class IndexBuilder 
  {
    public static void BuildFullTextIndexes()
    {
      var context = BuildingContext.Demand();
      var modelDef = context.ModelDef;
      var model = context.Model;
      var indexLookup = modelDef.FullTextIndexes.ToLookup(fi => model.Types[fi.Type.UnderlyingType].Hierarchy);
      foreach (var hierarchyIndexes in indexLookup) {
        var root = hierarchyIndexes.Key.Root;
        switch(hierarchyIndexes.Key.InheritanceSchema) {
          case InheritanceSchema.ClassTable:
            BuildFullTextIndexesClassTable(root, hierarchyIndexes);
            break;
          case InheritanceSchema.SingleTable:
            BuildFullTextIndexesSingleTable(root, hierarchyIndexes);
            break;
          case InheritanceSchema.ConcreteTable:
            BuildFullTextIndexesConcreteTable(root, hierarchyIndexes);
            break;
        }
      }
    }

    private static void BuildFullTextIndexesClassTable(TypeInfo root, IEnumerable<FullTextIndexDef> hierarchyIndexes)
    {
      var context = BuildingContext.Demand();
      var model = context.Model;
      var indexesToDefine = hierarchyIndexes.ToList();
      if (hierarchyIndexes.Any(fti => fti.Type.UnderlyingType != root.UnderlyingType) || indexesToDefine.Count > 1)
        throw new DomainBuilderException(string.Format(Strings.ExUnableToBuildFulltextIndexesForHierarchyWithInheritanceSchemaClassTable, root.Name));
      var descendants = root.GetDescendants(true)
        .AddOne(root)
        .ToList();
      var indexDef = indexesToDefine[0];
      var primaryIndex = root.Indexes.Single(i => i.IsPrimary && !i.IsVirtual);
      var name = context.NameBuilder.BuildFullTextIndexName(root);
      var index = new FullTextIndexInfo(primaryIndex, name);
      foreach (var fullTextFieldDef in indexDef.Fields) {
        var fullTextColumn = GetFullTextColumn(root, fullTextFieldDef);
        index.Columns.Add(fullTextColumn);
      }
      foreach (var type in descendants)
        model.FullTextIndexes.Add(type, index);
    }

    private static void BuildFullTextIndexesSingleTable(TypeInfo root, IEnumerable<FullTextIndexDef> hierarchyIndexes)
    {
      var context = BuildingContext.Demand();
      var model = context.Model;
      var primaryIndex = root.Indexes.Single(i => i.IsPrimary && !i.IsVirtual);
      var name = context.NameBuilder.BuildFullTextIndexName(root);
      var index = new FullTextIndexInfo(primaryIndex, name);
      var types = new HashSet<TypeInfo>();
      foreach (var fullTextIndexDef in hierarchyIndexes) {
        var type = model.Types[fullTextIndexDef.Type.UnderlyingType];
        types.Add(type);
        foreach (var descendant in type.GetDescendants(true))
          types.Add(descendant);
        foreach (var fullTextFieldDef in fullTextIndexDef.Fields) {
          var fullTextColumn = GetFullTextColumn(type, fullTextFieldDef);
          index.Columns.Add(fullTextColumn);
        }
      }
      foreach (var type in types)
        model.FullTextIndexes.Add(type, index);
    }

    private static void BuildFullTextIndexesConcreteTable(TypeInfo root, IEnumerable<FullTextIndexDef> hierarchyIndexes)
    {
      var context = BuildingContext.Demand();
      var model = context.Model;
      var indexDefs = GatherFullTextIndexDefinitons(root, hierarchyIndexes);

      foreach (var typeIndexDef in indexDefs) {
        var type = typeIndexDef.Key;
        var primaryIndex = type.Indexes.Single(i => i.IsPrimary && !i.IsVirtual);
        var name = context.NameBuilder.BuildFullTextIndexName(root);
        var index = new FullTextIndexInfo(primaryIndex, name);
        foreach (var fullTextFieldDef in typeIndexDef.Value.SelectMany(def => def.Fields)) {
          var fullTextColumn = GetFullTextColumn(type, fullTextFieldDef);
          index.Columns.Add(fullTextColumn);
        }
        model.FullTextIndexes.Add(type, index);
      }
    }

    private static FullTextColumnInfo GetFullTextColumn(TypeInfo type, FullTextFieldDef fullTextFieldDef)
    {
      var column = type.Fields[fullTextFieldDef.Name].Column;
      var typeColumn = fullTextFieldDef.TypeFieldName == null
        ? null
        : type.Fields[fullTextFieldDef.TypeFieldName].Column;
      return new FullTextColumnInfo(column) {
        IsAnalyzed = fullTextFieldDef.IsAnalyzed, 
        Configuration = fullTextFieldDef.Configuration, 
        TypeColumn = typeColumn
      };
    }

    private static Dictionary<TypeInfo, List<FullTextIndexDef>> GatherFullTextIndexDefinitons(TypeInfo root, IEnumerable<FullTextIndexDef> hierarchyIndexes)
    {
      var context = BuildingContext.Demand();
      var model = context.Model;
      var processQueue = new Queue<TypeInfo>();
      foreach (var type in root.GetDescendants())
        processQueue.Enqueue(type);

      var indexDefs = hierarchyIndexes.ToDictionary(
        ftid => model.Types[ftid.Type.UnderlyingType],
        ftid => new List<FullTextIndexDef>() {ftid});

      while (processQueue.Count > 0) {
        var type = processQueue.Dequeue();
        List<FullTextIndexDef> indexes;
        List<FullTextIndexDef> parentIndexes;
        var typeHasIndexDef = indexDefs.TryGetValue(type, out indexes);
        if (indexDefs.TryGetValue(type.GetAncestor(), out parentIndexes)) {
          if (typeHasIndexDef)
            indexes.AddRange(parentIndexes);
          else {
            indexes = new List<FullTextIndexDef>(parentIndexes);
            indexDefs.Add(type, indexes);
            typeHasIndexDef = true;
          }
        }
        if (typeHasIndexDef)
          foreach (var descendant in type.GetDescendants())
            processQueue.Enqueue(descendant);
      }
      return indexDefs;
    }


  }
}