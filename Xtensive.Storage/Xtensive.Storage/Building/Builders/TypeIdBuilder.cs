// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.14

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Upgrade;
using Type=Xtensive.Storage.Metadata.Type;
using Xtensive.Storage.Upgrade.Hints;

namespace Xtensive.Storage.Building.Builders
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
      var typeNameProvider = context.BuilderConfiguration.TypeNameProvider ?? (t => t.FullName);
      var types = Query<Type>.All.ToArray();
      var typeByName = new Dictionary<string, Type>();
      foreach (var type in types)
        typeByName.Add(type.Name, type);
      var nextTypeId = types.Count()==0 
        ? TypeInfo.MinTypeId 
        : Math.Max(TypeInfo.MinTypeId, types.Max(t => t.Id) + 1);

      foreach (var type in context.Model.Types) {
        if (!type.IsEntity || type.TypeId!=TypeInfo.NoTypeId)
          continue;
        var name = typeNameProvider.Invoke(type.UnderlyingType);
        if (typeByName.ContainsKey(name))
          // Type is found in metadata
          AssignTypeId(type, typeByName[name].Id);
        else {
          // Type is not found in metadata
          var upgradeContext = UpgradeContext.Current;
          var hasRenameHint = upgradeContext!=null &&
            upgradeContext.Hints
              .OfType<RenameTypeHint>()
              .Any(hint => hint.TargetType==type.UnderlyingType);
          if (!hasRenameHint) {
            AssignTypeId(type, nextTypeId++);
            new Type(type.TypeId, name);
            // Session.Current.Persist();
          }
        }
      }
      context.Model.Types.BuildTypeIdIndex();
    }

    private static void AssignTypeId(TypeInfo type, int typeId)
    {
      type.SetTypeId(typeId, BuildingContext.Current.ModelUnlockKey);
    }
  }
}