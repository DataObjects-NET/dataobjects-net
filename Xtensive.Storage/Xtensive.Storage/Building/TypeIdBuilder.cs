// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.14

using System.Linq;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Metadata;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Building
{  
  internal static class TypeIdBuilder
  {
    public static void RegisterSystemTypes(TypeRegistry typeRegistry)
    {
      typeRegistry.Register(typeof(Type).Assembly, typeof(Type).Namespace);
    }

    public static void BuildTypeIds()
    {
      BuildSystemTypeIds();
      ReadTypeIds();
    }

    public static void BuildSystemTypeIds()
    {
      var context = BuildingContext.Current;      
      foreach (System.Type type in context.SystemTypeIds.Keys) {
        TypeInfo typeInfo = context.Model.Types[type];
        SetTypeId(typeInfo, context.SystemTypeIds[type]);
      }
      context.Model.Types.BuildTypeIdIndex();
    }

    public static void ReadTypeIds()
    {
      var context = BuildingContext.Current;

      int maxTypeId = GetMaxTypeId();
      foreach (TypeInfo type in context.Model.Types) {
        if (!type.IsEntity)
          continue;

        if (type.TypeId!=TypeInfo.NoTypeId)
          continue;

        SetTypeId(type, LoadTypeId(type));
        if (type.TypeId!=TypeInfo.NoTypeId)
          continue;

        SetTypeId(type, maxTypeId++);
        SaveTypeId(type, type.TypeId);
      }
      context.Model.Types.BuildTypeIdIndex();
    }

    private static void SetTypeId(TypeInfo type, int typeId)
    {
      type.SetTypeId(typeId, BuildingContext.Current.ModelUnlockKey);
    }

    private static int GetMaxTypeId()
    {
      if (Query<Type>.All.Count() == 0)
        return TypeInfo.MinTypeId;
      return Query<Type>.All.Max(t => t.TypeId);
    }

    private static int LoadTypeId(TypeInfo typeInfo)
    {
      string name = GetTypeName(typeInfo);
      var metaType = Query<Type>.All.Where(type => type.Name==name).FirstOrDefault();
      return metaType==null ? TypeInfo.NoTypeId : metaType.Id;
    }

    private static void SaveTypeId(TypeInfo typeInfo, int typeId)
    {
      string name = GetTypeName(typeInfo);
      var metaType = Query<Type>.All
        .Where(type => type.Name==name)
        .FirstOrDefault() ?? new Type(typeId);
      metaType.Name = name;
    }

    private static string GetTypeName(TypeInfo typeInfo)
    {
      return BuildingContext.Current.Domain.TypeNameResolver.GetTypeName(typeInfo.UnderlyingType);
    }
  }
}