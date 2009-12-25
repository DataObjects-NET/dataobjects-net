// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.16

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Building.Builders
{
  internal static partial class IndexBuilder 
  {
    public static void BuildFullTextIndexes()
    {
      var context = BuildingContext.Current;
      var modelDef = context.ModelDef;
      var model = context.Model;
      var indexLookup = modelDef.FullTextIndexes.ToLookup(fi => model.Types[fi.Type.UnderlyingType].Hierarchy);
      foreach (var hierarchyIndexes in indexLookup) {
        switch(hierarchyIndexes.Key.Schema) {
          case InheritanceSchema.ClassTable:
            break;
          case InheritanceSchema.SingleTable:
            var root = hierarchyIndexes.Key.Root;
            var primaryIndex = root.Indexes.Single(i => i.IsPrimary && !i.IsVirtual);
            var index = new FullTextIndexInfo(primaryIndex, context.NameBuilder.BuildFullTextIndexName(root));
            foreach (var pair in primaryIndex.KeyColumns)
              index.KeyColumns.Add(pair.Key);
            
//            var keyColumns = new List<ColumnInfo>();
//            var columns = new List<ColumnInfo>();
//            var includedColumns = new List<ColumnInfo>();

//            foreach (var fullTextIndexDef in hierarchyIndexes) {
//              fullTextIndexDef.
//            }
            break;
          case InheritanceSchema.ConcreteTable:
            break;
        }
      }
//      foreach (var fullTextIndex in modelDef.FullTextIndexes.Select()) {
        
//      }
    }
  }
}