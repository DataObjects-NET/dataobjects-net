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
      var mainTypeIdMap = context.FullTypeMap;
      var additionalTypeIdMap = context.UserDefinedTypeMap;
      if (mainTypeIdMap==null && additionalTypeIdMap.Count==0)
        return TypeInfo.NoTypeId;
      var typeName = type.GetFullName();
      int typeId = TypeInfo.NoTypeId;
      if (context.Stage==UpgradeStage.Upgrading || context.UpgradeMode==DomainUpgradeMode.Validate) {
        var mapping = context.UpgradedTypesMapping;
        if (mapping!=null) {
          string oldTypeName;
          if (mapping.TryGetValue(typeName, out oldTypeName))
            return FindTypeIdInBothMapSources(mainTypeIdMap, additionalTypeIdMap, oldTypeName);
        }
      }
      return FindTypeIdInBothMapSources(mainTypeIdMap, additionalTypeIdMap, typeName);
    }

    private int FindTypeIdInBothMapSources(IDictionary<string, int> mainSource, IDictionary<string, int> additionalSource, string typeName)
    {
      int typeId = TypeInfo.NoTypeId;
      if (mainSource!=null)
        typeId = FindTypeId(mainSource, typeName);
      if (typeId!=TypeInfo.NoTypeId)
        return typeId;
      if (additionalSource!=null)
        return FindTypeId(additionalSource, typeName);
      return TypeInfo.NoTypeId;
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