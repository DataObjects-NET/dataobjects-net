// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.14

using System;
using System.Linq;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Building
{  
  internal static class TypeIdBuilder
  {
    public static void RegisterSystemTypes(TypeRegistry typeRegistry)
    {
      typeRegistry.Register(typeof(Metadata.Type).Assembly, typeof(Metadata.Type).Namespace);
    }

    public static void BuildTypeIds()
    {
      BuildSystemTypeIds();
      ReadTypeIds();
    }

    public static void BuildSystemTypeIds()
    {
      var context = BuildingContext.Current;      
      foreach (Type type in context.SystemTypeIds.Keys) {
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
      if (Query<Metadata.Type>.All.Count() == 0)
        return TypeInfo.MinTypeId;
        
      return Query<Metadata.Type>.All.Max(t => t.TypeId);
    }

    private static int LoadTypeId(TypeInfo type)
    {
      var metaType = Query<Metadata.Type>.All.Where(t => t.FullName==type.Name).FirstOrDefault();
      return metaType==null ? TypeInfo.NoTypeId : metaType.Id;
    }

    private static void SaveTypeId(TypeInfo type, int typeId)
    {
      new Metadata.Type(typeId) {FullName = type.Name};
    }
  }
}