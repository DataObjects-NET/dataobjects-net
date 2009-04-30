// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.14

using System.Collections.Generic;
using System.Linq;
using Xtensive.Storage.Metadata;
using Xtensive.Storage.Model;
using Xtensive.Storage.Upgrade;

namespace Xtensive.Storage.Building
{  
  internal static class TypeIdBuilder
  {
    public static void BuildTypeIds()
    {
      BuildSystemTypeIds();
      BuildRegularTypeIds();
    }

    public static void BuildSystemTypeIds()
    {
      var context = BuildingContext.Current;      
      foreach (var type in context.SystemTypeIds.Keys) {
        var typeInfo = context.Model.Types[type];
        AssignTypeId(typeInfo, context.SystemTypeIds[type]);
      }
      context.Model.Types.BuildTypeIdIndex();
    }

    public static void BuildRegularTypeIds()
    {
      var context = BuildingContext.Current;
      var types = CachedQuery.Execute(() => Query<Type>.All).ToArray();
      var typeByName = new Dictionary<string, Type>();
      foreach (var type in types)
        typeByName.Add(type.Name, type);
      var maxTypeId = types.Count()==0 ? TypeInfo.MinTypeId : types.Max(t => t.TypeId);

      foreach (var type in context.Model.Types) {
        if (!type.IsEntity || type.TypeId!=TypeInfo.NoTypeId)
          continue;
        var name = GetTypeName(type.UnderlyingType);
        if (typeByName.ContainsKey(name))
          // Type is found in metadata
          AssignTypeId(type, typeByName[name].Id);
        else {
          // Type is not found in metadata
          AssignTypeId(type, ++maxTypeId);
          new Type(type.TypeId, name);
        }
      }
      context.Model.Types.BuildTypeIdIndex();
    }

    public static string GetTypeName(System.Type type)
    {
      var context = UpgradeContext.Current;
      string name = type.FullName;
      if (context==null)
        return name;
      var assembly = type.Assembly;
      if (!context.UpgradeHandlers.ContainsKey(assembly))
        return name;
      return context.UpgradeHandlers[assembly].First().GetTypeName(type);
    }

    private static void AssignTypeId(TypeInfo type, int typeId)
    {
      type.SetTypeId(typeId, BuildingContext.Current.ModelUnlockKey);
    }
  }
}