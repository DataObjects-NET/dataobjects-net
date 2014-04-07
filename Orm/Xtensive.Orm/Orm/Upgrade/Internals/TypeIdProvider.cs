// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.19

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Model;
using Xtensive.Orm.Model.Stored;
using Xtensive.Reflection;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class TypeIdProvider : ITypeIdProvider
  {
    private readonly UpgradeContext context;

    public int GetTypeId(Type type)
    {
      var typeId = TypeInfo.NoTypeId;
      var oldModel = context.ExtractedDomainModel;
      if (oldModel==null && context.ExtractedTypeMap==null)
        return typeId;
      
      var fullName = type.GetFullName();
      var searchSource = context.UpgradingStageTypeMap ?? context.ExtractedTypeMap;
      return FindTypeId(searchSource, fullName);
    }

    private int FindTypeId(IDictionary<string, int> searchSource, string typeName)
    {
      int typeId;
      var name = GetSearchName(typeName);
      if (searchSource.TryGetValue(name, out typeId))
        return typeId;
      return TypeInfo.NoTypeId;
    }

    private string GetSearchName(string typeName)
    {
      if (context.UpgradingStageTypeMap!=null)
        return typeName;
      string oldName;
      return (TryGetOldName(typeName, out oldName)) ? oldName : typeName;
    }

    private bool TryGetOldName(string newName, out string oldName)
    {
      oldName = string.Empty;
      if (context.TypeMapAfterHintGeneration!=null)
        return context.TypeMapAfterHintGeneration.TryGetValue(newName, out oldName);
      return false;
    }

    // Constructors

    public TypeIdProvider(UpgradeContext context)
    {
      ArgumentValidator.EnsureArgumentNotNull(context, "context");
      this.context = context;
    }
  }
}