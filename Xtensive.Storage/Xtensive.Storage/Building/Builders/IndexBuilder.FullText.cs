// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.16

using System;
using System.Diagnostics;

namespace Xtensive.Storage.Building.Builders
{
  internal static partial class IndexBuilder 
  {
    public static void BuildFullTextIndexes()
    {
      var context = BuildingContext.Current;
      var modelDef = context.ModelDef;
      var model = context.Model;
      foreach (var fullTextIndex in modelDef.FullTextIndexes) {
        
      }
    }
  }
}