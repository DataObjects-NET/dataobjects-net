﻿// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.01.22

using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model.Stored;

namespace Xtensive.Orm.Upgrade.Internals.Extensions
{
  internal static class StoredDomainModelExtensions
  {
    public static ClassifiedCollection<string, Pair<string, string[]>> GetGenericTypes(this StoredDomainModel model)
    {
      var genericTypes = new ClassifiedCollection<string, Pair<string, string[]>>(pair => new[] { pair.First });
      foreach (var typeInfo in model.Types.Where(type => type.IsGeneric)) {
        var typeDefinitionName = typeInfo.GenericTypeDefinition;
        genericTypes.Add(new Pair<string, string[]>(typeDefinitionName, typeInfo.GenericArguments));
      }
      return genericTypes;
    }

    public static IEnumerable<StoredTypeInfo> GetNonConnectorTypes(this StoredDomainModel model)
    {
      var connectorTypes = (
        from association in model.Associations
        let type = association.ConnectorType
        where type!=null
        select type
        ).ToHashSet();
      return model.Types.Where(type => !connectorTypes.Contains(type));
    }
  }
}