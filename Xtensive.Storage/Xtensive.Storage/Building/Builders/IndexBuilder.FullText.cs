// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.16

using System;
using System.Diagnostics;
using System.Linq;

namespace Xtensive.Storage.Building.Builders
{
  internal static partial class IndexBuilder 
  {
    public static void BuildFullTextIndexes()
    {
      var context = BuildingContext.Current;
      var modelDef = context.ModelDef;
      var model = context.Model;
      var indexLookup = modelDef.FullTextIndexes.ToLookup(fi => modelDef.FindHierarchy(fi.Type));
      foreach (var hierarchyIndexes in indexLookup) {
        
      }
//      foreach (var fullTextIndex in modelDef.FullTextIndexes.Select()) {
        
//      }
    }
  }
}