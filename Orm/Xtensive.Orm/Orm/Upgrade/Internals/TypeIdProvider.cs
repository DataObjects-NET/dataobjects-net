// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.19

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Model;
using Xtensive.Reflection;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class TypeIdProvider : ITypeIdProvider
  {
    private readonly UpgradeContext context;

    public int GetTypeId(Type type)
    {
      var typeIdMap = context.FullTypeMap;
      if (typeIdMap==null)
        return TypeInfo.NoTypeId;
      var typeName = type.GetFullName();
      if (context.Stage==UpgradeStage.Upgrading) {
        var mapping = context.UpgradedTypesMapping;
        if (mapping!=null) {
          string oldTypeName;
          return mapping.TryGetValue(typeName, out oldTypeName)
            ? FindTypeId(typeIdMap, oldTypeName)
            : TypeInfo.NoTypeId;
        }
      }
      return FindTypeId(typeIdMap, typeName);
    }

    private int FindTypeId(IDictionary<string, int> typeIdMap, string typeName)
    {
      int typeId;
      return typeIdMap.TryGetValue(typeName, out typeId) ? typeId : TypeInfo.NoTypeId;
    }


    // Constructors

    public TypeIdProvider(UpgradeContext context)
    {
      ArgumentValidator.EnsureArgumentNotNull(context, "context");
      this.context = context;
    }
  }
}